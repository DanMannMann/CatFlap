using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper.QueryableExtensions;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq.Expressions;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using FelineSoft.CatFlap.Extensions;

namespace FelineSoft.CatFlap
{
    public abstract class CatFlap
    {
        private class KeyCacheEntry
        {
            public string[] PrimaryKeys { get; set; }
            public string[] ForeignKeys { get; set; }
            public string[] RequiredProperties { get; set; }
        }
                                //DbContext        Entity
        private static Dictionary<Type, Dictionary<Type, KeyCacheEntry>> _entityKeyCache = new Dictionary<Type, Dictionary<Type, KeyCacheEntry>>();
        private static Dictionary<SqlConnection, List<string>> _queryLogs = new Dictionary<SqlConnection, List<string>>();

        static CatFlap()
        {
            MappingStatements = new List<string>();
            AssertionStatements = new List<string>();
            ClientStatistics = new Dictionary<Type, SQLClientStatistics>();
            PrimaryKeyCheckCache = new List<Tuple<Type, Type, bool,string>>();
            RequiredPropertyCheckCache = new List<Tuple<Type, Type, bool, string>>();
        }

        internal static SqlConnection InitStats(DbContext db)
        {
            if (CatFlap.CollectSQLClientStatistics)
            {
                var connection = (SqlConnection)db.Database.Connection;
                connection.StatisticsEnabled = true;

                List<string> queries = null;
                if (_queryLogs.ContainsKey(connection))
                {
                    queries = _queryLogs[connection];
                }
                else
                {
                    queries = new List<string>();
                    _queryLogs.Add(connection, queries);
                }
                db.Database.Log = new Action<string>(x => queries.Add(x));
                return connection;
            }
            else
            {
                return null;
            }
        }

        internal static void CompleteStats(SqlConnection connection, Func<string> initiatingQuery, Type conType, bool isLongRunning)
        {
            if (CatFlap.CollectSQLClientStatistics && connection != null)
            {
                if (!CatFlap.ClientStatistics.ContainsKey(conType))
                {
                    CatFlap.ClientStatistics.Add(conType, new SQLClientStatistics(conType, isLongRunning));
                }
                var stats = new SQLConnectionStatistics();
                stats.RawStatistics = connection.RetrieveStatistics();
                stats.InitiatingQuery = initiatingQuery.Invoke();
                stats.ConnectionLengthMilliseconds = (long)stats.RawStatistics["ConnectionTime"];
                stats.ExecutionTimeMilliseconds = (long)stats.RawStatistics["ExecutionTime"];
                stats.TotalBytesSent = (long)stats.RawStatistics["BytesSent"];
                stats.TotalBytesReceived = (long)stats.RawStatistics["BytesReceived"];
                stats.DatabaseLogExcerpt.AddRange(_queryLogs[connection]);
                if (isLongRunning)
                {
                    connection.ResetStatistics();
                    _queryLogs[connection].Clear();
                }
                CatFlap.ClientStatistics[conType].Connections.Add(stats);
            }
        }

        internal static void IncrementInlineMappingCount(Type conType, bool isLongRunning)
        {
            if (CatFlap.CollectSQLClientStatistics)
            {
                if (!CatFlap.ClientStatistics.ContainsKey(conType))
                {
                    CatFlap.ClientStatistics.Add(conType, new SQLClientStatistics(conType, isLongRunning));
                }
                CatFlap.ClientStatistics[conType].InlineMappingCount++;
            }
        }

