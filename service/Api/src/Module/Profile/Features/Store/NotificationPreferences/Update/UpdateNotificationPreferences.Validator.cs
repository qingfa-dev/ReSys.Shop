using FluentValidation;

namespace Module.Profile.Features.Store.NotificationPreferences.Update;

public static partial class UpdateNotificationPreferences
{
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            // All booleans are valid; no rules needed beyond model binding.
        }
    }
}
