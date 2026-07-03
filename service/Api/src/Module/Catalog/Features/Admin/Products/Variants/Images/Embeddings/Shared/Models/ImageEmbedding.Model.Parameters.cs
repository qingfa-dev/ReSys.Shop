namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Models;

public abstract record ImageEmbeddingParameters(
    string ModelName = "", 
    string ModelVersion = "");
