using System.Text.RegularExpressions;

using FluentValidation;

namespace Shared.Security.Identity.Domain.Users;

public enum CredentialType
{
    Username,
    Email,
    Phone
}

public static partial class UserValidation
{
    public static IRuleBuilderOptions<T, string?> ApplyUserCredentialRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        CredentialType? credentialType = null,
        bool required = true)
    {
        if (required)
        {
            IRuleBuilderOptions<T, string?> builder = ruleBuilder.NotEmpty();

            (string? errorCode, string? errorMessage) = GetRequiredError(credentialType);
            builder = builder
                .WithErrorCode(errorCode)
                .WithMessage(errorMessage);

            if (credentialType.HasValue)
            {
                return ApplyTypeSpecificRules(builder, credentialType.Value);
            }

            return builder
                .Must(value => string.IsNullOrEmpty(value) || ValidateCredentialAuto(value))
                .WithErrorCode(UserResult.Failure.CredentialInvalid.Code)
                .WithMessage(UserResult.Failure.CredentialInvalid.Message);
        }

        if (credentialType.HasValue)
        {
            return credentialType.Value switch
            {
                CredentialType.Username => ruleBuilder
                    .Must(value => string.IsNullOrEmpty(value) || IsValidUsername(value))
                    .WithErrorCode(UserResult.Failure.UsernameInvalid.Code)
                    .WithMessage(UserResult.Failure.UsernameInvalid.Message),

                CredentialType.Email => ruleBuilder
                    .Must(value => string.IsNullOrEmpty(value) || Regex.IsMatch(value, UserConstant.Patterns.Email.Regex))
                    .WithErrorCode(UserResult.Failure.EmailInvalid.Code)
                    .WithMessage(UserResult.Failure.EmailInvalid.Message),

                CredentialType.Phone => ruleBuilder
                    .Must(value => string.IsNullOrEmpty(value) || Regex.IsMatch(value, UserConstant.Patterns.Phone.Regex))
                    .WithErrorCode(UserResult.Failure.PhoneInvalid.Code)
                    .WithMessage(UserResult.Failure.PhoneInvalid.Message),

                _ => throw new ArgumentOutOfRangeException(nameof(credentialType))
            };
        }

        return ruleBuilder
            .Must(value => string.IsNullOrEmpty(value) || ValidateCredentialAuto(value))
            .WithErrorCode(UserResult.Failure.CredentialInvalid.Code)
            .WithMessage(UserResult.Failure.CredentialInvalid.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyUserEmailRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool requireEmailFormat = true)
    {
        IRuleBuilderOptions<T, string> builder = ruleBuilder
            .NotEmpty()
            .WithErrorCode(UserResult.Failure.EmailRequired.Code)
            .WithMessage(UserResult.Failure.EmailRequired.Message)
            .MaximumLength(UserConstant.Constraints.Email.MaxLength)
            .WithErrorCode(UserResult.Failure.EmailTooLong.Code)
            .WithMessage(UserResult.Failure.EmailTooLong.Message);

        if (requireEmailFormat)
        {
            builder = builder
                .EmailAddress()
                .WithErrorCode(UserResult.Failure.EmailInvalid.Code)
                .WithMessage(UserResult.Failure.EmailInvalid.Message);
        }

        return builder;
    }

    public static IRuleBuilderOptions<T, string?> ApplyUserFirstNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true)
    {
        if (isRequired)
        {
            return ruleBuilder
                .NotEmpty()
                .WithErrorCode(UserResult.Failure.FirstNameRequired.Code)
                .WithMessage(UserResult.Failure.FirstNameRequired.Message)
                .MaximumLength(UserConstant.Constraints.Name.MaxFirstNameLength)
                .WithErrorCode(UserResult.Failure.FirstNameTooLong.Code)
                .WithMessage(UserResult.Failure.FirstNameTooLong.Message);
        }

        return ruleBuilder
            .MaximumLength(UserConstant.Constraints.Name.MaxFirstNameLength)
            .WithErrorCode(UserResult.Failure.FirstNameTooLong.Code)
            .WithMessage(UserResult.Failure.FirstNameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyUserLastNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true)
    {
        if (isRequired)
        {
            return ruleBuilder
                .NotEmpty()
                .WithErrorCode(UserResult.Failure.LastNameRequired.Code)
                .WithMessage(UserResult.Failure.LastNameRequired.Message)
                .MaximumLength(UserConstant.Constraints.Name.MaxLastNameLength)
                .WithErrorCode(UserResult.Failure.LastNameTooLong.Code)
                .WithMessage(UserResult.Failure.LastNameTooLong.Message);
        }

        return ruleBuilder
            .MaximumLength(UserConstant.Constraints.Name.MaxLastNameLength)
            .WithErrorCode(UserResult.Failure.LastNameTooLong.Code)
            .WithMessage(UserResult.Failure.LastNameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyUserOtpRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool requireExactLength = false)
    {
        IRuleBuilderOptions<T, string> builder = ruleBuilder
            .NotEmpty()
            .WithErrorCode(UserResult.Failure.OtpRequired.Code)
            .WithMessage(UserResult.Failure.OtpRequired.Message)
            .MaximumLength(UserConstant.Constraints.Otp.MaxLength)
            .WithErrorCode(UserResult.Failure.OtpTooLong.Code)
            .WithMessage(UserResult.Failure.OtpTooLong.Message);

        if (requireExactLength)
        {
            builder = builder
                .MinimumLength(UserConstant.Constraints.Otp.MinLength)
                .WithErrorCode(UserResult.Failure.OtpTooShort.Code)
                .WithMessage(UserResult.Failure.OtpTooShort.Message);
        }
        else
        {
            builder = builder
                .MinimumLength(UserConstant.Constraints.Otp.MinLength)
                .WithErrorCode(UserResult.Failure.OtpTooShort.Code)
                .WithMessage(UserResult.Failure.OtpTooShort.Message);
        }

        builder = builder
            .Matches(UserConstant.Patterns.Otp.Regex)
            .WithErrorCode(UserResult.Failure.OtpInvalid.Code)
            .WithMessage(UserResult.Failure.OtpInvalid.Message);

        return builder;
    }

    public static IRuleBuilderOptions<T, string?> ApplyUserPasswordRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool requireMinLength = true,
        bool requireStrongPassword = false)
    {
        IRuleBuilderOptions<T, string> builder = ruleBuilder
            .NotEmpty()
            .WithErrorCode(UserResult.Failure.PasswordRequired.Code)
            .WithMessage(UserResult.Failure.PasswordRequired.Message)
            .MaximumLength(UserConstant.Constraints.Password.MaxLength)
            .WithErrorCode(UserResult.Failure.PasswordTooLong.Code)
            .WithMessage(UserResult.Failure.PasswordTooLong.Message);

        if (requireMinLength)
        {
            builder = builder
                .MinimumLength(UserConstant.Constraints.Password.MinLength)
                .WithErrorCode(UserResult.Failure.PasswordTooShort.Code)
                .WithMessage(UserResult.Failure.PasswordTooShort.Message);
        }

        if (requireStrongPassword)
        {
            builder = builder
                .Must(BeStrongPassword)
                .WithErrorCode(UserResult.Failure.PasswordTooWeak.Code)
                .WithMessage(UserResult.Failure.PasswordTooWeak.Message);
        }

        return builder;
    }

    public static IRuleBuilderOptions<T, string?> ApplyUserPhoneRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool requirePhoneFormat = true)
    {
        IRuleBuilderOptions<T, string> builder = ruleBuilder
            .NotEmpty()
            .WithErrorCode(UserResult.Failure.PhoneRequired.Code)
            .WithMessage(UserResult.Failure.PhoneRequired.Message)
            .MaximumLength(UserConstant.Constraints.Phone.MaxLength)
            .WithErrorCode(UserResult.Failure.PhoneTooLong.Code)
            .WithMessage(UserResult.Failure.PhoneTooLong.Message);

        if (requirePhoneFormat)
        {
            builder = builder
                .Matches(UserConstant.Patterns.Phone.Regex)
                .WithErrorCode(UserResult.Failure.PhoneInvalid.Code)
                .WithMessage(UserResult.Failure.PhoneInvalid.Message);
        }

        return builder;
    }

    public static IRuleBuilderOptions<T, string?> ApplyUserTokenRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(UserResult.Failure.TokenRequired.Code)
            .WithMessage(UserResult.Failure.TokenRequired.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyUsernameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool required = true)
    {
        if (required)
        {
            return ruleBuilder
                .NotEmpty()
                .WithErrorCode(UserResult.Failure.UsernameRequired.Code)
                .WithMessage(UserResult.Failure.UsernameRequired.Message)
                .MinimumLength(UserConstant.Constraints.Username.MinLength)
                .WithErrorCode(UserResult.Failure.UsernameTooShort.Code)
                .WithMessage(UserResult.Failure.UsernameTooShort.Message)
                .MaximumLength(UserConstant.Constraints.Username.MaxLength)
                .WithErrorCode(UserResult.Failure.UsernameTooLong.Code)
                .WithMessage(UserResult.Failure.UsernameTooLong.Message)
                .Matches(UserConstant.Patterns.Username.Regex)
                .WithErrorCode(UserResult.Failure.UsernameInvalid.Code)
                .WithMessage(UserResult.Failure.UsernameInvalid.Message);
        }

        return ruleBuilder
            .Must(value => string.IsNullOrEmpty(value) || IsValidUsername(value))
            .WithErrorCode(UserResult.Failure.UsernameInvalid.Code)
            .WithMessage(UserResult.Failure.UsernameInvalid.Message);
    }

    private static (string code, string message) GetRequiredError(CredentialType? credentialType)
    {
        return credentialType switch
        {
            CredentialType.Username => (UserResult.Failure.UsernameRequired.Code, UserResult.Failure.UsernameRequired.Message),
            CredentialType.Email => (UserResult.Failure.EmailRequired.Code, UserResult.Failure.EmailRequired.Message),
            CredentialType.Phone => (UserResult.Failure.PhoneRequired.Code, UserResult.Failure.PhoneRequired.Message),
            _ => (UserResult.Failure.CredentialRequired.Code, UserResult.Failure.CredentialRequired.Message)
        };
    }

    private static IRuleBuilderOptions<T, string?> ApplyTypeSpecificRules<T>(
        IRuleBuilder<T, string?> builder,
        CredentialType credentialType)
    {
        return credentialType switch
        {
            CredentialType.Username => builder
                .MinimumLength(UserConstant.Constraints.Username.MinLength)
                .WithErrorCode(UserResult.Failure.UsernameTooShort.Code)
                .WithMessage(UserResult.Failure.UsernameTooShort.Message)
                .MaximumLength(UserConstant.Constraints.Username.MaxLength)
                .WithErrorCode(UserResult.Failure.UsernameTooLong.Code)
                .WithMessage(UserResult.Failure.UsernameTooLong.Message)
                .Matches(UserConstant.Patterns.Username.Regex)
                .WithErrorCode(UserResult.Failure.UsernameInvalid.Code)
                .WithMessage(UserResult.Failure.UsernameInvalid.Message),

            CredentialType.Email => builder
                .MaximumLength(UserConstant.Constraints.Email.MaxLength)
                .WithErrorCode(UserResult.Failure.EmailTooLong.Code)
                .WithMessage(UserResult.Failure.EmailTooLong.Message)
                .EmailAddress()
                .WithErrorCode(UserResult.Failure.EmailInvalid.Code)
                .WithMessage(UserResult.Failure.EmailInvalid.Message),

            CredentialType.Phone => builder
                .MaximumLength(UserConstant.Constraints.Phone.MaxLength)
                .WithErrorCode(UserResult.Failure.PhoneTooLong.Code)
                .WithMessage(UserResult.Failure.PhoneTooLong.Message)
                .Matches(UserConstant.Patterns.Phone.Regex)
                .WithErrorCode(UserResult.Failure.PhoneInvalid.Code)
                .WithMessage(UserResult.Failure.PhoneInvalid.Message),

            _ => throw new ArgumentOutOfRangeException(nameof(credentialType))
        };
    }

    private static bool ValidateCredentialAuto(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        CredentialType detectedType = DetectCredentialType(value);
        return ValidateCredential(value, detectedType);
    }

    private static CredentialType DetectCredentialType(string value)
    {
        if (Regex.IsMatch(value, UserConstant.Patterns.Email.Regex))
            return CredentialType.Email;

        if (Regex.IsMatch(value, UserConstant.Patterns.Phone.Regex))
            return CredentialType.Phone;

        return CredentialType.Username;
    }

    private static bool ValidateCredential(string value,
        CredentialType credentialType)
    {
        return credentialType switch
        {
            CredentialType.Username => Regex.IsMatch(value, UserConstant.Patterns.Username.Regex),
            CredentialType.Phone => Regex.IsMatch(value, UserConstant.Patterns.Phone.Regex),
            CredentialType.Email => Regex.IsMatch(value, UserConstant.Patterns.Email.Regex),
            _ => false
        };
    }

    private static bool BeStrongPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        return Regex.IsMatch(password, UserConstant.Patterns.Password.Regex)
            && Regex.IsMatch(password, UserConstant.Patterns.Password.Lowercase)
            && Regex.IsMatch(password, UserConstant.Patterns.Password.Digit)
            && Regex.IsMatch(password, UserConstant.Patterns.Password.SpecialChar);
    }

    public static bool IsValidEmail(string? value)
    {
        return !string.IsNullOrEmpty(value) && Regex.IsMatch(value, UserConstant.Patterns.Email.Regex);
    }

    public static bool IsValidPhone(string? value)
    {
        return !string.IsNullOrEmpty(value) && Regex.IsMatch(value, UserConstant.Patterns.Phone.Regex);
    }

    public static bool IsValidUsername(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        if (value.Length < UserConstant.Constraints.Username.MinLength)
            return false;

        if (value.Length > UserConstant.Constraints.Username.MaxLength)
            return false;

        return Regex.IsMatch(value, UserConstant.Patterns.Username.Regex);
    }
}
