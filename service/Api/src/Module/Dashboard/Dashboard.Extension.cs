namespace Module.Dashboard;

// @CAT-10 Boundary: Module -> Host — this is the composition root for the Dashboard module DI registration
public static class DashboardExtension
{
    /// <summary>
    /// Registers Dashboard module services into the dependency injection container.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static WebApplicationBuilder AddDashboardModule(this WebApplicationBuilder builder)
    {
        return builder;
    }
}
