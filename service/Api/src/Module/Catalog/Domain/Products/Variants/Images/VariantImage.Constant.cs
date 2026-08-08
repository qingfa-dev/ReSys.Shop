namespace Module.Catalog.Domain.Products.Variants.Images;

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
    /// Canonical model identifiers used across the system.
    /// No metadata here — only identity strings.
    /// </summary>
    public static class AIModels
    {
        // Multimodal
        public const string OpenClipB32 = "openclip-vit-b-32";
        public const string OpenClipL14 = "openclip-vit-l-14";
        public const string SigLipBase = "siglip-vit-b-16";

        // Fashion-specific
        public const string FashionClip = "fashion-clip";
        public const string DeepFashion = "deepfashion-embed-v2";

        // Visual similarity
        public const string DinoV2Small = "dinov2_vits14";
        public const string DinoV2Base = "dinov2-vit-base";
        public const string Ibot = "ibot-vit-base";
        public const string SwinBase = "swin-base";

        // Edge / fast
        public const string ConvNextTiny = "convnext-v2-tiny";
        public const string EfficientNetB0 = "efficientnet_b0";
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