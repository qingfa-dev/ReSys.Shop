using System.Collections;

namespace Shared.Application.Extensions.Exceptions;

public static class ExceptionExtensions
{
    private const string ExceptionKey = "exception";
    private const string TypeKey = "type";
    private const string MessageKey = "message";
    private const string StackTraceKey = "stackTrace";
    private const string SourceKey = "source";
    private const string InnerExceptionKey = "innerException";
    private const string DataKey = "data";
    private const int MaxDepth = 10;

    private static readonly HashSet<Type> PrimitiveTypes =
    [
        typeof(string), typeof(int), typeof(long), typeof(short), typeof(byte),
        typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte),
        typeof(float), typeof(double), typeof(decimal),
        typeof(bool), typeof(char), typeof(Guid), typeof(DateTime),
        typeof(DateTimeOffset), typeof(TimeSpan), typeof(Uri),
        typeof(nint), typeof(nuint)
    ];

    public static (string Key, object? Value)[] ToExceptionMetadata(this Exception? exception)
    {
        if (exception is null)
            return [];

        var dict = BuildExceptionDictionary(exception, 0);
        return [(ExceptionKey, dict)];
    }

    private static Dictionary<string, object?> BuildExceptionDictionary(Exception exception, int depth)
    {
        var result = new Dictionary<string, object?>
        {
            [TypeKey] = exception.GetType().FullName ?? exception.GetType().Name,
            [MessageKey] = exception.Message,
            [StackTraceKey] = exception.StackTrace,
            [SourceKey] = exception.Source
        };

        if (exception.InnerException is { } inner && depth < MaxDepth)
        {
            result[InnerExceptionKey] = BuildExceptionDictionary(inner, depth + 1);
        }

        if (exception.Data.Count > 0)
        {
            var data = new Dictionary<string, object?>();
            foreach (DictionaryEntry entry in exception.Data)
            {
                string key = entry.Key?.ToString() ?? string.Empty;
                object? value = entry.Value;

                if (value is null || PrimitiveTypes.Contains(value.GetType()))
                    data[key] = value;
                else
                    data[key] = value.ToString();
            }
            result[DataKey] = data;
        }

        return result;
    }
}
