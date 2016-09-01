using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap
{
    public class TypeReplacerExpressionVisitor : ExpressionVisitor
    {
        ParameterExpression _parameter;
        Type _replacing;

        public TypeReplacerExpressionVisitor(ParameterExpression parameter, Type target)
        {
            _parameter = parameter;
            _replacing = target;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Type == _replacing)
            {
                return _parameter;
            }
            else
            {
                return base.VisitParameter(node);
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Type == _replacing)
            {
                return Expression.Call(_parameter.Type.GetMethod(node.Method.Name), Visit(node.Arguments[0]), Visit(node.Arguments[1]));
            }
            else
            {
                return base.VisitMethodCall(node);
            }
        }

        //protected override Expression VisitMember(MemberExpression node)
        //{
        //    if (node.Member.MemberType == System.Reflection.MemberTypes.Property)
        //    {
        //        MemberExpression memberExpression = null;
        //        var otherMember = node.Member as PropertyInfo;
        //        if (otherMember.PropertyType == _replacing)
        //        {
        //            memberExpression = Expression.Property(Visit(node.Expression), otherMember);
        //            return memberExpression;
        //        }
        //        else
        //        {
        //            return base.VisitMember(node);
        //        }
        //    }
        //    else
        //    {
        //        return base.VisitMember(node);
        //    }
        //}
    }
}