        internal static string[] GetPrimaryKeyNames<Tdb>(DbContext context)
             where Tdb : class, new()
        {
            var contextType = context.GetType();
            var entityType = typeof(Tdb);
            if (_entityKeyCache.ContainsKey(contextType))
            {
                if (_entityKeyCache[contextType].ContainsKey(entityType))
                {
                    if (_entityKeyCache[contextType][entityType].PrimaryKeys == null)
                    {
                        ObjectSet<Tdb> objectSet = ((IObjectContextAdapter)context).ObjectContext.CreateObjectSet<Tdb>();
                        string[] keyNames = objectSet.EntitySet.ElementType.KeyMembers.Select(k => k.Name).ToArray();
                        _entityKeyCache[contextType][entityType].PrimaryKeys = keyNames;
                    }
                    return _entityKeyCache[contextType][entityType].PrimaryKeys;
                }
                else
                {
                    ObjectSet<Tdb> objectSet = ((IObjectContextAdapter)context).ObjectContext.CreateObjectSet<Tdb>();
                    string[] keyNames = objectSet.EntitySet.ElementType.KeyMembers.Select(k => k.Name).ToArray();
                    _entityKeyCache[contextType].Add(entityType, new KeyCacheEntry() { PrimaryKeys = keyNames, RequiredProperties = null, ForeignKeys = null });
                    return keyNames;
                }
            }
            else
            {
                ObjectSet<Tdb> objectSet = ((IObjectContextAdapter)context).ObjectContext.CreateObjectSet<Tdb>();
                string[] keyNames = objectSet.EntitySet.ElementType.KeyMembers.Select(k => k.Name).ToArray();
                _entityKeyCache.Add(contextType, new Dictionary<Type, KeyCacheEntry>());
                _entityKeyCache[contextType].Add(entityType, new KeyCacheEntry() { PrimaryKeys = keyNames, RequiredProperties = null, ForeignKeys = null });
                return keyNames;
            }
        }

        internal static List<string> GetRequiredPropertyNames<Tdb>(DbContext context)
             where Tdb : class, new()
        {
            var contextType = context.GetType();
            var entityType = typeof(Tdb);
            if (_entityKeyCache.ContainsKey(contextType))
            {
                if (_entityKeyCache[contextType].ContainsKey(entityType))
                {
                    if (_entityKeyCache[contextType][entityType].RequiredProperties == null)
                    {
                        ObjectSet<Tdb> objectSet = ((IObjectContextAdapter)context).ObjectContext.CreateObjectSet<Tdb>();
                        string[] requiredPropertyNames = objectSet.EntitySet.ElementType.Members.Where(x => x is EdmProperty && !(x as EdmProperty).Nullable).Select(x => x.Name).ToArray();
                        _entityKeyCache[contextType][entityType].RequiredProperties = requiredPropertyNames;
                    }
                    return _entityKeyCache[contextType][entityType].RequiredProperties.ToList();
                }
                else
                {
                    ObjectSet<Tdb> objectSet = ((IObjectContextAdapter)context).ObjectContext.CreateObjectSet<Tdb>();
                    string[] requiredPropertyNames = objectSet.EntitySet.ElementType.Members.Where(x => x is EdmProperty && !(x as EdmProperty).Nullable).Select(x => x.Name).ToArray();
                    _entityKeyCache[contextType].Add(entityType, new KeyCacheEntry() { PrimaryKeys = null, RequiredProperties = requiredPropertyNames, ForeignKeys = null });
                    return requiredPropertyNames.ToList();
                }
            }
            else
            {
                ObjectSet<Tdb> objectSet = ((IObjectContextAdapter)context).ObjectContext.CreateObjectSet<Tdb>();
                string[] requiredPropertyNames = objectSet.EntitySet.ElementType.Members.Where(x => x is EdmProperty && !(x as EdmProperty).Nullable).Select(x => x.Name).ToArray();
                _entityKeyCache.Add(contextType, new Dictionary<Type, KeyCacheEntry>());
                _entityKeyCache[contextType].Add(entityType, new KeyCacheEntry() { PrimaryKeys = null, RequiredProperties = requiredPropertyNames, ForeignKeys = null });
                return requiredPropertyNames.ToList();
            }
        }

        internal static List<Tuple<Type, Type, bool, string>> PrimaryKeyCheckCache { get; private set; }
        internal static List<Tuple<Type, Type, bool, string>> RequiredPropertyCheckCache { get; private set; }

