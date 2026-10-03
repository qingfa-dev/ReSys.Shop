namespace Module.Catalog.Domain.Variants.Images;

public static class VariantImageConstant
{
    public static class Constraints
    {
        public const int UrlMaxLength = 2048;
        public const int StoragePathMaxLength = 500;
        public const int AltMaxLength = 500;
        public const int ContentTypeMaxLength = 100;
        public const int FileNameMaxLength = 255;
        public const int TypeMaxLength = 50;

        public const int MinPosition = 0;
        public const int MinDimension = 1;
        public const int MaxDimension = 10000;
        public const int DimensionsUnitMaxLength = 10;

        public static class Dimensions
        {
            public static readonly string[] AllowedUnits = ["px", "in", "cm"];
        }

        public static class Upload
        {
            public const long MinFileSizeBytes = 1;
            public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
            public static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];
            public static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
        }
    }

    public static class Defaults
    {
        public const string DefaultImageAlt = "Product image";
        public const string DefaultContentType = "image/jpeg";
        public const string DimensionsUnit = "px";
        public const int Position = 0;

        public const string DefaultEmbeddingModel = AIModels.FashionClip;
        public const string DefaultSimilarityModel = AIModels.FashionClip;
    }

    /// <summary>
    /// Canonical model identifiers — snake_case slugs matching the benchmark registry.
    /// No metadata here — only identity strings.
    /// </summary>
    public static class AIModels
    {
        // Multimodal (CLIP family)
        public const string ClipB32 = "clip_b32";
        public const string ClipL14 = "clip_l14";
        public const string ClipViTB16 = "clip_vit_b16";
        public const string ClipGeneric = "clip_generic";

        // Fashion-specific
        public const string FashionClip = "fashion_clip";

        // Visual similarity
        public const string DinoV2ViTS14 = "dinov2_vits14";

        // CNN
        public const string ConvNextTiny = "convnext_tiny";
        public const string EfficientNetB0 = "efficientnet_b0";
        public const string ResNet50 = "resnet50";

        // CLIP variants
        public const string EvaClip = "eva_clip";
        public const string SigLIP = "siglip";
    }

    /// <summary>
    /// Field metadata for searching, sorting, and filtering.
    /// </summary>
    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(VariantImage.FileName),
            nameof(VariantImage.Alt)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(VariantImage.Position),
            nameof(VariantImage.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(VariantImage.Type),
            nameof(VariantImage.ContentType),
            nameof(VariantImage.DimensionsUnit)
        ];
    }
}