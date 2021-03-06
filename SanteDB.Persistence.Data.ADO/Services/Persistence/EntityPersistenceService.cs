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
using SanteDB.Core;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Extensions;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using SanteDB.OrmLite;
using SanteDB.Persistence.Data.ADO.Data;
using SanteDB.Persistence.Data.ADO.Data.Model;
using SanteDB.Persistence.Data.ADO.Data.Model.DataType;
using SanteDB.Persistence.Data.ADO.Data.Model.Entities;
using SanteDB.Persistence.Data.ADO.Data.Model.Extensibility;
using SanteDB.Persistence.Data.ADO.Data.Model.Roles;
using SanteDB.Persistence.Data.ADO.Data.Model.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;

namespace SanteDB.Persistence.Data.ADO.Services.Persistence
{
    /// <summary>
    /// Entity persistence service
    /// </summary>
    public class EntityPersistenceService : VersionedDataPersistenceService<Core.Model.Entities.Entity, DbEntityVersion, DbEntity>
    {

        /// <summary>
        /// To model instance
        /// </summary>
        public virtual TEntityType ToModelInstance<TEntityType>(DbEntityVersion dbVersionInstance, DbEntity entInstance, DataContext context) where TEntityType : Core.Model.Entities.Entity, new()
        {
            var retVal = m_mapper.MapDomainInstance<DbEntityVersion, TEntityType>(dbVersionInstance);

            if (retVal == null) return null;

            retVal.ClassConceptKey = entInstance.ClassConceptKey;
            retVal.DeterminerConceptKey = entInstance.DeterminerConceptKey;
            retVal.TemplateKey = entInstance.TemplateKey;

            // Attempt to load policies
            var policySql = context.CreateSqlStatement<DbEntitySecurityPolicy>()
                .SelectFrom(typeof(DbEntitySecurityPolicy), typeof(DbSecurityPolicy))
                .InnerJoin<DbSecurityPolicy>(o => o.PolicyKey, o => o.Key)
                .Where(o => o.SourceKey == retVal.Key && o.EffectiveVersionSequenceId <= retVal.VersionSequence && (!o.ObsoleteVersionSequenceId.HasValue || o.ObsoleteVersionSequenceId > retVal.VersionSequence));
            retVal.Policies = context.Query<CompositeResult<DbEntitySecurityPolicy, DbSecurityPolicy>>(policySql).Select(o => new SecurityPolicyInstance(new SecurityPolicy()
            {
                CanOverride = o.Object2.CanOverride,
                CreatedByKey = o.Object2.CreatedByKey,
                CreationTime = o.Object2.CreationTime,
                Handler = o.Object2.Handler,
                IsPublic = o.Object2.IsPublic,
                Name = o.Object2.Name,
                LoadState = LoadState.FullLoad,
                Key = o.Object2.Key,
                Oid = o.Object2.Oid
            }, PolicyGrantType.Grant)).ToList();

            // Inversion relationships
            //if (retVal.Relationships != null)
            //{
            //    retVal.Relationships.RemoveAll(o => o.InversionIndicator);
            //    retVal.Relationships.AddRange(context.EntityAssociations.Where(o => o.TargetEntityId == retVal.Key.Value).Distinct().Select(o => new EntityRelationship(o.AssociationTypeConceptId, o.TargetEntityId)
            //    {
            //        SourceEntityKey = o.SourceEntityId,
            //        Key = o.EntityAssociationId,
            //        InversionIndicator = true
            //    }));
            //}
            return retVal;
        }

