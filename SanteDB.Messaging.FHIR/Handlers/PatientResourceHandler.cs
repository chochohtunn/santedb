﻿/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 *
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
 * User: JustinFyfe
 * Date: 2019-1-22
 */
using RestSrvr;
using SanteDB.Core;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Services;
using SanteDB.Messaging.FHIR.Backbone;
using SanteDB.Messaging.FHIR.DataTypes;
using SanteDB.Messaging.FHIR.Resources;
using SanteDB.Messaging.FHIR.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using DatePrecision = SanteDB.Core.Model.DataTypes.DatePrecision;

namespace SanteDB.Messaging.FHIR.Handlers
{
    /// <summary>
    /// Represents a resource handler which can handle patients.
    /// </summary>
    public class PatientResourceHandler : RepositoryResourceHandlerBase<Patient, Core.Model.Roles.Patient>
	{
		/// <summary>
		/// The repository.
		/// </summary>
		private IPatientRepositoryService repository;

		/// <summary>
		/// Resource handler subscription
		/// </summary>
		public PatientResourceHandler()
		{
			ApplicationServiceContext.Current.Started += (o, e) => this.repository = ApplicationServiceContext.Current.GetService<IPatientRepositoryService>();
		}

		/// <summary>
		/// Map a patient object to FHIR.
		/// </summary>
		/// <param name="model">The model.</param>
		/// <returns>Returns the mapped FHIR resource.</returns>
		protected override Patient MapToFhir(Core.Model.Roles.Patient model, RestOperationContext RestOperationContext)
		{
			var retVal = DataTypeConverter.CreateResource<Patient>(model);
			retVal.Active = model.StatusConceptKey == StatusKeys.Active;
			retVal.Address = model.LoadCollection<EntityAddress>("Addresses").Select(o => DataTypeConverter.ToFhirAddress(o)).ToList();
			retVal.BirthDate = model.DateOfBirth;
			retVal.Deceased = model.DeceasedDate == DateTime.MinValue ? (object)new FhirBoolean(true) : model.DeceasedDate != null ? new FhirDate(model.DeceasedDate.Value) : null;
			retVal.Gender = DataTypeConverter.ToFhirCodeableConcept(model.LoadProperty<Concept>("GenderConcept"))?.GetPrimaryCode()?.Code;
			retVal.Identifier = model.Identifiers?.Select(o => DataTypeConverter.ToFhirIdentifier(o)).ToList();
			retVal.MultipleBirth = model.MultipleBirthOrder == 0 ? (FhirElement)new FhirBoolean(true) : model.MultipleBirthOrder.HasValue ? new FhirInt(model.MultipleBirthOrder.Value) : null;
			retVal.Name = model.LoadCollection<EntityName>("Names").Select(o => DataTypeConverter.ToFhirHumanName(o)).ToList();
			retVal.Timestamp = model.ModifiedOn.DateTime;
			retVal.Telecom = model.LoadCollection<EntityTelecomAddress>("Telecoms").Select(o => DataTypeConverter.ToFhirTelecom(o)).ToList();

			// TODO: Relationships
			foreach (var rel in model.LoadCollection<EntityRelationship>("Relationships").Where(o => !o.InversionIndicator))
			{
				// Family member
				if (rel.LoadProperty<Concept>(nameof(EntityRelationship.RelationshipType)).ConceptSetsXml.Contains(ConceptSetKeys.FamilyMember))
				{
					// Create the relative object
					var relative = DataTypeConverter.CreateResource<RelatedPerson>(rel.LoadProperty<Person>(nameof(EntityRelationship.TargetEntity)));
					relative.Relationship = DataTypeConverter.ToFhirCodeableConcept(rel.LoadProperty<Concept>(nameof(EntityRelationship.RelationshipType)));
					relative.Address = DataTypeConverter.ToFhirAddress(rel.TargetEntity.Addresses.FirstOrDefault());
					relative.Gender = DataTypeConverter.ToFhirCodeableConcept((rel.TargetEntity as Core.Model.Roles.Patient)?.LoadProperty<Concept>(nameof(Core.Model.Roles.Patient.GenderConcept)));
					relative.Identifier = rel.TargetEntity.LoadCollection<EntityIdentifier>(nameof(Entity.Identifiers)).Select(o => DataTypeConverter.ToFhirIdentifier(o)).ToList();
					relative.Name = DataTypeConverter.ToFhirHumanName(rel.TargetEntity.LoadCollection<EntityName>(nameof(Entity.Names)).FirstOrDefault());
					if (rel.TargetEntity is Core.Model.Roles.Patient)
						relative.Patient = DataTypeConverter.CreateReference<Patient>(rel.TargetEntity, RestOperationContext);
					relative.Telecom = rel.TargetEntity.LoadCollection<EntityTelecomAddress>(nameof(Entity.Telecoms)).Select(o => DataTypeConverter.ToFhirTelecom(o)).ToList();
					retVal.Contained.Add(new ContainedResource()
					{
						Item = relative
					});
				}
				else if (rel.RelationshipTypeKey == EntityRelationshipTypeKeys.HealthcareProvider)
					retVal.Provider = DataTypeConverter.CreateReference<Practitioner>(rel.LoadProperty<Entity>(nameof(EntityRelationship.TargetEntity)), RestOperationContext);
			}

			var photo = model.LoadCollection<EntityExtension>("Extensions").FirstOrDefault(o => o.ExtensionTypeKey == ExtensionTypeKeys.JpegPhotoExtension);
			if (photo != null)
				retVal.Photo = new List<Attachment>() {
					new Attachment()
					{
						ContentType = "image/jpg",
						Data = photo.ExtensionValueXml
					}
				};

			// TODO: Links
			return retVal;
		}

