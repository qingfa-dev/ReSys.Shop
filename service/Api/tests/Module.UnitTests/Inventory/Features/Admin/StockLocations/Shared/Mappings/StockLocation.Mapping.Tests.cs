using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Features.Admin.Shared.Mappings;
using Module.Inventory.Features.Admin.Shared.Models;

namespace Module.UnitTests.Inventory.Features.Admin.StockLocations.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockLocationMapping")]
public class StockLocationMappingTests
{
    [Fact(DisplayName = "ToDomain: Should map request to domain entity")]
    public void ToDomain_ShouldMapRequestToEntity()
    {
        var request = new StockLocationRequest
        {
            Name = "Main Warehouse",
            Presentation = "Main",
            Code = "WH-001",
            Address1 = "123 Main St",
            Address2 = "Suite 100",
            City = "New York",
            PostalCode = "10001",
            Phone = "+1-555-0100",
            Active = true,
            Default = true,
            BackorderableDefault = true,
            PropagateAllVariants = false,
            Position = 1,
        };

        var result = request.MapToDomain();
        var location = result.Value;

        result.IsSuccess.Should().BeTrue();
        location.Should().NotBeNull();
        location.Name.Should().Be(request.Name);
        location.Presentation.Should().Be(request.Presentation);
        location.Code.Should().Be(request.Code);
        location.Address1.Should().Be(request.Address1);
        location.Address2.Should().Be(request.Address2);
        location.City.Should().Be(request.City);
        location.PostalCode.Should().Be(request.PostalCode);
        location.Phone.Should().Be(request.Phone);
        location.Active.Should().Be(request.Active);
        location.Default.Should().Be(request.Default);
        location.BackorderableDefault.Should().Be(request.BackorderableDefault);
        location.PropagateAllVariants.Should().Be(request.PropagateAllVariants);
        location.Position.Should().Be(request.Position);
        location.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "ToDomain: Should use defaults for optional fields")]
    public void ToDomain_WhenOptionalFieldsAreDefault_ShouldUseDefaults()
    {
        var request = new StockLocationRequest
        {
            Name = "Default Location",
        };

        var result = request.MapToDomain();
        var location = result.Value;

        location.Name.Should().Be("Default Location");
        location.Active.Should().Be(StockLocationConstant.Defaults.Active);
        location.Default.Should().Be(StockLocationConstant.Defaults.Default);
        location.BackorderableDefault.Should().Be(StockLocationConstant.Defaults.BackorderableDefault);
        location.PropagateAllVariants.Should().Be(StockLocationConstant.Defaults.PropagateAllVariants);
        location.Position.Should().Be(StockLocationConstant.Defaults.Position);
    }

    [Fact(DisplayName = "ToDomain (Update): Should update existing entity from request")]
    public void ToDomain_Update_ShouldUpdateEntity()
    {
        var request = new StockLocationRequest
        {
            Name = "Updated Name",
            Presentation = "Updated",
            Active = false,
        };

        var location = StockLocationMethod.Create("Original").Value;

        var result = request.MapToDomain(location);

        result.IsSuccess.Should().BeTrue();
        location.Name.Should().Be("Updated Name");
        location.Presentation.Should().Be("Updated");
    }

    [Fact(DisplayName = "ToDetail: Should map entity to detail response")]
    public void ToDetail_ShouldMapEntityToDetail()
    {
        var entity = CreateStockLocation();

        var response = entity.MapToDetail<StockLocationDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(entity.Id);
        response.Name.Should().Be(entity.Name);
        response.Presentation.Should().Be(entity.Presentation);
        response.Code.Should().Be(entity.Code);
        response.Address1.Should().Be(entity.Address1);
        response.Address2.Should().Be(entity.Address2);
        response.City.Should().Be(entity.City);
        response.PostalCode.Should().Be(entity.PostalCode);
        response.Phone.Should().Be(entity.Phone);
        response.Active.Should().Be(entity.Active);
        response.Default.Should().Be(entity.Default);
        response.BackorderableDefault.Should().Be(entity.BackorderableDefault);
        response.PropagateAllVariants.Should().Be(entity.PropagateAllVariants);
        response.Position.Should().Be(entity.Position);
        response.CreatedAtUtc.Should().Be(entity.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(entity.ModifiedAtUtc);
    }

    [Fact(DisplayName = "ToDetail: Should handle null optional fields")]
    public void ToDetail_WhenOptionalFieldsAreNull_ShouldMapCorrectly()
    {
        var entity = CreateStockLocation(e =>
        {
            e.Presentation = null;
            e.Code = null;
            e.Address1 = null;
            e.Address2 = null;
            e.City = null;
            e.PostalCode = null;
            e.Phone = null;
            e.ModifiedAtUtc = null;
        });

        var response = entity.MapToDetail<StockLocationDetailResponse>();

        response.Presentation.Should().BeNull();
        response.Code.Should().BeNull();
        response.Address1.Should().BeNull();
        response.Address2.Should().BeNull();
        response.City.Should().BeNull();
        response.PostalCode.Should().BeNull();
        response.Phone.Should().BeNull();
        response.ModifiedAtUtc.Should().BeNull();
    }

    [Fact(DisplayName = "ToListItem: Should map entity to list item response")]
    public void ToListItem_ShouldMapEntityToList()
    {
        var entity = CreateStockLocation();

        var response = entity.MapToListItem<StockLocationListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(entity.Id);
        response.Name.Should().Be(entity.Name);
        response.Presentation.Should().Be(entity.Presentation);
        response.Code.Should().Be(entity.Code);
        response.Address1.Should().Be(entity.Address1);
        response.City.Should().Be(entity.City);
        response.PostalCode.Should().Be(entity.PostalCode);
        response.Phone.Should().Be(entity.Phone);
        response.Active.Should().Be(entity.Active);
        response.Default.Should().Be(entity.Default);
        response.BackorderableDefault.Should().Be(entity.BackorderableDefault);
        response.PropagateAllVariants.Should().Be(entity.PropagateAllVariants);
        response.Position.Should().Be(entity.Position);
        response.CreatedAtUtc.Should().Be(entity.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(entity.ModifiedAtUtc);
    }

    private static StockLocation CreateStockLocation(Action<StockLocation>? configure = null)
    {
        var location = new StockLocation
        {
            Id = Guid.NewGuid(),
            Name = "Main Warehouse",
            Presentation = "Main",
            Code = "WH-001",
            Address1 = "123 Main St",
            Address2 = "Suite 100",
            City = "New York",
            PostalCode = "10001",
            Phone = "+1-555-0100",
            Active = true,
            Default = true,
            BackorderableDefault = true,
            PropagateAllVariants = false,
            Position = 1,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
            ModifiedAtUtc = DateTimeOffset.UtcNow,
        };
        configure?.Invoke(location);
        return location;
    }
}
