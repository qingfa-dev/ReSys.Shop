using Pgvector;

namespace Module.Catalog.Domain.Products.Variants.Images.Embeddings;

public static class ImageEmbeddingExtensions
{
    #region Factory Methods
    /// <summary>
    /// Creates a new image embedding from ML model output.
    /// </summary>
    /// <param name="variantImageId">The parent variant image identifier.</param>
    /// <param name="modelName">The name of the ML model that generated the embedding.</param>
    /// <param name="modelVersion">The version of the ML model.</param>
    /// <param name="vectorData">The float array representing the embedding vector.</param>
    /// <returns>The created ImageEmbedding entity.</returns>
    // Contract: pre=variantImageId!=Guid.Empty&&modelName!=null&&vectorData!=null,
    //           post=entity.Vector!=null&&entity.Dimensions==vectorData.Length, throws=ArgumentException
    public static ImageEmbedding Create(
        Guid variantImageId,
        string modelName,
        string modelVersion,
        float[] vectorData)
    {
        return new ImageEmbedding
        {
            Id = Guid.NewGuid(),
            VariantImageId = variantImageId,
            ModelName = modelName,
            ModelVersion = modelVersion,
            Vector = new Vector(vectorData),
            Dimensions = vectorData.Length
        };
    }
    #endregion
}