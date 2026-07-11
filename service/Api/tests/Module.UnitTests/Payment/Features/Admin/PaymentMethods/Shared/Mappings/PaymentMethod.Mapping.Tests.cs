using Module.Payment.Domain.PaymentMethods;
using PaymentRecord = Module.Payment.Domain.PaymentMethods.PaymentMethod;
using Module.Payment.Features.Admin.PaymentMethods.Shared.Mappings;
using Module.Payment.Features.Admin.PaymentMethods.Shared.Models;

namespace Module.UnitTests.Payment.Features.Admin.PaymentMethods.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "PaymentMethodMapping")]
public class PaymentMethodMappingTests
{
    [Fact(DisplayName = "MapToDetail: Should map PaymentMethod to detail response")]
    public void MapToDetail_ShouldMapEntityToDetail()
    {
        var method = CreatePaymentMethod();

        var response = method.MapToDetail<PaymentMethodDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(method.Id);
        response.Name.Should().Be(method.Name);
        response.Code.Should().Be(method.Code);
        response.Description.Should().Be(method.Description);
        response.ProviderKey.Should().Be(method.ProviderKey);
        response.AutoCapture.Should().Be(method.AutoCapture);
        response.DisplayOn.Should().Be(method.DisplayOn);
        response.Position.Should().Be(method.Position);
        response.Presentation.Should().Be(method.Presentation);
        response.Active.Should().Be(method.Active);
        response.WebhookEnabled.Should().Be(method.WebhookEnabled);
        response.CreatedAtUtc.Should().Be(method.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(method.ModifiedAtUtc);
        response.CreatedBy.Should().Be(method.CreatedBy);
        response.ModifiedBy.Should().Be(method.ModifiedBy);
    }

    [Fact(DisplayName = "MapToListItem: Should map PaymentMethod to list item response")]
    public void MapToListItem_ShouldMapEntityToList()
    {
        var method = CreatePaymentMethod();

        var response = method.MapToListItem<PaymentMethodListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(method.Id);
        response.Name.Should().Be(method.Name);
        response.Code.Should().Be(method.Code);
        response.ProviderKey.Should().Be(method.ProviderKey);
        response.AutoCapture.Should().Be(method.AutoCapture);
        response.DisplayOn.Should().Be(method.DisplayOn);
        response.Position.Should().Be(method.Position);
        response.Presentation.Should().Be(method.Presentation);
        response.Active.Should().Be(method.Active);
        response.WebhookEnabled.Should().Be(method.WebhookEnabled);
        response.CreatedAtUtc.Should().Be(method.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(method.ModifiedAtUtc);
    }

    [Fact(DisplayName = "MapToDomain: Should map request to new PaymentMethod entity")]
    public void MapToDomain_Create_ShouldMapRequestToEntity()
    {
        var request = new PaymentMethodRequest
        {
            Name = "Credit Card",
            Code = "CC",
            ProviderKey = "stripe",
            AutoCapture = true,
            DisplayOn = DisplayOn.Backend,
            Settings = new Dictionary<string, string> { ["key"] = "val" }
        };

        var result = request.MapToDomain();
        var entity = result.Value;

        result.IsSuccess.Should().BeTrue();
        entity.Should().NotBeNull();
        entity.Name.Should().Be(request.Name);
        entity.Code.Should().Be(request.Code);
        entity.ProviderKey.Should().Be(request.ProviderKey);
        entity.AutoCapture.Should().Be(request.AutoCapture);
        entity.DisplayOn.Should().Be(request.DisplayOn);
        entity.Settings.Should().ContainKey("key");
    }

    [Fact(DisplayName = "MapToDomain (Update): Should map request onto existing PaymentMethod entity")]
    public void MapToDomain_Update_ShouldUpdateEntity()
    {
        var method = CreatePaymentMethod();
        var request = new PaymentMethodRequest
        {
            Name = "Updated Name",
            Code = "UPD",
            ProviderKey = "stripe",
            AutoCapture = false,
            DisplayOn = DisplayOn.Frontend,
            Description = "Updated"
        };

        var result = request.MapToDomain(method);

        result.IsSuccess.Should().BeTrue();
        method.Name.Should().Be("Updated Name");
        method.Code.Should().Be("UPD");
        method.AutoCapture.Should().BeFalse();
        method.DisplayOn.Should().Be(DisplayOn.Frontend);
        method.Description.Should().Be("Updated");
    }

    [Fact(DisplayName = "MapUpdateToDomain: Should apply partial update to PaymentMethod")]
    public void MapUpdateToDomain_ShouldApplyPatch()
    {
        var method = CreatePaymentMethod();
        var patchRequest = new PaymentMethodUpdateRequest
        {
            Name = "Patched Name",
            Description = "Patched description"
        };

        var result = patchRequest.MapUpdateToDomain(method);

        result.IsSuccess.Should().BeTrue();
        method.Name.Should().Be("Patched Name");
        method.Description.Should().Be("Patched description");
        method.Code.Should().NotBeNull(); // unchanged fields preserved
    }

    private static PaymentRecord CreatePaymentMethod()
    {
        var result = PaymentMethodMethod.Create(
            "Credit Card", "CC", "stripe", true, DisplayOn.Both,
            new Dictionary<string, string> { ["key"] = "val" }, "Test method");
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}
