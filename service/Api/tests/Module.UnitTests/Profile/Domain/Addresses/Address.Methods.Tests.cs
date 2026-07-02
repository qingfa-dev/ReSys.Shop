using Module.Profile.Domain.Addresses;

namespace Module.UnitTests.Profile.Domain.Addresses;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Entity", "Address")]
public class AddressMethodTests
{
    [Fact(DisplayName = "Create: Should return Address with correct properties")]
    public void Create_WithValidParameters_ShouldReturnAddress()
    {
        Result<Address> result = AddressMethod.Create("John", "123 Main St", "New York", "United States",
            lastName: "Doe", address2: "Apt 4", zipCode: "10001", phone: "555-0100",
            countryCode: "US", stateCode: "NY", stateProvince: "New York", label: "Home", isDefault: true);
        Address address = result.Value;

        result.IsSuccess.Should().BeTrue();
        address.FirstName.Should().Be("John");
        address.LastName.Should().Be("Doe");
        address.Address1.Should().Be("123 Main St");
        address.Address2.Should().Be("Apt 4");
        address.City.Should().Be("New York");
        address.ZipCode.Should().Be("10001");
        address.Phone.Should().Be("555-0100");
        address.CountryCode.Should().Be("US");
        address.StateCode.Should().Be("NY");
        address.CountryName.Should().Be("United States");
        address.StateProvince.Should().Be("New York");
        address.Label.Should().Be("Home");
        address.IsDefault.Should().BeTrue();
    }

    [Theory(DisplayName = "Create: Should fail when firstName is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyFirstName_ShouldReturnFailure(string? firstName)
    {
        Result<Address> result = AddressMethod.Create(firstName!, "123 Main St", "NYC", "USA",
            lastName: "Doe", zipCode: "10001", countryCode: "US");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(AddressResult.Failure.FirstNameRequired);
    }

    [Theory(DisplayName = "Create: Should fail when address1 is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyAddress1_ShouldReturnFailure(string? address1)
    {
        Result<Address> result = AddressMethod.Create("John", address1!, "NYC", "USA",
            lastName: "Doe", zipCode: "10001", countryCode: "US");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(AddressResult.Failure.Address1Required);
    }

    [Theory(DisplayName = "Create: Should fail when city is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyCity_ShouldReturnFailure(string? city)
    {
        Result<Address> result = AddressMethod.Create("John", "123 Main St", city!, "USA",
            lastName: "Doe", zipCode: "10001", countryCode: "US");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(AddressResult.Failure.CityRequired);
    }

    [Theory(DisplayName = "Create: Should fail when countryName is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyCountryName_ShouldReturnFailure(string? countryName)
    {
        Result<Address> result = AddressMethod.Create("John", "123 Main St", "NYC", countryName!,
            lastName: "Doe", zipCode: "10001", countryCode: "US");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(AddressResult.Failure.CountryNameRequired);
    }

    [Fact(DisplayName = "MarkAsDefault: Should set IsDefault to true")]
    public void MarkAsDefault_ShouldSetIsDefault()
    {
        Address address = AddressMethod.Create("John", "123 Main St", "NYC", "USA",
            lastName: "Doe", zipCode: "10001", countryCode: "US").Value;

        address.MarkAsDefault();

        address.IsDefault.Should().BeTrue();
    }

    [Fact(DisplayName = "FullName: Should combine first and last name")]
    public void FullName_ShouldCombineFirstAndLast()
    {
        Address address = AddressMethod.Create("John", "123 Main St", "NYC", "USA",
            lastName: "Doe", zipCode: "10001", countryCode: "US").Value;

        string fullName = address.FullName();

        fullName.Should().Be("John Doe");
    }

    [Fact(DisplayName = "FullName: Should return only firstName when lastName is empty")]
    public void FullName_WhenLastNameEmpty_ShouldReturnFirstName()
    {
        Address address = AddressMethod.Create("John", "123 Main St", "NYC", "USA",
            zipCode: "10001", countryCode: "US").Value;

        string fullName = address.FullName();

        fullName.Should().Be("John");
    }

    [Fact(DisplayName = "FullAddress: Should format complete address")]
    public void FullAddress_ShouldFormatCorrectly()
    {
        Address address = AddressMethod.Create("John", "123 Main St", "New York", "United States",
            lastName: "Doe", address2: "Apt 4", zipCode: "10001", countryCode: "US",
            stateCode: "NY", stateProvince: "New York").Value;

        string fullAddress = address.FullAddress();

        fullAddress.Should().Contain("John Doe");
        fullAddress.Should().Contain("123 Main St");
        fullAddress.Should().Contain("Apt 4");
        fullAddress.Should().Contain("New York");
        fullAddress.Should().Contain("United States");
    }
}