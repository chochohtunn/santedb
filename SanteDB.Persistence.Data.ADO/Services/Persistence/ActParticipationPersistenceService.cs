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
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Services;
using SanteDB.OrmLite;
using SanteDB.Persistence.Data.ADO.Data;
using SanteDB.Persistence.Data.ADO.Data.Model.Acts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SanteDB.Core.Model.Query;

namespace SanteDB.Persistence.Data.ADO.Services.Persistence
{
    /// <summary>
    /// Act participation persistence service
    /// </summary>
    public class ActParticipationPersistenceService : IdentifiedPersistenceService<ActParticipation, DbActParticipation, DbActParticipation> ,IAdoAssociativePersistenceService
    {
        /// <summary>
        /// Get from source id
        /// </summary>
        public IEnumerable GetFromSource(DataContext context, Guid id, decimal? versionSequenceId)
        {
            int tr = 0;
            return this.QueryInternal(context, base.BuildSourceQuery<ActParticipation>(id, versionSequenceId), Guid.Empty, 0, null, out tr, null, false).ToList();

        }

        /// <summary>
        /// Append orderby
        /// </summary>
        protected override SqlStatement AppendOrderBy(SqlStatement rawQuery, ModelSort<ActParticipation>[] orderBy)
        {
            rawQuery = base.AppendOrderBy(rawQuery, orderBy);
            return rawQuery.OrderBy<DbActParticipation>(o => o.SequenceId);
        }

        /// <summary>
        /// Represents as a model instance
        /// </summary>
        public override ActParticipation ToModelInstance(object dataInstance, DataContext context)
        {
            if (dataInstance == null) return null;

            var participationPart = dataInstance as DbActParticipation;
            var retVal = new ActParticipation()
            {
                EffectiveVersionSequenceId = participationPart.EffectiveVersionSequenceId,
                ObsoleteVersionSequenceId = participationPart.ObsoleteVersionSequenceId,
                ActKey = participationPart.SourceKey,
                PlayerEntityKey = participationPart.TargetKey,
                ParticipationRoleKey = participationPart.ParticipationRoleKey,
                LoadState = context.LoadState,
                Quantity = participationPart.Quantity,
                Key = participationPart.Key,
                SourceEntityKey = participationPart.SourceKey
            };

            if (context.LoadState == Core.Model.LoadState.FullLoad)
            {
                var concept = this.m_persistenceService.GetPersister(typeof(Concept)).Get(participationPart.ParticipationRoleKey);
                if (concept != null)
                    retVal.ParticipationRole = concept as Concept;
            }

            return retVal;
        }

        /// <summary>
        /// Insert the relationship
        /// </summary>
        public override ActParticipation InsertInternal(DataContext context, ActParticipation data)
        {
            // Ensure we haven't already persisted this
            if (data.PlayerEntity != null) data.PlayerEntity = data.PlayerEntity.EnsureExists(context) as Entity;
            data.PlayerEntityKey = data.PlayerEntity?.Key ?? data.PlayerEntityKey;
            data.ParticipationRoleKey = data.ParticipationRole?.Key ?? data.ParticipationRoleKey;
            if (data.Act != null) data.Act = data.Act.EnsureExists(context) as Act;
            data.ActKey = data.Act?.Key ?? data.ActKey;

            byte[] target = data.PlayerEntityKey.Value.ToByteArray(),
                source = data.SourceEntityKey.Value.ToByteArray(),
                typeKey = data.ParticipationRoleKey.Value.ToByteArray();

            //SqlStatement sql = new SqlStatement<DbActParticipation>().SelectFrom()
            //   .Where<DbActParticipation>(o => o.ActUuid == source )
            //   .Limit(1).Build();

            //IEnumerable<DbActParticipation> dbrelationships = context.TryGetData($"EX:{sql.ToString()}") as IEnumerable<DbActParticipation>;
            //if (dbrelationships == null) { 
            //    dbrelationships = context.Connection.Query<DbActParticipation>(sql.SQL, sql.Arguments.ToArray()).ToList();
            //    context.AddData($"EX{sql.ToString()}", dbrelationships);
            //}

            //var existing = dbrelationships.FirstOrDefault(
            //        o => o.ParticipationRoleUuid == typeKey &&
            //        o.EntityUuid == target);

            //if (existing == null)
            //{
            return base.InsertInternal(context, data);
            //    (dbrelationships as List<DbActParticipation>).Add(new DbActParticipation()
            //    {
            //        Uuid = retVal.Key.Value.ToByteArray(),
            //        ParticipationRoleUuid = typeKey,
            //        ActUuid = source,
            //        EntityUuid = target
            //    });
            //    return retVal;
            //}
            //else
            //{
            //    data.Key = new Guid(existing.Uuid);
            //    return data;
            //}
        }

        /// <summary>
        /// Update the specified object
        /// </summary>
        public override ActParticipation UpdateInternal(DataContext context, ActParticipation data)
        {
            data.PlayerEntityKey = data.PlayerEntity?.Key ?? data.PlayerEntityKey;
            data.ParticipationRoleKey = data.ParticipationRole?.Key ?? data.ParticipationRoleKey;
            data.ActKey = data.Act?.Key ?? data.ActKey;

            return base.UpdateInternal(context, data);
        }

        /// <summary>
        /// Comparer for entity relationships
        /// </summary>
        internal class Comparer : IEqualityComparer<ActParticipation>
        {
            /// <summary>
            /// Determine equality between the two relationships
            /// </summary>
            public bool Equals(ActParticipation x, ActParticipation y)
            {
                return x.SourceEntityKey == y.SourceEntityKey &&
                    x.PlayerEntityKey == y.PlayerEntityKey &&
                    (x.ParticipationRoleKey == y.ParticipationRoleKey || x.ParticipationRole?.Mnemonic == y.ParticipationRole?.Mnemonic);
            }

            /// <summary>
            /// Get hash code
            /// </summary>
            public int GetHashCode(ActParticipation obj)
            {
                int result = obj.SourceEntityKey.GetHashCode();
                result = 37 * result + obj.PlayerEntityKey.GetHashCode();
                result = 37 * result + obj.ParticipationRoleKey.GetHashCode();
                result = 37 * result + (obj.ParticipationRole?.Mnemonic.GetHashCode() ?? 0);

                return result;
            }
        }
    }
}
