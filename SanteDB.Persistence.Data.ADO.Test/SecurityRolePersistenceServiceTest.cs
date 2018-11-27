﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SanteDB.Core;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System.Linq;
using System.Security.Principal;

namespace SanteDB.Persistence.Data.ADO.Test
{
    /// <summary>
    /// Summary description for SecurityRolePersistenceServiceTest
    /// </summary>
    [TestClass]
    public class SecurityRolePersistenceServiceTest : PersistenceTest<SecurityRole>
    {

        // Chicken costumer policy
        private static SecurityPolicy s_chickenCostumePolicy;
        private static IPrincipal s_authorization;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            DataTestUtil.Start(context);

            AuthenticationContext.Current = new AuthenticationContext(AuthenticationContext.SystemPrincipal);
            IIdentityProviderService identityProvider = ApplicationServiceContext.Current.GetService<IIdentityProviderService>();
            var identity = identityProvider.CreateIdentity(nameof(SecurityRolePersistenceServiceTest),  "password", AuthenticationContext.Current.Principal);

            // Give this identity the administrative functions group
            IRoleProviderService roleProvider = ApplicationServiceContext.Current.GetService<IRoleProviderService>();
            roleProvider.AddUsersToRoles(new string[] { identity.Name },  new string[] { "ADMINISTRATORS" }, AuthenticationContext.Current.Principal);

            // Authorize
            s_authorization = identityProvider.Authenticate(nameof(SecurityRolePersistenceServiceTest), "password");


            IDataPersistenceService<SecurityPolicy> policyService = ApplicationServiceContext.Current.GetService<IDataPersistenceService<SecurityPolicy>>();
            s_chickenCostumePolicy = new SecurityPolicy()
            {
                Name = "Allow wearing of chicken costume",
                Oid = "2.3.23.543.25.2"
            };
            s_chickenCostumePolicy = policyService.Insert(s_chickenCostumePolicy, TransactionMode.Commit, s_authorization);

        }

        /// <summary>
        /// Test inseration of valid group
        /// </summary>
        [TestMethod]
        public void TestInsertValidGroupSimple()
        {
            var roleUnderTest = new SecurityRole()
            {
                Name = "Test Users"
            };

            var roleAfterInsert = base.DoTestInsert(roleUnderTest, s_authorization);
            Assert.AreEqual("Test Users", roleAfterInsert.Name);
        }

        /// <summary>
        /// Tests insertion of a group with valid policies
        /// </summary>
        [TestMethod]
        public void TestInsertValidGroupPolicies()
        {

            var roleUnderTest = new SecurityRole()
            {
                Name = "Chicken Costume Users"
            };
            roleUnderTest.Policies.Add(new SecurityPolicyInstance(s_chickenCostumePolicy, PolicyGrantType.Grant));
            var roleAfterInsert = base.DoTestInsert(roleUnderTest, s_authorization);
            Assert.AreEqual(1, roleAfterInsert.Policies.Count);

        }

        /// <summary>
        /// Test the updating of a valid group
        /// </summary>
        [TestMethod]
        public void TestUpdateValidGroup()
        {

            var roleUnderTest = new SecurityRole()
            {
                Name = "Test Update"
            };
            var roleAfterTest = base.DoTestUpdate(roleUnderTest, "Description", s_authorization);
            Assert.IsNotNull(roleAfterTest.Description);

        }

        /// <summary>
        /// Test the updating of a valid group
        /// </summary>
        [TestMethod]
        public void TestUpdateValidGroupPermissions()
        {

            var roleUnderTest = new SecurityRole()
            {
                Name = "Non Chicken Costume Users"
            };
            roleUnderTest.Policies.Add(new SecurityPolicyInstance(s_chickenCostumePolicy, PolicyGrantType.Deny));
            var roleAfterInsert = base.DoTestInsert(roleUnderTest, s_authorization);

            // Now we want to update the grant so that users can elevate
            roleAfterInsert.Policies[0].GrantType = PolicyGrantType.Elevate;
            var dataPersistence = ApplicationServiceContext.Current.GetService<IDataPersistenceService<SecurityRole>>();
            dataPersistence.Update(roleAfterInsert, TransactionMode.Commit, s_authorization);
            var roleAfterTest = dataPersistence.Get(roleAfterInsert.Key.Value, null, false, s_authorization);
            Assert.AreEqual(PolicyGrantType.Elevate, roleAfterTest.Policies[0].GrantType);

        }

        /// <summary>
        /// Tests that the persistence layer removes a policy
        /// </summary>
        [TestMethod]
        public void TestRemoveRolePolicy()
        {
            var roleUnderTest = new SecurityRole()
            {
                Name = "Indifferent Chicken-Costume Users"
            };
            roleUnderTest.Policies.Add(new SecurityPolicyInstance(s_chickenCostumePolicy, PolicyGrantType.Grant));
            var roleAfterInsert = base.DoTestInsert(roleUnderTest, s_authorization);
            Assert.AreEqual(1, roleAfterInsert.Policies.Count);

            // Now we want to update the grant so that users can elevate
            roleAfterInsert.Policies.Clear();
            var dataPersistence = ApplicationServiceContext.Current.GetService<IDataPersistenceService<SecurityRole>>();
            dataPersistence.Update(roleAfterInsert, TransactionMode.Commit, s_authorization);
            var roleAfterTest = dataPersistence.Get(roleAfterInsert.Key.Value, null, false, s_authorization);
            Assert.AreEqual(0, roleAfterTest.Policies.Count);

        }

        /// <summary>
        /// Test querying of role
        /// </summary>
        public void TestQueryRole()
        {
            var roleUnderTest = new SecurityRole()
            {
                Name = "Query Test"
            };
            var roleAfterInsert = base.DoTestInsert(roleUnderTest, s_authorization);

            var roleQuery = base.DoTestQuery(r => r.Name == "Query Test", roleAfterInsert.Key, s_authorization);
            Assert.AreEqual("Query Test", roleQuery.First().Name);
        }

    }
}
