namespace Module.Catalog.Domain.Products.Variants.Images;

public enum VariantImageType
{
    Default, // Primary display image
    Thumbnail, // Small preview
    Square, // Fixed 1:1 aspect ratio
    Gallery, // High-resolution detail view
    Search // AI semantic search source
}