        public static void AssertKeysExist<Tentity,Tinput>(Func<DbContext> dbContextFactory, bool longRunningContext = false)
             where Tentity : class, new()
        {
            var check = PrimaryKeyCheckCache.FirstOrDefault(x => x.Item1 == typeof(Tentity) && x.Item2 == typeof(Tinput));
            if (check == null)
            {
                var context = dbContextFactory.Invoke();

                if (CollectMappingStatements)
                {
                    var src = typeof(Tinput).FullName.Replace("+", ".");
                    var dst = typeof(Tentity).FullName.Replace("+", ".");
                    string statement = string.Format("//Primary Key Assertion for {0} ({1}) -> {2} ({3})\r\n    {4};\r\n",
                        src,
                        "ViewModel",
                        dst,
                        "Entity",
                        "global::FelineSoft.CatFlap.CatFlap.AssertKeysExist<global::" + dst + ",global::" + src + ">(() => new global::" + context.GetType().FullName.Replace("+", ".") + "())");
                    AssertionStatements.Add(statement);
                }

                var keys = GetPrimaryKeyNames<Tentity>(context);

                var properties = typeof(Tinput).GetProperties().Select(x =>
                    {
                        var attr = x.GetCustomAttributes(typeof(MapToAttribute), false)
                            .Cast<MapToAttribute>()
                            .FirstOrDefault(y => y.EntityType == typeof(Tentity));
                        if (attr == null)
                        {
                            return x.Name;
                        }
                        else
                        {
                            return attr.PropertyName;
                        }
                    });
                var complies = properties.Where(x => keys.Contains(x)).Distinct().Count() == keys.Count();
                if (!complies)
                {
                    PrimaryKeyCheckCache.Add(new Tuple<Type, Type, bool, string>(typeof(Tentity), typeof(Tinput), false, keys.CommaList()));

                    throw new ApplicationException(string.Format("Primary keys for {0} do not exist on {1}.\r\n\r\nRequired primary key properties are: {2}", typeof(Tentity).FullName, typeof(Tinput).FullName, keys.CommaList()));
                }
                else
                {
                    PrimaryKeyCheckCache.Add(new Tuple<Type, Type, bool, string>(typeof(Tentity), typeof(Tinput), true, keys.CommaList()));
                }

                if (!longRunningContext)
                {
                    context.Dispose();
                }
            }
            else
            {
                if (!check.Item3)
                {
                    throw new ApplicationException(string.Format("Primary keys for {0} do not exist on {1}.\r\n\r\nRequired primary key properties are: {2}", typeof(Tentity).FullName, typeof(Tinput).FullName, check.Item4));
                }
            }
        }

        public static void AssertRequiredPropertiesExist<Tentity, Tinput>(Func<DbContext> dbContextFactory, bool longRunningContext = false)
             where Tentity : class, new()
        {
            var check = RequiredPropertyCheckCache.FirstOrDefault(x => x.Item1 == typeof(Tentity) && x.Item2 == typeof(Tinput));
            
            if (check == null)
            {
                var context = dbContextFactory.Invoke();
                
                if (CollectMappingStatements)
                {
                    var src = typeof(Tinput).FullName.Replace("+", ".");
                    var dst = typeof(Tentity).FullName.Replace("+", ".");
                    string statement = string.Format("//Required Property Assertion for {0} ({1}) -> {2} ({3})\r\n    {4};\r\n",
                        src,
                        "ViewModel",
                        dst,
                        "Entity",
                        "global::FelineSoft.CatFlap.CatFlap.AssertRequiredPropertiesExist<global::" + dst + ",global::" + src + ">(() => new global::" + context.GetType().FullName.Replace("+", ".") + "())");
                    AssertionStatements.Add(statement);
                }

                var properties = GetRequiredPropertyNames<Tentity>(context);
                var propertiesCopy = properties.Clone();
                var inputProperties = typeof(Tinput).GetProperties().Select(x =>
                {
                    var attr = x.GetCustomAttributes(typeof(MapToAttribute), false)
                        .Cast<MapToAttribute>()
                        .FirstOrDefault(y => y.EntityType == typeof(Tentity));
                    if (attr == null)
                    {
                        return x.Name;
                    }
                    else
                    {
                        return attr.PropertyName;
                    }
                });

                var map = AutoMapper.Mapper.FindTypeMapFor<Tinput, Tentity>();
                foreach (var propMap in map.GetPropertyMaps())
                {
                    if (propertiesCopy.Any(x => x == propMap.DestinationProperty.MemberInfo.Name))
                    {
                        propertiesCopy.Remove(propMap.DestinationProperty.MemberInfo.Name);
                    }
                }

                var complies = propertiesCopy.Count == 0;

                //var complies = inputProperties.Where(x => properties.Contains(x)).Distinct().Count() == properties.Count();
                if (!complies)
                {
                    RequiredPropertyCheckCache.Add(new Tuple<Type, Type, bool, string>(typeof(Tentity), typeof(Tinput), false, properties.CommaList()));
                    throw new ApplicationException(string.Format("Required properties for {0} do not exist on {1}.\r\n\r\nRequired properties are: {2}", typeof(Tentity).FullName, typeof(Tinput).FullName, properties.CommaList()));
                }
                else
                {
                    RequiredPropertyCheckCache.Add(new Tuple<Type, Type, bool, string>(typeof(Tentity), typeof(Tinput), true, properties.CommaList()));
                }
                if (!longRunningContext)
                {
                    context.Dispose();
                }
            }
            else if (!check.Item3)
            {
                throw new ApplicationException(string.Format("Required properties for {0} do not exist on {1}.\r\n\r\nRequired properties are: {2}", typeof(Tentity).FullName, typeof(Tinput).FullName, check.Item4));
            }
        }

