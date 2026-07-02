using System.Text.Json;

using JsonElement = System.Text.Json.JsonElement;

namespace Shared.UnitTests.Application.Models.Results;

[Trait("Category", "Unit")]
[Trait("Module", "Results")]
[Trait("Feature", "Serialization")]
public sealed class ResultSerializationTests(ITestOutputHelper output)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    #region Serialization

    [Fact(DisplayName = "Serialize: success result should omit null message and null metadata")]
    public void Serialize_SuccessResult_ShouldOmitNullMessageAndMetadata()
    {
        var result = Result.Ok();
        var json = JsonSerializer.Serialize(result, Options);

        output.WriteLine("JSON: {0}", json);

        json.Should().NotContain("message");
        json.Should().NotContain("metadata");
        json.Should().Contain("\"isSuccess\":true");
        json.Should().Contain("\"statusCode\":200");
    }

    [Fact(DisplayName = "Serialize: success result with message should include message")]
    public void Serialize_SuccessResultWithMessage_ShouldIncludeMessage()
    {
        var result = Result.Ok("completed");
        var json = JsonSerializer.Serialize(result, Options);

        output.WriteLine("JSON: {0}", json);

        json.Should().Contain("\"message\":\"completed\"");
    }

    [Fact(DisplayName = "Serialize: failure result should include errors")]
    public void Serialize_FailureResult_ShouldIncludeErrors()
    {
        var result = Result.BadRequest("invalid", [Error.BadRequest("V.E", "error")]);
        var json = JsonSerializer.Serialize(result, Options);

        output.WriteLine("JSON: {0}", json);

        json.Should().Contain("\"isSuccess\":false");
        json.Should().Contain("\"statusCode\":400");
        json.Should().Contain("\"errors\"");
        json.Should().NotContain("\"isFailure\"");
    }

    [Fact(DisplayName = "Serialize: result with metadata should include metadata")]
    public void Serialize_WithMetadata_ShouldIncludeMetadata()
    {
        var result = Result.Ok(metadata: ("trace", "abc123"));
        var json = JsonSerializer.Serialize(result, Options);

        output.WriteLine("JSON: {0}", json);

        json.Should().Contain("\"metadata\"");
        json.Should().Contain("\"trace\"");
    }

    #endregion

    #region Deserialization

    [Fact(DisplayName = "Deserialize: success result should restore all fields")]
    public void Deserialize_SuccessResult_ShouldRestoreFields()
    {
        const string json = """{"isSuccess":true,"statusCode":200,"message":"ok","errors":[],"metadata":{"ver":"1"}}""";

        Result result = JsonSerializer.Deserialize<Result>(json, Options);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Message.Should().Be("ok");
        result.Errors.Should().BeEmpty();
        result.Metadata.Should().ContainKey("ver");
        result.Metadata!["ver"].Should().BeOfType<JsonElement>().Which.GetString().Should().Be("1");
    }

    [Fact(DisplayName = "Deserialize: failure result should restore errors")]
    public void Deserialize_FailureResult_ShouldRestoreErrors()
    {
        const string json = """{"isSuccess":false,"statusCode":400,"message":"bad","errors":[{"code":"V.E","message":"error","type":400}]}""";

        Result result = JsonSerializer.Deserialize<Result>(json, Options);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Be("bad");
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("V.E");
    }

    [Fact(DisplayName = "Deserialize: result without errors should have empty list")]
    public void Deserialize_WithoutErrors_ShouldHaveEmptyList()
    {
        const string json = """{"isSuccess":true,"statusCode":204}""";

        Result result = JsonSerializer.Deserialize<Result>(json, Options);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(204);
        result.Errors.Should().BeEmpty();
    }

    #endregion
}
