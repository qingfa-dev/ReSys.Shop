using System.Net;
using System.Text;
using System.Text.Json;

using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "Inference")]
public class InferenceClientTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact(DisplayName = "CreateEmbeddingAsync: Success returns EmbeddingResponse with correct values")]
    public async Task CreateEmbeddingAsync_Success_ReturnsEmbeddingResponse()
    {
        var expectedResponse = new EmbeddingResponse
        {
            Vector = [0.1f, 0.2f, 0.3f],
            ModelVersion = "v1.0",
            Dimension = 3,
            Metadata = new Dictionary<string, object> { ["model"] = "efficientnet_b0" }
        };

        var handler = new MockHttpMessageHandler(req =>
        {
            req.RequestUri!.PathAndQuery.Should().Be("/embeddings");
            req.Method.Should().Be(HttpMethod.Post);
            var body = JsonSerializer.Serialize(Result<EmbeddingResponse>.Ok(expectedResponse), JsonOptions);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        });

        var client = CreateInferenceClient(handler);
        var request = new EmbeddingRequest { ImageUrl = "http://test/img.jpg", Model = "efficientnet_b0" };

        var result = await client.CreateEmbeddingAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Vector.Should().BeEquivalentTo(expectedResponse.Vector);
        result.Value.ModelVersion.Should().Be(expectedResponse.ModelVersion);
        result.Value.Dimension.Should().Be(expectedResponse.Dimension);
    }

    [Fact(DisplayName = "CreateEmbeddingAsync: Non-success status code returns ServiceError")]
    public async Task CreateEmbeddingAsync_NonSuccessStatusCode_ReturnsServiceError()
    {
        var handler = new MockHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Internal error")
            }));

        var client = CreateInferenceClient(handler);
        var request = new EmbeddingRequest { ImageUrl = "http://test/img.jpg" };

        var result = await client.CreateEmbeddingAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.Errors.Should().Contain(e =>
            e.Code == "Inference.ServiceError" &&
            e.Message.Contains("500"));
    }

    [Fact(DisplayName = "CreateEmbeddingAsync: Invalid JSON body returns CommunicationFailed")]
    public async Task CreateEmbeddingAsync_InvalidResponseBody_ReturnsCommunicationFailed()
    {
        var handler = new MockHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not valid json")
            }));

        var client = CreateInferenceClient(handler);
        var request = new EmbeddingRequest { ImageUrl = "http://test/img.jpg" };

        var result = await client.CreateEmbeddingAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Inference.CommunicationFailed");
    }

    [Fact(DisplayName = "CreateEmbeddingAsync: Null JSON literal body returns InvalidResponse")]
    public async Task CreateEmbeddingAsync_NullResponseBody_ReturnsInvalidResponse()
    {
        var handler = new MockHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            }));

        var client = CreateInferenceClient(handler);
        var request = new EmbeddingRequest { ImageUrl = "http://test/img.jpg" };

        var result = await client.CreateEmbeddingAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Inference.InvalidResponse");
    }

    [Fact(DisplayName = "CreateEmbeddingAsync: OperationCanceledException returns RequestTimeout")]
    public async Task CreateEmbeddingAsync_OperationCanceled_ReturnsRequestTimeout()
    {
        var handler = new MockHttpMessageHandler(_ =>
            throw new OperationCanceledException());

        var client = CreateInferenceClient(handler);
        var request = new EmbeddingRequest { ImageUrl = "http://test/img.jpg" };

        var result = await client.CreateEmbeddingAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Inference.RequestTimeout");
    }

    [Fact(DisplayName = "CreateEmbeddingAsync: HttpRequestException returns CommunicationFailed")]
    public async Task CreateEmbeddingAsync_NetworkFailure_ReturnsCommunicationFailed()
    {
        var handler = new MockHttpMessageHandler(_ =>
            throw new HttpRequestException("Connection refused"));

        var client = CreateInferenceClient(handler);
        var request = new EmbeddingRequest { ImageUrl = "http://test/img.jpg" };

        var result = await client.CreateEmbeddingAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Inference.CommunicationFailed");
    }

    [Fact(DisplayName = "ListModelsAsync: Success returns model list")]
    public async Task ListModelsAsync_Success_ReturnsModelList()
    {
        var expectedModels = new List<ModelMetadata>
        {
            new() { Id = "efficientnet_b0", Name = "EfficientNet B0", Dimension = 512, IsOnnx = true }
        };

        var handler = new MockHttpMessageHandler(req =>
        {
            req.RequestUri!.PathAndQuery.Should().Be("/models");
            req.Method.Should().Be(HttpMethod.Get);
            var body = JsonSerializer.Serialize(Result<List<ModelMetadata>>.Ok(expectedModels), JsonOptions);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        });

        var client = CreateInferenceClient(handler);

        var result = await client.ListModelsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedModels);
    }

    [Fact(DisplayName = "ListModelsAsync: Non-success status code returns ServiceError")]
    public async Task ListModelsAsync_NonSuccessStatusCode_ReturnsServiceError()
    {
        var handler = new MockHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("Service unavailable")
            }));

        var client = CreateInferenceClient(handler);

        var result = await client.ListModelsAsync();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Inference.ServiceError");
    }

    [Fact(DisplayName = "ListModelsAsync: OperationCanceledException returns RequestTimeout")]
    public async Task ListModelsAsync_OperationCanceled_ReturnsRequestTimeout()
    {
        var handler = new MockHttpMessageHandler(_ =>
            throw new OperationCanceledException());

        var client = CreateInferenceClient(handler);

        var result = await client.ListModelsAsync();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Inference.RequestTimeout");
    }

    [Fact(DisplayName = "ListModelsAsync: HttpRequestException returns CommunicationFailed")]
    public async Task ListModelsAsync_NetworkFailure_ReturnsCommunicationFailed()
    {
        var handler = new MockHttpMessageHandler(_ =>
            throw new HttpRequestException("Connection refused"));

        var client = CreateInferenceClient(handler);

        var result = await client.ListModelsAsync();

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "Inference.CommunicationFailed");
    }

    private static InferenceClient CreateInferenceClient(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://inference")
        };
        return new InferenceClient(httpClient);
    }

    private sealed class MockHttpMessageHandler : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            return await _handler(request);
        }
    }
}