		/// <summary>
		/// Maps a FHIR patient resource to an HDSI patient.
		/// </summary>
		/// <param name="resource">The resource.</param>
		/// <returns>Returns the mapped model.</returns>
		protected override Core.Model.Roles.Patient MapToModel(Patient resource, RestOperationContext RestOperationContext)
		{
			var patient = new Core.Model.Roles.Patient
			{
				Addresses = resource.Address.Select(DataTypeConverter.ToEntityAddress).ToList(),
				CreationTime = DateTimeOffset.Now,
				DateOfBirth = resource.BirthDate?.DateValue,
				// TODO: Extensions
				Extensions = resource.Extension.Select(DataTypeConverter.ToEntityExtension).ToList(),
				GenderConceptKey = DataTypeConverter.ToConcept(new FhirCoding(new Uri("http://hl7.org/fhir/administrative-gender"), resource.Gender?.Value))?.Key,
				Identifiers = resource.Identifier.Select(DataTypeConverter.ToEntityIdentifier).ToList(),
				LanguageCommunication = resource.Communication.Select(DataTypeConverter.ToPersonLanguageCommunication).ToList(),
				Key = Guid.NewGuid(),
				Names = resource.Name.Select(DataTypeConverter.ToEntityName).ToList(),
				Relationships = resource.Contact.Select(DataTypeConverter.ToEntityRelationship).ToList(),
				StatusConceptKey = resource.Active?.Value == true ? StatusKeys.Active : StatusKeys.Obsolete,
				Telecoms = resource.Telecom.Select(DataTypeConverter.ToEntityTelecomAddress).ToList()
			};

			Guid key;

			if (!Guid.TryParse(resource.Id, out key))
			{
				key = Guid.NewGuid();
			}

			patient.Key = key;

			if (resource.Deceased is FhirDateTime)
			{
				patient.DeceasedDate = (FhirDateTime)resource.Deceased;
			}
			else if (resource.Deceased is FhirBoolean)
			{
				// we don't have a field for "deceased indicator" to say that the patient is dead, but we don't know that actual date/time of death
				// should find a better way to do this
				patient.DeceasedDate = DateTime.Now;
				patient.DeceasedDatePrecision = DatePrecision.Year;
			}

			if (resource.MultipleBirth is FhirBoolean)
			{
				patient.MultipleBirthOrder = 0;
			}
			else if (resource.MultipleBirth is FhirInt)
			{
				patient.MultipleBirthOrder = ((FhirInt)resource.MultipleBirth).Value;
			}

			return patient;
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
                TypeRestfulInteraction.Delete,
                TypeRestfulInteraction.Create,
                TypeRestfulInteraction.Update
            }.Select(o => new InteractionDefinition() { Type = o });
        }
    }
}