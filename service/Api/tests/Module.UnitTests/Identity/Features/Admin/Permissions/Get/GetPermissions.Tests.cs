using Module.Identity.Features.Admin.Permissions.Get;

namespace Module.UnitTests.Identity.Features.Admin.Permissions.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Permissions")]
public class GetPermissionsTests
{
    private readonly GetPermissions.QueryHandler _handler;

    public GetPermissionsTests()
    {
        _handler = new GetPermissions.QueryHandler();
    }

    [Fact(DisplayName = "Should return all permissions as flat paged result")]
    public async Task Handle_ShouldReturnAllPermissions_AsFlatPagedResult()
    {
        // Arrange
        var query = new GetPermissions.Query();

        // Act
        var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Items.Should().NotBeEmpty();

        // Verify some known permissions exist
        result.Items.Should().Contain(p => p.Identifier == "admin.identity.users.create");
        result.Items.Should().Contain(p => p.Identifier == "admin.catalog.products.view");
    }
}
