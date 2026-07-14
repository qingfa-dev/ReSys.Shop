using System.Collections;

namespace Shared.UnitTests.Application.Extensions.Exceptions;

public sealed class ExceptionExtensionsTests
{
    [Fact(DisplayName = "ToExceptionMetadata: null exception returns empty array")]
    public void NullException_ReturnsEmptyArray()
    {
        Exception? ex = null;
        var result = ex.ToExceptionMetadata();

        result.Should().BeEmpty();
    }

    [Fact(DisplayName = "ToExceptionMetadata: simple exception returns exception dictionary")]
    public void SimpleException_ReturnsExceptionDictionary()
    {
        var ex = new InvalidOperationException("Something went wrong.");

        var result = ex.ToExceptionMetadata();

        result.Should().HaveCount(1);
        result[0].Key.Should().Be("exception");
        var dict = result[0].Value.Should().BeOfType<Dictionary<string, object?>>().Subject;
        dict["type"].Should().Be("System.InvalidOperationException");
        dict["message"].Should().Be("Something went wrong.");
        dict.Should().ContainKey("source");
        dict.Should().ContainKey("stackTrace");
        dict.Should().NotContainKey("innerException");
        dict.Should().NotContainKey("data");
    }

    [Fact(DisplayName = "ToExceptionMetadata: nested inner exceptions produce nested dictionaries")]
    public void NestedInnerException_ProducesNestedDictionary()
    {
        var deep = new NullReferenceException("deep");
        var inner = new ArgumentException("inner", deep);
        var ex = new InvalidOperationException("outer", inner);

        var dict = ex.ToExceptionMetadata()[0].Value as Dictionary<string, object?>;

        dict.Should().NotBeNull();
        dict!["type"].Should().Be("System.InvalidOperationException");
        dict["message"].Should().Be("outer");

        var innerDict = dict["innerException"].Should().BeOfType<Dictionary<string, object?>>().Subject;
        innerDict["type"].Should().Be("System.ArgumentException");
        innerDict["message"].Should().Be("inner");
        innerDict.Should().ContainKey("stackTrace");

        var deepDict = innerDict["innerException"].Should().BeOfType<Dictionary<string, object?>>().Subject;
        deepDict["type"].Should().Be("System.NullReferenceException");
        deepDict["message"].Should().Be("deep");
        deepDict.Should().NotContainKey("innerException");
    }

    [Fact(DisplayName = "ToExceptionMetadata: exception with Data populates data dictionary")]
    public void ExceptionWithData_PopulatesDataDictionary()
    {
        var ex = new InvalidOperationException("with data");
        ex.Data["key1"] = "value1";
        ex.Data["count"] = 42;
        ex.Data["flag"] = true;

        var dict = ex.ToExceptionMetadata()[0].Value as Dictionary<string, object?>;

        dict.Should().NotBeNull();
        var data = dict!["data"].Should().BeOfType<Dictionary<string, object?>>().Subject;
        data["key1"].Should().Be("value1");
        data["count"].Should().Be(42);
        data["flag"].Should().Be(true);
    }

    [Fact(DisplayName = "ToExceptionMetadata: non-primitive Data values converted to string")]
    public void NonPrimitiveDataValues_ConvertedToString()
    {
        var ex = new InvalidOperationException("non-primitive data");
        ex.Data["obj"] = new Uri("https://example.com");

        var dict = ex.ToExceptionMetadata()[0].Value as Dictionary<string, object?>;

        dict.Should().NotBeNull();
        var data = dict!["data"].Should().BeOfType<Dictionary<string, object?>>().Subject;
        data["obj"].Should().Be("https://example.com");
    }

    [Fact(DisplayName = "ToExceptionMetadata: depth limit stops recursion")]
    public void DepthLimit_StopsRecursion()
    {
        Exception current = new InvalidOperationException("level 0");
        for (int i = 1; i <= 12; i++)
        {
            current = new InvalidOperationException($"level {i}", current);
        }

        var dict = current.ToExceptionMetadata()[0].Value as Dictionary<string, object?>;

        dict.Should().NotBeNull();
        int depth = 0;
        var probe = dict;
        while (probe!.ContainsKey("innerException"))
        {
            depth++;
            probe = probe["innerException"] as Dictionary<string, object?>;
        }
        depth.Should().Be(10);
    }

    [Fact(DisplayName = "ToExceptionMetadata: circular reference handled by depth guard")]
    public void CircularReference_HandledByDepthGuard()
    {
        var ex = new InvalidOperationException("circular");
        var field = typeof(Exception).GetField("_innerException",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field!.SetValue(ex, ex);

        var dict = ex.ToExceptionMetadata();

        dict.Should().HaveCount(1);
        dict[0].Value.Should().BeOfType<Dictionary<string, object?>>();
    }

    [Fact(DisplayName = "ToExceptionMetadata: always includes full stack trace")]
    public void AlwaysIncludesFullStackTrace()
    {
        try
        {
            throw new InvalidOperationException("test trace");
        }
        catch (Exception ex)
        {
            var dict = ex.ToExceptionMetadata()[0].Value as Dictionary<string, object?>;
            dict.Should().NotBeNull();
            dict!["stackTrace"].Should().NotBeNull();
            dict["stackTrace"].Should().BeOfType<string>();
            ((string)dict["stackTrace"]!).Should().Contain(nameof(AlwaysIncludesFullStackTrace));
        }
    }
}
