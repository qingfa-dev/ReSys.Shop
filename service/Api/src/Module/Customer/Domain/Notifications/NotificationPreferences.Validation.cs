namespace Module.Customer.Domain.Notifications;

public static class NotificationPreferencesValidation
{
    public static IRuleBuilderOptions<T, bool> ApplyEnableEmailRules<T>(this IRuleBuilder<T, bool> ruleBuilder)
    {
        return ruleBuilder
            .Must(_ => true);
    }

    public static IRuleBuilderOptions<T, bool> ApplyEnableSmsRules<T>(this IRuleBuilder<T, bool> ruleBuilder)
    {
        return ruleBuilder
            .Must(_ => true);
    }

    public static IRuleBuilderOptions<T, bool> ApplyEnableNewsfeedsRules<T>(this IRuleBuilder<T, bool> ruleBuilder)
    {
        return ruleBuilder
            .Must(_ => true);
    }

    public static IRuleBuilderOptions<T, NotificationPreferences?> ValidateNotificationPreferences<T>(
        this IRuleBuilder<T, NotificationPreferences?> ruleBuilder)
    {
        return ruleBuilder
            .SetValidator(new NotificationPreferencesValidator());
    }
}

public class NotificationPreferencesValidator : AbstractValidator<NotificationPreferences?>
{
    public NotificationPreferencesValidator()
    {
        RuleFor(x => x!.EnableSms).ApplyEnableSmsRules();
        RuleFor(x => x!.EnableEmail).ApplyEnableEmailRules();
        RuleFor(x => x!.EnableNewsfeeds).ApplyEnableNewsfeedsRules();
    }
}