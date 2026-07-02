namespace Api.Tests.Infrastructure;

[Collection("ApiIntegration")]
public abstract class ApiIntegrationTestBase : IAsyncLifetime
{
    private readonly ApiFixture _fixture;

    protected ApiIntegrationTestBase(ApiFixture fixture)
    {
        _fixture = fixture;
    }

    protected HttpClient Client => _fixture.Client;

    public async ValueTask InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
