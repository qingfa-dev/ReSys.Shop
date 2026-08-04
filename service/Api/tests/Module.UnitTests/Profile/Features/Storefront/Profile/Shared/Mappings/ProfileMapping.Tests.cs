using Module.Profile.Domain.Preferences;
using Module.Profile.Features.Shared.Profiles.Mappings;
using Module.Profile.Features.Shared.Profiles.Models;
using Module.UnitTests.Profile.Domain;

namespace Module.UnitTests.Profile.Features.Store.Profile.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "ProfileMapping")]
public class ProfileMappingTests
{
    private sealed record TestProfileRequest : ProfileRequest;
    private sealed record TestProfileDetailResponse : ProfileDetailResponse;
    private sealed record TestProfileListItemResponse : ProfileListItemResponse;

    [Fact(DisplayName = "Should map request to entity with all properties including preferences")]
    public void ToEntity_ShouldMapRequestToEntity()
    {
        var request = new TestProfileRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "+12025550101",
            DateOfBirth = new DateTimeOffset(1990, 1, 15, 0, 0, 0, TimeSpan.Zero),
            Preferences = new ProfilePreferences
            {
                PreferredStyle = "casual",
                PreferredFit = "regular",
                FavoriteColors = ["Black", "Blue"],
                FavoriteCategories = ["Tops"],
                PreferredBrands = ["Nike"],
                SizeTop = "M",
                SizeBottom = "32",
                ShoeSize = "10"
            },
            Notifications = new ProfileNotificationPreferences
            {
                EnableSms = true,
                EnableEmail = false,
                EnableNewsfeeds = true
            }
        };

        var profile = request.MapToDomain();

        profile.FirstName.Should().Be(request.FirstName);
        profile.LastName.Should().Be(request.LastName);
        profile.Email.Should().Be(request.Email);
        profile.PhoneNumber.Should().Be(request.PhoneNumber);
        profile.DateOfBirth.Should().Be(request.DateOfBirth);
        profile.Preferences.PreferredStyle.Should().Be(request.Preferences.PreferredStyle);
        profile.Preferences.PreferredFit.Should().Be(request.Preferences.PreferredFit);
        profile.Preferences.FavoriteColors.Should().BeEquivalentTo(request.Preferences.FavoriteColors);
        profile.Preferences.FavoriteCategories.Should().BeEquivalentTo(request.Preferences.FavoriteCategories);
        profile.Preferences.PreferredBrands.Should().BeEquivalentTo(request.Preferences.PreferredBrands);
        profile.Preferences.SizeTop.Should().Be(request.Preferences.SizeTop);
        profile.Preferences.SizeBottom.Should().Be(request.Preferences.SizeBottom);
        profile.Preferences.ShoeSize.Should().Be(request.Preferences.ShoeSize);
        profile.Notifications.EnableSms.Should().Be(request.Notifications.EnableSms);
        profile.Notifications.EnableEmail.Should().Be(request.Notifications.EnableEmail);
        profile.Notifications.EnableNewsfeeds.Should().Be(request.Notifications.EnableNewsfeeds);
    }

    [Fact(DisplayName = "Should map request with null preferences to entity with domain defaults")]
    public void ToEntity_WhenPreferencesNull_ShouldUseDefaults()
    {
        var request = new TestProfileRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Preferences = null,
            Notifications = null
        };

        var profile = request.MapToDomain();

        profile.Preferences.PreferredStyle.Should().Be(UserPreferenceConstant.Defaults.PreferredStyle);
        profile.Preferences.PreferredFit.Should().Be(UserPreferenceConstant.Defaults.PreferredFit);
        profile.Preferences.FavoriteColors.Should().BeEmpty();
        profile.Notifications.EnableSms.Should().BeTrue();
        profile.Notifications.EnableEmail.Should().BeTrue();
        profile.Notifications.EnableNewsfeeds.Should().BeTrue();
    }

    [Fact(DisplayName = "Should update existing entity properties including preferences")]
    public void ToEntity_Update_ShouldUpdateEntityProperties()
    {
        var existingProfile = ProfileUserFactory.Create(Guid.NewGuid());
        var request = new TestProfileRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Email = "updated@example.com",
            PhoneNumber = "+12025550202",
            DateOfBirth = new DateTimeOffset(1985, 5, 20, 0, 0, 0, TimeSpan.Zero),
            Preferences = new ProfilePreferences
            {
                PreferredStyle = "formal"
            },
            Notifications = new ProfileNotificationPreferences
            {
                EnableSms = false
            }
        };

        request.MapToDomain(existingProfile);

        existingProfile.FirstName.Should().Be("Updated");
        existingProfile.LastName.Should().Be("Name");
        existingProfile.Email.Should().Be("updated@example.com");
        existingProfile.PhoneNumber.Should().Be("+12025550202");
        existingProfile.DateOfBirth.Should().Be(new DateTimeOffset(1985, 5, 20, 0, 0, 0, TimeSpan.Zero));
        existingProfile.Preferences.PreferredStyle.Should().Be("formal");
        existingProfile.Notifications.EnableSms.Should().BeFalse();
    }

    [Fact(DisplayName = "Should map entity to detail response with all preference properties")]
    public void ToDetail_ShouldMapEntityToResponse()
    {
        var profile = ProfileUserFactory.Create(Guid.NewGuid());
        profile.Preferences.PreferredStyle = "sporty";
        profile.Preferences.PreferredFit = "slim";
        profile.Preferences.FavoriteColors = ["Red", "Green"];
        profile.Notifications.EnableEmail = false;

        var response = profile.MapToDetail<TestProfileDetailResponse>();

        response.FirstName.Should().Be(profile.FirstName);
        response.LastName.Should().Be(profile.LastName);
        response.Email.Should().Be(profile.Email);
        response.Preferences.Should().NotBeNull();
        response.Preferences!.PreferredStyle.Should().Be("sporty");
        response.Preferences.PreferredFit.Should().Be("slim");
        response.Preferences.FavoriteColors.Should().BeEquivalentTo(["Red", "Green"]);
        response.Notifications.Should().NotBeNull();
        response.Notifications!.EnableEmail.Should().BeFalse();
        response.FullName.Should().Be("Test User");
    }

    [Fact(DisplayName = "Should map entity to list item response with preference properties")]
    public void ToListItem_ShouldMapEntityToResponse()
    {
        var profile = ProfileUserFactory.Create(Guid.NewGuid());
        profile.Preferences.PreferredFit = "relaxed";
        profile.Notifications.EnableSms = false;

        var response = profile.MapToListItem<TestProfileListItemResponse>();

        response.Preferences.Should().NotBeNull();
        response.Preferences!.PreferredFit.Should().Be("relaxed");
        response.Notifications.Should().NotBeNull();
        response.Notifications!.EnableSms.Should().BeFalse();
        response.FullName.Should().Be("Test User");
    }

    [Fact(DisplayName = "Should not have null preferences and notifications on response")]
    public void ToDetail_ShouldNotHaveNullPreferences()
    {
        var profile = ProfileUserFactory.Create(Guid.NewGuid());

        var response = profile.MapToDetail<TestProfileDetailResponse>();

        response.Preferences.Should().NotBeNull();
        response.Preferences!.PreferredStyle.Should().Be(UserPreferenceConstant.Defaults.PreferredStyle);
        response.Preferences.FavoriteColors.Should().BeEmpty();
        response.Notifications.Should().NotBeNull();
        response.Notifications!.EnableSms.Should().BeTrue();
    }
}
