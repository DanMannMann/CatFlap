using AutoMapper;
using AutoMapper.QueryableExtensions;
using ConsoleTables.Core;
using FelineSoft.CatFlap.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FelineSoft.CatFlap.Extensions
{
    internal static class InternalExtensions
    {
        internal static ObjectContext GetObjectContext(this DbContext context)
        {
            return ((System.Data.Entity.Infrastructure.IObjectContextAdapter)context).ObjectContext;
        }

        internal static PropertyInfo GetPropertyInfo<Tsource, TProperty>(this Type type,
            Expression<Func<Tsource, TProperty>> propertyLambda)
        {
            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expresion '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }

        internal static PropertyInfo GetPropertyInfo<TProperty>(this Type type)
        {
            var props = type.GetProperties().Where(x => x.PropertyType == typeof(TProperty)).ToList();
            if (props.Count == 0)
            {
                throw new CatFlapInvalidPropertyTypeException(string.Format("Type {0} does not contain a property of type {1}.", type, typeof(TProperty)));
            }
            else if (props.Count > 1)
            {
                throw new CatFlapInvalidPropertyTypeException(string.Format("Type {0} contains more than one property of type {1}, so the property to affect cannot be inferred. Use the SetAccessor.With() overload with the property selector lambda to specify the target property explicitly.", type, typeof(TProperty)));
            }
            else
            {
                return props[0];
            }
        }

        internal static IMappingExpression<TSource, TDestination> IgnoreAllNonExisting<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression)
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);
            var existingMaps = Mapper.GetAllTypeMaps().First(x => x.SourceType.Equals(sourceType)
                && x.DestinationType.Equals(destinationType));
            foreach (var property in existingMaps.GetUnmappedPropertyNames())
            {
                expression.ForMember(property, opt => opt.Ignore());
            }
            return expression;
        }

        internal static Dictionary<PropertyInfo, MapToAttribute[]> MapToAttributes(this Type type)
        {
            return type
                .GetProperties()
                .ToDictionary(x => x, y => y.GetCustomAttributes(typeof(MapToAttribute), false).Cast<MapToAttribute>().ToArray());
        }

        internal static byte[] ZipString(this string str)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (DeflateStream gzip =
                  new DeflateStream(output, CompressionMode.Compress))
                {
                    using (StreamWriter writer =
                      new StreamWriter(gzip, System.Text.Encoding.UTF8))
                    {
                        writer.Write(str);
                    }
                }

                return output.ToArray();
            }
        }

        internal static string UnzipString(this byte[] input)
        {
            using (MemoryStream inputStream = new MemoryStream(input))
            {
                using (DeflateStream gzip =
                  new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    using (StreamReader reader =
                      new StreamReader(gzip, System.Text.Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        internal static List<string> IgnoredProperties(this Type type)
        {
            return type
                .GetProperties()
                .Where(y => y.GetCustomAttributes(typeof(IgnoreAttribute), false).Count() > 0)
                .Select(x => x.Name)
                .ToList();
        }

        internal static Type TrackAs(this Type type)
        {
            var attr = type
                .GetCustomAttributes(typeof(TrackAsAttribute))
                .FirstOrDefault();

            if (attr == null)
            {
                return null;
            }
            else
            {
                return (attr as TrackAsAttribute).Type;
            }
        }

        internal static bool IsIgnored(this PropertyInfo info)
        {
            return info.GetCustomAttributes(typeof(IgnoreAttribute), false).Count() > 0;
        }

        internal static Expression GetCall(this Expression expression)
        {
            var ei = new ExpressionInterceptor();
            MethodCallExpression call = null;
            ei.MethodCall += new Func<MethodCallExpression, MethodCallExpression>(x =>
                {
                    call = x;
                    return x;
                });
            ei.Visit(expression);
            return call;
        }

        internal static Dictionary<T,K> Clone<T,K>(this Dictionary<T,K> input)
        {
            Dictionary<T, K> with = new Dictionary<T, K>();
            input.Keys.ToList().ForEach(x =>
            {
                with.Add(x, input[x]);
            });
            return with;
        }

        internal static Dictionary<T, List<K>> Clone<T, K>(this Dictionary<T, List<K>> input)
        {
            Dictionary<T, List<K>> clone = new Dictionary<T, List<K>>();
            input.Keys.ToList().ForEach(x =>
            {
                clone.Add(x, input[x].Clone());
            });
            return clone;
        }

        internal static Lictionary<T, K> Clone<T, K>(this Lictionary<T, K> input)
        {
            Lictionary<T, K> clone = new Lictionary<T, K>();
            input.Keys.ToList().ForEach(x =>
            {
                clone.Add(x, input[x].Clone());
            });
            return clone;
        }

        internal static List<T> Clone<T>(this List<T> input)
        {
            List<T> clone = new List<T>();
            input.ForEach(x =>
            {
                clone.Add(x);
            });
            return clone;
        }

        internal static Type GetElementTypeIfCollection(this Type input)
        {
            var matchArgs = input.GetGenericArguments();
            if (matchArgs == null || matchArgs.Length == 0) //not a collection
            {
                return input;
            }
            else //TODO make this not be so assumptive and shitty
            {
                return matchArgs[0];
            }
        }

        internal static string CommaList(this string[] input)
        {
            var keyList = "";
            input.ToList().ForEach(x => keyList = keyList + x + ", ");
            return keyList.Substring(0, keyList.Length - 2);
        }

        internal static string CommaList(this ICollection<string> input)
        {
            var keyList = "";
            input.ToList().ForEach(x => keyList = keyList + x + ", ");
            return keyList.Substring(0, keyList.Length - 2);
        }
    }

    public static class PublicExtensions
    {
        public static T Then<T>(this ICollection<T> input)
        {
            return default(T);
        }

        public static string ToStatsString(this Dictionary<Type, SQLClientStatistics> stats)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var stat in stats)
            {
                sb.AppendLine("=========Stats for " + stat.Key.Name + "=========");
                sb.AppendLine(stat.Value.ToString());
                var table = new ConsoleTable("Time Connected", "Execution Time", "Bytes Sent", "Bytes Recvd");
                foreach (var con in stat.Value.Connections)
                {
                    table.AddRow(con.ConnectionLengthMilliseconds, con.ExecutionTimeMilliseconds, con.TotalBytesSent, con.TotalBytesReceived);
                }
                sb.AppendLine(table.ToString());
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}