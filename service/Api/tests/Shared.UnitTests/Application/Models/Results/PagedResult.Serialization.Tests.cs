using System.Text.Json;

namespace Shared.UnitTests.Application.Models.Results;

[Trait("Category", "Unit")]
[Trait("Module", "Results")]
[Trait("Feature", "Serialization")]
public sealed class PagedResultSerializationTests(ITestOutputHelper output)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    #region Serialization

    [Fact(DisplayName = "Serialize: success result should include items and pagination")]
    public void Serialize_SuccessResult_ShouldIncludeItemsAndPagination()
    {
        var result = PagedResult<int>.Ok([1, 2, 3], 1, 10, 100);
        var json = JsonSerializer.Serialize(result, Options);

        output.WriteLine("JSON: {0}", json);

        json.Should().Contain("\"isSuccess\":true");
        json.Should().Contain("\"items\":[1,2,3]");
        json.Should().Contain("\"page\":1");
        json.Should().Contain("\"pageSize\":10");
        json.Should().Contain("\"totalCount\":100");
    }

    [Fact(DisplayName = "Serialize: success with message and metadata should include them")]
    public void Serialize_WithMessageAndMetadata_ShouldInclude()
    {
        var result = PagedResult<int>.Ok([], 1, 10, 0, "empty", ("query", "test"));
        var json = JsonSerializer.Serialize(result, Options);

        output.WriteLine("JSON: {0}", json);

        json.Should().Contain("\"message\":\"empty\"");
        json.Should().Contain("\"metadata\"");
        json.Should().Contain("\"query\":\"test\"");
    }

    [Fact(DisplayName = "Serialize: failure result should include errors")]
    public void Serialize_FailureResult_ShouldIncludeErrors()
    {
        var result = PagedResult<object>.NotFound("missing");
        var json = JsonSerializer.Serialize(result, Options);

        output.WriteLine("JSON: {0}", json);

        json.Should().Contain("\"isSuccess\":false");
        json.Should().Contain("\"statusCode\":404");
        json.Should().Contain("\"errors\"");
    }

    [Fact(DisplayName = "Serialize: totalPages computed property should not appear")]
    public void Serialize_TotalPages_ShouldNotAppearInJson()
    {
        var result = PagedResult<int>.Ok([], 1, 10, 50);
        var json = JsonSerializer.Serialize(result, Options);

        output.WriteLine("JSON: {0}", json);

        json.Should().NotContain("totalPages");
    }

    [Fact(DisplayName = "Serialize: NoContent should include status 204 with defaults")]
    public void Serialize_NoContent_ShouldIncludeDefaults()
    {
        var result = PagedResult<int>.NoContent();
        var json = JsonSerializer.Serialize(result, Options);

        output.WriteLine("JSON: {0}", json);

        json.Should().Contain("\"isSuccess\":true");
        json.Should().Contain("\"statusCode\":204");
        json.Should().Contain("\"items\":[]");
        json.Should().Contain("\"page\":1");
        json.Should().Contain("\"pageSize\":10");
        json.Should().Contain("\"totalCount\":0");
        json.Should().NotContain("totalPages");
    }

    #endregion

    #region Deserialization

    [Fact(DisplayName = "Deserialize: success result should restore all fields")]
    public void Deserialize_SuccessResult_ShouldRestoreFields()
    {
        const string json = """{"isSuccess":true,"statusCode":200,"items":[10,20],"page":2,"pageSize":5,"totalCount":25}""";

        PagedResult<int> result = JsonSerializer.Deserialize<PagedResult<int>>(json, Options);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Items.Should().BeEquivalentTo([10, 20]);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(25);
    }

    [Fact(DisplayName = "Deserialize: failure result should restore errors")]
    public void Deserialize_FailureResult_ShouldRestoreErrors()
    {
        const string json = """{"isSuccess":false,"statusCode":422,"errors":[{"code":"V.E","message":"err","type":422}]}""";

        PagedResult<object> result = JsonSerializer.Deserialize<PagedResult<object>>(json, Options);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(422);
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("V.E");
    }

    [Fact(DisplayName = "Deserialize: with empty items should restore empty collection")]
    public void Deserialize_WithEmptyItems_ShouldRestoreEmpty()
    {
        const string json = """{"isSuccess":true,"statusCode":200,"items":[],"page":1,"pageSize":10,"totalCount":0}""";

        PagedResult<int> result = JsonSerializer.Deserialize<PagedResult<int>>(json, Options);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    #endregion
}
