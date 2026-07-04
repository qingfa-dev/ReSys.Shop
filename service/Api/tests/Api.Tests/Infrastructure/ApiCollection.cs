namespace Api.Tests.Infrastructure;

[CollectionDefinition("ApiIntegration")]
public sealed class ApiIntegrationCollection : ICollectionFixture<ApiFixture>;

[CollectionDefinition("Catalog")]
public sealed class CatalogIntegrationCollection : ICollectionFixture<ApiFixture>;

[CollectionDefinition("Identity")]
public sealed class IdentityIntegrationCollection : ICollectionFixture<ApiFixture>;

[CollectionDefinition("Location")]
public sealed class LocationIntegrationCollection : ICollectionFixture<ApiFixture>;

[CollectionDefinition("Profile")]
public sealed class ProfileIntegrationCollection : ICollectionFixture<ApiFixture>;
