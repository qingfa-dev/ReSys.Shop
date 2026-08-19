namespace Module.Billing.Domain.WebhookEvents;

public static class WebhookEventValidation
{
    public static IRuleBuilderOptions<T, string?> ApplyStripeEventIdRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(WebhookEventResult.Errors.StripeEventIdRequired.Code)
            .WithMessage(WebhookEventResult.Errors.StripeEventIdRequired.Message)
            .MaximumLength(WebhookEventConstant.Constraints.MaxStripeEventIdLength)
            .WithErrorCode(WebhookEventResult.Errors.StripeEventIdTooLong.Code)
            .WithMessage(WebhookEventResult.Errors.StripeEventIdTooLong.Message);
    }

    public static IRuleBuilderOptions<T, string?> ApplyTypeRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(WebhookEventResult.Errors.TypeRequired.Code)
            .WithMessage(WebhookEventResult.Errors.TypeRequired.Message)
            .MaximumLength(WebhookEventConstant.Constraints.MaxTypeLength)
            .WithErrorCode(WebhookEventResult.Errors.TypeTooLong.Code)
            .WithMessage(WebhookEventResult.Errors.TypeTooLong.Message);
    }
}