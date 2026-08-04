namespace Module.Catalog.Domain.Products.Variants.Images.Embeddings;

public static class ImageEmbeddingResult
{
    public static class Success
    {
        /// <summary>Returns a success message for embedding creation.</summary>
        public static string Created(Guid id) => $"Image embedding with ID '{id}' was successfully created.";
        /// <summary>Returns a success message for embedding deletion.</summary>
        public static string Deleted(Guid id) => $"Image embedding with ID '{id}' was successfully deleted.";
    }

    public static class Errors
    {
        #region Validation
        /// <summary>Model name is required.</summary>
        public static Error ModelNameRequired => Error.Validation(
            code: "ImageEmbedding.ModelName.Required",
            message: "Model name is required.");

        /// <summary>Model name exceeds the maximum length.</summary>
        public static Error ModelNameTooLong => Error.Validation(
            code: "ImageEmbedding.ModelName.TooLong",
            message: $"Model name cannot exceed {ImageEmbeddingConstant.Constraints.ModelNameMaxLength} characters.");

        /// <summary>Model version exceeds the maximum length.</summary>
        public static Error ModelVersionTooLong => Error.Validation(
            code: "ImageEmbedding.ModelVersion.TooLong",
            message: $"Model version cannot exceed {ImageEmbeddingConstant.Constraints.ModelVersionMaxLength} characters.");

        /// <summary>Embedding vector is required.</summary>
        public static Error VectorRequired => Error.Validation(
            code: "ImageEmbedding.Vector.Required",
            message: "Embedding vector is required.");
        #endregion

        #region Business
        /// <summary>An embedding with a pending or processing status already exists.</summary>
        public static Error Conflict(Guid variantImageId) => Error.Conflict(
            code: "ImageEmbedding.Conflict",
            message: $"An embedding with a pending or processing status already exists for variant image '{variantImageId}'.");

        /// <summary>Embedding was not found by ID.</summary>
        public static Error NotFound(Guid id) => Error.NotFound(
            code: "ImageEmbedding.NotFound",
            message: $"Embedding with ID '{id}' was not found.");

        /// <summary>No embedding exists for the given variant image.</summary>
        public static Error NotFoundByVariantImage(Guid variantImageId) => Error.NotFound(
            code: "ImageEmbedding.VariantImageNotFound",
            message: $"No embedding exists for variant image '{variantImageId}'.");

        /// <summary>The variant image was deleted before embedding could be persisted.</summary>
        public static Error VariantImageDeletedDuringEmbedding(Guid variantImageId) => Error.NotFound(
            code: "ImageEmbedding.VariantImageDeleted",
            message: $"Variant image with ID '{variantImageId}' was deleted before embedding could be persisted.");
        #endregion

        #region Lifecycle
        /// <summary>The embedding cannot be transitioned between the given statuses.</summary>
        public static Error InvalidStatusTransition(Guid embeddingId, EmbeddingStatus from, EmbeddingStatus to)
            => Error.Validation(
                code: "ImageEmbedding.InvalidStatusTransition",
                message: $"Cannot transition embedding '{embeddingId}' from {from} to {to}.");
        #endregion

        #region Inference
        /// <summary>Request to inference service timed out.</summary>
        public static Error RequestTimeout => Error.Unexpected(
            code: "Inference.RequestTimeout",
            message: "Request to inference service timed out.");

        /// <summary>Failed to communicate with the inference service.</summary>
        public static Error CommunicationFailed(string details) => Error.Unexpected(
            code: "Inference.CommunicationFailed",
            message: "Failed to communicate with the inference service: " + details);

        /// <summary>Inference service returned an error response.</summary>
        public static Error ServiceError(string body, int statusCode) => Error.Unexpected(
            code: "Inference.ServiceError",
            message: "Inference service error: " + body);

        /// <summary>Invalid response received from inference service.</summary>
        public static Error InvalidResponse => Error.Unexpected(
            "Inference.InvalidResponse",
            "Invalid response from inference service.");

        /// <summary>Failed to generate embedding from raw image bytes.</summary>
        public static Error EmbeddingFromBytesFailed(string details) => Error.Unexpected(
            code: "Inference.EmbeddingFromBytesFailed",
            message: "Failed to generate embedding from image bytes: " + details);
        #endregion
    }
}