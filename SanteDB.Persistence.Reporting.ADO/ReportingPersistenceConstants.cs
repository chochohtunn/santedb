﻿/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
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
 * User: justin
 * Date: 2018-6-22
 */
namespace SanteDB.Persistence.Reporting.ADO
{
    /// <summary>
    /// Represents reporting persistence constants.
    /// </summary>
    public static class ReportingPersistenceConstants
	{
		/// <summary>
		/// The configuration section name.
		/// </summary>
		public const string ConfigurationSectionName = "santedb.persistence.reporting.psql";

		/// <summary>
		/// The map resource name.
		/// </summary>
		public const string MapResourceName = "SanteDB.Persistence.Reporting.ADO.Map.ModelMap.xml";

		/// <summary>
		/// The trace name.
		/// </summary>
		public const string TraceName = "SanteDB.Persistence.Reporting.ADO";
	}
}