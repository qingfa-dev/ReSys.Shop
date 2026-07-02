using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Profile.Domain.Notifications;
using Module.Profile.Domain.Preferences;

using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Profile.Domain;

[Trait("Category", "Unit")]
[Trait("Module", "Profiles")]
[Trait("Feature", "UserProfileMethods")]
public class UserProfileMethodTests
{
    private const string FirstName = "John";
    private const string LastName = "Doe";
    private const string Email = "john@example.com";

    #region Factory Methods

    [Fact(DisplayName = "Create should return success with valid required fields")]
    public void Create_WithValidFields_ShouldReturnSuccess()
    {
        Result<UserProfile> result = UserProfileMethod.Create(FirstName, LastName, Email);

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(FirstName);
        result.Value.LastName.Should().Be(LastName);
        result.Value.Email.Should().Be(Email);
    }

    [Fact(DisplayName = "Create should set default property values")]
    public void Create_ShouldSetDefaultValues()
    {
        Result<UserProfile> result = UserProfileMethod.Create(FirstName, LastName, Email);

        result.Value.IsActive.Should().BeTrue();
        result.Value.Preferences.Should().NotBeNull();
        result.Value.Notifications.Should().NotBeNull();
        result.Value.Notifications.EnableSms.Should().BeTrue();
        result.Value.Notifications.EnableEmail.Should().BeTrue();
        result.Value.Notifications.EnableNewsfeeds.Should().BeTrue();
    }