        /// <summary>
        /// Convert to model instance
        /// </summary>
        public override Core.Model.Entities.Entity ToModelInstance(object dataInstance, DataContext context)
        {

#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif 
            if (dataInstance == null)
                return null;
            // Alright first, which type am I mapping to?
            var dbEntityVersion = (dataInstance as CompositeResult)?.Values.OfType<DbEntityVersion>().FirstOrDefault() ?? dataInstance as DbEntityVersion ?? context.FirstOrDefault<DbEntityVersion>(o => o.VersionKey == (dataInstance as DbEntitySubTable).ParentKey);
            var dbEntity = (dataInstance as CompositeResult)?.Values.OfType<DbEntity>().FirstOrDefault() ?? context.FirstOrDefault<DbEntity>(o => o.Key == dbEntityVersion.Key);
            Entity retVal = null;

            switch (dbEntity.ClassConceptKey.ToString().ToLower())
            {
                case EntityClassKeyStrings.Device:
                    retVal = new DeviceEntityPersistenceService().ToModelInstance(
                        (dataInstance as CompositeResult)?.Values.OfType<DbDeviceEntity>().FirstOrDefault() ?? context.FirstOrDefault<DbDeviceEntity>(o => o.ParentKey == dbEntityVersion.VersionKey),
                        dbEntityVersion,
                        dbEntity,
                        context);
                    break;
                case EntityClassKeyStrings.NonLivingSubject:
                    retVal = new ApplicationEntityPersistenceService().ToModelInstance(
                        (dataInstance as CompositeResult)?.Values.OfType<DbApplicationEntity>().FirstOrDefault() ?? context.FirstOrDefault<DbApplicationEntity>(o => o.ParentKey == dbEntityVersion.VersionKey),
                        dbEntityVersion,
                        dbEntity,
                        context);
                    break;
                case EntityClassKeyStrings.Person:
                    var ue = (dataInstance as CompositeResult)?.Values.OfType<DbUserEntity>().FirstOrDefault() ?? context.FirstOrDefault<DbUserEntity>(o => o.ParentKey == dbEntityVersion.VersionKey);

                    if (ue != null)
                        retVal = new UserEntityPersistenceService().ToModelInstance(
                            ue,
                            (dataInstance as CompositeResult)?.Values.OfType<DbPerson>().FirstOrDefault() ?? context.FirstOrDefault<DbPerson>(o => o.ParentKey == dbEntityVersion.VersionKey),
                            dbEntityVersion,
                            dbEntity,
                            context);
                    else
                        retVal = new PersonPersistenceService().ToModelInstance(
                            (dataInstance as CompositeResult)?.Values.OfType<DbPerson>().FirstOrDefault() ?? context.FirstOrDefault<DbPerson>(o => o.ParentKey == dbEntityVersion.VersionKey),
                            dbEntityVersion,
                            dbEntity,
                            context);
                    break;
                case EntityClassKeyStrings.Patient:
                    retVal = new PatientPersistenceService().ToModelInstance(
                        (dataInstance as CompositeResult)?.Values.OfType<DbPatient>().FirstOrDefault() ?? context.FirstOrDefault<DbPatient>(o => o.ParentKey == dbEntityVersion.VersionKey),
                        (dataInstance as CompositeResult)?.Values.OfType<DbPerson>().FirstOrDefault() ?? context.FirstOrDefault<DbPerson>(o => o.ParentKey == dbEntityVersion.VersionKey),
                        dbEntityVersion,
                        dbEntity,
                        context);
                    break;
                case EntityClassKeyStrings.Provider:
                    retVal = new ProviderPersistenceService().ToModelInstance(
                        (dataInstance as CompositeResult)?.Values.OfType<DbProvider>().FirstOrDefault() ?? context.FirstOrDefault<DbProvider>(o => o.ParentKey == dbEntityVersion.VersionKey),
                        (dataInstance as CompositeResult)?.Values.OfType<DbPerson>().FirstOrDefault() ?? context.FirstOrDefault<DbPerson>(o => o.ParentKey == dbEntityVersion.VersionKey),
                        dbEntityVersion,
                        dbEntity,
                        context);
                    break;
                case EntityClassKeyStrings.Place:
                case EntityClassKeyStrings.CityOrTown:
                case EntityClassKeyStrings.Country:
                case EntityClassKeyStrings.CountyOrParish:
                case EntityClassKeyStrings.State:
                case EntityClassKeyStrings.ServiceDeliveryLocation:
                    retVal = new PlacePersistenceService().ToModelInstance(
                        (dataInstance as CompositeResult)?.Values.OfType<DbPlace>().FirstOrDefault() ?? context.FirstOrDefault<DbPlace>(o => o.ParentKey == dbEntityVersion.VersionKey),
                        dbEntityVersion,
                        dbEntity,
                        context);
                    break;
                case EntityClassKeyStrings.Organization:
                    retVal = new OrganizationPersistenceService().ToModelInstance(
                        (dataInstance as CompositeResult)?.Values.OfType<DbOrganization>().FirstOrDefault() ?? context.FirstOrDefault<DbOrganization>(o => o.ParentKey == dbEntityVersion.VersionKey),
                        dbEntityVersion,
                        dbEntity,
                        context);
                    break;
                case EntityClassKeyStrings.Material:
                    retVal = new MaterialPersistenceService().ToModelInstance<Material>(
                        (dataInstance as CompositeResult)?.Values.OfType<DbMaterial>().FirstOrDefault() ?? context.FirstOrDefault<DbMaterial>(o => o.ParentKey == dbEntityVersion.VersionKey),
                        dbEntityVersion,
                        dbEntity,
                        context);
                    break;
                case EntityClassKeyStrings.ManufacturedMaterial:
                    retVal = new ManufacturedMaterialPersistenceService().ToModelInstance(
                        (dataInstance as CompositeResult)?.Values.OfType<DbManufacturedMaterial>().FirstOrDefault() ?? context.FirstOrDefault<DbManufacturedMaterial>(o => o.ParentKey == dbEntityVersion.VersionKey),
                        (dataInstance as CompositeResult)?.Values.OfType<DbMaterial>().FirstOrDefault() ?? context.FirstOrDefault<DbMaterial>(o => o.ParentKey == dbEntityVersion.VersionKey),
                        dbEntityVersion,
                        dbEntity,
                        context);
                    break;
                default:
                    retVal = this.ToModelInstance<Entity>(dbEntityVersion, dbEntity, context);
                    break;

            }

#if DEBUG
            sw.Stop();
            this.m_tracer.TraceEvent(EventLevel.Verbose, "Basic conversion took: {0}", sw.ElapsedMilliseconds);
#endif 
            retVal.LoadAssociations(context);
            return retVal;
        }

