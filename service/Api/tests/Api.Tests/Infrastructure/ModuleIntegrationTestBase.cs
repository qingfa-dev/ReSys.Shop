using Module.Catalog.Persistence;
using Module.Location.Persistence;
using Module.Customer.Persistence;

using Shared.Security.Identity.Domain.Shared;

namespace Api.Tests.Infrastructure;

public abstract class ModuleIntegrationTestBase : ApiIntegrationTestBase
{
    protected ModuleIntegrationTestBase(ApiFixture fixture) : base(fixture)
    {
    }

    protected abstract IReadOnlyList<string> Schemas { get; }

    public override async ValueTask InitializeAsync()
    {
        await Fixture.ResetSchemasAsync(Schemas);
    }
}

[Collection("Catalog")]
public abstract class CatalogIntegrationTestBase : ModuleIntegrationTestBase
{
    protected CatalogIntegrationTestBase(ApiFixture fixture) : base(fixture)
    {
    }

    protected override IReadOnlyList<string> Schemas => [CatalogSchema.Name];
}

[Collection("Identity")]
public abstract class IdentityIntegrationTestBase : ModuleIntegrationTestBase
{
    protected IdentityIntegrationTestBase(ApiFixture fixture) : base(fixture)
    {
    }

    protected override IReadOnlyList<string> Schemas => [IdentitySchema.Name];
}

[Collection("Location")]
public abstract class LocationIntegrationTestBase : ModuleIntegrationTestBase
{
    protected LocationIntegrationTestBase(ApiFixture fixture) : base(fixture)
    {
    }

    protected override IReadOnlyList<string> Schemas => [LocationSchema.Name];
}

[Collection("Profile")]
public abstract class ProfileIntegrationTestBase : ModuleIntegrationTestBase
{
    protected ProfileIntegrationTestBase(ApiFixture fixture) : base(fixture)
    {
    }

    protected override IReadOnlyList<string> Schemas => [ProfileSchema.Name];
}
