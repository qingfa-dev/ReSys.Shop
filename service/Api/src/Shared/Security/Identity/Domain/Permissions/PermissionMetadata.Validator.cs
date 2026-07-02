using FluentValidation;

namespace Shared.Security.Identity.Domain.Permissions;

public sealed class PermissionMetadataValidator : AbstractValidator<PermissionMetadata>
{
    public PermissionMetadataValidator()
    {
        RuleFor(x => x.Domain)
            .NotEmpty()
            .WithErrorCode(Code(nameof(PermissionMetadata.Domain), "Required"))
            .WithMessage(Message(nameof(PermissionMetadata.Domain), "is required and cannot be empty."))
            .MaximumLength(PermissionMetadataConstant.Constraints.MaxPartLength)
            .WithErrorCode(Code(nameof(PermissionMetadata.Domain), "TooLong"))
            .WithMessage(MaxLengthMessage(nameof(PermissionMetadata.Domain)))
            .Matches(PermissionMetadataConstant.Patterns.PartRegex)
            .WithErrorCode(Code(nameof(PermissionMetadata.Domain), "InvalidChars"))
            .WithMessage(InvalidCharsMessage(nameof(PermissionMetadata.Domain)));

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithErrorCode(Code(nameof(PermissionMetadata.Category), "Required"))
            .WithMessage(Message(nameof(PermissionMetadata.Category), "is required and cannot be empty."))
            .MaximumLength(PermissionMetadataConstant.Constraints.MaxPartLength)
            .WithErrorCode(Code(nameof(PermissionMetadata.Category), "TooLong"))
            .WithMessage(MaxLengthMessage(nameof(PermissionMetadata.Category)))
            .Matches(PermissionMetadataConstant.Patterns.PartRegex)
            .WithErrorCode(Code(nameof(PermissionMetadata.Category), "InvalidChars"))
            .WithMessage(InvalidCharsMessage(nameof(PermissionMetadata.Category)));

        RuleFor(x => x.Resource)
            .NotEmpty()
            .WithErrorCode(Code(nameof(PermissionMetadata.Resource), "Required"))
            .WithMessage(Message(nameof(PermissionMetadata.Resource), "is required and cannot be empty."))
            .MaximumLength(PermissionMetadataConstant.Constraints.MaxPartLength)
            .WithErrorCode(Code(nameof(PermissionMetadata.Resource), "TooLong"))
            .WithMessage(MaxLengthMessage(nameof(PermissionMetadata.Resource)))
            .Matches(PermissionMetadataConstant.Patterns.PartRegex)
            .WithErrorCode(Code(nameof(PermissionMetadata.Resource), "InvalidChars"))
            .WithMessage(InvalidCharsMessage(nameof(PermissionMetadata.Resource)));

        RuleFor(x => x.Action)
            .NotEmpty()
            .WithErrorCode(Code(nameof(PermissionMetadata.Action), "Required"))
            .WithMessage(Message(nameof(PermissionMetadata.Action), "is required and cannot be empty."))
            .MaximumLength(PermissionMetadataConstant.Constraints.MaxPartLength)
            .WithErrorCode(Code(nameof(PermissionMetadata.Action), "TooLong"))
            .WithMessage(MaxLengthMessage(nameof(PermissionMetadata.Action)))
            .Matches(PermissionMetadataConstant.Patterns.PartRegex)
            .WithErrorCode(Code(nameof(PermissionMetadata.Action), "InvalidChars"))
            .WithMessage(InvalidCharsMessage(nameof(PermissionMetadata.Action)));

        RuleFor(x => x.Name)
            .MaximumLength(PermissionMetadataConstant.Constraints.MaxNameLength)
            .WithErrorCode("Permission.Name.TooLong")
            .WithMessage($"Permission name exceeds maximum length of {PermissionMetadataConstant.Constraints.MaxNameLength}.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(PermissionMetadataConstant.Constraints.MaxDescriptionLength)
            .WithErrorCode("Permission.Description.TooLong")
            .WithMessage($"Permission description exceeds maximum length of {PermissionMetadataConstant.Constraints.MaxDescriptionLength}.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }

    private static string Code(string partName, string suffix) => $"Permission.{partName}.{suffix}";

    private static string Message(string partName, string text) => $"{partName} {text}";

    private static string MaxLengthMessage(string partName) =>
        $"{partName} exceeds maximum length of {PermissionMetadataConstant.Constraints.MaxPartLength}.";

    private static string InvalidCharsMessage(string partName) =>
        $"{partName} contains invalid characters. Allowed: {PermissionMetadataConstant.Constraints.AllowedPartChars}.";
}