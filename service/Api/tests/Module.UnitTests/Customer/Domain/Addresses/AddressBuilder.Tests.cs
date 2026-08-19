using Module.Customer.Domain.Addresses;

namespace Module.UnitTests.Profile.Domain.Addresses;

[Trait("Category", "Unit")]
[Trait("Module", "Profiles")]
[Trait("Feature", "AddressMethods")]
public class AddressMethodCreateAndUpdateTests
{
    private const string FirstName = "John";
    private const string Address1 = "123 Main St";
    private const string City = "New York";
    private const string CountryName = "United States";

    [Theory(DisplayName = "Create should fail when firstName is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyFirstName_ShouldReturnFailure(string? firstName)
    {
        Result<Address> result = AddressMethod.Create(firstName!, Address1, City, CountryName);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.FirstNameRequired.Code);
    }

    [Theory(DisplayName = "Create should fail when address1 is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyAddress1_ShouldReturnFailure(string? address1)
    {
        Result<Address> result = AddressMethod.Create(FirstName, address1!, City, CountryName);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.Address1Required.Code);
    }

    [Theory(DisplayName = "Create should fail when city is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyCity_ShouldReturnFailure(string? city)
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, city!, CountryName);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.CityRequired.Code);
    }

    [Theory(DisplayName = "Create should fail when countryName is empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyCountryName_ShouldReturnFailure(string? countryName)
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, countryName!);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(AddressResult.Failure.CountryNameRequired.Code);
    }

    [Fact(DisplayName = "Create should return success with valid required fields and defaults")]
    public void Create_WithValidFields_ShouldReturnSuccessWithDefaults()
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, CountryName);

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(FirstName);
        result.Value.Address1.Should().Be(Address1);
        result.Value.City.Should().Be(City);
        result.Value.CountryName.Should().Be(CountryName);
        result.Value.AddressType.Should().Be(AddressType.Shipping);
        result.Value.IsDefault.Should().BeFalse();
        result.Value.UserProfileId.Should().BeNull();
    }

    [Fact(DisplayName = "Create with all fields via named params should produce correct address")]
    public void Create_WithAllFields_ShouldProduceCorrectAddress()
    {
        Guid userProfileId = Guid.NewGuid();
        Result<Address> result = AddressMethod.Create(
            firstName: "Jane",
            address1: "456 Oak Ave",
            city: "Los Angeles",
            countryName: "United States",
            lastName: "Doe",
            address2: "Suite 200",
            zipCode: "90001",
            phone: "+1987654321",
            label: "Work",
            isDefault: true,
            isDefaultBilling: true,
            isDefaultShipping: false,
            stateProvince: "CA",
            countryCode: "US",
            stateCode: "CA",
            addressType: AddressType.Billing,
            userProfileId: userProfileId);

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("Jane");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Address1.Should().Be("456 Oak Ave");
        result.Value.Address2.Should().Be("Suite 200");
        result.Value.City.Should().Be("Los Angeles");
        result.Value.ZipCode.Should().Be("90001");
        result.Value.Phone.Should().Be("+1987654321");
        result.Value.Label.Should().Be("Work");
        result.Value.IsDefault.Should().BeTrue();
        result.Value.IsDefaultBilling.Should().BeTrue();
        result.Value.IsDefaultShipping.Should().BeFalse();
        result.Value.StateProvince.Should().Be("CA");
        result.Value.CountryCode.Should().Be("US");
        result.Value.StateCode.Should().Be("CA");
        result.Value.CountryName.Should().Be("United States");
        result.Value.AddressType.Should().Be(AddressType.Billing);
        result.Value.UserProfileId.Should().Be(userProfileId);
    }

    [Theory(DisplayName = "Update should set lastName")]
    [InlineData("Smith")]
    [InlineData(null)]
    public void Update_ShouldSetLastName(string? lastName)
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, CountryName)
            .Value.Update(lastName: lastName);

        result.Value.LastName.Should().Be(lastName);
    }

    [Fact(DisplayName = "Update should set address2")]
    public void Update_ShouldSetAddress2()
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, CountryName)
            .Value.Update(address2: "Apt 4B");

        result.Value.Address2.Should().Be("Apt 4B");
    }

    [Fact(DisplayName = "Update should set zipCode")]
    public void Update_ShouldSetZipCode()
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, CountryName)
            .Value.Update(zipCode: "10001");

        result.Value.ZipCode.Should().Be("10001");
    }

    [Fact(DisplayName = "Update should set phone")]
    public void Update_ShouldSetPhone()
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, CountryName)
            .Value.Update(phone: "+1234567890");

        result.Value.Phone.Should().Be("+1234567890");
    }

    [Fact(DisplayName = "Update should set label")]
    public void Update_ShouldSetLabel()
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, CountryName)
            .Value.Update(label: "Home");

        result.Value.Label.Should().Be("Home");
    }

    [Fact(DisplayName = "Update should set isDefault")]
    public void Update_ShouldSetIsDefault()
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, CountryName)
            .Value.Update(isDefault: true);

        result.Value.IsDefault.Should().BeTrue();
    }

    [Fact(DisplayName = "Update should set isDefaultBilling")]
    public void Update_ShouldSetIsDefaultBilling()
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, CountryName)
            .Value.Update(isDefaultBilling: true);

        result.Value.IsDefaultBilling.Should().BeTrue();
    }

    [Fact(DisplayName = "Update should set isDefaultShipping")]
    public void Update_ShouldSetIsDefaultShipping()
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, CountryName)
            .Value.Update(isDefaultShipping: true);

        result.Value.IsDefaultShipping.Should().BeTrue();
    }

    [Fact(DisplayName = "Update should set stateProvince")]
    public void Update_ShouldSetStateProvince()
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, CountryName)
            .Value.Update(stateProvince: "NY");

        result.Value.StateProvince.Should().Be("NY");
    }

    [Fact(DisplayName = "Update should set countryCode")]
    public void Update_ShouldSetCountryCode()
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, CountryName)
            .Value.Update(countryCode: "US");

        result.Value.CountryCode.Should().Be("US");
    }

    [Fact(DisplayName = "Update should set stateCode")]
    public void Update_ShouldSetStateCode()
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, CountryName)
            .Value.Update(stateCode: "NY");

        result.Value.StateCode.Should().Be("NY");
    }

    [Fact(DisplayName = "Update should set countryName")]
    public void Update_ShouldSetCountryName()
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, CountryName)
            .Value.Update(countryName: "Canada");

        result.Value.CountryName.Should().Be("Canada");
    }

    [Fact(DisplayName = "Update should set addressType")]
    public void Update_ShouldSetAddressType()
    {
        Result<Address> result = AddressMethod.Create(FirstName, Address1, City, CountryName)
            .Value.Update(addressType: AddressType.Billing);

        result.Value.AddressType.Should().Be(AddressType.Billing);
    }
}
