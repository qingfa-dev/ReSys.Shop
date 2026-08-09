namespace Module.Identity.Features.Shared.Admin.Users.Shared.Models;

public abstract record UserParameter
{
    /// <summary>
    /// Gets or initializes the email address of the user.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the username of the user.
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the first name of the user.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the optional last name of the user.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the optional phone number of the user.
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Gets or initializes a value indicating whether the email address has been confirmed.
    /// </summary>
    public bool EmailConfirmed { get; init; }

    /// <summary>
    /// Gets or initializes a value indicating whether the phone number has been confirmed.
    /// </summary>
    public bool PhoneNumberConfirmed { get; init; }
}