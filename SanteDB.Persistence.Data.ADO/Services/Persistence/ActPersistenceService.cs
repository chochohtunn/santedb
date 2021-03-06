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
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using SanteDB.OrmLite;
using SanteDB.Persistence.Data.ADO.Data;
using SanteDB.Persistence.Data.ADO.Data.Model;
using SanteDB.Persistence.Data.ADO.Data.Model.Acts;
using SanteDB.Persistence.Data.ADO.Data.Model.DataType;
using SanteDB.Persistence.Data.ADO.Data.Model.Extensibility;
using SanteDB.Persistence.Data.ADO.Data.Model.Security;
using System;
using System.Diagnostics.Tracing;
using System.Linq;

namespace SanteDB.Persistence.Data.ADO.Services.Persistence
{
    /// <summary>
    /// Represents a persistence service which persists ACT classes
    /// </summary>
    public class ActPersistenceService : VersionedDataPersistenceService<Core.Model.Acts.Act, DbActVersion, DbAct>
    {
       
        /// <summary>
        /// To model instance
        /// </summary>
        public virtual TActType ToModelInstance<TActType>(DbActVersion dbInstance, DbAct actInstance, DataContext context) where TActType : Core.Model.Acts.Act, new()
        {

            var retVal = m_mapper.MapDomainInstance<DbActVersion, TActType>(dbInstance);
            if (retVal == null) return null;

            retVal.ClassConceptKey = actInstance?.ClassConceptKey;
            retVal.MoodConceptKey = actInstance?.MoodConceptKey;
            retVal.TemplateKey = actInstance?.TemplateKey;
            return retVal;
        }

        /// <summary>
        /// Create an appropriate entity based on the class code
        /// </summary>
        public override Core.Model.Acts.Act ToModelInstance(object dataInstance, DataContext context)
        {
            // Alright first, which type am I mapping to?

            if (dataInstance == null) return null;

            DbActVersion dbActVersion = (dataInstance as CompositeResult)?.Values.OfType<DbActVersion>().FirstOrDefault() ?? dataInstance as DbActVersion ?? context.FirstOrDefault<DbActVersion>(o => o.VersionKey == (dataInstance as DbActSubTable).ParentKey);
            DbAct dbAct = (dataInstance as CompositeResult)?.Values.OfType<DbAct>().FirstOrDefault() ?? context.FirstOrDefault<DbAct>(o => o.Key == dbActVersion.Key);
            Act retVal = null;

            // 
            switch (dbAct.ClassConceptKey.ToString().ToUpper())
            {
                case ActClassKeyStrings.ControlAct:
                    retVal = new ControlActPersistenceService().ToModelInstance(
                                (dataInstance as CompositeResult)?.Values.OfType<DbControlAct>().FirstOrDefault() ?? context.FirstOrDefault<DbControlAct>(o => o.ParentKey == dbActVersion.VersionKey),
                                dbActVersion,
                                dbAct,
                                context);
                    break;
                case ActClassKeyStrings.SubstanceAdministration:
                    retVal = new SubstanceAdministrationPersistenceService().ToModelInstance(
                                (dataInstance as CompositeResult)?.Values.OfType<DbSubstanceAdministration>().FirstOrDefault() ?? context.FirstOrDefault<DbSubstanceAdministration>(o => o.ParentKey == dbActVersion.VersionKey),
                                dbActVersion,
                                dbAct,
                                context);
                    break;
                case ActClassKeyStrings.Observation:
                    var dbObs = (dataInstance as CompositeResult)?.Values.OfType<DbObservation>().FirstOrDefault() ?? context.FirstOrDefault<DbObservation>(o => o.ParentKey == dbActVersion.VersionKey);
                    if (dbObs == null)
                    {
                        this.m_tracer.TraceEvent(EventLevel.Warning, "Observation {0} is missing observation data! Even though class code is {1}", dbAct.Key, dbAct.ClassConceptKey);
                        retVal = this.ToModelInstance<Core.Model.Acts.Act>(dbActVersion, dbAct, context);
                    }
                    else
                        switch (dbObs.ValueType)
                        {
                            case "ST":
                                retVal = new TextObservationPersistenceService().ToModelInstance(
                                    (dataInstance as CompositeResult)?.Values.OfType<DbTextObservation>().FirstOrDefault() ?? context.FirstOrDefault<DbTextObservation>(o => o.ParentKey == dbObs.ParentKey),
                                    dbObs,
                                    dbActVersion,
                                    dbAct,
                                    context);
                                break;
                            case "CD":
                                retVal = new CodedObservationPersistenceService().ToModelInstance(
                                    (dataInstance as CompositeResult)?.Values.OfType<DbCodedObservation>().FirstOrDefault() ?? context.FirstOrDefault<DbCodedObservation>(o => o.ParentKey == dbObs.ParentKey),
                                    dbObs,
                                    dbActVersion,
                                    dbAct,
                                    context);
                                break;
                            case "PQ":
                                retVal = new QuantityObservationPersistenceService().ToModelInstance(
                                    (dataInstance as CompositeResult)?.Values.OfType<DbQuantityObservation>().FirstOrDefault() ?? context.FirstOrDefault<DbQuantityObservation>(o => o.ParentKey == dbObs.ParentKey),
                                    dbObs,
                                    dbActVersion,
                                    dbAct,
                                    context);
                                break;
                            default:
                                retVal = new ObservationPersistenceService().ToModelInstance(
                                    dbObs,
                                    dbActVersion,
                                    dbAct,
                                    context);
                                break;
                        }
                    break;
                case ActClassKeyStrings.Encounter:
                    retVal = new EncounterPersistenceService().ToModelInstance(
                                (dataInstance as CompositeResult)?.Values.OfType<DbPatientEncounter>().FirstOrDefault() ?? context.FirstOrDefault<DbPatientEncounter>(o => o.ParentKey == dbActVersion.VersionKey),
                                dbActVersion,
                                dbAct,
                                context);
                    break;
                case ActClassKeyStrings.Condition:
                default:
                    retVal = this.ToModelInstance<Core.Model.Acts.Act>(dbActVersion, dbAct, context);
                    break;
            }

            retVal.LoadAssociations(context);
            return retVal;
        }

