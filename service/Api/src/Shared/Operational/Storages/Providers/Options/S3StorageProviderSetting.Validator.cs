using System.Text.RegularExpressions;

using FluentValidation;

namespace Shared.Operational.Storages.Providers.Options;

public sealed class S3StorageProviderSettingValidator : AbstractValidator<S3StorageProviderSetting>
{
    private static readonly Regex BucketNameRegex = new(
        @"^[a-z0-9]([a-z0-9-]*[a-z0-9])?$",
        RegexOptions.Compiled);

    public S3StorageProviderSettingValidator()
    {
        When(x => x.IsEnabled, () =>
        {
            When(x => !string.IsNullOrEmpty(x.ServiceUrl), () =>
            {
                RuleFor(x => x.ServiceUrl)
                    .MaximumLength(S3StorageProviderConstant.Constraints.ServiceUrlMaxLength)
                    .WithErrorCode(S3StorageProviderResult.Failure.ServiceUrlInvalid.Code)
                    .WithMessage(S3StorageProviderResult.Failure.ServiceUrlInvalid.Message)
                    .Must(BeValidUri)
                    .WithErrorCode(S3StorageProviderResult.Failure.ServiceUrlInvalid.Code)
                    .WithMessage(S3StorageProviderResult.Failure.ServiceUrlInvalid.Message);
            });

            RuleFor(x => x.AccessKey)
                .NotEmpty()
                .WithErrorCode(S3StorageProviderResult.Failure.AccessKeyRequired.Code)
                .WithMessage(S3StorageProviderResult.Failure.AccessKeyRequired.Message)
                .MaximumLength(S3StorageProviderConstant.Constraints.AccessKeyMaxLength)
                .WithErrorCode(S3StorageProviderResult.Failure.AccessKeyRequired.Code)
                .WithMessage(S3StorageProviderResult.Failure.AccessKeyRequired.Message);

            RuleFor(x => x.SecretKey)
                .NotEmpty()
                .WithErrorCode(S3StorageProviderResult.Failure.SecretKeyRequired.Code)
                .WithMessage(S3StorageProviderResult.Failure.SecretKeyRequired.Message)
                .MaximumLength(S3StorageProviderConstant.Constraints.SecretKeyMaxLength)
                .WithErrorCode(S3StorageProviderResult.Failure.SecretKeyRequired.Code)
                .WithMessage(S3StorageProviderResult.Failure.SecretKeyRequired.Message);

            RuleFor(x => x.BucketName)
                .NotEmpty()
                .WithErrorCode(S3StorageProviderResult.Failure.BucketNameRequired.Code)
                .WithMessage(S3StorageProviderResult.Failure.BucketNameRequired.Message)
                .Length(S3StorageProviderConstant.Constraints.BucketNameMinLength, S3StorageProviderConstant.Constraints.BucketNameMaxLength)
                .WithErrorCode(S3StorageProviderResult.Failure.BucketNameInvalid.Code)
                .WithMessage(S3StorageProviderResult.Failure.BucketNameInvalid.Message)
                .Must(name => BucketNameRegex.IsMatch(name))
                .WithErrorCode(S3StorageProviderResult.Failure.BucketNameInvalid.Code)
                .WithMessage(S3StorageProviderResult.Failure.BucketNameInvalid.Message);

            RuleFor(x => x.Region)
                .NotEmpty()
                .WithErrorCode(S3StorageProviderResult.Failure.RegionRequired.Code)
                .WithMessage(S3StorageProviderResult.Failure.RegionRequired.Message)
                .MaximumLength(S3StorageProviderConstant.Constraints.RegionMaxLength)
                .WithErrorCode(S3StorageProviderResult.Failure.RegionInvalid.Code)
                .WithMessage(S3StorageProviderResult.Failure.RegionInvalid.Message);

            RuleFor(x => x.BufferSize)
                .GreaterThanOrEqualTo(S3StorageProviderConstant.Constraints.BufferSizeMin)
                .WithErrorCode(S3StorageProviderResult.Failure.BufferSizeInvalid.Code)
                .WithMessage(S3StorageProviderResult.Failure.BufferSizeInvalid.Message);
        });
    }

    private static bool BeValidUri(string? uri)
    {
        return Uri.TryCreate(uri, UriKind.Absolute, out _);
    }
}
