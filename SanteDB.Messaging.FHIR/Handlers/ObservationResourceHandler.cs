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
using RestSrvr;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Query;
using SanteDB.Messaging.FHIR.Backbone;
using SanteDB.Messaging.FHIR.DataTypes;
using SanteDB.Messaging.FHIR.Resources;
using SanteDB.Messaging.FHIR.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SanteDB.Messaging.FHIR.Handlers
{
    /// <summary>
    /// Observation handler
    /// </summary>
    public class ObservationResourceHandler : RepositoryResourceHandlerBase<SanteDB.Messaging.FHIR.Resources.Observation, Core.Model.Acts.Observation>
    {
        /// <summary>
        /// Map to FHIR
        /// </summary>
        protected override SanteDB.Messaging.FHIR.Resources.Observation MapToFhir(Core.Model.Acts.Observation model, RestOperationContext restOperationContext)
        {
            var retVal = DataTypeConverter.CreateResource<SanteDB.Messaging.FHIR.Resources.Observation>(model);

            retVal.EffectiveDateTime = (FhirDate)model.ActTime.DateTime;

            retVal.Code = DataTypeConverter.ToFhirCodeableConcept(model.LoadProperty<Concept>("TypeConcept"));
            if (model.StatusConceptKey == StatusKeys.Completed)
                retVal.Status = new FhirCode<ObservationStatus>(ObservationStatus.Final);
            else if (model.StatusConceptKey == StatusKeys.Active)
                retVal.Status = new FhirCode<ObservationStatus>(ObservationStatus.Preliminary);
            else if (model.StatusConceptKey == StatusKeys.Nullified)
                retVal.Status = new FhirCode<ObservationStatus>(ObservationStatus.EnteredInError);

            if (model.Relationships.Any(o => o.RelationshipTypeKey == ActRelationshipTypeKeys.Replaces))
                retVal.Status = new FhirCode<ObservationStatus>(ObservationStatus.Corrected);

            // RCT
            var rct = model.Participations.FirstOrDefault(o => o.ParticipationRoleKey == ActParticipationKey.RecordTarget);
            if (rct != null)
            {
                retVal.Subject = Reference.CreateResourceReference(new Patient() { Id = rct.PlayerEntityKey.ToString() }, restOperationContext.IncomingRequest.Url);
            }

            // Performer
            var prf = model.Participations.FirstOrDefault(o => o.ParticipationRoleKey == ActParticipationKey.Performer);
            if (prf != null)
                retVal.Performer = Reference.CreateResourceReference(new Practitioner() { Id = rct.PlayerEntityKey.ToString() }, restOperationContext.IncomingRequest.Url);

            retVal.Issued = new FhirInstant() { DateValue = model.CreationTime.DateTime };

            // Value
            switch(model.ValueType)
            {
                case "CD":
                    retVal.Value = DataTypeConverter.ToFhirCodeableConcept((model as Core.Model.Acts.CodedObservation).Value);
                    break;
                case "PQ":
                    retVal.Value = new FhirQuantity()
                    {
                        Value = (model as Core.Model.Acts.QuantityObservation).Value,
                        Units = DataTypeConverter.ToFhirCodeableConcept((model as Core.Model.Acts.QuantityObservation).LoadProperty<Concept>("UnitOfMeasure"), "http://hl7.org/fhir/sid/ucum").GetPrimaryCode()?.Code?.Value
                    };
                    break;
                case "ED":
                case "ST":
                    retVal.Value = new FhirString((model as Core.Model.Acts.TextObservation).Value);
                    break;
            }

            var loc = model.LoadCollection<ActParticipation>("Participations").FirstOrDefault(o => o.ParticipationRoleKey == ActParticipationKey.Location);
            if (loc != null)
                retVal.Extension.Add(new Extension()
                {
                    Url = "http://santedb.org/extensions/act/fhir/location",
                    Value = new FhirString(loc.PlayerEntityKey.ToString())
                });

            if(model.InterpretationConceptKey.HasValue)
                retVal.Interpretation = DataTypeConverter.ToFhirCodeableConcept(model.LoadProperty<Concept>("InterpretationConcept"));


            return retVal;
        }

        /// <summary>
        /// Map to model
        /// </summary>
        protected override Core.Model.Acts.Observation MapToModel(SanteDB.Messaging.FHIR.Resources.Observation resource, RestOperationContext restOperationContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Query
        /// </summary>
        protected override IEnumerable<Core.Model.Acts.Observation> Query(Expression<Func<Core.Model.Acts.Observation, bool>> query, Guid queryId, int offset, int count, out int totalResults)
        {
            //var anyRef = Expression.OrElse(base.CreateConceptSetFilter(ConceptSetKeys.VitalSigns, query.Parameters[0]), base.CreateConceptSetFilter(ConceptSetKeys.ProblemObservations, query.Parameters[0]));
            //query = Expression.Lambda<Func<Core.Model.Acts.Observation, bool>>(Expression.AndAlso(
            //             query.Body, 
            //             anyRef
            //         ), query.Parameters);

            return base.Query(query, queryId, offset, count, out totalResults);
        }


        /// <summary>
        /// Parameters
        /// </summary>
        public override FhirQueryResult Query(System.Collections.Specialized.NameValueCollection parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            Core.Model.Query.NameValueCollection hdsiQuery = null;
            FhirQuery query = QueryRewriter.RewriteFhirQuery<SanteDB.Messaging.FHIR.Resources.Observation, Core.Model.Acts.Observation>(parameters, out hdsiQuery);

            // Do the query
            int totalResults = 0;

            IEnumerable<Core.Model.Acts.Observation> hdsiResults = null;

            if (parameters["value-concept"] != null)
            {
                var predicate = QueryExpressionParser.BuildLinqExpression<Core.Model.Acts.CodedObservation>(hdsiQuery);
                hdsiResults = this.QueryEx<Core.Model.Acts.CodedObservation>(predicate, query.QueryId, query.Start, query.Quantity, out totalResults).OfType<Core.Model.Acts.Observation>();
            }
            else if (parameters["value-quantity"] != null)
            {
                var predicate = QueryExpressionParser.BuildLinqExpression<Core.Model.Acts.QuantityObservation>(hdsiQuery);
                hdsiResults = this.QueryEx<Core.Model.Acts.QuantityObservation>(predicate, query.QueryId, query.Start, query.Quantity, out totalResults).OfType<Core.Model.Acts.Observation>();
            }
            else
            {
                var predicate = QueryExpressionParser.BuildLinqExpression<Core.Model.Acts.Observation>(hdsiQuery);
                hdsiResults = this.Query(predicate, query.QueryId, query.Start, query.Quantity, out totalResults);
            }


            var restOperationContext = RestOperationContext.Current;

            // Return FHIR query result
            return new FhirQueryResult()
            {
                Results = hdsiResults.AsParallel().Select(o => this.MapToFhir(o, restOperationContext)).OfType<ResourceBase>().ToList(),
                Query = query,
                TotalResults = totalResults
            };
        }

        /// <summary>
        /// Get interactions
        /// </summary>
        protected override IEnumerable<InteractionDefinition> GetInteractions()
        {
            return new TypeRestfulInteraction[]
            {
                TypeRestfulInteraction.InstanceHistory,
                TypeRestfulInteraction.Read,
                TypeRestfulInteraction.Search,
                TypeRestfulInteraction.VersionRead,
                TypeRestfulInteraction.Delete
            }.Select(o => new InteractionDefinition() { Type = o });
        }

    }
}