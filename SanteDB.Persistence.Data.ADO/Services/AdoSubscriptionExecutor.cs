﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SanteDB.Core;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Subscription;
using SanteDB.Core.Services;
using SanteDB.Persistence.Data.ADO.Configuration;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Map;
using SanteDB.OrmLite;
using System.Text.RegularExpressions;
using SanteDB.Persistence.Data.ADO.Data.Model;
using System.Collections;
using SanteDB.Core.Event;
using SanteDB.Core.Security;

namespace SanteDB.Persistence.Data.ADO.Services
{
    /// <summary>
    /// Represents a default implementation of the subscription executor
    /// </summary>
    [ServiceProvider("ADO.NET Subscription Executor", Dependencies = new Type[] { typeof(ISqlDataPersistenceService) })]
    public class AdoSubscriptionExector : ISubscriptionExecutor
    {

        // Parameter regex
        private readonly Regex m_parmRegex = new Regex(@"(.*?)(\$\w*?\$)", RegexOptions.Multiline);

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "ADO.NET Subscription Executor";

        // Ref to query builder
        private QueryBuilder m_queryBuilder;

        // Ref to mapper
        private ModelMapper m_mapper;

        // Tracer
        private Tracer m_tracer = new Tracer(AdoDataConstants.TraceSourceName);

        /// <summary>
        /// Create the default subscription executor
        /// </summary>
        public AdoSubscriptionExector()
        {

            var adoService = ApplicationServiceContext.Current.GetService<AdoPersistenceService>();
            if (adoService == null)
                throw new InvalidOperationException("AdoSubscriptionExector requires the AdoPersistenceService to be registered");
            this.m_mapper = adoService.GetMapper();
            this.m_queryBuilder = adoService.GetQueryBuilder();
        }

        /// <summary>
        /// Gets the configuration for this object
        /// </summary>
        protected static AdoPersistenceConfigurationSection m_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<AdoPersistenceConfigurationSection>();

        /// <summary>
        /// Fired when the query is executed
        /// </summary>
        public event EventHandler<QueryResultEventArgs<IdentifiedData>> Executed;

        /// <summary>
        /// Fired when the query is about to execute
        /// </summary>
        public event EventHandler<QueryRequestEventArgs<IdentifiedData>> Executing;

        /// <summary>
        /// Exectue the specified subscription
        /// </summary>
        public IEnumerable<object> Execute(Guid subscriptionKey, NameValueCollection parameters, int offset, int? count, out int totalResults, Guid queryId)
        {
            var subscription = ApplicationServiceContext.Current.GetService<IRepositoryService<SubscriptionDefinition>>()?.Get(subscriptionKey);
            if (subscription == null)
                throw new KeyNotFoundException(subscriptionKey.ToString());
            else
                return this.Execute(subscription, parameters, offset, count, out totalResults, queryId);
        }

