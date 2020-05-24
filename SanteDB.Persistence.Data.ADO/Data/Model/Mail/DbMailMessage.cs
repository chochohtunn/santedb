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
using SanteDB.OrmLite.Attributes;
using SanteDB.Persistence.Data.ADO.Data.Model.Security;
using System;

namespace SanteDB.Persistence.Data.ADO.Data.Model.Mail
{
    /// <summary>
    /// Represents an alert message.
    /// </summary>
    [Table("mail_msg_tbl")]
    [AssociativeTable(typeof(DbSecurityUser), typeof(DbMailMessageRcptTo))]
	public class DbMailMessage : DbNonVersionedBaseData
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DbMailMessage"/> class.
		/// </summary>
		public DbMailMessage()
		{
			
		}

		/// <summary>
		/// Gets or sets the body of the message.
		/// </summary>
		[Column("body")]
		public string Body { get; set; }

		/// <summary>
		/// Gets or sets the alert message flags.
		/// </summary>
		[Column("flags")]
		public int Flags { get; set; }
		
		/// <summary>
		/// Gets or sets the from info of the alert message.
		/// </summary>
		[Column("from_info")]
		public string FromInfo { get; set; }

		/// <summary>
		/// Gets or sets the key of the alert message.
		/// </summary>
		[Column("mail_msg_id"), PrimaryKey, AutoGenerated]
		public override Guid Key { get; set; }

		/// <summary>
		/// Gets or sets creation time of the alert message.
		/// </summary>
		[Column("msg_utc")]
		public DateTimeOffset MessageUtc { get; set; }

		/// <summary>
		/// Gets or sets the subject of the message.
		/// </summary>
		[Column("subj")]
		public string Subject { get; set; }

		/// <summary>
		/// Gets or sets the to info of the message.
		/// </summary>
		[Column("to_info")]
		public string ToInfo { get; set; }
	}
}
