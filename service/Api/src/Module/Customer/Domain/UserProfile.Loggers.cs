namespace Module.Customer.Domain;

public static partial class UserProfileLoggers
{
    public static partial class Management
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Debug,
            Message = "[UserProfile.Created]: {Email} ({Id}) by {ActionBy}")]
        public static partial void Created(ILogger logger, string Email, Guid Id, string? ActionBy = "System");

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Debug,
            Message = "[UserProfile.Updated]: {Email} ({Id}) by {ActionBy}")]
        public static partial void Updated(ILogger logger, string Email, Guid Id, string? ActionBy = "System");

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Debug,
            Message = "[UserProfile.Deleted]: {Email} ({Id}) by {ActionBy}")]
        public static partial void Deleted(ILogger logger, string Email, Guid Id, string? ActionBy = "System");

        [LoggerMessage(
            EventId = 4,
            Level = LogLevel.Information,
            Message = "[UserProfile.Integration.Created]: Profile {ProfileId} created for user {UserId}")]
        public static partial void ProfileCreated(ILogger logger, Guid UserId, Guid ProfileId);

        [LoggerMessage(
            EventId = 5,
            Level = LogLevel.Warning,
            Message = "[UserProfile.Integration.AlreadyExists]: Profile already exists for user {UserId}")]
        public static partial void ProfileAlreadyExists(ILogger logger, Guid UserId);

        [LoggerMessage(
            EventId = 6,
            Level = LogLevel.Error,
            Message = "[UserProfile.Integration.CreationFailed]: Profile creation failed for user {UserId}: {Errors}")]
        public static partial void ProfileCreationFailed(ILogger logger, Guid UserId, string Errors);

        [LoggerMessage(
            EventId = 7,
            Level = LogLevel.Information,
            Message = "[UserProfile.Integration.Deleted]: Profile {ProfileId} deleted for user {UserId}")]
        public static partial void ProfileDeleted(ILogger logger, Guid UserId, Guid ProfileId);

        [LoggerMessage(
            EventId = 8,
            Level = LogLevel.Warning,
            Message = "[UserProfile.Integration.DeletionSkipped]: No profile found for user {UserId}: {Reason}")]
        public static partial void ProfileDeletionSkipped(ILogger logger, Guid UserId, string Reason);
    }
}