        /// <summary>
        /// Conversion based on type
        /// </summary>
        protected override Entity CacheConvert(object dataInstance, DataContext context)
        {
            return this.DoCacheConvert(dataInstance, context);
        }

        /// <summary>
        /// Perform the cache convert
        /// </summary>
        internal Entity DoCacheConvert(object dataInstance, DataContext context)
        {
            if (dataInstance == null)
                return null;
            // Alright first, which type am I mapping to?
            var dbEntityVersion = (dataInstance as CompositeResult)?.Values.OfType<DbEntityVersion>().FirstOrDefault() ?? dataInstance as DbEntityVersion ?? context.FirstOrDefault<DbEntityVersion>(o => o.VersionKey == (dataInstance as DbEntitySubTable).ParentKey);
            var dbEntity = (dataInstance as CompositeResult)?.Values.OfType<DbEntity>().FirstOrDefault() ?? context.FirstOrDefault<DbEntity>(o => o.Key == dbEntityVersion.Key);
            Entity retVal = null;
            var cache = new AdoPersistenceCache(context);

            if (!dbEntityVersion.ObsoletionTime.HasValue)
                switch (dbEntity.ClassConceptKey.ToString().ToUpper())
                {
                    case EntityClassKeyStrings.Device:
                        retVal = cache?.GetCacheItem<DeviceEntity>(dbEntity.Key);
                        break;
                    case EntityClassKeyStrings.NonLivingSubject:
                        retVal = cache?.GetCacheItem<ApplicationEntity>(dbEntity.Key);
                        break;
                    case EntityClassKeyStrings.Person:
                        var ue = (dataInstance as CompositeResult)?.Values.OfType<DbUserEntity>().FirstOrDefault() ?? context.FirstOrDefault<DbUserEntity>(o => o.ParentKey == dbEntityVersion.VersionKey);
                        if (ue != null)
                            retVal = cache?.GetCacheItem<UserEntity>(dbEntity.Key);

                        else
                            retVal = cache?.GetCacheItem<Person>(dbEntity.Key);
                        break;
                    case EntityClassKeyStrings.Patient:
                        retVal = cache?.GetCacheItem<Patient>(dbEntity.Key);
                        break;
                    case EntityClassKeyStrings.Provider:
                        retVal = cache?.GetCacheItem<Provider>(dbEntity.Key);

                        break;
                    case EntityClassKeyStrings.Place:
                    case EntityClassKeyStrings.CityOrTown:
                    case EntityClassKeyStrings.Country:
                    case EntityClassKeyStrings.CountyOrParish:
                    case EntityClassKeyStrings.State:
                    case EntityClassKeyStrings.ServiceDeliveryLocation:
                        retVal = cache?.GetCacheItem<Place>(dbEntity.Key);

                        break;
                    case EntityClassKeyStrings.Organization:
                        retVal = cache?.GetCacheItem<Organization>(dbEntity.Key);

                        break;
                    case EntityClassKeyStrings.Material:
                        retVal = cache?.GetCacheItem<Material>(dbEntity.Key);

                        break;
                    case EntityClassKeyStrings.ManufacturedMaterial:
                        retVal = cache?.GetCacheItem<ManufacturedMaterial>(dbEntity.Key);

                        break;
                    default:
                        retVal = cache?.GetCacheItem<Entity>(dbEntity.Key);
                        break;
                }

            // Return cache value
            if (retVal != null)
                return retVal;
            else
                return base.CacheConvert(dataInstance, context);
        }

