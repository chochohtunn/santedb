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
using MARC.Everest.Connectors;
using SanteDB.Core;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Services;
using SanteDB.Messaging.FHIR.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SanteDB.Messaging.FHIR.Handlers
{
    /// <summary>
    /// Resource handler for acts base
    /// </summary>
    public abstract class RepositoryResourceHandlerBase<TFhirResource, TModel> : ResourceHandlerBase<TFhirResource, TModel>
		where TFhirResource : ResourceBase, new()
		where TModel : IdentifiedData, new()
	{
		// Repository service model
		protected IRepositoryService<TModel> m_repository;

		/// <summary>
		/// CTOR
		/// </summary>
		public RepositoryResourceHandlerBase()
		{
            ApplicationServiceContext.Current.Started += (o, e) =>
            {
                this.m_repository = ApplicationServiceContext.Current.GetService<IRepositoryService<TModel>>();

            };
		}

		/// <summary>
		/// Create the object
		/// </summary>
		protected override TModel Create(TModel modelInstance, List<IResultDetail> issues, TransactionMode mode)
		{
			return this.m_repository.Insert(modelInstance);
		}

		/// <summary>
		/// Create concept set filter based on act type
		/// </summary>
		protected Expression CreateConceptSetFilter(Guid conceptSetKey, ParameterExpression queryParameter)
		{
			var conceptSetRef = Expression.MakeMemberAccess(Expression.MakeMemberAccess(queryParameter, typeof(Act).GetProperty(nameof(Act.TypeConcept))), typeof(Concept).GetProperty(nameof(Concept.ConceptSets)));
			var lParam = Expression.Parameter(typeof(ConceptSet));
			var conceptSetFilter = Expression.MakeBinary(ExpressionType.Equal, Expression.Convert(Expression.MakeMemberAccess(lParam, typeof(ConceptSet).GetProperty(nameof(ConceptSet.Key))), typeof(Guid)), Expression.Constant(conceptSetKey));
			return Expression.Call((MethodInfo)typeof(Enumerable).GetGenericMethod("Any", new Type[] { typeof(ConceptSet) }, new Type[] { typeof(IEnumerable<ConceptSet>), typeof(Func<ConceptSet, bool>) }), conceptSetRef, Expression.Lambda(conceptSetFilter, lParam));
		}

		/// <summary>
		/// Perform a delete operation
		/// </summary>
		protected override TModel Delete(Guid modelId, List<IResultDetail> details)
		{
			return this.m_repository.Obsolete(modelId);
		}

		/// <summary>
		/// Query for patients.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="issues">The issues.</param>
		/// <param name="offset">The offset.</param>
		/// <param name="count">The count.</param>
		/// <param name="totalResults">The total results.</param>
		/// <returns>Returns the list of models which match the given parameters.</returns>
		protected override IEnumerable<TModel> Query(Expression<Func<TModel, bool>> query, List<IResultDetail> issues, Guid queryId, int offset, int count, out int totalResults)
        {
            return this.QueryEx<TModel>(query, issues, queryId, offset, count, out totalResults);
        }

        /// <summary>
        /// Represents the predicate model
        /// </summary>
        protected virtual IEnumerable<TPredicate> QueryEx<TPredicate>(Expression<Func<TPredicate, bool>> query, List<IResultDetail> issues, Guid queryId, int offset, int count, out int totalResults)
            where TPredicate : IdentifiedData
        {
            if (typeof(TPredicate).GetProperty(nameof(Entity.StatusConceptKey)) != null)
			{
				var obsoletionReference = Expression.MakeBinary(ExpressionType.NotEqual, Expression.Convert(Expression.MakeMemberAccess(query.Parameters[0], typeof(TPredicate).GetProperty(nameof(Entity.StatusConceptKey))), typeof(Guid)), Expression.Constant(StatusKeys.Obsolete));
				query = Expression.Lambda<Func<TPredicate, bool>>(Expression.AndAlso(obsoletionReference, query.Body), query.Parameters);
			}

            var repo = ApplicationServiceContext.Current.GetService<IRepositoryService<TPredicate>>();
            if (queryId == Guid.Empty)
                return repo.Find(query, offset, count, out totalResults);
            else
                return (repo as IPersistableQueryRepositoryService<TPredicate>).Find(query, offset, count, out totalResults, queryId);
		}

		/// <summary>
		/// Perform a read operation
		/// </summary>
		protected override TModel Read(Guid id, Guid versionId, List<IResultDetail> details)
		{
			return this.m_repository.Get(id, versionId);
		}

		/// <summary>
		/// Perform an update operation
		/// </summary>
		protected override TModel Update(TModel model, List<IResultDetail> details, TransactionMode mode)
		{
			return this.m_repository.Save(model);
		}
	}
}