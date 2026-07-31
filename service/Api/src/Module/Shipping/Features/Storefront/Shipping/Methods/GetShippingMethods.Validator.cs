namespace Module.Shipping.Features.Storefront.Shipping.Methods;

public static partial class GetShippingMethods
{
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Parameters.PageSize)
                .Must(value => value.HasValue && value.Value >= 1 && value.Value <= 100)
                .WithErrorCode("InvalidPageSize")
                .When(x => x.Parameters.PageSize.HasValue);
        }
    }
}