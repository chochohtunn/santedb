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
using SanteDB.Core.Model.Entities;
using SanteDB.Messaging.FHIR.Backbone;
using SanteDB.Messaging.FHIR.Resources;
using SanteDB.Messaging.FHIR.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SanteDB.Messaging.FHIR.Handlers
{

    /// <summary>
    /// Adverse event resource handler
    /// </summary>
    public class AdverseEventResourceHandler : RepositoryResourceHandlerBase<AdverseEvent, Act>
    {
        /// <summary>
        /// Maps the specified act to an adverse event
        /// </summary>
        protected override AdverseEvent MapToFhir(Act model, RestOperationContext restOperationContext)
        {
            var retVal = DataTypeConverter.CreateResource<AdverseEvent>(model);

            retVal.Identifier = DataTypeConverter.ToFhirIdentifier<Act>(model.Identifiers.FirstOrDefault());
            retVal.Category = AdverseEventCategory.AdverseEvent;
            retVal.Type = DataTypeConverter.ToFhirCodeableConcept(model.LoadProperty<Concept>("TypeConcept"));

            var recordTarget = model.LoadCollection<ActParticipation>("Participations").FirstOrDefault(o => o.ParticipationRoleKey == ActParticipationKey.RecordTarget);
            if (recordTarget != null)
                retVal.Subject = DataTypeConverter.CreateReference<Patient>(recordTarget.LoadProperty<Entity>("PlayerEntity"), restOperationContext);

            // Main topic of the concern
            var subject = model.LoadCollection<ActRelationship>("Relationships").FirstOrDefault(o => o.RelationshipTypeKey == ActRelationshipTypeKeys.HasSubject)?.LoadProperty<Act>("TargetAct");
            if (subject == null) throw new InvalidOperationException("This act does not appear to be an adverse event");
            retVal.Date = subject.ActTime.DateTime;

            // Reactions = HasManifestation
            var reactions = subject.LoadCollection<ActRelationship>("Relationships").Where(o => o.RelationshipTypeKey == ActRelationshipTypeKeys.HasManifestation);
            retVal.Reaction = reactions.Select(o => DataTypeConverter.CreateReference<Condition>(o.LoadProperty<Act>("TargetAct"), restOperationContext)).ToList();

            var location = model.LoadCollection<ActParticipation>("Participations").FirstOrDefault(o => o.ParticipationRoleKey == ActParticipationKey.Location);
            if (location != null)
                retVal.Location = DataTypeConverter.CreateReference<Location>(location.LoadProperty<Entity>("PlayerEntity"), restOperationContext);

            // Severity
            var severity = subject.LoadCollection<ActRelationship>("Relationships").First(o => o.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent && o.LoadProperty<Act>("TargetAct").TypeConceptKey == ObservationTypeKeys.Severity);
            if (severity != null)
                retVal.Seriousness = DataTypeConverter.ToFhirCodeableConcept(severity.LoadProperty<CodedObservation>("TargetAct").Value, "http://hl7.org/fhir/adverse-event-seriousness");

            // Did the patient die?
            var causeOfDeath = model.LoadCollection<ActRelationship>("Relationships").FirstOrDefault(o => o.RelationshipTypeKey == ActRelationshipTypeKeys.IsCauseOf && o.LoadProperty<Act>("TargetAct").TypeConceptKey == ObservationTypeKeys.ClinicalState && (o.TargetAct as CodedObservation)?.ValueKey == Guid.Parse("6df3720b-857f-4ba2-826f-b7f1d3c3adbb"));
            if (causeOfDeath != null)
                retVal.Outcome = new SanteDB.Messaging.FHIR.DataTypes.FhirCodeableConcept(new Uri("http://hl7.org/fhir/adverse-event-outcome"), "fatal");
            else if (model.StatusConceptKey == StatusKeys.Active)
                retVal.Outcome = new SanteDB.Messaging.FHIR.DataTypes.FhirCodeableConcept(new Uri("http://hl7.org/fhir/adverse-event-outcome"), "ongoing");
            else if (model.StatusConceptKey == StatusKeys.Completed)
                retVal.Outcome = new SanteDB.Messaging.FHIR.DataTypes.FhirCodeableConcept(new Uri("http://hl7.org/fhir/adverse-event-outcome"), "resolved");

            var author = model.LoadCollection<ActParticipation>("Participations").FirstOrDefault(o => o.ParticipationRoleKey == ActParticipationKey.Authororiginator);
            if (author != null)
                retVal.Recorder = DataTypeConverter.CreatePlainReference<Practitioner>(author.LoadProperty<Entity>("PlayerEntity"), restOperationContext);

            // Suspect entities
            var refersTo = model.LoadCollection<ActRelationship>("Relationships").Where(o => o.RelationshipTypeKey == ActRelationshipTypeKeys.RefersTo);
            if (refersTo.Count() > 0)
                retVal.SuspectEntity = refersTo.Select(o => o.LoadProperty<Act>("TargetAct")).OfType<SubstanceAdministration>().Select(o =>
                {
                    var consumable = o.LoadCollection<ActParticipation>("Participations").FirstOrDefault(x => x.ParticipationRoleKey == ActParticipationKey.Consumable)?.LoadProperty<ManufacturedMaterial>("PlayerEntity");
                    if (consumable == null)
                    {
                        var product = o.LoadCollection<ActParticipation>("Participations").FirstOrDefault(x => x.ParticipationRoleKey == ActParticipationKey.Product)?.LoadProperty<Material>("PlayerEntity");
                        return new AdverseEventSuspectEntity() { Instance = DataTypeConverter.CreatePlainReference<Substance>(product, restOperationContext) };
                    }
                    else
                        return new AdverseEventSuspectEntity() { Instance = DataTypeConverter.CreatePlainReference<Medication>(consumable, restOperationContext) };
                }).ToList();

            return retVal;
        }

        /// <summary>
        /// Map adverse events to the model 
        /// </summary>
        protected override Act MapToModel(AdverseEvent resource, RestOperationContext restOperationContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Query for specified adverse event 
        /// </summary>
        protected override IEnumerable<Act> Query(Expression<Func<Act, bool>> query, Guid queryId, int offset, int count, out int totalResults)
        {
            var typeReference = Expression.MakeBinary(ExpressionType.Equal, Expression.Convert(Expression.MakeMemberAccess(query.Parameters[0], typeof(Act).GetProperty(nameof(Act.ClassConceptKey))), typeof(Guid)), Expression.Constant(ActClassKeys.Condition));

            var anyRef = base.CreateConceptSetFilter(ConceptSetKeys.AdverseEventActs, query.Parameters[0]);
            query = Expression.Lambda<Func<Act, bool>>(Expression.AndAlso(Expression.AndAlso(query.Body, anyRef), typeReference), query.Parameters);

            return base.Query(query, queryId, offset, count, out totalResults);
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