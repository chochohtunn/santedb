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



namespace SanteDB.Persistence.Data.ADO.Data.Model.Concepts
{
    /// <summary>
    /// Represents storage entity for concept class
    /// </summary>
    [Table("cd_cls_tbl")]
	public class DbConceptClass : DbNonVersionedBaseData
	{

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[Column("cls_name")]
		public string Name {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the mnemonic.
		/// </summary>
		/// <value>The mnemonic.</value>
		[Column("mnemonic")]
		public String Mnemonic {
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the key
        /// </summary>
        [Column("cls_id"), PrimaryKey, AutoGenerated]
        public override Guid Key { get; set; }
    }
}

