using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap
{
    public class PropertySelectorExpressionVisitor : System.Linq.Expressions.ExpressionVisitor
    {
        private bool _seenParam = false;
        public string Path { get; private set; }
        private bool _first = true;

        public string GetPath(Expression expression)
        {
            var expr = Visit(expression);
            return Path;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _seenParam = true;
            return base.VisitParameter(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (!_seenParam)
            {
                if (node.Member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    if (_first && (node.Member as PropertyInfo).PropertyType.GetInterface("IEnumerable", true) == null)
                    {
                        throw new CatFlapException("Only enumerable properties may be specified in With calls");
                    }
                    else
                    {
                        _first = false;
                    }

                    if (Path != null && Path.Length > 0)
                    {
                        Path = node.Member.Name + "." + Path;
                    }
                    else
                    {
                        Path = node.Member.Name;
                    }
                }
            }
            return base.VisitMember(node);
        }
    }
}
