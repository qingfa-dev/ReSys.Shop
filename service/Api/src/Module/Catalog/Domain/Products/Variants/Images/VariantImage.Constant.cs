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

        public const string DefaultEmbeddingModel = AIModels.OpenClipB32;
        public const string DefaultSimilarityModel = AIModels.DinoV2Small;
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
        public const string FashionClip = "fashion-clip-v1";
        public const string DeepFashion = "deepfashion-embed-v2";

        // Visual similarity
        public const string DinoV2Small = "dinov2-vit-small";
        public const string DinoV2Base = "dinov2-vit-base";
        public const string Ibot = "ibot-vit-base";
        public const string SwinBase = "swin-base";

        // Edge / fast
        public const string ConvNextTiny = "convnext-v2-tiny";
        public const string EfficientNetB0 = "efficientnet-b0";
    }
}