namespace Module.Inventory.Domain.StockLocations;

public static class StockLocationValidation
{
    public static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(StockLocationResult.Errors.NameRequired.Code)
            .WithMessage(StockLocationResult.Errors.NameRequired.Message)
            .MaximumLength(StockLocationConstant.Constraints.NameMaxLength)
            .WithErrorCode(StockLocationResult.Errors.NameTooLong.Code)
            .WithMessage(StockLocationResult.Errors.NameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCodeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.CodeMaxLength)
            .WithErrorCode(StockLocationResult.Errors.CodeTooLong.Code)
            .WithMessage(StockLocationResult.Errors.CodeTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAddressRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.AddressMaxLength)
            .WithErrorCode(StockLocationResult.Errors.AddressTooLong.Code)
            .WithMessage(StockLocationResult.Errors.AddressTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyCityRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.CityMaxLength)
            .WithErrorCode(StockLocationResult.Errors.CityTooLong.Code)
            .WithMessage(StockLocationResult.Errors.CityTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyPhoneRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.PhoneMaxLength)
            .WithErrorCode(StockLocationResult.Errors.PhoneTooLong.Code)
            .WithMessage(StockLocationResult.Errors.PhoneTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyPostalCodeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.PostalCodeMaxLength)
            .WithErrorCode(StockLocationResult.Errors.PostalCodeTooLong.Code)
            .WithMessage(StockLocationResult.Errors.PostalCodeTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyAdminNameRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.AdminNameMaxLength)
            .WithErrorCode(StockLocationResult.Errors.AdminNameTooLong.Code)
            .WithMessage(StockLocationResult.Errors.AdminNameTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyPresentationRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.PresentationMaxLength)
            .WithErrorCode(StockLocationResult.Errors.PresentationTooLong.Code)
            .WithMessage(StockLocationResult.Errors.PresentationTooLong.Message);
    }
}
