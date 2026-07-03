using Module.Catalog.Features.Storefront.Digitals.Get.DownloadLink;

namespace Module.UnitTests.Catalog.Features.Storefront.Digitals.Get.DownloadLink;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontDigitalDownload")]
public class GenerateDigitalDownloadLinkTests
{
    [Fact(DisplayName = "Handler: Should generate download response with signed URL")]
    public async Task Handle_ShouldReturnDownloadUrl()
    {
        var handler = new GenerateDownloadLink.QueryHandler();
        var digitalId = Guid.NewGuid();

        var result = await handler.Handle(new GenerateDownloadLink.Query(digitalId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DigitalId.Should().Be(digitalId);
        result.Value.DownloadUrl.Should().Contain(digitalId.ToString());
        result.Value.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }
}
