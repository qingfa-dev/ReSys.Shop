using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.Shared.Models;

namespace Module.Inventory.Features.Admin.Shared.Validators;

public static partial class StockLocationValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyAddressRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.AddressMaxLength)
            .WithErrorCode(StockLocationResult.Errors.AddressTooLong.Code)
            .WithMessage(StockLocationResult.Errors.AddressTooLong.Message);
    }
}

public static partial class StockLocationValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyCityRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.CityMaxLength)
            .WithErrorCode(StockLocationResult.Errors.CityTooLong.Code)
            .WithMessage(StockLocationResult.Errors.CityTooLong.Message);
    }
}

public static partial class StockLocationValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyCodeRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.CodeMaxLength)
            .WithErrorCode(StockLocationResult.Errors.CodeTooLong.Code)
            .WithMessage(StockLocationResult.Errors.CodeTooLong.Message);
    }
}

public static partial class StockLocationValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(StockLocationResult.Errors.NameRequired.Code)
            .WithMessage(StockLocationResult.Errors.NameRequired.Message)
            .MaximumLength(StockLocationConstant.Constraints.NameMaxLength)
            .WithErrorCode(StockLocationResult.Errors.NameTooLong.Code)
            .WithMessage(StockLocationResult.Errors.NameTooLong.Message);
    }
}

public static partial class StockLocationValidator
{
    public sealed class StockLocationParametersValidator : AbstractValidator<StockLocationParameters>
    {
        public StockLocationParametersValidator()
        {
            RuleFor(x => x.Name).ApplyNameRules();
            RuleFor(x => x.Code).ApplyCodeRules();
            RuleFor(x => x.Address1).ApplyAddressRules();
            RuleFor(x => x.Address2).ApplyAddressRules();
            RuleFor(x => x.City).ApplyCityRules();
            RuleFor(x => x.Phone).ApplyPhoneRules();
            RuleFor(x => x.PostalCode).ApplyPostalCodeRules();
        }
    }

    public static IRuleBuilderOptions<T, StockLocationParameters> ApplyStockLocationParametersRules<T>(
        this IRuleBuilder<T, StockLocationParameters> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .SetValidator(new StockLocationParametersValidator());
    }
}

public static partial class StockLocationValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyPhoneRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.PhoneMaxLength)
            .WithErrorCode(StockLocationResult.Errors.PhoneTooLong.Code)
            .WithMessage(StockLocationResult.Errors.PhoneTooLong.Message);
    }
}

public static partial class StockLocationValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyPostalCodeRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(StockLocationConstant.Constraints.PostalCodeMaxLength)
            .WithErrorCode(StockLocationResult.Errors.PostalCodeTooLong.Code)
            .WithMessage(StockLocationResult.Errors.PostalCodeTooLong.Message);
    }
}
