using Shared.Application.Domain.Concerns.Auditable;

namespace Shared.Security.Identity.Domain.Users;

public static partial class UserMethod
{
    public static Result<User> Create(
        string userName,
        string email,
        string firstName,
        string lastName,
        string? phoneNumber = null,
        bool emailConfirmed = false,
        bool phoneNumberConfirmed = false)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return UserResult.Failure.UsernameRequired;

        if (string.IsNullOrWhiteSpace(email))
            return UserResult.Failure.EmailRequired;

        var user = new User
        {
            UserName = userName,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            EmailConfirmed = emailConfirmed,
            PhoneNumberConfirmed = phoneNumberConfirmed
        };

        return user;
    }

    public static Result<User> Update(
        this User user,
        Optional<string?> userName = default,
        Optional<string?> email = default,
        Optional<string?> firstName = default,
        Optional<string?> lastName = default,
        Optional<string?> phoneNumber = default,
        Optional<bool> emailConfirmed = default,
        Optional<bool> phoneNumberConfirmed = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        bool isChanged = false;

        // Update: username with validation
        if (userName.HasValue)
        {
            string newUserName = userName.Value!;
            if (string.IsNullOrWhiteSpace(newUserName))
                return UserResult.Failure.UsernameRequired;

            isChanged |= userName.ApplyIfChanged(user.UserName, x => user.UserName = x);
        }

        // Update: email via SetEmail for consistent validation
        if (email.HasValue)
        {
            Result<User> emailResult = user.SetEmail(email.Value!);
            if (emailResult.IsFailure)
                return emailResult;

            isChanged = true;
        }

        // Update: first name via ApplyIfChanged
        if (firstName.HasValue)
        {
            isChanged |= firstName.ApplyIfChanged(user.FirstName!, x => user.FirstName = x!);
        }

        // Update: last name via ApplyIfChanged
        if (lastName.HasValue)
        {
            isChanged |= lastName.ApplyIfChanged(user.LastName!, x => user.LastName = x);
        }

        // Update: phone number via SetPhoneNumber for consistent validation
        if (phoneNumber.HasValue)
        {
            Result<User> phoneResult = user.SetPhoneNumber(phoneNumber.Value!);
            if (phoneResult.IsFailure)
                return phoneResult;

            isChanged = true;
        }

        // Update: email confirmed via ApplyIfChanged
        if (emailConfirmed.HasValue)
        {
            isChanged |= emailConfirmed.ApplyIfChanged(user.EmailConfirmed, x => user.EmailConfirmed = x);
        }

        // Update: phone number confirmed via ApplyIfChanged
        if (phoneNumberConfirmed.HasValue)
        {
            isChanged |= phoneNumberConfirmed.ApplyIfChanged(user.PhoneNumberConfirmed, x => user.PhoneNumberConfirmed = x);
        }

        // Audit: touch only when something actually changed
        if (isChanged)
            AuditableBehavior.Touch(user);

        return user;
    }

    public static Result<User> SetUserName(
        this User user,
        string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return UserResult.Failure.UsernameRequired;

        user.UserName = userName;

        return user;
    }

    public static Result<User> SetEmail(
        this User user,
        string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return UserResult.Failure.EmailRequired;

        user.Email = email;

        return user;
    }

    public static Result<User> SetPhoneNumber(
        this User user,
        string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return UserResult.Failure.PhoneRequired;

        user.PhoneNumber = phoneNumber;

        return user;
    }

    public static Result<User> SetFirstName(
        this User user,
        string firstName)
    {
        user.FirstName = firstName;

        return user;
    }

    public static Result<User> SetLastName(
        this User user,
        string lastName)
    {
        user.LastName = lastName;

        return user;
    }

    public static Result<User> ConfirmEmail(
        this User user)
    {
        if (user.EmailConfirmed)
            return user;

        user.EmailConfirmed = true;

        return user;
    }

    public static Result<User> UnconfirmEmail(
        this User user)
    {
        if (!user.EmailConfirmed)
            return user;

        user.EmailConfirmed = false;

        return user;
    }

    public static Result<User> ConfirmPhoneNumber(
        this User user)
    {
        if (user.PhoneNumberConfirmed)
            return user;

        user.PhoneNumberConfirmed = true;

        return user;
    }

    public static Result<User> UnconfirmPhoneNumber(
        this User user)
    {
        if (!user.PhoneNumberConfirmed)
            return user;

        user.PhoneNumberConfirmed = false;

        return user;
    }

    public static Result<User> Enable(
        this User user)
    {
        if (user.IsActive)
            return UserResult.Failure.AlreadyActive;

        user.IsActive = true;

        return user;
    }

    public static Result<User> Disable(
        this User user)
    {
        if (!user.IsActive)
            return UserResult.Failure.AlreadyDeactivated;

        user.IsActive = false;

        return user;
    }

    public static Result<User> Lock(
        this User user,
        DateTimeOffset? lockoutEnd = null)
    {
        user.LockoutEnabled = true;
        user.LockoutEnd = lockoutEnd ?? DateTimeOffset.MaxValue;

        return user;
    }

    public static Result<User> Unlock(
        this User user)
    {
        user.LockoutEnabled = false;
        user.LockoutEnd = null;

        return user;
    }

    public static Result<User> RecordSignIn(
        this User user,
        string currentIp)
    {
        user.LastSignInIp = user.CurrentSignInIp;
        user.LastSignInAtUtc = user.CurrentSignInAtUtc;

        user.CurrentSignInIp = currentIp;
        user.CurrentSignInAtUtc = DateTimeOffset.UtcNow;

        user.SignInCount++;

        return user;
    }

    public static Result<User> RecordFailedAttempt(
        this User user)
    {
        user.AccessFailedCount++;

        return user;
    }

    public static Result<User> ResetFailedAttempts(
        this User user)
    {
        user.AccessFailedCount = 0;

        return user;
    }
}