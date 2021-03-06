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
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Entities;
using SanteDB.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SanteDB.Persistence.Data.ADO.Data.Hax
{
    /// <summary>
    /// Provides a much faster way of search by address and name on entities
    /// </summary>
    public class EntityAddressNameQueryHack : IQueryBuilderHack
    {
        /// <summary>
        /// Hack the query 
        /// </summary>
        public bool HackQuery(QueryBuilder builder, SqlStatement sqlStatement, SqlStatement whereClause, Type tmodel, PropertyInfo property, string queryPrefix, QueryPredicate predicate, object values, IEnumerable<TableMapping> scopedTables, params KeyValuePair<String, object>[] queryFilter)
        {

            String cmpTblType = String.Empty, valTblType = String.Empty, keyName = String.Empty;
            Type guardType = null, componentType = null;
            // We can attempt to hack the address
            if (typeof(EntityAddress).IsAssignableFrom(tmodel))
            {
                cmpTblType = "ent_addr_cmp_tbl";
                valTblType = "ent_addr_cmp_val_tbl";
                guardType = typeof(AddressComponentKeys);
                componentType = typeof(EntityAddressComponent);
                keyName = "addr_id";
            }
            else if (typeof(EntityName).IsAssignableFrom(tmodel))
            {
                cmpTblType = "ent_name_cmp_tbl";
                valTblType = "phon_val_tbl";
                guardType = typeof(NameComponentKeys);
                componentType = typeof(EntityNameComponent);
                keyName = "name_id";
            }
            else
                return false;

           // Not applicable for us if
            //  - Not a name or address
            //  - Predicate is not component.value
            //  - There is already other where clause stuff
            if (guardType == null ||
                predicate.Path != "component" ||
                predicate.SubPath != "value" ||
                !String.IsNullOrEmpty(whereClause.SQL))
                return false;

            whereClause.And($" {keyName} IN (SELECT {keyName} FROM  {cmpTblType} AS {queryPrefix}{cmpTblType} ");
            // Filter through other name or address components which may be in the query at this query level
            List <String> componentTypeGuard = new List<String>();
            var localChar = 'a';
            int vCount = 0;
            foreach (var itm in queryFilter)
            {
                var pred = QueryPredicate.Parse(itm.Key);
                String guardFilter = String.Empty;

                // Do we have a guard for address?
                if (!String.IsNullOrEmpty(pred.Guard))
                {
                    // Translate Guards to UUIDs
                    var guards = pred.Guard.Split('|');
                    for (int i = 0; i < guards.Length; i++)
                        guards[i] = guardType.GetField(guards[i]).GetValue(null).ToString();
                    if (guards.Any(o => o == null)) return false;

                    // Add to where clause
                   componentTypeGuard.AddRange(guards);
                    guardFilter = $"AND {queryPrefix}{cmpTblType}.typ_cd_id IN ({String.Join(",", guards.Select(o => $"'{o}'"))})";
                }

                // Filter on the component value
                var value = itm.Value;
                if (value is String)
                    value = new List<Object>() { value };
                var qValues = value as List<Object>;
                vCount++;


                whereClause.Append($"LEFT JOIN (SELECT val_seq_id FROM {valTblType} AS {queryPrefix}{valTblType} WHERE ");
                whereClause.Append(builder.CreateSqlPredicate($"{queryPrefix}{valTblType}", "val", componentType.GetProperty("Value"), qValues));
                whereClause.Append($") AS {queryPrefix}{valTblType}_{localChar} ON ({queryPrefix}{valTblType}_{localChar++}.val_seq_id = {queryPrefix}{cmpTblType}.val_seq_id {guardFilter})");
            }

            whereClause.Append($" WHERE ");
            if(componentTypeGuard.Count > 0 )
                whereClause.Append($"{queryPrefix}{cmpTblType}.typ_cd_id IN ({String.Join(",", componentTypeGuard.Select(o => $"'{o}'"))}) AND ");
            whereClause.Append("(");
            // Ensure that the left joined objects exist
            for (char s = 'a'; s < localChar; s++)
                whereClause.Append($" {queryPrefix}{valTblType}_{s}.val_seq_id IS NOT NULL ").Append("OR");
            whereClause.RemoveLast();
            whereClause.Append($") GROUP BY {keyName} HAVING COUNT(DISTINCT {queryPrefix}{cmpTblType}.cmp_id) >= {vCount})");

            return true;
        }
    }
}
