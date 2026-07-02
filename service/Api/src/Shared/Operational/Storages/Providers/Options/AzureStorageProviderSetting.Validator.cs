using FluentValidation;

namespace Shared.Operational.Storages.Providers.Options;

public sealed class AzureStorageProviderSettingValidator : AbstractValidator<AzureStorageProviderSetting>
{
    public AzureStorageProviderSettingValidator()
    {
        When(x => x.IsEnabled, () =>
        {
            RuleFor(x => x.ConnectionString)
                .NotEmpty()
                .WithErrorCode(AzureStorageProviderResult.Failure.ConnectionStringRequired.Code)
                .WithMessage(AzureStorageProviderResult.Failure.ConnectionStringRequired.Message);

            RuleFor(x => x.ContainerName)
                .NotEmpty()
                .WithErrorCode(AzureStorageProviderResult.Failure.ContainerNameRequired.Code)
                .WithMessage(AzureStorageProviderResult.Failure.ContainerNameRequired.Message)
                .Must(BeValidContainerName)
                .WithErrorCode(AzureStorageProviderResult.Failure.ContainerNameInvalid.Code)
                .WithMessage(AzureStorageProviderResult.Failure.ContainerNameInvalid.Message);

            RuleFor(x => x.BufferSize)
                .GreaterThanOrEqualTo(AzureStorageProviderConstant.Constraints.BufferSizeMin)
                .WithErrorCode(AzureStorageProviderResult.Failure.BufferSizeInvalid.Code)
                .WithMessage(AzureStorageProviderResult.Failure.BufferSizeInvalid.Message);
        });
    }

    private static bool BeValidContainerName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return true;

        if (name.Length < 3 || name.Length > 63)
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-z0-9]([a-z0-9]|-(?!-))*[a-z0-9]$");
    }
}