        /// <summary>
        /// Override cache conversion
        /// </summary>
        protected override Act CacheConvert(object dataInstance, DataContext context)
        {
            if (dataInstance == null) return null;
            DbActVersion dbActVersion = (dataInstance as CompositeResult)?.Values.OfType<DbActVersion>().FirstOrDefault() ?? dataInstance as DbActVersion ?? context.FirstOrDefault<DbActVersion>(o => o.VersionKey == (dataInstance as DbActSubTable).ParentKey);
            DbAct dbAct = (dataInstance as CompositeResult)?.Values.OfType<DbAct>().FirstOrDefault() ?? context.FirstOrDefault<DbAct>(o => o.Key == dbActVersion.Key);
            Act retVal = null;
            var cache= new AdoPersistenceCache(context);

            if (!dbActVersion.ObsoletionTime.HasValue)
                switch (dbAct.ClassConceptKey.ToString().ToUpper())
                {
                    case ActClassKeyStrings.ControlAct:
                        retVal = cache?.GetCacheItem<ControlAct>(dbAct.Key);
                        break;
                    case ActClassKeyStrings.SubstanceAdministration:
                        retVal = cache?.GetCacheItem<SubstanceAdministration>(dbAct.Key);
                        break;
                    case ActClassKeyStrings.Observation:
                        var dbObs = (dataInstance as CompositeResult)?.Values.OfType<DbObservation>().FirstOrDefault() ?? context.FirstOrDefault<DbObservation>(o => o.ParentKey == dbActVersion.VersionKey);
                        if (dbObs != null)
                            switch (dbObs.ValueType)
                            {
                                case "ST":
                                    retVal = cache?.GetCacheItem<TextObservation>(dbAct.Key);
                                    break;
                                case "CD":
                                    retVal = cache?.GetCacheItem<CodedObservation>(dbAct.Key);
                                    break;
                                case "PQ":
                                    retVal = cache?.GetCacheItem<QuantityObservation>(dbAct.Key);
                                    break;
                            }
                        break;
                    case ActClassKeyStrings.Encounter:
                        retVal = cache?.GetCacheItem<PatientEncounter>(dbAct.Key);
                        break;
                    case ActClassKeyStrings.Condition:
                    default:
                        retVal = cache?.GetCacheItem<Act>(dbAct.Key);
                        break;
                }

            // Return cache value
            if (retVal != null)
                return retVal;
            else
                return base.CacheConvert(dataInstance, context);
        }

