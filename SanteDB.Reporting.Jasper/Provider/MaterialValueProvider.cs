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
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.RISI.Interfaces;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Reporting.Jasper.Provider
{
    /// <summary>
    /// Represents a material value provider.
    /// </summary>
    public class MaterialValueProvider : IParameterValuesProvider
	{
		/// <summary>
		/// Gets or sets the query identifier.
		/// </summary>
		/// <value>The query identifier.</value>
		public Guid QueryId => Guid.Parse("8E34B464-E5A4-4AD9-8F07-1EB83403D3AA");

		/// <summary>
		/// Gets a list of values.
		/// </summary>
		/// <typeparam name="T">The type of parameter for which to retrieve values.</typeparam>
		/// <returns>Returns a list of values.</returns>
		public IEnumerable<T> GetValues<T>() where T : IdentifiedData
		{
			var results = new List<Material>();

			var materialPersistenceService = ApplicationServiceContext.Current.GetService<IStoredQueryDataPersistenceService<Material>>();

			if (materialPersistenceService == null)
			{
				throw new InvalidOperationException($"Unable to locate { nameof(IStoredQueryDataPersistenceService<Material>) }");
			}

			int totalCount;
			var materials = materialPersistenceService.Query(m => m.ObsoletionTime == null && m.ClassConceptKey == EntityClassKeys.Material, this.QueryId, 0, null, out totalCount, AuthenticationContext.Current.Principal);

			results.AddRange(materials);

			return results.Cast<T>();
		}
	}
}