using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Storefront.Shared.Mappings;
using Module.Shipping.Features.Storefront.Shared.Models;

namespace Module.UnitTests.Shipping.Features.Storefront.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "ShippingMethodMapping")]
public class ShippingMethodMappingTests
{
    [Fact(DisplayName = "ToDetail: Should map entity to detail response")]
    public void ToDetail_ShouldMapEntityToDetail()
    {
        var entity = CreateShippingMethod();

        var response = entity.MapToDetail<ShippingMethodDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(entity.Id);
        response.MethodName.Should().Be(entity.Name);
        response.Description.Should().Be(entity.AdminName);
        response.Cost.Should().Be(0m);
        response.Currency.Should().Be("USD");
        response.IsActive.Should().Be(entity.AvailableToUsers);
        response.CreatedAtUtc.Should().Be(entity.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(entity.ModifiedAtUtc);
        response.CreatedBy.Should().Be(entity.CreatedBy);
        response.ModifiedBy.Should().Be(entity.ModifiedBy);
    }

    [Fact(DisplayName = "ToDetail: Should handle null AdminName as empty string")]
    public void ToDetail_WhenAdminNameIsNull_ShouldUseEmptyString()
    {
        var entity = CreateShippingMethod(e => e.AdminName = null);

        var response = entity.MapToDetail<ShippingMethodDetailResponse>();

        response.Description.Should().BeEmpty();
    }

    [Fact(DisplayName = "ToDetail: Should handle inactive shipping method")]
    public void ToDetail_WhenShippingMethodIsInactive_ShouldMapCorrectly()
    {
        var entity = CreateShippingMethod(e =>
        {
            e.AvailableToUsers = false;
            e.AdminName = "Express Delivery";
        });

        var response = entity.MapToDetail<ShippingMethodDetailResponse>();

        response.IsActive.Should().BeFalse();
        response.Description.Should().Be("Express Delivery");
    }

    [Fact(DisplayName = "ToListItem: Should map entity to list item response")]
    public void ToListItem_ShouldMapEntityToList()
    {
        var entity = CreateShippingMethod();

        var response = entity.MapToListItem<ShippingMethodListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(entity.Id);
        response.MethodName.Should().Be(entity.Name);
        response.Description.Should().Be(entity.AdminName);
        response.Cost.Should().Be(0m);
        response.Currency.Should().Be("USD");
        response.IsActive.Should().Be(entity.AvailableToUsers);
    }

    [Fact(DisplayName = "ToListItem: Should handle null ModifiedAtUtc and auditable fields")]
    public void ToListItem_WhenAuditableFieldsAreNull_ShouldMapCorrectly()
    {
        var entity = CreateShippingMethod(e =>
        {
            e.ModifiedAtUtc = null;
            e.CreatedBy = null;
            e.ModifiedBy = null;
        });

        var response = entity.MapToListItem<ShippingMethodListItemResponse>();

        response.Id.Should().Be(entity.Id);
        response.IsActive.Should().Be(entity.AvailableToUsers);
    }

    private static ShippingMethod CreateShippingMethod(Action<ShippingMethod>? configure = null)
    {
        var entity = new ShippingMethod
        {
            Id = Guid.NewGuid(),
            Name = "Standard Shipping",
            AdminName = "Standard Delivery (3-5 days)",
            AvailableToUsers = true,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-30),
            ModifiedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "admin",
            ModifiedBy = "admin",
        };
        configure?.Invoke(entity);
        return entity;
    }
}
