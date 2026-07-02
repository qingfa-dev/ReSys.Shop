namespace Module.Profile.Domain.Addresses;

public static class AddressesValidation
{
    // Validate: Address name fields (first name, last name) — required first name with length check
    public static IRuleBuilderOptions<T, string?> ApplyAddressNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(AddressResult.Failure.FirstNameRequired.Code)
            .WithMessage(AddressResult.Failure.FirstNameRequired.Message)
            .MaximumLength(AddressConstant.Constraints.MaxFirstNameLength)
            .WithErrorCode(AddressResult.Failure.FirstNameTooLong.Code)
            .WithMessage(AddressResult.Failure.FirstNameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAddress1Rules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(AddressResult.Failure.Address1Required.Code)
            .WithMessage(AddressResult.Failure.Address1Required.Message)
            .MaximumLength(AddressConstant.Constraints.MaxAddress1Length)
            .WithErrorCode(AddressResult.Failure.Address1TooLong.Code)
            .WithMessage(AddressResult.Failure.Address1TooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAddress2Rules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(AddressConstant.Constraints.MaxAddress2Length)
            .WithErrorCode(AddressResult.Failure.Address2TooLong.Code)
            .WithMessage(AddressResult.Failure.Address2TooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAddressCityRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(AddressResult.Failure.CityRequired.Code)
            .WithMessage(AddressResult.Failure.CityRequired.Message)
            .MaximumLength(AddressConstant.Constraints.MaxCityLength)
            .WithErrorCode(AddressResult.Failure.CityTooLong.Code)
            .WithMessage(AddressResult.Failure.CityTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAddressCountryCodeRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(AddressConstant.Constraints.MaxCountryCodeLength)
            .WithErrorCode(AddressResult.Failure.CountryCodeTooLong.Code)
            .WithMessage(AddressResult.Failure.CountryCodeTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAddressCountryNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(AddressResult.Failure.CountryNameRequired.Code)
            .WithMessage(AddressResult.Failure.CountryNameRequired.Message)
            .MaximumLength(AddressConstant.Constraints.MaxCountryNameLength)
            .WithErrorCode(AddressResult.Failure.CountryNameTooLong.Code)
            .WithMessage(AddressResult.Failure.CountryNameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAddressFirstNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true)
    {
        if (isRequired)
        {
            return ruleBuilder
                .NotEmpty()
                .WithErrorCode(AddressResult.Failure.FirstNameRequired.Code)
                .WithMessage(AddressResult.Failure.FirstNameRequired.Message)
                .MaximumLength(AddressConstant.Constraints.MaxFirstNameLength)
                .WithErrorCode(AddressResult.Failure.FirstNameTooLong.Code)
                .WithMessage(AddressResult.Failure.FirstNameTooLong.Message);
        }

        return ruleBuilder
            .MaximumLength(AddressConstant.Constraints.MaxFirstNameLength)
            .WithErrorCode(AddressResult.Failure.FirstNameTooLong.Code)
            .WithMessage(AddressResult.Failure.FirstNameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAddressLabelRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(AddressConstant.Constraints.MaxLabelLength)
            .WithErrorCode(AddressResult.Failure.LabelTooLong.Code)
            .WithMessage(AddressResult.Failure.LabelTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAddressLastNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool isRequired = true)
    {
        if (isRequired)
        {
            return ruleBuilder
                .NotEmpty()
                .WithErrorCode(AddressResult.Failure.LastNameRequired.Code)
                .WithMessage(AddressResult.Failure.LastNameRequired.Message)
                .MaximumLength(AddressConstant.Constraints.MaxLastNameLength)
                .WithErrorCode(AddressResult.Failure.LastNameTooLong.Code)
                .WithMessage(AddressResult.Failure.LastNameTooLong.Message);
        }

        return ruleBuilder
            .MaximumLength(AddressConstant.Constraints.MaxLastNameLength)
            .WithErrorCode(AddressResult.Failure.LastNameTooLong.Code)
            .WithMessage(AddressResult.Failure.LastNameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAddressPhoneRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(AddressConstant.Constraints.MaxPhoneLength)
            .WithErrorCode(AddressResult.Failure.PhoneTooLong.Code)
            .WithMessage(AddressResult.Failure.PhoneTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAddressStateCodeRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(AddressConstant.Constraints.MaxStateCodeLength)
            .WithErrorCode(AddressResult.Failure.StateCodeTooLong.Code)
            .WithMessage(AddressResult.Failure.StateCodeTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAddressStateProvinceRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(AddressConstant.Constraints.MaxStateProvinceLength)
            .WithErrorCode(AddressResult.Failure.StateProvinceTooLong.Code)
            .WithMessage(AddressResult.Failure.StateProvinceTooLong.Message);
    }

    public static IRuleBuilderOptions<T, AddressType> ApplyAddressTypeRules<T>(
        this IRuleBuilder<T, AddressType> ruleBuilder,
        bool isRequired = true)
    {
        if (isRequired)
        {
            return ruleBuilder
                .NotEmpty()
                .WithErrorCode(AddressResult.Failure.AddressTypeRequired.Code)
                .WithMessage(AddressResult.Failure.AddressTypeRequired.Message)
                .IsInEnum()
                .WithErrorCode(AddressResult.Failure.AddressTypeInvalid.Code)
                .WithMessage(AddressResult.Failure.AddressTypeInvalid.Message);
        }

        return ruleBuilder
            .IsInEnum()
            .WithErrorCode(AddressResult.Failure.AddressTypeInvalid.Code)
            .WithMessage(AddressResult.Failure.AddressTypeInvalid.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAddressZipCodeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(AddressConstant.Constraints.MaxZipCodeLength)
            .WithErrorCode(AddressResult.Failure.ZipCodeTooLong.Code)
            .WithMessage(AddressResult.Failure.ZipCodeTooLong.Message);
    }
}