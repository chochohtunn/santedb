﻿/*
 * Based on OpenIZ - Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
 * Portions Copyright 2019-2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej (Justin Fyfe)
 * Date: 2019-11-27
 */
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Security.Principal;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Local policy decision service
    /// </summary>
    [ServiceProvider("Default PDP Service")]
    public class DefaultPolicyDecisionService : IPolicyDecisionService
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public String ServiceName => "Default PDP Decision Service";

        /// <summary>
        /// Get a policy decision 
        /// </summary>
        public PolicyDecision GetPolicyDecision(IPrincipal principal, object securable)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            else if (securable == null)
                throw new ArgumentNullException(nameof(securable));
            // We need to get the active policies for this
            var pip = ApplicationServiceContext.Current.GetService<IPolicyInformationService>();
            IEnumerable<IPolicyInstance> securablePolicies = pip.GetActivePolicies(securable),
                principalPolicies = pip.GetActivePolicies(principal);

            List<PolicyDecisionDetail> details = new List<PolicyDecisionDetail>();
            var retVal = new PolicyDecision(securable, details);

            foreach (var pol in securablePolicies)
            {
                // Get most restrictive from principal
                var rules = principalPolicies.Where(p => p.Policy.Oid.StartsWith(pol.Policy.Oid)).Select(o => o.Rule);
                PolicyGrantType rule = PolicyGrantType.Deny;
                if(rules.Any())
                    rule = rules.Min();

                // Rule for elevate can only be made when the policy allows for it & the principal is allowed
                if (rule == PolicyGrantType.Elevate &&
                    (!pol.Policy.CanOverride ||
                    principalPolicies.Any(o => o.Policy.Oid == PermissionPolicyIdentifiers.ElevateClinicalData && o.Rule == PolicyGrantType.Grant)))
                    rule = PolicyGrantType.Deny;

                details.Add(new PolicyDecisionDetail(pol.Policy.Oid, rule));
            }

            return retVal;
        }

        /// <summary>
        /// Get a policy outcome
        /// </summary>
        public PolicyGrantType GetPolicyOutcome(IPrincipal principal, string policyId)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            else if (String.IsNullOrEmpty(policyId))
                throw new ArgumentNullException(nameof(policyId));

            // Get the user object from the principal
            var pip = ApplicationServiceContext.Current.GetService<IPolicyInformationService>();

            // Can we make this decision based on the claims? 
            if (principal is IClaimsPrincipal claimsPrincipal && claimsPrincipal.HasClaim(c => c.Type == SanteDBClaimTypes.SanteDBGrantedPolicyClaim)) // must adhere to the token
            {
                if (claimsPrincipal.HasClaim(c => c.Type == SanteDBClaimTypes.SanteDBGrantedPolicyClaim && policyId.StartsWith(c.Value)))
                    return PolicyGrantType.Grant;
                else
                {
                    // Can override?
                    var polInfo = pip.GetPolicy(policyId);
                    if (polInfo.CanOverride)
                        return PolicyGrantType.Elevate;
                    else return PolicyGrantType.Deny;
                }
            }
            else
            {
                
                // Policies
                var activePolicies = pip.GetActivePolicies(principal).Where(o => policyId.StartsWith(o.Policy.Oid));
                // Most restrictive
                IPolicyInstance policyInstance = null;
                foreach (var pol in activePolicies)
                    if (policyInstance == null)
                        policyInstance = pol;
                    else if (pol.Rule < policyInstance.Rule || // More restrictive
                        pol.Policy.Oid.Length > policyInstance.Policy.Oid.Length // More specific
                        )
                        policyInstance = pol;

                var retVal = PolicyGrantType.Deny;

                if (policyInstance == null)
                    retVal = PolicyGrantType.Deny;
                else if (!policyInstance.Policy.CanOverride && policyInstance.Rule == PolicyGrantType.Elevate)
                    retVal = PolicyGrantType.Deny;
                else if (!policyInstance.Policy.IsActive)
                    retVal = PolicyGrantType.Grant;
                else if ((policyInstance.Policy as ILocalPolicy)?.Handler != null)
                {
                    var policy = policyInstance.Policy as ILocalPolicy;
                    if (policy != null)
                        retVal = policy.Handler.GetPolicyDecision(principal, policy, null).Outcome;

                }
                else
                    retVal = policyInstance.Rule;
                return retVal;

            }
        }
    }
}