        /// <summary>
        /// Execute the current operation
        /// </summary>
        public IEnumerable<object> Execute(SubscriptionDefinition subscription, NameValueCollection parameters, int offset, int? count, out int totalResults, Guid queryId)
        {
            if (subscription == null || subscription.ServerDefinitions.Count == 0)
                throw new InvalidOperationException("Subscription does not have server definition");

            var preArgs = new QueryRequestEventArgs<IdentifiedData>(o => o.Key == subscription.Key, offset, count, queryId, AuthenticationContext.Current.Principal);
            this.Executing?.Invoke(this, preArgs);
            if (preArgs.Cancel)
            {
                this.m_tracer.TraceWarning("Pre-Event for executor failed");
                totalResults = preArgs.TotalResults;
                return preArgs.Results;
            }

            var persistenceType = typeof(IDataPersistenceService<>).MakeGenericType(subscription.ResourceType);
            var persistenceInstance = ApplicationServiceContext.Current.GetService(persistenceType) as IAdoPersistenceService;
            var queryService = ApplicationServiceContext.Current.GetService<IQueryPersistenceService>();
            var cacheService = ApplicationServiceContext.Current.GetService<IDataCachingService>();

            // Get the definition
            var definition = subscription.ServerDefinitions.FirstOrDefault(o => o.InvariantName == m_configuration.Provider.Invariant);
            if (definition == null)
                throw new InvalidOperationException($"Subscription does not provide definition for provider {m_configuration.Provider.Invariant}");

            // No obsoletion time?
            if (typeof(IBaseEntityData).IsAssignableFrom(subscription.ResourceType) && !parameters.ContainsKey("obsoletionTime"))
                parameters.Add("obsoletionTime", "null");

            // Query expression
            var queryExpression = typeof(QueryExpressionParser).GetGenericMethod(
                nameof(QueryExpressionParser.BuildLinqExpression),
                new Type[] { subscription.ResourceType },
                new Type[] { typeof(NameValueCollection) }
            ).Invoke(null, new object[] { parameters });


            // Query has been registered?
            if (queryId != Guid.Empty && queryService?.IsRegistered(queryId) == true)
            {
                totalResults = (int)queryService.QueryResultTotalQuantity(queryId);
                return queryService.GetQueryResults(queryId, offset, count ?? 100)
                    .AsParallel()
                    .AsOrdered()
                    .Select(o =>
                    {
                        try
                        {
                            var retVal = cacheService.GetCacheItem(o);
                            if (retVal == null)
                                using (var ctx = m_configuration.Provider.GetReadonlyConnection())
                                {
                                    ctx.Open();
                                    ctx.LoadState = LoadState.FullLoad;
                                    retVal = persistenceInstance.Get(ctx, o);
                                    cacheService?.Add(retVal as IdentifiedData);
                                }
                            return retVal;

                        }
                        catch (Exception e)
                        {
                            this.m_tracer.TraceError("Error fetching query results for {0}: {1}", queryId, e);
                            throw new DataPersistenceException("Error fetching query results", e);
                        }
                    }).ToList();
            }
            else
            {
                // Now grab the context and query!!!
                using (var connection = m_configuration.Provider.GetReadonlyConnection())
                {
                    try
                    {
                        connection.Open();
                        connection.LoadState = LoadState.FullLoad;

                        // First, build the query using the query build
                        var tableMapping = TableMapping.Get(this.m_mapper.MapModelType(subscription.ResourceType));
                        var query = (typeof(QueryBuilder).GetGenericMethod(
                            nameof(QueryBuilder.CreateQuery),
                            new Type[] { subscription.ResourceType },
                            new Type[] { queryExpression.GetType(), typeof(ColumnMapping).MakeArrayType() }
                        ).Invoke(this.m_queryBuilder, new object[] { queryExpression, null }) as SqlStatement).Build();

                        // Now we want to remove the portions of the built query statement after FROM and before WHERE as the definition will be the source of our selection
                        SqlStatement domainQuery = new SqlStatement(m_configuration.Provider, query.SQL.Substring(0, query.SQL.IndexOf(" FROM ")));

                        // Append our query
                        var definitionQuery = definition.Definition;
                        List<Object> values = new List<object>();
                        definitionQuery = this.m_parmRegex.Replace(definitionQuery, (o) =>
                        {
                            var strValue = parameters["_" + o.Groups[2].Value.Substring(1, o.Groups[2].Value.Length - 2)].First();
                            Guid uuid = Guid.Empty;
                            if (Guid.TryParse(strValue, out uuid))
                                values.Add(uuid);
                            else
                                values.Add(strValue);
                            return o.Groups[1].Value + "?";
                        });

                        // Now we want to append 
                        domainQuery.Append(" FROM (").Append(definitionQuery, values.ToArray()).Append($") AS {tableMapping.TableName} ");
                        domainQuery.Append(query.SQL.Substring(query.SQL.IndexOf("WHERE ")), query.Arguments.ToArray());

                        // Now we want to create the result type
                        var resultType = tableMapping.OrmType;
                        if (typeof(IDbVersionedData).IsAssignableFrom(resultType)) // type is versioned so we have to join
                        {
                            var fkType = tableMapping.GetColumn("Key").ForeignKey.Table;
                            resultType = typeof(CompositeResult<,>).MakeGenericType(resultType, fkType);
                        }

                        // Now we want to select out our results
                        if (count == 0)
                        {
                            totalResults = connection.Count(domainQuery);
                            return null;
                        }
                        else
                        {
                            // Fetch
                            var domainResults = typeof(DataContext).GetGenericMethod(
                                nameof(DataContext.Query),
                                new Type[] { resultType },
                                new Type[] { typeof(SqlStatement) }).Invoke(connection, new object[] { domainQuery }) as IOrmResultSet;

                            IEnumerable<object> resultObjects = null;

                            // Register query if query id specified
                            if (queryId != Guid.Empty)
                            {
                                var results = domainResults.Keys<Guid>().OfType<Guid>().ToArray();
                                totalResults = results.Count();
                                ApplicationContext.Current.GetService<IQueryPersistenceService>()?.RegisterQuerySet(queryId, results, null, totalResults);
                                resultObjects = results.Skip(offset).Take(count ?? 100).OfType<Object>();
                            }
                            else if (m_configuration.UseFuzzyTotals)
                            {
                                resultObjects = domainResults.Skip(offset).Take((count ?? 100) + 1).OfType<Object>();
                                totalResults = domainResults.Count();
                            }
                            else
                            {
                                totalResults = domainResults.Count();
                                resultObjects = domainResults.Skip(offset).Take(count ?? 100).OfType<Object>();
                            }

                            // Return
                            return resultObjects
                                .Take(count ?? 100)
                            .OfType<Object>()
                            .ToList()
                            .AsParallel()
                            .AsOrdered()
                            .Select(o =>
                            {
                                try
                                {
                                    if (o is Guid)
                                    {
                                        var retVal = cacheService.GetCacheItem((Guid)o);
                                        if(retVal == null)
                                            using (var subConn = connection.OpenClonedContext())
                                            {
                                                retVal = persistenceInstance.Get(subConn, (Guid)o);
                                                cacheService?.Add(retVal as IdentifiedData);
                                            }
                                        return retVal;
                                    }
                                    else
                                    {
                                        var idData = (o as CompositeResult)?.Values.OfType<IDbIdentified>().FirstOrDefault() ?? o as IDbIdentified;
                                        var retVal = cacheService.GetCacheItem(idData.Key);

                                        if (retVal == null)
                                            using (var subConn = connection.OpenClonedContext())
                                            {
                                                retVal = persistenceInstance.ToModelInstance(o, subConn);
                                                cacheService?.Add(retVal as IdentifiedData);
                                            }
                                        return retVal;
                                    }
                                }
                                catch (Exception e)
                                {
                                    this.m_tracer.TraceError("Error converting result: {0}", e);
                                    throw;
                                }
                            }).ToList();

                        }

                        totalResults = 0;
                    }
                    catch (Exception e)
                    {

#if DEBUG
                        this.m_tracer.TraceError("Error executing subscription: {0}", e);
#else
                this.m_tracer.TraceError("Error executing subscription: {0}", e.Message);
#endif

                        throw new DataPersistenceException($"Error executing subscription: {e.Message}", e);
                    }
                } // using conn
            } // if


        }
    }
}
