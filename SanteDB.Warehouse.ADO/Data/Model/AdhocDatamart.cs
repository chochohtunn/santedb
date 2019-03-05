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
using SanteDB.OrmLite.Attributes;
using System;

namespace SanteDB.Warehouse.ADO.Data.Model
{
    /// <summary>
    /// Represents an adhoc datamart
    /// </summary>
    [Table("adhoc_mart")]
    public class AdhocDatamart
    {

        /// <summary>
        /// Datamart identifier
        /// </summary>
        [Column("uuid"), NotNull, AutoGenerated]
        public Guid DatamartId { get; set; }

        /// <summary>
        /// Name of the datamart
        /// </summary>
        [Column("name"), NotNull]
        public String Name { get; set; }

        /// <summary>
        /// Creation time
        /// </summary>
        [Column("crt_utc"), NotNull, AutoGenerated]
        public DateTimeOffset CreationTime { get; set; }

        /// <summary>
        /// Schema identifier
        /// </summary>
        [Column("sch_uuid"), NotNull, ForeignKey(typeof(AdhocSchema), nameof(AdhocSchema.SchemaId))]
        public Guid SchemaId { get; set; }
    }
}