        /// <summary>
        /// Insert the specified entity into the data context
        /// </summary>
        public Core.Model.Entities.Entity InsertCoreProperties(DataContext context, Core.Model.Entities.Entity data)
        {

            // The creator asserted from the persistence call
            Guid? assertedCreator = data.CreatedByKey;

            // Ensure FK exists
            if (data.ClassConcept != null) data.ClassConcept = data.ClassConcept?.EnsureExists(context) as Concept;
            if (data.DeterminerConcept != null) data.DeterminerConcept = data.DeterminerConcept?.EnsureExists(context) as Concept;
            if (data.StatusConcept != null) data.StatusConcept = data.StatusConcept?.EnsureExists(context) as Concept;
            if (data.TypeConcept != null) data.TypeConcept = data.TypeConcept?.EnsureExists(context) as Concept;
            if (data.Template != null) data.Template = data.Template?.EnsureExists(context) as TemplateDefinition;
            if (data.CreationAct != null) data.CreationAct = data.CreationAct?.EnsureExists(context) as Act;
            data.TypeConceptKey = data.TypeConcept?.Key ?? data.TypeConceptKey;
            data.DeterminerConceptKey = data.DeterminerConcept?.Key ?? data.DeterminerConceptKey;
            data.ClassConceptKey = data.ClassConcept?.Key ?? data.ClassConceptKey;
            data.StatusConceptKey = data.StatusConcept?.Key ?? data.StatusConceptKey;
            data.StatusConceptKey = data.StatusConceptKey == Guid.Empty || data.StatusConceptKey == null ? StatusKeys.New : data.StatusConceptKey;
            data.CreationActKey = data.CreationAct?.Key ?? data.CreationActKey;

            var retVal = base.InsertInternal(context, data);

            // Identifiers
            if (data.Identifiers != null)
            {
                this.VerifyIdentities(context, data, assertedCreator);
                // Assert 
                base.UpdateVersionedAssociatedItems<Core.Model.DataTypes.EntityIdentifier, DbEntityIdentifier>(
                    data.Identifiers.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);
            }

            // Relationships
            if (data.Relationships != null)
                base.UpdateVersionedAssociatedItems<Core.Model.Entities.EntityRelationship, DbEntityRelationship>(
                   data.Relationships.Distinct(new EntityRelationshipPersistenceService.Comparer()).Where(o => o != null && !o.InversionIndicator && !o.IsEmpty()).ToList(),
                    retVal,
                    context);

            // Telecoms
            if (data.Telecoms != null)
                base.UpdateVersionedAssociatedItems<Core.Model.Entities.EntityTelecomAddress, DbTelecomAddress>(
                   data.Telecoms.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            // Extensions
            if (data.Extensions != null)
                base.UpdateVersionedAssociatedItems<Core.Model.DataTypes.EntityExtension, DbEntityExtension>(
                   data.Extensions.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            // Names
            if (data.Names != null)
                base.UpdateVersionedAssociatedItems<Core.Model.Entities.EntityName, DbEntityName>(
                   data.Names.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            // Addresses
            if (data.Addresses != null)
                base.UpdateVersionedAssociatedItems<Core.Model.Entities.EntityAddress, DbEntityAddress>(
                   data.Addresses.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            // Notes
            if (data.Notes != null)
                base.UpdateVersionedAssociatedItems<Core.Model.DataTypes.EntityNote, DbEntityNote>(
                   data.Notes.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            // Tags
            if (data.Tags != null)
                base.UpdateAssociatedItems<Core.Model.DataTypes.EntityTag, DbEntityTag>(
                   data.Tags.Where(o => o != null && !o.IsEmpty() && !o.TagKey.StartsWith("$")),
                    retVal,
                    context);

            // Persist policies
            if (data.Policies != null && data.Policies.Any())
            {
                foreach (var p in data.Policies)
                {
                    var pol = p.Policy?.EnsureExists(context);
                    if (pol == null) // maybe we can retrieve it from the PIP?
                    {
                        var pipInfo = ApplicationServiceContext.Current.GetService<IPolicyInformationService>().GetPolicy(p.PolicyKey.ToString());
                        if (pipInfo != null)
                        {
                            p.Policy = new Core.Model.Security.SecurityPolicy()
                            {
                                Oid = pipInfo.Oid,
                                Name = pipInfo.Name,
                                CanOverride = pipInfo.CanOverride
                            };
                            pol = p.Policy.EnsureExists(context);
                        }
                        else throw new InvalidOperationException("Cannot find policy information");
                    }

                    // Insert
                    context.Insert(new DbEntitySecurityPolicy()
                    {
                        Key = Guid.NewGuid(),
                        PolicyKey = pol.Key.Value,
                        SourceKey = retVal.Key.Value,
                        EffectiveVersionSequenceId = retVal.VersionSequence.Value
                    });
                }
            }

            return retVal;
        }

        /// <summary>
        /// Verify the identities are appropriately designed
        /// </summary>
        private void VerifyIdentities(DataContext context, Entity data, Guid? assertedCreator)
        {
            // Validate unique values for IDs
            DbSecurityProvenance provenance = null;
            var issues = new List<DetectedIssue>();
            var priority = this.m_persistenceService.GetConfiguration().Validation.ValidationLevel;

            foreach (var id in data.Identifiers)
            {
                // Get ID
                var dbAuth = context.FirstOrDefault<DbAssigningAuthority>(o => o.Key == id.AuthorityKey);
                if (dbAuth == null)
                {
                    dbAuth = context.FirstOrDefault<DbAssigningAuthority>(o => o.DomainName == id.Authority.DomainName);
                    if(dbAuth != null)
                        id.AuthorityKey = dbAuth.Key;
                }
                if (dbAuth == null)
                {
                    issues.Add(new DetectedIssue(priority, "id.aa.notFound", $"Missing assigning authority with ID {String.Join(",", data.Identifiers.Select(o => o.AuthorityKey))}", DetectedIssueKeys.SafetyConcernIssue));
                    continue;
                }

                // Get this identifier records which is not owned by my record
                var ownedByOthers = context.Query<DbEntityIdentifier>(
                    context.CreateSqlStatement()
                    .SelectFrom(typeof(DbEntityIdentifier))
                    .Where<DbEntityIdentifier>(o => o.Value == id.Value && o.AuthorityKey == id.AuthorityKey && o.ObsoleteVersionSequenceId == null && o.SourceKey != data.Key)
                    .And("NOT EXISTS (SELECT 1 FROM ent_rel_tbl WHERE src_ent_id = ? OR trg_ent_id = ? AND obslt_vrsn_seq_id IS NULL)", data.Key, data.Key)
                ).ToList();
                var ownedByMe = context.Query<DbEntityIdentifier>(
                    context.CreateSqlStatement()
                    .SelectFrom(typeof(DbEntityIdentifier))
                    .Where<DbEntityIdentifier>(o => o.Value == id.Value && o.AuthorityKey == id.AuthorityKey && o.ObsoleteVersionSequenceId == null)
                    .And("(ent_id = ? OR EXISTS (SELECT 1 FROM ent_rel_tbl WHERE src_ent_id = ? OR trg_ent_id = ? AND obslt_vrsn_seq_id IS NULL))", data.Key, data.Key, data.Key)
                ).ToList();

                // Verify scope
                var scopes = context.Query<DbAuthorityScope>(o => o.AssigningAuthorityKey == dbAuth.Key);
                if (scopes.Any() && !scopes.Any(s => s.ScopeConceptKey == data.ClassConceptKey) // This type of identifier is not allowed to be assigned to this type of object
                    && !ownedByOthers.Any()
                    && !ownedByMe.Any()) // Unless it was already associated to another type of object related to me
                    issues.Add(new DetectedIssue(DetectedIssuePriorityType.Error, "id.target", $"Identifier of type {dbAuth.DomainName} cannot be assigned to object of type {data.ClassConceptKey}", DetectedIssueKeys.BusinessRuleViolationIssue));

                // If the identity domain is unique, and we've been asked to raid identifier uq issues
                if (dbAuth.IsUnique && this.m_persistenceService.GetConfiguration().Validation.IdentifierUniqueness &&
                    ownedByOthers.Any())
                    issues.Add(new DetectedIssue(priority, $"id.uniqueness", $"Identifier {id.Value} in domain {dbAuth.DomainName} violates unique constraint", DetectedIssueKeys.FormalConstraintIssue));
                if (dbAuth.AssigningApplicationKey.HasValue) // Must have permission
                {
                    if (provenance == null) provenance = context.FirstOrDefault<DbSecurityProvenance>(o => o.Key == data.CreatedByKey);


                    if (provenance.ApplicationKey != dbAuth.AssigningApplicationKey  // Established prov key
                        && assertedCreator != dbAuth.AssigningApplicationKey  // original prov key
                        && !ownedByMe.Any() 
                        && !ownedByOthers.Any()) // and has not already been assigned to me or anyone else (it is a new , unknown identifier)
                        issues.Add(new DetectedIssue(DetectedIssuePriorityType.Error, $"id.authority", $"Application {provenance.ApplicationKey} does not have permission to assign {dbAuth.DomainName}", DetectedIssueKeys.SecurityIssue));
                }
                if (!String.IsNullOrEmpty(dbAuth.ValidationRegex) && this.m_persistenceService.GetConfiguration().Validation.IdentifierFormat) // must be valid
                {
                    var nonMatch = !new Regex(dbAuth.ValidationRegex).IsMatch(id.Value);
                    if (nonMatch)
                        issues.Add(new DetectedIssue(priority, $"id.format", $"Identifier {id.Value} in domain {dbAuth.DomainName} failed format validation", DetectedIssueKeys.FormalConstraintIssue));
                }
            }

            // Validate issues
            if (issues.Any(o => o.Priority == DetectedIssuePriorityType.Error))
                throw new DetectedIssueException(issues);
            else if (issues.Count > 0)
            {
                var extension = data.Extensions.FirstOrDefault(o => o.ExtensionTypeKey == ExtensionTypeKeys.DataQualityExtension);
                if (extension == null)
                    data.Extensions.Add(new EntityExtension(ExtensionTypeKeys.DataQualityExtension, typeof(DictionaryExtensionHandler), issues));
                else
                {
                    var existingValues = extension.GetValue<List<DetectedIssue>>();
                    existingValues.AddRange(issues);
                    extension.ExtensionValue = existingValues;
                }
            }

        }

        /// <summary>
        /// Update the specified entity
        /// </summary>
        public Core.Model.Entities.Entity UpdateCoreProperties(DataContext context, Core.Model.Entities.Entity data)
        {

            // Asserted creator of this version
            Guid? assertedCreator = data.CreatedByKey;

            // Esnure exists
            if (data.ClassConcept != null) data.ClassConcept = data.ClassConcept?.EnsureExists(context) as Concept;
            if (data.DeterminerConcept != null) data.DeterminerConcept = data.DeterminerConcept?.EnsureExists(context) as Concept;
            if (data.StatusConcept != null) data.StatusConcept = data.StatusConcept?.EnsureExists(context) as Concept;
            if (data.Template != null) data.Template = data.Template?.EnsureExists(context) as TemplateDefinition;
            if (data.TypeConcept != null) data.TypeConcept = data.TypeConcept?.EnsureExists(context) as Concept;
            data.TypeConceptKey = data.TypeConcept?.Key ?? data.TypeConceptKey;
            data.DeterminerConceptKey = data.DeterminerConcept?.Key ?? data.DeterminerConceptKey;
            data.ClassConceptKey = data.ClassConcept?.Key ?? data.ClassConceptKey;
            data.StatusConceptKey = data.StatusConcept?.Key ?? data.StatusConceptKey;
            data.StatusConceptKey = data.StatusConceptKey == Guid.Empty || data.StatusConceptKey == null ? StatusKeys.New : data.StatusConceptKey;

            var retVal = base.UpdateInternal(context, data);


            // Identifiers
            if (data.Identifiers != null)
            {
                this.VerifyIdentities(context, data, assertedCreator);

                // Validate unique values for IDs
                base.UpdateVersionedAssociatedItems<Core.Model.DataTypes.EntityIdentifier, DbEntityIdentifier>(
                    data.Identifiers.Where(o => !o.IsEmpty()),
                    retVal,
                    context);
            }

            // Relationships
            if (data.Relationships != null)
                base.UpdateVersionedAssociatedItems<Core.Model.Entities.EntityRelationship, DbEntityRelationship>(
                   data.Relationships.Distinct(new EntityRelationshipPersistenceService.Comparer()).Where(o => o != null && !o.InversionIndicator && !o.IsEmpty() && (o.SourceEntityKey == data.Key || !o.SourceEntityKey.HasValue)).ToList(),
                    retVal,
                    context);

            // Telecoms
            if (data.Telecoms != null)
                base.UpdateVersionedAssociatedItems<Core.Model.Entities.EntityTelecomAddress, DbTelecomAddress>(
                   data.Telecoms.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            // Extensions
            if (data.Extensions != null)
                base.UpdateVersionedAssociatedItems<Core.Model.DataTypes.EntityExtension, DbEntityExtension>(
                   data.Extensions.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            // Names
            if (data.Names != null)
                base.UpdateVersionedAssociatedItems<Core.Model.Entities.EntityName, DbEntityName>(
                   data.Names.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            // Addresses
            if (data.Addresses != null)
                base.UpdateVersionedAssociatedItems<Core.Model.Entities.EntityAddress, DbEntityAddress>(
                   data.Addresses.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            // Notes
            if (data.Notes != null)
                base.UpdateVersionedAssociatedItems<Core.Model.DataTypes.EntityNote, DbEntityNote>(
                   data.Notes.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            // Tags
            if (data.Tags != null)
                base.UpdateAssociatedItems<Core.Model.DataTypes.EntityTag, DbEntityTag>(
                   data.Tags.Where(o => o != null && !o.IsEmpty() && !o.TagKey.StartsWith("$")),
                    retVal,
                    context);


            return retVal;
        }

        /// <summary>
        /// Obsoleted status key
        /// </summary>
        public override Core.Model.Entities.Entity ObsoleteInternal(DataContext context, Core.Model.Entities.Entity data)
        {
            data.StatusConceptKey = StatusKeys.Obsolete;
            return base.UpdateInternal(context, data);
        }

        /// <summary>
        /// Insert the entity
        /// </summary>
        public override Entity InsertInternal(DataContext context, Entity data)
        {
            switch (data.ClassConceptKey.ToString().ToUpper())
            {
                case EntityClassKeyStrings.Device:
                    return new DeviceEntityPersistenceService().InsertInternal(context, data.Convert<DeviceEntity>());
                case EntityClassKeyStrings.NonLivingSubject:
                    return new ApplicationEntityPersistenceService().InsertInternal(context, data.Convert<ApplicationEntity>());
                case EntityClassKeyStrings.Person:
                    return new PersonPersistenceService().InsertInternal(context, data.Convert<Person>());
                case EntityClassKeyStrings.Patient:
                    return new PatientPersistenceService().InsertInternal(context, data.Convert<Patient>());
                case EntityClassKeyStrings.Provider:
                    return new ProviderPersistenceService().InsertInternal(context, data.Convert<Provider>());
                case EntityClassKeyStrings.Place:
                case EntityClassKeyStrings.CityOrTown:
                case EntityClassKeyStrings.Country:
                case EntityClassKeyStrings.CountyOrParish:
                case EntityClassKeyStrings.State:
                case EntityClassKeyStrings.ServiceDeliveryLocation:
                    return new PlacePersistenceService().InsertInternal(context, data.Convert<Place>());
                case EntityClassKeyStrings.Organization:
                    return new OrganizationPersistenceService().InsertInternal(context, data.Convert<Organization>());
                case EntityClassKeyStrings.Material:
                    return new MaterialPersistenceService().InsertInternal(context, data.Convert<Material>());
                case EntityClassKeyStrings.ManufacturedMaterial:
                    return new ManufacturedMaterialPersistenceService().InsertInternal(context, data.Convert<ManufacturedMaterial>());
                default:
                    return this.InsertCoreProperties(context, data);

            }
        }

        /// <summary>
        /// Update entity
        /// </summary>
        public override Entity UpdateInternal(DataContext context, Entity data)
        {
            switch (data.ClassConceptKey.ToString().ToUpper())
            {
                case EntityClassKeyStrings.Device:
                    return new DeviceEntityPersistenceService().UpdateInternal(context, data.Convert<DeviceEntity>());
                case EntityClassKeyStrings.NonLivingSubject:
                    return new ApplicationEntityPersistenceService().UpdateInternal(context, data.Convert<ApplicationEntity>());
                case EntityClassKeyStrings.Person:
                    return new PersonPersistenceService().UpdateInternal(context, data.Convert<Person>());
                case EntityClassKeyStrings.Patient:
                    return new PatientPersistenceService().UpdateInternal(context, data.Convert<Patient>());
                case EntityClassKeyStrings.Provider:
                    return new ProviderPersistenceService().UpdateInternal(context, data.Convert<Provider>());
                case EntityClassKeyStrings.Place:
                case EntityClassKeyStrings.CityOrTown:
                case EntityClassKeyStrings.Country:
                case EntityClassKeyStrings.CountyOrParish:
                case EntityClassKeyStrings.State:
                case EntityClassKeyStrings.ServiceDeliveryLocation:
                    return new PlacePersistenceService().UpdateInternal(context, data.Convert<Place>());
                case EntityClassKeyStrings.Organization:
                    return new OrganizationPersistenceService().UpdateInternal(context, data.Convert<Organization>());
                case EntityClassKeyStrings.Material:
                    return new MaterialPersistenceService().UpdateInternal(context, data.Convert<Material>());
                case EntityClassKeyStrings.ManufacturedMaterial:
                    return new ManufacturedMaterialPersistenceService().UpdateInternal(context, data.Convert<ManufacturedMaterial>());
                default:
                    return this.UpdateCoreProperties(context, data);

            }
        }

    }
}
