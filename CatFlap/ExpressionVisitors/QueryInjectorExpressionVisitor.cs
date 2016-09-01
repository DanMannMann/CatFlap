using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FelineSoft.CatFlap.Extensions;
using System.Data.Entity.Core.Objects;

namespace FelineSoft.CatFlap
{
    public class QueryInjectorExpressionVisitor : System.Linq.Expressions.ExpressionVisitor
    {
        private const string ProjectToParameterName = "dto.";
        private Dictionary<string, Expression> _expressions = new Dictionary<string, Expression>();
        private string _pathSoFar = "";
        private Dictionary<Type, Expression> _rootFilters;

        public QueryInjectorExpressionVisitor(Dictionary<Expression, Expression> expressions, Dictionary<Type, Expression> rootFilters)
        {
            var filters = new Dictionary<string, Expression>();
            foreach (var key in expressions.Keys)
            {
                var path = new PropertySelectorExpressionVisitor().GetPath(key);
                if (filters.ContainsKey(path))
                {
                    throw new CatFlapException("Only one With call is allowed per target path. Duplicated target: " + key.ToString());
                }
                else
                {
                    filters.Add(path, expressions[key]);
                }
            }
            _rootFilters = rootFilters;
            _expressions = filters;
        }

        private QueryInjectorExpressionVisitor() { }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type.IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(ObjectQuery<>))
            { //this is the root node. Need to transform any applicable root filter and apply it.
                var elementType = node.Type.GetElementTypeIfCollection();
                if (_rootFilters.ContainsKey(elementType))
                {
                    var lamb = _rootFilters[elementType] as LambdaExpression;

                    var casted = Expression.Convert(lamb.Body, typeof(ObjectQuery<>).MakeGenericType(elementType));

                    //combine the single filtering expression with the existing member access expression
                    var composed = ParameterReplacer.Replace(casted, lamb.Parameters.First(), node);

                    return composed;
                }
            }
            return base.VisitConstant(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "Select")
            {
                if (node.Arguments[0].ToString().StartsWith(ProjectToParameterName)) //this is a Select created by Project.To(). 
                {
                    //Apply relevant filters.
                    var newArg0 = ApplyFilters(node.Arguments[0]);

                    //pass the second argument to a new query visitor to visit the remainder of the tree recursively & return the result
                    QueryInjectorExpressionVisitor inner = new QueryInjectorExpressionVisitor();
                    inner._expressions = _expressions;
                    inner._rootFilters = _rootFilters;
                    inner._pathSoFar = _pathSoFar + node.Arguments[0].ToString().Replace(ProjectToParameterName, string.Empty) + ".";
                    var newArg1 = inner.Visit(node.Arguments[1]);

                    //Construct a new call to Select using the new args & return it
                    return Expression.Call(null, node.Method, newArg0, newArg1);
                }
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            //Find out if this node accesses ICollection<>.Count
            var countMember = node.Member.DeclaringType.GetMember("Count").FirstOrDefault();
            if (node.Member == countMember && node.Member.DeclaringType.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                //We change calls to ICollection.Count into calls to the IEnumerable.Count() extension method, as AutoMapper flattening will use 
                //the ICollection<>.Count property but our filter expression will return an IEnumerable<> rather than an ICollection<>
                //TODO may need to support other conversions here in situations hitherto unknown that AutoMapper flattening might throw at us
                var countExtension = typeof(Enumerable).GetMember("Count").Cast<MethodInfo>().Where(x => x.GetParameters().Count() == 1).Single();
                var countExtConcrete = countExtension.MakeGenericMethod(node.Expression.Type.GetElementTypeIfCollection());
                return Expression.Call(null, countExtConcrete, Visit(node.Expression));
            }
            else if (node.Member.MemberType == MemberTypes.Property)
            {
                return ApplyFilters(node);
            }
            else
            {
                return base.VisitMember(node);
            }
        }

        private Expression ApplyFilters(Expression node)
        {
            var nodeElementType = node.Type.GetElementTypeIfCollection();
            if (_expressions.Keys.Any(x => x == _pathSoFar + node.ToString().Replace(ProjectToParameterName, string.Empty)))
            {
                var exp = _expressions[_pathSoFar + node.ToString().Replace(ProjectToParameterName, string.Empty)] as LambdaExpression;

                Expression newExp = null;

                //See if there's an applicable root filter to combine with the .With expression
                var parameterType = exp.Parameters[0].Type.GetElementTypeIfCollection();
                if (_rootFilters != null && _rootFilters.ContainsKey(parameterType))
                {
                    //Combine the 2 filtering expressions
                    var lamb = _rootFilters[parameterType] as LambdaExpression;
                    newExp = ParameterReplacer.Replace(exp.Body, exp.Parameters.First(), lamb.Body);

                    MethodCallExpression arg = null;
                    if ((newExp as MethodCallExpression).Arguments[0].NodeType == ExpressionType.Call)
                    {
                        arg = ((newExp as MethodCallExpression).Arguments[0] as MethodCallExpression);
                    }
                    else
                    {
                        arg = ((newExp as MethodCallExpression).Arguments[0] as UnaryExpression).Operand as MethodCallExpression;
                    }

                    Expression param = arg;

                    while (param.NodeType != ExpressionType.Parameter)
                    {
                        param = (param as MethodCallExpression).Arguments[0];
                    }

                    //combine the new expression with the existing member access expression
                    newExp = ParameterReplacer.Replace(newExp, param as ParameterExpression, node);
                }
                else //no root filter, just use the .With expression
                {
                    //combine the single filtering expression with the existing member access expression
                    newExp = ParameterReplacer.Replace(exp.Body, exp.Parameters.First(), node);
                }
                return newExp;
            }
            else if (_rootFilters != null && _rootFilters.ContainsKey(nodeElementType))//see if there's an applicable root filter to use
            {
                var lamb = _rootFilters[nodeElementType] as LambdaExpression;

                //combine the single filtering expression with the existing member access expression
                var newExp = ParameterReplacer.Replace(lamb.Body, lamb.Parameters.First(), node);

                return newExp;
            }
            else //nothing to apply, just continue
            {
                return node;
            }
        }
    }
}
