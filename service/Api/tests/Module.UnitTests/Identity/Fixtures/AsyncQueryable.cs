using System.Collections;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.Query;

namespace Module.UnitTests.Identity.Fixtures;

internal static class AsyncQueryable
{
    public static IQueryable<T> Create<T>(params T[] items)
        => new InMemoryDbSet<T>(items);
}

/// <summary>Minimal in-memory IQueryable that supports EF Core async extensions.</summary>
internal class InMemoryDbSet<T> : IQueryable<T>, IAsyncEnumerable<T>, IAsyncQueryProvider
{
    private readonly List<T> _items;

    public InMemoryDbSet(IEnumerable<T> items) => _items = items.ToList();

    // --- IQueryable ---
    public Type ElementType => typeof(T);
    public Expression Expression => Expression.Constant(this);
    public IQueryProvider Provider => this;

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

    // --- IQueryProvider ---
    IQueryable IQueryProvider.CreateQuery(Expression e) => new InMemoryDbSet<T>(_items);
    IQueryable<U> IQueryProvider.CreateQuery<U>(Expression e)
        => typeof(U) == typeof(T) && _items is List<U> typed
            ? new InMemoryDbSet<U>(typed)
            : new InMemoryDbSet<U>([]);

    object? IQueryProvider.Execute(Expression expression) => Eval(StripAsNoTracking(expression));
    TResult IQueryProvider.Execute<TResult>(Expression expression) => (TResult)Eval(StripAsNoTracking(expression))!;

    // --- IAsyncQueryProvider ---
    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression) => throw new NotSupportedException();
    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken ct)
        => (TResult)Eval(StripAsNoTracking(expression))!;

    // --- IAsyncEnumerable ---
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default)
        => new AsyncEnumerator<T>(_items.GetEnumerator());

    // --- expression evaluator (supports Queryable.FirstOrDefault + Where) ---
    private object? Eval(Expression expr)
    {
        if (expr is ConstantExpression c) return c.Value;
        if (expr is UnaryExpression u && u.NodeType == ExpressionType.Quote) return Eval(u.Operand);

        if (expr is MethodCallExpression mc)
        {
            if (mc.Method.DeclaringType == typeof(Queryable) || mc.Method.DeclaringType == typeof(Enumerable))
                return EvalQueryableCall(mc);

            // fallback: compile & invoke
            return Expression.Lambda(expr).Compile().DynamicInvoke();
        }

        return Expression.Lambda(expr).Compile().DynamicInvoke();
    }

    private object? EvalQueryableCall(MethodCallExpression mc)
    {
        var src = EvalSource(mc.Arguments[0]);

        switch (mc.Method.Name)
        {
            case nameof(Queryable.FirstOrDefault):
                var fn = mc.Arguments.Count > 1 ? CompileLambda<T>(mc.Arguments[1]) : null;
                if (src is null) return default;
                return src.Cast<T>().FirstOrDefault(fn!);

            case nameof(Queryable.Where):
                var wfn = CompileLambda<T>(mc.Arguments[1]);
                if (src is null) return null;
                return src.Cast<T>().Where(wfn!).ToList();

            default:
                throw new NotSupportedException($"Method {mc.Method.Name} not supported");
        }
    }

    private static IEnumerable? EvalSource(Expression expr)
    {
        expr = StripAsNoTracking(expr);
        if (expr is ConstantExpression c) return c.Value as IEnumerable;
        return null;
    }

    private static Func<TEl, bool>? CompileLambda<TEl>(Expression expr)
    {
        while (expr is UnaryExpression u && u.NodeType == ExpressionType.Quote) expr = u.Operand;
        return expr is LambdaExpression l ? (Func<TEl, bool>)l.Compile() : null;
    }

    private static Expression StripAsNoTracking(Expression e) => new AsNoTrackingRemover().Visit(e);

    private sealed class AsNoTrackingRemover : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions)
                && node.Method.Name is "AsNoTracking" or "AsNoTrackingWithIdentityResolution")
                return Visit(node.Arguments[0]);
            return base.VisitMethodCall(node);
        }
    }
}

internal class AsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;
    public AsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
    public T Current => _inner.Current;
    public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());
    public ValueTask DisposeAsync() { _inner.Dispose(); return default; }
}