        public static void PreProject<T, K>(Func<DbContext> dbFactory, bool longRunningContext = false) where T : class
        {
            var db = dbFactory.Invoke();
            
            SqlConnection connection = null;
            if (CollectSQLStatisticsForPreProjection)
            {
                InitStats(db);
            }

            var query = db.Set<T>().Take(0).Project().To<K>();
            query.ToList();

            if (CollectSQLStatisticsForPreProjection)
            {
                CompleteStats(connection, () => "[[CatFlap PreProject]]" + query.ToString(), db.GetType(), false);
            }
            if (!longRunningContext)
            {
                db.Dispose();
            }
        }

        public static void PreProject<T, K, V>(Func<DbContext> dbFactory, bool longRunningContext = false) where T : class
        {
            var db = dbFactory.Invoke();

            SqlConnection connection = null;
            if (CollectSQLStatisticsForPreProjection)
            {
                InitStats(db);
            }

            var newExpr = Expression.New(typeof(K).GetConstructor(Type.EmptyTypes));
            List<MemberBinding> init = new List<MemberBinding>();
            var param = Expression.Parameter(typeof(T), "x");

            foreach (var prop in typeof(K).GetProperties())
            {
                var p = typeof(T).GetProperty(prop.Name);
                if (p == null)
                {
                    throw new CatFlapInvalidPropertyTypeException("No matching property on entity for entity query type");
                }
                init.Add(Expression.Bind(prop, Expression.Property(param, p)));
            }

            var initExpr = Expression.MemberInit(newExpr, init);

            var relatedEntityFilter = Expression.Lambda<Func<T, K>>(initExpr, param);

            var query = db.Set<T>().Take(0).Select(relatedEntityFilter).Project().To<V>();
            query.ToList();

            if (CollectSQLStatisticsForPreProjection)
            {
                CompleteStats(connection, () => "[[CatFlap PreProject]]" + query.ToString(), db.GetType(), false);
            }
            if (!longRunningContext)
            {
                db.Dispose();
            }
        }

        public static bool CollectSQLClientStatistics { get; set; }
        public static bool CollectSQLStatisticsForPreProjection { get; set; }
        public static bool CollectMappingStatements { get; set; }
        public static Dictionary<Type, SQLClientStatistics> ClientStatistics { get; private set; }
        public static List<string> MappingStatements { get; private set; }
        public static List<string> AssertionStatements { get; private set; }
        public static string MappingCode
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("void ConfigureMaps()");
                sb.AppendLine("{");
                sb.AppendLine("    ////////////////////////////////////////////");
                sb.AppendLine("    //Entity-ViewModel & ViewModel-Entity Maps//");
                sb.AppendLine("    ////////////////////////////////////////////");
                sb.AppendLine();
                foreach (var statement in MappingStatements)
                {
                    sb.AppendLine("    " + statement);
                }

                if (AssertionStatements.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("    ////////////////////////////////////////////");
                    sb.AppendLine("    //    Key & Required Property Assertions  //");
                    sb.AppendLine("    ////////////////////////////////////////////");
                    sb.AppendLine();
                    foreach (var statement in AssertionStatements)
                    {
                        sb.AppendLine("    " + statement);
                    }
                }

                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("    ////////////////////////////////////////////");
                sb.AppendLine("    //       AutoMapper Config Assertion      //");
                sb.AppendLine("    ////////////////////////////////////////////");
                sb.AppendLine();
                sb.AppendLine("    global::AutoMapper.Mapper.AssertConfigurationIsValid();");
                sb.AppendLine("}");
                sb.AppendLine();

                return sb.ToString();
            }
        }
    }
}
