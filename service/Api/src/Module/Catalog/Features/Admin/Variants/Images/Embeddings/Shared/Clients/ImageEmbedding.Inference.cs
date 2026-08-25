using System.Net.Http.Json;
using System.Text.Json;

using Module.Catalog.Domain.Variants.Images.Embeddings;

namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Clients;

/// <summary>HTTP client for the external image embedding inference service (FastAPI ML sidecar).</summary>
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

    /// <summary>Sends an image URL to the inference service for embedding generation.</summary>
    /// <param name="request">The embedding request containing the image URL and model name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the embedding response or an error.</returns>
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

    /// <summary>Sends raw image bytes as a multipart upload to the inference service for embedding generation.</summary>
    /// <param name="imageBytes">Raw image byte data.</param>
    /// <param name="contentType">MIME content type of the image.</param>
    /// <param name="modelName">Optional model name override.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the embedding response or an error.</returns>
    public async Task<Result<EmbeddingResponse>> CreateEmbeddingFromBytesAsync(byte[] imageBytes, string contentType, string? modelName = null, CancellationToken ct = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(imageContent, "image", "upload");

            var requestUri = "/embeddings/bytes";
            if (!string.IsNullOrEmpty(modelName))
                requestUri += $"?model_name={Uri.EscapeDataString(modelName)}";

            var response = await _httpClient.PostAsync(requestUri, content, ct);
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

    /// <summary>Lists available embedding models from the inference service.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the list of available model metadata.</returns>
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