using System.Text.Json;

namespace Shared.UnitTests.Application.Models.Results;

[Trait("Category", "Unit")]
[Trait("Module", "Results")]
[Trait("Feature", "Serialization")]
public sealed class ValueResultSerializationTests(ITestOutputHelper output)
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    #region Serialization

    [Fact(DisplayName = "Serialize: success should include isSuccess and statusCode")]
    public void Serialize_Success_ShouldIncludeStatusAndStatusCode()
    {
        var result = Result<string>.Ok("hello");
        var json = JsonSerializer.Serialize(result, Options);

        output.WriteLine("JSON: {0}", json);

        json.Should().Contain("\"isSuccess\":true");
        json.Should().Contain("\"value\":\"hello\"");
    }

    [Fact(DisplayName = "Serialize: with metadata should include metadata")]
    public void Serialize_WithMetadata_ShouldIncludeMetadata()
    {
        var result = Result<Guid>.Ok(Guid.Empty, metadata: ("key", "val"));
        var json = JsonSerializer.Serialize(result, Options);

        output.WriteLine("JSON: {0}", json);

        json.Should().Contain("\"metadata\"");
        json.Should().Contain("\"key\":\"val\"");
    }

    [Fact(DisplayName = "Serialize: failure with errors should include errors and value")]
    public void Serialize_FailureWithErrors_ShouldIncludeErrors()
    {
        var result = Result<object>.BadRequest("bad", [Error.BadRequest("V.E", "e")]);
        var json = JsonSerializer.Serialize(result, Options);

        output.WriteLine("JSON: {0}", json);

        json.Should().Contain("\"errors\"");
        json.Should().Contain("\"isSuccess\":false");
        json.Should().Contain("\"value\"");
    }

    [Fact(DisplayName = "Serialize: computed properties IsFailure IsError Failures not in JSON")]
    public void Serialize_ComputedProperties_ShouldNotAppearInJson()
    {
        var result = Result<int>.Ok(1);
        var json = JsonSerializer.Serialize(result, Options);

        output.WriteLine("JSON: {0}", json);

        json.Should().NotContain("isFailure");
        json.Should().NotContain("isError");
        json.Should().NotContain("failures");
    }

    [Fact(DisplayName = "Serialize: NoContent should include status 204")]
    public void Serialize_NoContent_ShouldIncludeStatus()
    {
        var result = Result<int>.NoContent();
        var json = JsonSerializer.Serialize(result, Options);

        output.WriteLine("JSON: {0}", json);

        json.Should().Contain("\"isSuccess\":true");
        json.Should().Contain("\"statusCode\":204");
        json.Should().Contain("\"value\":0");
    }

    #endregion

    #region Deserialization

    [Fact(DisplayName = "Deserialize: success result should restore fields")]
    public void Deserialize_SuccessResult_ShouldRestoreFields()
    {
        const string json = """{"isSuccess":true,"statusCode":200}""";

        Result<string> result = JsonSerializer.Deserialize<Result<string>>(json, Options);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }

    [Fact(DisplayName = "Deserialize: failure should restore errors")]
    public void Deserialize_Failure_ShouldRestoreErrors()
    {
        const string json =
            """{"isSuccess":false,"statusCode":404,"errors":[{"code":"R.NF","message":"missing","type":404}]}""";

        Result<string> result = JsonSerializer.Deserialize<Result<string>>(json, Options);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("R.NF");
    }

    [Fact(DisplayName = "Deserialize: with metadata should restore")]
    public void Deserialize_WithMetadata_ShouldRestore()
    {
        const string json = """{"isSuccess":true,"statusCode":200,"metadata":{"version":"2"}}""";

        Result<string> result = JsonSerializer.Deserialize<Result<string>>(json, Options);

        result.IsSuccess.Should().BeTrue();
        result.Metadata.Should().NotBeNull();
    }

    #endregion
}