    [Theory(DisplayName = "Create should fail when required field is empty/whitespace")]
    [InlineData("", LastName, Email)]
    [InlineData("   ", LastName, Email)]
    [InlineData(FirstName, "", Email)]
    [InlineData(FirstName, "   ", Email)]
    [InlineData(FirstName, LastName, "")]
    [InlineData(FirstName, LastName, "   ")]
    public void Create_WithEmptyOrWhitespaceRequiredField_ShouldReturnFailure(
        string firstName, string lastName, string email)
    {
        Result<UserProfile> result = UserProfileMethod.Create(firstName, lastName, email);

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "Create with all optional fields should produce correct profile")]
    public void Create_WithAllOptionalFields_ShouldProduceCorrectProfile()
    {
        DateTimeOffset dob = new DateTimeOffset(1985, 6, 20, 0, 0, 0, TimeSpan.Zero);
        UserPreferences prefs = UserPreferenceMethod.Create(preferredStyle: "formal").Value;
        NotificationPreferences notifications = NotificationPreferencesExtensions.Create(
            enableSms: false, enableEmail: false, enableNewsfeeds: true).Value;

        Result<UserProfile> result = UserProfileMethod.Create(
            firstName: FirstName,
            lastName: LastName,
            email: Email,
            phoneNumber: "+987654321",
            dateOfBirth: dob,
            gender: "Female",
            bio: "Designer",
            avatarUrl: "https://example.com/avatar2.jpg",
            preferences: prefs,
            notifications: notifications);

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(FirstName);
        result.Value.LastName.Should().Be(LastName);
        result.Value.Email.Should().Be(Email);
        result.Value.PhoneNumber.Should().Be("+987654321");
        result.Value.DateOfBirth.Should().Be(dob);
        result.Value.Gender.Should().Be("Female");
        result.Value.Bio.Should().Be("Designer");
        result.Value.AvatarUrl.Should().Be("https://example.com/avatar2.jpg");
        result.Value.Preferences.PreferredStyle.Should().Be("formal");
        result.Value.Notifications.EnableNewsfeeds.Should().BeTrue();
    }

    #endregion

    #region Update

    [Fact(DisplayName = "Update should set phone number")]
    public void Update_WithPhoneNumber_ShouldSetPhoneNumber()
    {
        Result<UserProfile> result = UserProfileMethod.Create(FirstName, LastName, Email);

        Result<UserProfile> updated = result.Value.Update(phoneNumber: "+1234567890");

        updated.Value.PhoneNumber.Should().Be("+1234567890");
    }

    [Fact(DisplayName = "Update should set date of birth")]
    public void Update_WithDateOfBirth_ShouldSetDateOfBirth()
    {
        DateTimeOffset dob = new DateTimeOffset(1990, 1, 15, 0, 0, 0, TimeSpan.Zero);
        Result<UserProfile> result = UserProfileMethod.Create(FirstName, LastName, Email);

        Result<UserProfile> updated = result.Value.Update(dateOfBirth: dob);

        updated.Value.DateOfBirth.Should().Be(dob);
    }

    [Fact(DisplayName = "Update should set gender")]
    public void Update_WithGender_ShouldSetGender()
    {
        Result<UserProfile> result = UserProfileMethod.Create(FirstName, LastName, Email);

        Result<UserProfile> updated = result.Value.Update(gender: "Female");

        updated.Value.Gender.Should().Be("Female");
    }

    [Fact(DisplayName = "Update should set bio")]
    public void Update_WithBio_ShouldSetBio()
    {
        Result<UserProfile> result = UserProfileMethod.Create(FirstName, LastName, Email);

        Result<UserProfile> updated = result.Value.Update(bio: "Designer");

        updated.Value.Bio.Should().Be("Designer");
    }

    [Fact(DisplayName = "Update should set avatar URL")]
    public void Update_WithAvatarUrl_ShouldSetAvatarUrl()
    {
        Result<UserProfile> result = UserProfileMethod.Create(FirstName, LastName, Email);

        Result<UserProfile> updated = result.Value.Update(avatarUrl: "https://example.com/avatar.jpg");

        updated.Value.AvatarUrl.Should().Be("https://example.com/avatar.jpg");
    }

    [Fact(DisplayName = "Update should set preferences")]
    public void Update_WithPreferences_ShouldSetPreferences()
    {
        UserPreferences prefs = UserPreferenceMethod.Create(preferredStyle: "casual", preferredFit: "slim").Value;
        Result<UserProfile> result = UserProfileMethod.Create(FirstName, LastName, Email);

        Result<UserProfile> updated = result.Value.Update(preferences: prefs);

        updated.Value.Preferences.PreferredStyle.Should().Be("casual");
        updated.Value.Preferences.PreferredFit.Should().Be("slim");
    }

    [Fact(DisplayName = "Update should set notifications")]
    public void Update_WithNotifications_ShouldSetNotifications()
    {
        NotificationPreferences notifications = NotificationPreferencesExtensions.Create(
            enableSms: false, enableEmail: true, enableNewsfeeds: false).Value;
        Result<UserProfile> result = UserProfileMethod.Create(FirstName, LastName, Email);

        Result<UserProfile> updated = result.Value.Update(notifications: notifications);

        updated.Value.Notifications.EnableSms.Should().BeFalse();
        updated.Value.Notifications.EnableEmail.Should().BeTrue();
        updated.Value.Notifications.EnableNewsfeeds.Should().BeFalse();
    }

    #endregion

    #region Address Management

    [Fact(DisplayName = "AddAddress should add address to profile")]
    public void AddAddress_ShouldAddAddress()
    {
        Result<UserProfile> profile = UserProfileMethod.Create(FirstName, LastName, Email);
        Address address = AddressMethod.Create("Jane", "123 St", "City", "Country").Value;

        Result<UserProfile> result = profile.Value.AddAddress(address);

        result.IsSuccess.Should().BeTrue();
        result.Value.Addresses.Should().Contain(address);
    }

    [Fact(DisplayName = "RemoveAddress should remove address")]
    public void RemoveAddress_ShouldRemoveAddress()
    {
        Result<UserProfile> profile = UserProfileMethod.Create(FirstName, LastName, Email);
        Address address = AddressMethod.Create("Jane", "123 St", "City", "Country").Value;
        profile.Value.AddAddress(address);

        Result<UserProfile> result = profile.Value.RemoveAddress(address.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Addresses.Should().NotContain(address);
    }

    [Fact(DisplayName = "UpdateAddress should apply action to address")]
    public void UpdateAddress_ShouldApplyAction()
    {
        Result<UserProfile> profile = UserProfileMethod.Create(FirstName, LastName, Email);
        Address address = AddressMethod.Create("Jane", "123 St", "City", "Country",
            lastName: "Doe").Value;
        profile.Value.AddAddress(address);

        Result<UserProfile> result = profile.Value.UpdateAddress(address.Id, a => a.MarkAsDefault());

        result.IsSuccess.Should().BeTrue();
        result.Value.Addresses.First().IsDefault.Should().BeTrue();
    }

    [Fact(DisplayName = "HasDefaultBillingAddress should return true when set")]
    public void HasDefaultBillingAddress_WhenSet_ShouldReturnTrue()
    {
        UserProfile profile = UserProfileMethod.Create(FirstName, LastName, Email,
            defaultBillingAddressId: Guid.NewGuid()).Value;

        profile.HasDefaultBillingAddress().Should().BeTrue();
    }

    [Fact(DisplayName = "CanAddAddressOfType should return true when under limit")]
    public void CanAddAddressOfType_WhenUnderLimit_ShouldReturnTrue()
    {
        UserProfile profile = UserProfileMethod.Create(FirstName, LastName, Email).Value;

        profile.CanAddAddressOfType(AddressType.Shipping).Should().BeTrue();
    }

    #endregion

    #region Preferences

    [Fact(DisplayName = "WithPreferredStyle should set style")]
    public void WithPreferredStyle_ShouldSetStyle()
    {
        Result<UserProfile> profile = UserProfileMethod.Create(FirstName, LastName, Email);

        Result<UserProfile> result = profile.Value.WithPreferredStyle("formal");

        result.Value.Preferences.PreferredStyle.Should().Be("formal");
    }

    [Fact(DisplayName = "WithPreferredFit should set fit")]
    public void WithPreferredFit_ShouldSetFit()
    {
        Result<UserProfile> profile = UserProfileMethod.Create(FirstName, LastName, Email);

        Result<UserProfile> result = profile.Value.WithPreferredFit("slim");

        result.Value.Preferences.PreferredFit.Should().Be("slim");
    }

    [Fact(DisplayName = "WithFavoriteColors should set colors")]
    public void WithFavoriteColors_ShouldSetColors()
    {
        List<string> colors = new List<string> { "Red", "Blue" };
        Result<UserProfile> profile = UserProfileMethod.Create(FirstName, LastName, Email);

        Result<UserProfile> result = profile.Value.WithFavoriteColors(colors);

        result.Value.Preferences.FavoriteColors.Should().BeEquivalentTo(colors);
    }

    #endregion

    #region Notifications

    [Fact(DisplayName = "WithSmsNotification should set SMS")]
    public void WithSmsNotification_ShouldSetSms()
    {
        Result<UserProfile> profile = UserProfileMethod.Create(FirstName, LastName, Email);

        Result<UserProfile> result = profile.Value.WithSmsNotification(false);

        result.Value.Notifications.EnableSms.Should().BeFalse();
    }

    #endregion

    #region Commerce

    [Fact(DisplayName = "UpdateCommerceProfile should update marketing flag")]
    public void UpdateCommerceProfile_WithMarketing_ShouldUpdate()
    {
        Result<UserProfile> profile = UserProfileMethod.Create(FirstName, LastName, Email);

        Result<UserProfile> updated = profile.Value.UpdateCommerceProfile(true);

        updated.Value.AcceptsEmailMarketing.Should().BeTrue();
    }

    [Fact(DisplayName = "RecordCompletedOrder should update totals")]
    public void RecordCompletedOrder_ShouldUpdateTotals()
    {
        UserProfile profile = UserProfileMethod.Create(FirstName, LastName, Email).Value;

        profile.RecordCompletedOrder(150.00m);

        profile.OrdersCount.Should().Be(1);
        profile.TotalSpent.Should().Be(150.00m);
        profile.LastOrderCompletedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "LifetimeValue should return total spent")]
    public void LifetimeValue_ShouldReturnTotalSpent()
    {
        UserProfile profile = UserProfileMethod.Create(FirstName, LastName, Email,
            totalSpent: 500m).Value;

        profile.LifetimeValue().Should().Be(500m);
    }

    [Fact(DisplayName = "IsActiveCustomer should return true when active and has orders")]
    public void IsActiveCustomer_WhenActiveAndHasOrders_ShouldReturnTrue()
    {
        UserProfile profile = new UserProfile
        {
            FirstName = FirstName,
            LastName = LastName,
            Email = Email,
            IsActive = true,
            OrdersCount = 5
        };

        profile.IsActiveCustomer().Should().BeTrue();
    }

    [Fact(DisplayName = "IsActiveCustomer should return false when inactive")]
    public void IsActiveCustomer_WhenInactive_ShouldReturnFalse()
    {
        UserProfile profile = new UserProfile
        {
            FirstName = FirstName,
            LastName = LastName,
            Email = Email,
            IsActive = false,
            OrdersCount = 5
        };

        profile.IsActiveCustomer().Should().BeFalse();
    }

    #endregion

    #region Sign In

    [Fact(DisplayName = "RecordSignIn (Result) should rotate IPs and increment count")]
    public void RecordSignIn_ShouldRotateIpsAndIncrement()
    {
        User user = new User { UserName = "johndoe" };
        Result<UserProfile> profile = UserProfileMethod.Create(FirstName, LastName, Email);
        profile.Value.User = user;
        profile.Value.UserId = user.Id;

        Result<UserProfile> firstSignIn = profile.Value.RecordSignIn("192.168.1.1");

        firstSignIn.Value.User.CurrentSignInIp.Should().Be("192.168.1.1");
        firstSignIn.Value.User.SignInCount.Should().Be(1);

        Result<UserProfile> secondSignIn = firstSignIn.Value.RecordSignIn("10.0.0.1");

        secondSignIn.Value.User.LastSignInIp.Should().Be("192.168.1.1");
        secondSignIn.Value.User.CurrentSignInIp.Should().Be("10.0.0.1");
        secondSignIn.Value.User.SignInCount.Should().Be(2);
    }

    [Fact(DisplayName = "RecordSignIn (instance) should update profile")]
    public void RecordSignIn_Instance_ShouldUpdateProfile()
    {
        User user = new User { UserName = "johndoe" };
        UserProfile profile = UserProfileMethod.Create(FirstName, LastName, Email).Value;
        profile.User = user;
        profile.UserId = user.Id;

        profile.RecordSignIn("192.168.1.1");

        profile.User.CurrentSignInIp.Should().Be("192.168.1.1");
        profile.User.SignInCount.Should().Be(1);
    }

    [Fact(DisplayName = "RecordFailedAttempt should increment")]
    public void RecordFailedAttempt_ShouldIncrement()
    {
        User user = new User { UserName = "johndoe" };
        UserProfile profile = UserProfileMethod.Create(FirstName, LastName, Email).Value;
        profile.User = user;
        profile.UserId = user.Id;

        profile.RecordFailedAttempt();

        profile.User.AccessFailedCount.Should().Be(1);
    }

    #endregion

    #region Status

    [Fact(DisplayName = "IsUsable should return true when active and has email")]
    public void IsUsable_WhenActive_ShouldReturnTrue()
    {
        UserProfile profile = UserProfileMethod.Create(FirstName, LastName, Email).Value;

        profile.IsUsable().Should().BeTrue();
    }

    [Fact(DisplayName = "Deactivate should set IsActive false")]
    public void Deactivate_ShouldSetInactive()
    {
        UserProfile profile = UserProfileMethod.Create(FirstName, LastName, Email).Value;

        profile.Deactivate();

        profile.IsActive.Should().BeFalse();
    }

    [Fact(DisplayName = "Activate should set IsActive true")]
    public void Activate_ShouldSetActive()
    {
        UserProfile profile = UserProfileMethod.Create(FirstName, LastName, Email).Value;
        profile.Deactivate();

        profile.Activate();

        profile.IsActive.Should().BeTrue();
    }

    #endregion

    #region Display

    [Fact(DisplayName = "DisplayName should return first and last name")]
    public void DisplayName_ShouldReturnFirstAndLast()
    {
        UserProfile profile = UserProfileMethod.Create(FirstName, LastName, Email).Value;

        profile.DisplayName().Should().Be("John Doe");
    }

    [Fact(DisplayName = "FullName should return combined name")]
    public void FullName_ShouldReturnCombined()
    {
        UserProfile profile = UserProfileMethod.Create(FirstName, LastName, Email).Value;

        profile.FullName().Should().Be("John Doe");
    }

    #endregion

    #region Roles

    [Fact(DisplayName = "HasRole should return true when role exists")]
    public void HasRole_WhenRoleExists_ShouldReturnTrue()
    {
        UserProfile profile = UserProfileMethod.Create(FirstName, LastName, Email).Value;
        List<Role> roles = new List<Role> { new Role { Name = "Admin" } };

        profile.HasRole("Admin", roles).Should().BeTrue();
    }

    [Fact(DisplayName = "IsAdmin should return true when admin role exists")]
    public void IsAdmin_WhenAdminRoleExists_ShouldReturnTrue()
    {
        UserProfile profile = UserProfileMethod.Create(FirstName, LastName, Email).Value;
        List<Role> roles = new List<Role> { new Role { Name = RoleConstant.Defaults.Admin } };

        profile.IsAdmin(roles).Should().BeTrue();
    }

    [Fact(DisplayName = "HasAnyRole should return true when any role matches")]
    public void HasAnyRole_WhenAnyRoleMatches_ShouldReturnTrue()
    {
        UserProfile profile = UserProfileMethod.Create(FirstName, LastName, Email).Value;
        List<Role> roles = new List<Role> { new Role { Name = "Editor" } };
        List<string> roleNames = new List<string> { "Admin", "Editor" };

        profile.HasAnyRole(roleNames, roles).Should().BeTrue();
    }

    #endregion
}