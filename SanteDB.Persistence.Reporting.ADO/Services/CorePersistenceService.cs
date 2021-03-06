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
using SanteDB.Core.Model;
using SanteDB.OrmLite;
using SanteDB.Persistence.Reporting.ADO.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;

namespace SanteDB.Persistence.Reporting.ADO.Services
{
    /// <summary>
    /// Represents a core persistence service.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TDomain">The type of the domain.</typeparam>
    /// <typeparam name="TQueryReturn">The type of the query return.</typeparam>
    /// <seealso cref="ReportPersistenceServiceBase{TModel}" />
    public abstract class CorePersistenceService<TModel, TDomain, TQueryReturn> : ReportPersistenceServiceBase<TModel> where TModel : IdentifiedData, new() where TDomain : DbIdentified, new()
	{
		/// <summary>
		/// Converts a model instance to a domain instance.
		/// </summary>
		/// <param name="modelInstance">The model instance to convert.</param>
		/// <param name="context">The context.</param>
		/// <param name="overrideAuthContext">The principal to use instead of the default.</param>
		/// <returns>Returns the converted model instance.</returns>
		public override object FromModelInstance(TModel modelInstance, DataContext context)
		{
			return ModelMapper.MapModelInstance<TModel, TDomain>(modelInstance);
		}

		/// <summary>
		/// Inserts the model.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="model">The model.</param>
		/// <param name="overrideAuthContext">The principal to use instead of the default.</param>
		/// <returns>Returns the inserted model.</returns>
		public override TModel InsertInternal(DataContext context, TModel model)
		{
			var domainInstance = this.FromModelInstance(model, context);

			domainInstance = context.Insert(domainInstance as TDomain);

			return this.ToModelInstance(domainInstance, context);
		}

		/// <summary>
		/// Obsoletes the specified data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="model">The model.</param>
		/// <param name="overrideAuthContext">The principal to use instead of the default.</param>
		/// <returns>Returns the obsoleted data.</returns>
		/// <exception cref="System.InvalidOperationException"></exception>
		public override TModel ObsoleteInternal(DataContext context, TModel model)
		{
			if (model.Key == Guid.Empty)
			{
				throw new InvalidOperationException($"Cannot delete data using key: {model.Key}");
			}

			context.Delete<TDomain>(o => o.Key == model.Key);

			return model;
		}

		/// <summary>
		/// Queries for the specified model.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="query">The query.</param>
		/// <param name="offset">The offset.</param>
		/// <param name="count">The count.</param>
		/// <param name="totalResults">The total results.</param>
		/// <param name="countResults">if set to <c>true</c> [count results].</param>
		/// <param name="overrideAuthContext">The principal to use instead of the default.</param>
		/// <returns>Returns a list of the specified model instance which match the given query expression.</returns>
		public override IEnumerable<TModel> QueryInternal(DataContext context, Expression<Func<TModel, bool>> query, int offset, int? count, out int totalResults, bool countResults)
		{
			try
			{
				// Domain query
				SqlStatement domainQuery = context.CreateSqlStatement<TDomain>().SelectFrom();

				var expression = ModelMapper.MapModelExpression<TModel, TDomain, bool>(query, false);

				if (expression != null)
				{
					Type lastJoined = typeof(TDomain);
					if (typeof(CompositeResult).IsAssignableFrom(typeof(TQueryReturn)))
						foreach (var p in typeof(TQueryReturn).GenericTypeArguments.Select(o => ReportingPersistenceService.ModelMapper.MapModelType(o)))
							if (p != typeof(TDomain))
							{
								// Find the FK to join
								domainQuery.InnerJoin(lastJoined, p);
								lastJoined = p;
							}

					domainQuery.Where(expression);
				}
				else
				{
					this.traceSource.TraceEvent(EventLevel.Verbose, "Will use slow query construction due to complex mapped fields");
					domainQuery = ReportingPersistenceService.QueryBuilder.CreateQuery(query);
				}

				// Build and see if the query already exists on the stack???
				domainQuery = domainQuery.Build();

				if (Configuration.TraceSql)
				{
					traceSource.TraceEvent(EventLevel.Verbose, "Trace SQL flag is set to true, printing SQL statement");
					traceSource.TraceEvent(EventLevel.Verbose, $"GENERATED SQL STATEMENT: {domainQuery.SQL}");
				}

				if (offset > 0)
				{
					domainQuery.Offset(offset);
				}
				if (count.HasValue)
				{
					domainQuery.Limit(count.Value);
				}

				var results = context.Query<TQueryReturn>(domainQuery).OfType<object>();
				totalResults = results.Count();

				return results.Select(r => ToModelInstance(r, context));
			}
			catch (Exception e)
			{
				traceSource.TraceEvent(EventLevel.Error,  $"Unable to query: {e}");
				throw;
			}
		}

		/// <summary>
		/// Converts a domain instance to a model instance.
		/// </summary>
		/// <param name="domainInstance">The domain instance to convert.</param>
		/// <param name="context">The context.</param>
		/// <param name="overrideAuthContext">The principal to use instead of the default.</param>
		/// <returns>Returns the converted model instance.</returns>
		public override TModel ToModelInstance(object domainInstance, DataContext context)
		{
			return ModelMapper.MapDomainInstance<TDomain, TModel>(domainInstance as TDomain);
		}

		/// <summary>
		/// Updates the specified storage data.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="model">The model.</param>
		/// <param name="overrideAuthContext">The principal to use instead of the default.</param>
		/// <returns>Returns the updated model instance.</returns>
		public override TModel UpdateInternal(DataContext context, TModel model)
		{
			var domainInstance = this.FromModelInstance(model, context);

			domainInstance = context.Update(domainInstance as TDomain);

			return this.ToModelInstance(domainInstance, context);
		}
	}
}