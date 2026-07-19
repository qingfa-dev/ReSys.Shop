namespace Module.Profile.Domain.Notifications;

public static class NotificationPreferencesMethod
{
    #region Factory Methods

    public static Result<NotificationPreferences> Create(
        bool? enableSms = true,
        bool? enableEmail = true,
        bool? enableNewsfeeds = true)
    {
        return new NotificationPreferences
        {
            EnableSms = enableSms ?? true,
            EnableEmail = enableEmail ?? true,
            EnableNewsfeeds = enableNewsfeeds ?? true
        };
    }

    #endregion

    #region Update

    public static Result<NotificationPreferences> Update(
        this NotificationPreferences prefs,
        bool? enableSms = default,
        bool? enableEmail = default,
        bool? enableNewsfeeds = default)
    {
        if (enableSms.HasValue) prefs.EnableSms = enableSms.Value;
        if (enableEmail.HasValue) prefs.EnableEmail = enableEmail.Value;
        if (enableNewsfeeds.HasValue) prefs.EnableNewsfeeds = enableNewsfeeds.Value;

        return prefs;
    }

    #endregion
}