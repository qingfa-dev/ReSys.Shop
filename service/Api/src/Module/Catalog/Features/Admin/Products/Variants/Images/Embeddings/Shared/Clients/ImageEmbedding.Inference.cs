using System.Net.Http.Json;
using System.Text.Json;

using Module.Catalog.Domain.Products.Variants.Images.Embeddings;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;

public class InferenceClient : IInferenceClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public InferenceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<EmbeddingResponse>> CreateEmbeddingAsync(EmbeddingRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/embeddings", request, JsonOptions, ct);
            return await DeserializeResultAsync<EmbeddingResponse>(response, ct);
        }
        catch (OperationCanceledException)
        {
            return ImageEmbeddingResult.Errors.RequestTimeout;
        }
        catch (Exception ex)
        {
            return Result<EmbeddingResponse>.Unexpected(
                exception: ex,
                errors: [ImageEmbeddingResult.Errors.CommunicationFailed(ex.Message)]);
        }
    }

    public async Task<Result<EmbeddingResponse>> CreateEmbeddingFromBytesAsync(byte[] imageBytes, string contentType, string? model = null, CancellationToken ct = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(imageContent, "image", "upload");
            if (!string.IsNullOrEmpty(model))
                content.Add(new StringContent(model), "model");

            var response = await _httpClient.PostAsync("/embeddings/bytes", content, ct);
            return await DeserializeResultAsync<EmbeddingResponse>(response, ct);
        }
        catch (OperationCanceledException)
        {
            return ImageEmbeddingResult.Errors.RequestTimeout;
        }
        catch (Exception ex)
        {
            return Result<EmbeddingResponse>.Unexpected(
                exception: ex,
                errors: [ImageEmbeddingResult.Errors.CommunicationFailed(ex.Message)]);
        }
    }

    public async Task<Result<List<ModelMetadata>>> ListModelsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/models", ct);
            return await DeserializeResultAsync<List<ModelMetadata>>(response, ct);
        }
        catch (OperationCanceledException)
        {
            return ImageEmbeddingResult.Errors.RequestTimeout;
        }
        catch (Exception ex)
        {
            return Result<List<ModelMetadata>>.Unexpected(
                exception: ex,
                errors: [ImageEmbeddingResult.Errors.CommunicationFailed(ex.Message)]);
        }
    }

    private static async Task<Result<T>> DeserializeResultAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            return new Result<T>(false, (int)response.StatusCode, default,
                "Inference service error.",
                [Error.Unexpected("Inference.ServiceError", $"Service returned {(int)response.StatusCode}: {body}")]);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<T>?>(JsonOptions, ct);
        if (result is null)
        {
            return ImageEmbeddingResult.Errors.InvalidResponse;
        }

        return result.Value;
    }
}