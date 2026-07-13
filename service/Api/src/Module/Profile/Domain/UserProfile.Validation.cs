namespace Module.Profile.Domain;

public static class ProfileValidation
{
    public static IRuleBuilderOptions<T, string?> ApplyFirstNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true)
    {
        if (isRequired)
        {
            return ruleBuilder
                .NotEmpty()
                .WithErrorCode(UserProfileResult.Failure.FirstNameRequired.Code)
                .WithMessage(UserProfileResult.Failure.FirstNameRequired.Message)
                .MaximumLength(UserProfileConstant.Constraints.MaxFirstNameLength)
                .WithErrorCode(UserProfileResult.Failure.FirstNameTooLong.Code)
                .WithMessage(UserProfileResult.Failure.FirstNameTooLong.Message);
        }

        return ruleBuilder
            .MaximumLength(UserProfileConstant.Constraints.MaxFirstNameLength)
            .WithErrorCode(UserProfileResult.Failure.FirstNameTooLong.Code)
            .WithMessage(UserProfileResult.Failure.FirstNameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyLastNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true)
    {
        if (isRequired)
        {
            return ruleBuilder
                .NotEmpty()
                .WithErrorCode(UserProfileResult.Failure.LastNameRequired.Code)
                .WithMessage(UserProfileResult.Failure.LastNameRequired.Message)
                .MaximumLength(UserProfileConstant.Constraints.MaxLastNameLength)
                .WithErrorCode(UserProfileResult.Failure.LastNameTooLong.Code)
                .WithMessage(UserProfileResult.Failure.LastNameTooLong.Message);
        }

        return ruleBuilder
            .MaximumLength(UserProfileConstant.Constraints.MaxLastNameLength)
            .WithErrorCode(UserProfileResult.Failure.LastNameTooLong.Code)
            .WithMessage(UserProfileResult.Failure.LastNameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, DateTimeOffset?> ApplyDateOfBirthRules<T>(
        this IRuleBuilder<T, DateTimeOffset?> ruleBuilder,
        ISystemDateTime systemDateTime)
    {
        var today = systemDateTime.UtcNow;
        var minDate = today.AddYears(-120);

        return ruleBuilder
            .LessThanOrEqualTo(today)
            .WithErrorCode(UserProfileResult.Failure.DateOfBirthFuture.Code)
            .WithMessage(UserProfileResult.Failure.DateOfBirthFuture.Message)
            .GreaterThanOrEqualTo(minDate)
            .WithErrorCode(UserProfileResult.Failure.DateOfBirthTooOld.Code)
            .WithMessage(UserProfileResult.Failure.DateOfBirthTooOld.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyEmailRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true)
    {
        if (isRequired)
        {
            return ruleBuilder
                .NotEmpty()
                .WithErrorCode(UserProfileResult.Failure.EmailRequired.Code)
                .WithMessage(UserProfileResult.Failure.EmailRequired.Message)
                .MaximumLength(UserProfileConstant.Constraints.MaxEmailLength)
                .WithErrorCode(UserProfileResult.Failure.EmailTooLong.Code)
                .WithMessage(UserProfileResult.Failure.EmailTooLong.Message);
        }

        return ruleBuilder
            .MaximumLength(UserProfileConstant.Constraints.MaxEmailLength)
            .WithErrorCode(UserProfileResult.Failure.EmailTooLong.Code)
            .WithMessage(UserProfileResult.Failure.EmailTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyPhoneNumberRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(UserProfileConstant.Constraints.MaxPhoneNumberLength)
            .WithErrorCode(UserProfileResult.Failure.PhoneNumberTooLong.Code)
            .WithMessage(UserProfileResult.Failure.PhoneNumberTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyGenderRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(UserProfileConstant.Constraints.MaxGenderLength)
            .WithErrorCode(UserProfileResult.Failure.GenderTooLong.Code)
            .WithMessage(UserProfileResult.Failure.GenderTooLong.Message)
            .Must(g => g == null || UserProfileConstant.AllowedGenders.Values.Contains(g, StringComparer.OrdinalIgnoreCase))
            .WithErrorCode(UserProfileResult.Failure.InvalidGender.Code)
            .WithMessage(UserProfileResult.Failure.InvalidGender.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyBioRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(UserProfileConstant.Constraints.MaxBioLength)
            .WithErrorCode(UserProfileResult.Failure.BioTooLong.Code)
            .WithMessage(UserProfileResult.Failure.BioTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAvatarUrlRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(UserProfileConstant.Constraints.MaxAvatarUrlLength)
            .WithErrorCode(UserProfileResult.Failure.AvatarUrlTooLong.Code)
            .WithMessage(UserProfileResult.Failure.AvatarUrlTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyInternalNoteHtmlRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(UserProfileConstant.Constraints.MaxInternalNoteLength)
            .WithErrorCode(UserProfileResult.Failure.InternalNoteTooLong.Code)
            .WithMessage(UserProfileResult.Failure.InternalNoteTooLong.Message);
    }

}