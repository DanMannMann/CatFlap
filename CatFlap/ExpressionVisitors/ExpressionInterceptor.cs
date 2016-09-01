using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public sealed class ExpressionInterceptor : ExpressionVisitor
{
    #region Public events
    public event Func<BinaryExpression, BinaryExpression> Binary;
    public event Func<BlockExpression, BlockExpression> Block;
    public event Func<CatchBlock, CatchBlock> CatchBlock;
    public event Func<ConditionalExpression, ConditionalExpression> Conditional;
    public event Func<ConstantExpression, ConstantExpression> Constant;
    public event Func<DebugInfoExpression, DebugInfoExpression> DebugInfo;
    public event Func<DefaultExpression, DefaultExpression> Default;
    public event Func<DynamicExpression, DynamicExpression> Dynamic;
    public event Func<ElementInit, ElementInit> ElementInit;
    public event Func<Expression, Expression> Expression;
    public event Func<Expression, Expression> Extension;
    public event Func<GotoExpression, GotoExpression> Goto;
    public event Func<IndexExpression, IndexExpression> Index;
    public event Func<InvocationExpression, InvocationExpression> Invocation;
    public event Func<LabelExpression, LabelExpression> Label;
    public event Func<LabelTarget, LabelTarget> LabelTarget;
    public event Func<LambdaExpression, LambdaExpression> Lambda;
    public event Func<ListInitExpression, ListInitExpression> ListInit;
    public event Func<LoopExpression, LoopExpression> Loop;
    public event Func<MemberExpression, MemberExpression> Member;
    public event Func<MemberAssignment, MemberAssignment> MemberAssignment;
    public event Func<MethodCallExpression, MethodCallExpression> MethodCall;
    public event Func<MemberInitExpression, MemberInitExpression> MemberInit;
    public event Func<NewExpression, NewExpression> New;
    public event Func<NewArrayExpression, NewArrayExpression> NewArray;
    public event Func<ParameterExpression, ParameterExpression> Parameter;
    public event Func<RuntimeVariablesExpression, RuntimeVariablesExpression> RuntimeVariables;
    public event Func<SwitchExpression, SwitchExpression> Switch;
    public event Func<TryExpression, TryExpression> Try;
    public event Func<TypeBinaryExpression, TypeBinaryExpression> TypeBinary;
    public event Func<UnaryExpression, UnaryExpression> Unary;
    #endregion

    #region Public methods
    public IQueryable<T> Visit<T>(IQueryable<T> query)
    {
        return (this.Visit(query as IQueryable) as IQueryable<T>);
    }

    public IQueryable<T> Visit<T, TExpression>(IQueryable<T> query, Func<TExpression, TExpression> action) where TExpression : Expression
    {
        EventInfo evt = this.GetType().GetEvents(BindingFlags.Public | BindingFlags.Instance).Where(x => x.EventHandlerType == typeof(Func<TExpression, TExpression>)).First();
        evt.AddEventHandler(this, action);

        query = this.Visit(query);

        evt.RemoveEventHandler(this, action);

        return (query);
    }

    public IQueryable Visit(IQueryable query)
    {
        var s = query.Expression.ToString();
        return (query.Provider.CreateQuery(this.Visit(query.Expression)));
    }

    public IEnumerable<Expression> Flatten(IQueryable query)
    {
        Queue<Expression> list = new Queue<Expression>();
        Func<Expression, Expression> action = delegate(Expression expression)
        {
            if (expression != null)
            {
                list.Enqueue(expression);
            }

            return (expression);
        };

        this.Expression += action;

        this.Visit(query);

        this.Expression -= action;

        return (list);
    }
    #endregion

    #region Public override methods
    public override Expression Visit(Expression node)
    {
        if ((this.Expression != null) && (node != null))
        {
            return (base.Visit(this.Expression(base.Visit(node))));
        }
        else
        {
            return (base.Visit(node));
        }
    }
    #endregion

    #region Protected override methods
    protected override Expression VisitNew(NewExpression node)
    {
        if ((this.New != null) && (node != null))
        {
            return (base.VisitNew(this.New(node)));
        }
        else
        {
            return (base.VisitNew(node));
        }
    }

    protected override Expression VisitNewArray(NewArrayExpression node)
    {
        if ((this.NewArray != null) && (node != null))
        {
            return (base.VisitNewArray(this.NewArray(node)));
        }
        else
        {
            return (base.VisitNewArray(node));
        }
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if ((this.Parameter != null) && (node != null))
        {
            return (base.VisitParameter(this.Parameter(node)));
        }
        else
        {
            return (base.VisitParameter(node));
        }
    }

    protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
    {
        if ((this.RuntimeVariables != null) && (node != null))
        {
            return (base.VisitRuntimeVariables(this.RuntimeVariables(node)));
        }
        else
        {
            return (base.VisitRuntimeVariables(node));
        }
    }

    protected override Expression VisitSwitch(SwitchExpression node)
    {
        if ((this.Switch != null) && (node != null))
        {
            return (base.VisitSwitch(this.Switch(node)));
        }
        else
        {
            return (base.VisitSwitch(node));
        }
    }

    protected override Expression VisitTry(TryExpression node)
    {
        if ((this.Try != null) && (node != null))
        {
            return (base.VisitTry(this.Try(node)));
        }
        else
        {
            return (base.VisitTry(node));
        }
    }

    protected override Expression VisitTypeBinary(TypeBinaryExpression node)
    {
        if ((this.TypeBinary != null) && (node != null))
        {
            return (base.VisitTypeBinary(this.TypeBinary(node)));
        }
        else
        {
            return (base.VisitTypeBinary(node));
        }
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        if ((this.Unary != null) && (node != null))
        {
            return (base.VisitUnary(this.Unary(node)));
        }
        else
        {
            return (base.VisitUnary(node));
        }
    }

    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
        if ((this.MemberInit != null) && (node != null))
        {
            return (base.VisitMemberInit(this.MemberInit(node)));
        }
        else
        {
            return (base.VisitMemberInit(node));
        }
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if ((this.MethodCall != null) && (node != null))
        {
            return (base.VisitMethodCall(this.MethodCall(node)));
        }
        else
        {
            return (base.VisitMethodCall(node));
        }
    }


    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        if ((this.Lambda != null) && (node != null))
        {
            return (base.VisitLambda<T>(this.Lambda(node) as Expression<T>));
        }
        else
        {
            return (base.VisitLambda<T>(node));
        }
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        if ((this.Binary != null) && (node != null))
        {
            return (base.VisitBinary(this.Binary(node)));
        }
        else
        {
            return (base.VisitBinary(node));
        }
    }

    protected override Expression VisitBlock(BlockExpression node)
    {
        if ((this.Block != null) && (node != null))
        {
            return (base.VisitBlock(this.Block(node)));
        }
        else
        {
            return (base.VisitBlock(node));
        }
    }

    protected override CatchBlock VisitCatchBlock(CatchBlock node)
    {
        if ((this.CatchBlock != null) && (node != null))
        {
            return (base.VisitCatchBlock(this.CatchBlock(node)));
        }
        else
        {
            return (base.VisitCatchBlock(node));
        }
    }

    protected override Expression VisitConditional(ConditionalExpression node)
    {
        if ((this.Conditional != null) && (node != null))
        {
            return (base.VisitConditional(this.Conditional(node)));
        }
        else
        {
            return (base.VisitConditional(node));
        }
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        if ((this.Constant != null) && (node != null))
        {
            return (base.VisitConstant(this.Constant(node)));
        }
        else
        {
            return (base.VisitConstant(node));
        }
    }

    protected override Expression VisitDebugInfo(DebugInfoExpression node)
    {
        if ((this.DebugInfo != null) && (node != null))
        {
            return (base.VisitDebugInfo(this.DebugInfo(node)));
        }
        else
        {
            return (base.VisitDebugInfo(node));
        }
    }

    protected override Expression VisitDefault(DefaultExpression node)
    {
        if ((this.Default != null) && (node != null))
        {
            return (base.VisitDefault(this.Default(node)));
        }
        else
        {
            return (base.VisitDefault(node));
        }
    }

    protected override Expression VisitDynamic(DynamicExpression node)
    {
        if ((this.Dynamic != null) && (node != null))
        {
            return (base.VisitDynamic(this.Dynamic(node)));
        }
        else
        {
            return (base.VisitDynamic(node));
        }
    }

    protected override ElementInit VisitElementInit(ElementInit node)
    {
        if ((this.ElementInit != null) && (node != null))
        {
            return (base.VisitElementInit(this.ElementInit(node)));
        }
        else
        {
            return (base.VisitElementInit(node));
        }
    }

    protected override Expression VisitExtension(Expression node)
    {
        if ((this.Extension != null) && (node != null))
        {
            return (base.VisitExtension(this.Extension(node)));
        }
        else
        {
            return (base.VisitExtension(node));
        }
    }

    protected override Expression VisitGoto(GotoExpression node)
    {
        if ((this.Goto != null) && (node != null))
        {
            return (base.VisitGoto(this.Goto(node)));
        }
        else
        {
            return (base.VisitGoto(node));
        }
    }

    protected override Expression VisitIndex(IndexExpression node)
    {
        if ((this.Index != null) && (node != null))
        {
            return (base.VisitIndex(this.Index(node)));
        }
        else
        {
            return (base.VisitIndex(node));
        }
    }

    protected override Expression VisitInvocation(InvocationExpression node)
    {
        if ((this.Invocation != null) && (node != null))
        {
            return (base.VisitInvocation(this.Invocation(node)));
        }
        else
        {
            return (base.VisitInvocation(node));
        }
    }

    protected override Expression VisitLabel(LabelExpression node)
    {
        if ((this.Label != null) && (node != null))
        {
            return (base.VisitLabel(this.Label(node)));
        }
        else
        {
            return (base.VisitLabel(node));
        }
    }

    protected override LabelTarget VisitLabelTarget(LabelTarget node)
    {
        if ((this.LabelTarget != null) && (node != null))
        {
            return (base.VisitLabelTarget(this.LabelTarget(node)));
        }
        else
        {
            return (base.VisitLabelTarget(node));
        }
    }

    protected override Expression VisitListInit(ListInitExpression node)
    {
        if ((this.ListInit != null) && (node != null))
        {
            return (base.VisitListInit(this.ListInit(node)));
        }
        else
        {
            return (base.VisitListInit(node));
        }
    }

    protected override Expression VisitLoop(LoopExpression node)
    {
        if ((this.Loop != null) && (node != null))
        {
            return (base.VisitLoop(this.Loop(node)));
        }
        else
        {
            return (base.VisitLoop(node));
        }
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if ((this.Member != null) && (node != null))
        {
            return (base.VisitMember(this.Member(node)));
        }
        else
        {
            return (base.VisitMember(node));
        }
    }

    protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
    {
        if ((this.MemberAssignment != null) && (node != null))
        {
            return (base.VisitMemberAssignment(this.MemberAssignment(node)));
        }
        else
        {
            return (base.VisitMemberAssignment(node));
        }
    }
    #endregion
}