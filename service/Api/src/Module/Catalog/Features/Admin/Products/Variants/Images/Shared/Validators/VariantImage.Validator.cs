using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Validators;

/// <summary>
/// Shared FluentValidation rules for variant image request models.
/// Composable via ApplyVariantImageParametersRules extension.
/// </summary>
public static class VariantImageValidator
{
    /// <summary>
    /// Validates common image metadata fields: alt text, position, and type classification.
    /// </summary>
    public sealed class VariantImageParametersValidator : AbstractValidator<VariantImageParameters>
    {
        public VariantImageParametersValidator()
        {
            // Validate: Alt text length must not exceed domain constraint
            RuleFor(x => x.Alt).ApplyAltRules();
            // Validate: Position must be non-negative
            RuleFor(x => x.Position).ApplyPositionRules();

            // Validate: Type must be a valid VariantImageType enum value when provided
            When(x => !string.IsNullOrEmpty(x.Type), () =>
            {
                RuleFor(x => x.Type)
                    .Must(x => Enum.TryParse<VariantImageType>(x, ignoreCase: true, out _))
                    .WithErrorCode(VariantImageResult.Failure.InvalidType.Code)
                    .WithMessage(VariantImageResult.Failure.InvalidType.Message);
            });
        }
    }

    /// <summary>
    /// Applies VariantImageParameters validation rules to a parent validator chain.
    /// </summary>
    public static IRuleBuilderOptions<T, VariantImageParameters> ApplyVariantImageParametersRules<T>(
        this IRuleBuilder<T, VariantImageParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new VariantImageParametersValidator());
    }

    /// <summary>
    /// Validates upload requests: file presence, size, content type, and metadata.
    /// </summary>
    public sealed class UploadImageRequestValidator : AbstractValidator<UploadImageRequest>
    {
        public UploadImageRequestValidator()
        {
            // Validate: File attachment is mandatory
            RuleFor(x => x.File)
                .NotNull()
                .WithErrorCode(VariantImageResult.Failure.FileRequired.Code)
                .WithMessage(VariantImageResult.Failure.FileRequired.Message);

            When(x => x.File is not null, () =>
            {
                // Validate: File must contain data (non-zero length)
                RuleFor(x => x.File.Length)
                    .GreaterThan(VariantImageConstant.Constraints.Upload.MinFileSizeBytes - 1)
                    .WithErrorCode(VariantImageResult.Failure.FileEmpty.Code)
                    .WithMessage(VariantImageResult.Failure.FileEmpty.Message);

                // Validate: File size must not exceed the configured maximum (10 MB)
                RuleFor(x => x.File.Length)
                    .LessThanOrEqualTo(VariantImageConstant.Constraints.Upload.MaxFileSizeBytes)
                    .WithErrorCode(VariantImageResult.Failure.FileTooLarge.Code)
                    .WithMessage(VariantImageResult.Failure.FileTooLarge.Message);

                // Validate: Content type must be in the allowed image formats list
                RuleFor(x => x.File.ContentType)
                    .Must(x => VariantImageConstant.Constraints.Upload.AllowedContentTypes.Contains(x))
                    .WithErrorCode(VariantImageResult.Failure.InvalidContentType.Code)
                    .WithMessage(x => VariantImageResult.Failure.InvalidContentTypeMessage(x.File.ContentType));
            });

            // Validate: Common image metadata (alt, position, type)
            RuleFor(x => (VariantImageParameters)x)
                .ApplyVariantImageParametersRules();
        }
    }

    /// <summary>
    /// Validates update requests: applies common metadata rules only (no file).
    /// </summary>
    public sealed class UpdateImageRequestValidator : AbstractValidator<UpdateImageRequest>
    {
        public UpdateImageRequestValidator()
        {
            // Validate: Common image metadata (alt, position, type)
            RuleFor(x => (VariantImageParameters)x)
                .ApplyVariantImageParametersRules();
        }
    }
}