        /// <summary>
        /// Insert the act into the database
        /// </summary>
        public Core.Model.Acts.Act InsertCoreProperties(DataContext context, Core.Model.Acts.Act data)
        {
            if (data.ClassConcept != null) data.ClassConcept = data.ClassConcept?.EnsureExists(context) as Concept;
            if (data.MoodConcept != null) data.MoodConcept = data.MoodConcept?.EnsureExists(context) as Concept;
            if (data.ReasonConcept != null) data.ReasonConcept = data.ReasonConcept?.EnsureExists(context) as Concept;
            if (data.StatusConcept != null) data.StatusConcept = data.StatusConcept?.EnsureExists(context) as Concept;
            if (data.TypeConcept != null) data.TypeConcept = data.TypeConcept?.EnsureExists(context) as Concept;
            if (data.Template != null) data.Template = data.Template?.EnsureExists(context) as TemplateDefinition;

            data.ClassConceptKey = data.ClassConcept?.Key ?? data.ClassConceptKey;
            data.MoodConceptKey = data.MoodConcept?.Key ?? data.MoodConceptKey;
            data.ReasonConceptKey = data.ReasonConcept?.Key ?? data.ReasonConceptKey;
            data.StatusConceptKey = data.StatusConcept?.Key ?? data.StatusConceptKey ?? StatusKeys.New;
            data.TypeConceptKey = data.TypeConcept?.Key ?? data.TypeConceptKey;

            // Do the insert
            var retVal = base.InsertInternal(context, data);

            if (data.Extensions != null && data.Extensions.Any())
                base.UpdateVersionedAssociatedItems<Core.Model.DataTypes.ActExtension, DbActExtension>(
                   data.Extensions.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            if (data.Identifiers != null && data.Identifiers.Any())
                base.UpdateVersionedAssociatedItems<Core.Model.DataTypes.ActIdentifier, DbActIdentifier>(
                   data.Identifiers.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            if (data.Notes != null && data.Notes.Any())
                base.UpdateVersionedAssociatedItems<Core.Model.DataTypes.ActNote, DbActNote>(
                   data.Notes.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            if (data.Participations != null && data.Participations.Any())
            {
                data.Participations = data.Participations.Where(o => o != null && !o.IsEmpty()).Select(o => new ActParticipation(o.ParticipationRole?.EnsureExists(context)?.Key ?? o.ParticipationRoleKey , o.PlayerEntityKey) { Quantity = o.Quantity }).ToList();
                base.UpdateVersionedAssociatedItems<Core.Model.Acts.ActParticipation, DbActParticipation>(
                   data.Participations,
                    retVal,
                    context);
            }

            if (data.Relationships != null && data.Relationships.Any())
                base.UpdateVersionedAssociatedItems<Core.Model.Acts.ActRelationship, DbActRelationship>(
                   data.Relationships.Distinct(new ActRelationshipPersistenceService.Comparer()).Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            if (data.Tags != null && data.Tags.Any())
                base.UpdateAssociatedItems<Core.Model.DataTypes.ActTag, DbActTag>(
                   data.Tags.Where(o => o != null && !o.IsEmpty() && !o.TagKey.StartsWith("$")),
                    retVal,
                    context);

            if (data.Protocols != null && data.Protocols.Any())
                foreach (var p in data.Protocols)
                {
                    var proto = p.Protocol?.EnsureExists(context);
                    if (proto == null) // maybe we can retrieve the protocol from the protocol repository?
                    {
                        int t = 0;
                        proto = ApplicationServiceContext.Current.GetService<IClinicalProtocolRepositoryService>().FindProtocol(o => o.Key == p.ProtocolKey, 0, 1, out t).FirstOrDefault();
                        proto = proto.EnsureExists(context);
                    }
                    if (proto != null)
                        context.Insert(new DbActProtocol()
                        {
                            SourceKey = retVal.Key.Value,
                            ProtocolKey = proto.Key.Value,
                            Sequence = p.Sequence
                        });
                }

            // Persist policies
            if(data.Policies != null && data.Policies.Any())
            {
                foreach(var p in data.Policies)
                {
                    var pol = p.Policy?.EnsureExists(context);
                    if(pol == null) // maybe we can retrieve it from the PIP?
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
                    context.Insert(new DbActSecurityPolicy()
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
        /// Update the specified data
        /// </summary>
        public Core.Model.Acts.Act UpdateCoreProperties(DataContext context, Core.Model.Acts.Act data)
        {
            if (data.ClassConcept != null) data.ClassConcept = data.ClassConcept?.EnsureExists(context) as Concept;
            if (data.MoodConcept != null) data.MoodConcept = data.MoodConcept?.EnsureExists(context) as Concept;
            if (data.ReasonConcept != null) data.ReasonConcept = data.ReasonConcept?.EnsureExists(context) as Concept;
            if (data.StatusConcept != null) data.StatusConcept = data.StatusConcept?.EnsureExists(context) as Concept;
            if (data.TypeConcept != null) data.TypeConcept = data.TypeConcept?.EnsureExists(context) as Concept;
            if (data.Template != null) data.Template = data.Template?.EnsureExists(context) as TemplateDefinition;

            data.ClassConceptKey = data.ClassConcept?.Key ?? data.ClassConceptKey;
            data.MoodConceptKey = data.MoodConcept?.Key ?? data.MoodConceptKey;
            data.ReasonConceptKey = data.ReasonConcept?.Key ?? data.ReasonConceptKey;
            data.StatusConceptKey = data.StatusConcept?.Key ?? data.StatusConceptKey ?? StatusKeys.New;

            // Do the update
            var retVal = base.UpdateInternal(context, data);

            if (data.Extensions != null)
                base.UpdateVersionedAssociatedItems<Core.Model.DataTypes.ActExtension, DbActExtension>(
                   data.Extensions.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            if (data.Identifiers != null)
                base.UpdateVersionedAssociatedItems<Core.Model.DataTypes.ActIdentifier, DbActIdentifier>(
                   data.Identifiers.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            if (data.Notes != null)
                base.UpdateVersionedAssociatedItems<Core.Model.DataTypes.ActNote, DbActNote>(
                   data.Notes.Where(o => o != null && !o.IsEmpty()),
                    retVal,
                    context);

            if (data.Participations != null)
            {
                // Correct mixed keys
                if(this.m_persistenceService.GetConfiguration().DataCorrectionKeys.Contains("edmonton-participation-keyfix"))
                {
                    // Obsolete all
                    foreach(var itm in context.Query<DbActParticipation>(o=>o.SourceKey == retVal.Key && o.ObsoleteVersionSequenceId == null && o.ParticipationRoleKey == ActParticipationKey.Consumable)) {
                        itm.ObsoleteVersionSequenceId = retVal.VersionSequence;
                        context.Update(itm);
                    }
                    // Now we want to re-point to correct the issue
                    foreach (var itm in context.Query<DbActParticipation>(o => o.SourceKey == retVal.Key && o.ParticipationRoleKey == ActParticipationKey.Consumable && o.ObsoleteVersionSequenceId == retVal.VersionSequence))
                    {

                        var dItm = data.Participations.Find(o => o.Key == itm.Key);
                        if(dItm != null)
                            itm.TargetKey = dItm.PlayerEntityKey.Value;
                        itm.ObsoleteVersionSequenceId = null;
                        context.Update(itm);
                    }
                }

                // Update versioned association items
                base.UpdateVersionedAssociatedItems<Core.Model.Acts.ActParticipation, DbActParticipation>(
                      data.Participations.Where(o => o != null && !o.IsEmpty()),
                        retVal,
                        context);


            }

            if (data.Relationships != null)
                base.UpdateVersionedAssociatedItems<Core.Model.Acts.ActRelationship, DbActRelationship>(
                   data.Relationships.Distinct(new ActRelationshipPersistenceService.Comparer()).Where(o => o != null && !o.IsEmpty() && (o.SourceEntityKey == data.Key || !o.SourceEntityKey.HasValue)),
                    retVal,
                    context);

            if (data.Tags != null)
                base.UpdateAssociatedItems<Core.Model.DataTypes.ActTag, DbActTag>(
                   data.Tags.Where(o => o != null && !o.IsEmpty() && !o.TagKey.StartsWith("$")),
                    retVal,
                    context);

            return retVal;
        }

        /// <summary>
        /// Obsolete the act
        /// </summary>
        /// <param name="context"></param>
        public override Core.Model.Acts.Act ObsoleteInternal(DataContext context, Core.Model.Acts.Act data)
        {
            data.StatusConceptKey = StatusKeys.Obsolete;
            return base.UpdateInternal(context, data);
        }

        /// <summary>
        /// Perform insert
        /// </summary>
        public override Act InsertInternal(DataContext context, Act data)
        {
            switch (data.ClassConceptKey.ToString().ToUpper())
            {
                case ActClassKeyStrings.ControlAct:
                    return new ControlActPersistenceService().InsertInternal(context, data.Convert<ControlAct>());
                case ActClassKeyStrings.SubstanceAdministration:
                    return new SubstanceAdministrationPersistenceService().InsertInternal(context, data.Convert<SubstanceAdministration>());
                case ActClassKeyStrings.Observation:
                    switch (data.GetType().Name)
                    {
                        case "TextObservation":
                            return new TextObservationPersistenceService().InsertInternal(context, data.Convert<TextObservation>());
                        case "CodedObservation":
                            return new CodedObservationPersistenceService().InsertInternal(context, data.Convert<CodedObservation>());
                        case "QuantityObservation":
                            return new QuantityObservationPersistenceService().InsertInternal(context, data.Convert<QuantityObservation>());
                        default:
                            return this.InsertCoreProperties(context, data);
                    }
                case ActClassKeyStrings.Encounter:
                    return new EncounterPersistenceService().InsertInternal(context, data.Convert<PatientEncounter>());
                case ActClassKeyStrings.Condition:
                default:
                    return this.InsertCoreProperties(context, data);

            }
        }

        /// <summary>
        /// Perform update
        /// </summary>
        public override Act UpdateInternal(DataContext context, Act data)
        {
            switch (data.ClassConceptKey.ToString().ToUpper())
            {
                case ActClassKeyStrings.ControlAct:
                    return new ControlActPersistenceService().UpdateInternal(context, data.Convert<ControlAct>());
                case ActClassKeyStrings.SubstanceAdministration:
                    return new SubstanceAdministrationPersistenceService().UpdateInternal(context, data.Convert<SubstanceAdministration>());
                case ActClassKeyStrings.Observation:
                    switch (data.GetType().Name)
                    {
                        case "TextObservation":
                            return new TextObservationPersistenceService().UpdateInternal(context, data.Convert<TextObservation>());
                        case "CodedObservation":
                            return new CodedObservationPersistenceService().UpdateInternal(context, data.Convert<CodedObservation>());
                        case "QuantityObservation":
                            return new QuantityObservationPersistenceService().UpdateInternal(context, data.Convert<QuantityObservation>());
                        default:
                            return this.UpdateCoreProperties(context, data);
                    }
                case ActClassKeyStrings.Encounter:
                    return new EncounterPersistenceService().UpdateInternal(context, data.Convert<PatientEncounter>());
                case ActClassKeyStrings.Condition:
                default:
                    return this.UpdateCoreProperties(context, data);

            }
        }
    }
}
