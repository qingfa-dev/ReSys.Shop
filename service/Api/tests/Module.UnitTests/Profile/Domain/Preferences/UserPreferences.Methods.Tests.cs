using Microsoft.CodeAnalysis;

using Module.Profile.Domain.Preferences;

namespace Module.UnitTests.Profile.Domain.Preferences;

[Trait("Category", "Unit")]
[Trait("Module", "Profiles")]
[Trait("Feature", "UserPreferenceMethods")]
public class UserPreferenceMethodsTests
{
    [Fact(DisplayName = "Create should return preferences with default values")]
    public void Create_ShouldReturnDefaultValues()
    {
        Result<UserPreferences> result = UserPreferenceMethod.Create();

        result.IsSuccess.Should().BeTrue();
        result.Value.PreferredStyle.Should().Be(UserPreferenceConstant.Defaults.PreferredStyle);
        result.Value.PreferredFit.Should().Be(UserPreferenceConstant.Defaults.PreferredFit);
        result.Value.FavoriteColors.Should().BeEmpty();
        result.Value.FavoriteCategories.Should().BeEmpty();
        result.Value.PreferredBrands.Should().BeEmpty();
        result.Value.SizeTop.Should().BeNull();
        result.Value.SizeBottom.Should().BeNull();
        result.Value.ShoeSize.Should().BeNull();
    }

    [Fact(DisplayName = "Create with all fields should set each field")]
    public void Create_WithAllFields_ShouldSetEachField()
    {
        List<string> colors = ["Black", "White"];
        List<string> categories = ["Shoes"];
        List<string> brands = ["Puma"];

        Result<UserPreferences> result = UserPreferenceMethod.Create(
            preferredStyle: "sporty",
            preferredFit: "loose",
            favoriteColors: colors,
            favoriteCategories: categories,
            preferredBrands: brands,
            sizeTop: "XL",
            sizeBottom: "34",
            shoeSize: "12");

        result.IsSuccess.Should().BeTrue();
        result.Value.PreferredStyle.Should().Be("sporty");
        result.Value.PreferredFit.Should().Be("loose");
        result.Value.FavoriteColors.Should().BeEquivalentTo(colors);
        result.Value.FavoriteCategories.Should().BeEquivalentTo(categories);
        result.Value.PreferredBrands.Should().BeEquivalentTo(brands);
        result.Value.SizeTop.Should().Be("XL");
        result.Value.SizeBottom.Should().Be("34");
        result.Value.ShoeSize.Should().Be("12");
    }

    [Theory(DisplayName = "Update preferred style should set or default")]
    [InlineData("formal")]
    [InlineData(null)]
    public void Update_WithPreferredStyle_ShouldSetStyle(string? style)
    {
        string expected = style ?? UserPreferenceConstant.Defaults.PreferredStyle;

        Result<UserPreferences> result = UserPreferenceMethod.Create()
            .Value.Update(preferredStyle: style);

        result.IsSuccess.Should().BeTrue();
        result.Value.PreferredStyle.Should().Be(expected);
    }

    [Theory(DisplayName = "Update preferred fit should set or default")]
    [InlineData("slim")]
    [InlineData(null)]
    public void Update_WithPreferredFit_ShouldSetFit(string? fit)
    {
        string expected = fit ?? UserPreferenceConstant.Defaults.PreferredFit;

        Result<UserPreferences> result = UserPreferenceMethod.Create()
            .Value.Update(preferredFit: fit);

        result.IsSuccess.Should().BeTrue();
        result.Value.PreferredFit.Should().Be(expected);
    }

    [Fact(DisplayName = "Update favorite colors should set colors")]
    public void Update_WithFavoriteColors_ShouldSetColors()
    {
        List<string> colors = ["Red", "Blue"];

        Result<UserPreferences> result = UserPreferenceMethod.Create()
            .Value.Update(favoriteColors: colors);

        result.IsSuccess.Should().BeTrue();
        result.Value.FavoriteColors.Should().BeEquivalentTo(colors);
    }

    [Fact(DisplayName = "Update favorite categories should set categories")]
    public void Update_WithFavoriteCategories_ShouldSetCategories()
    {
        List<string> categories = ["Electronics", "Books"];

        Result<UserPreferences> result = UserPreferenceMethod.Create()
            .Value.Update(favoriteCategories: categories);

        result.IsSuccess.Should().BeTrue();
        result.Value.FavoriteCategories.Should().BeEquivalentTo(categories);
    }

    [Fact(DisplayName = "Update preferred brands should set brands")]
    public void Update_WithPreferredBrands_ShouldSetBrands()
    {
        List<string> brands = ["Nike", "Adidas"];

        Result<UserPreferences> result = UserPreferenceMethod.Create()
            .Value.Update(preferredBrands: brands);

        result.IsSuccess.Should().BeTrue();
        result.Value.PreferredBrands.Should().BeEquivalentTo(brands);
    }

    [Theory(DisplayName = "Update size fields should set sizes")]
    [InlineData("L", null, null)]
    [InlineData(null, "32", null)]
    [InlineData(null, null, "10")]
    [InlineData("XL", "34", "12")]
    public void Update_WithSizeFields_ShouldSetSizes(string? sizeTop, string? sizeBottom, string? shoeSize)
    {
        Result<UserPreferences> result = UserPreferenceMethod.Create()
            .Value.Update(sizeTop: sizeTop, sizeBottom: sizeBottom, shoeSize: shoeSize);

        result.IsSuccess.Should().BeTrue();
        if (sizeTop is not null)
            result.Value.SizeTop.Should().Be(sizeTop);
        if (sizeBottom is not null)
            result.Value.SizeBottom.Should().Be(sizeBottom);
        if (shoeSize is not null)
            result.Value.ShoeSize.Should().Be(shoeSize);
    }

    [Fact(DisplayName = "Full update chaining should produce correct preferences")]
    public void FullUpdateChaining_ShouldProduceCorrectPreferences()
    {
        List<string> colors = ["Black", "White"];
        List<string> categories = ["Shoes"];
        List<string> brands = ["Puma"];

        Result<UserPreferences> result = UserPreferenceMethod.Create(preferredStyle: "casual", preferredFit: "regular")
            .Value.Update(
                preferredStyle: "sporty",
                preferredFit: "loose",
                favoriteColors: colors,
                favoriteCategories: categories,
                preferredBrands: brands,
                sizeTop: "XL",
                sizeBottom: "34",
                shoeSize: "12");

        result.IsSuccess.Should().BeTrue();
        result.Value.PreferredStyle.Should().Be("sporty");
        result.Value.PreferredFit.Should().Be("loose");
        result.Value.FavoriteColors.Should().BeEquivalentTo(colors);
        result.Value.FavoriteCategories.Should().BeEquivalentTo(categories);
        result.Value.PreferredBrands.Should().BeEquivalentTo(brands);
        result.Value.SizeTop.Should().Be("XL");
        result.Value.SizeBottom.Should().Be("34");
        result.Value.ShoeSize.Should().Be("12");
    }

}
