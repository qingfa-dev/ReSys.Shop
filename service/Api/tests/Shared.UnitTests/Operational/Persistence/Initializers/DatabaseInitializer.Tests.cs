#pragma warning disable CA1873

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Shared.Operational.Persistence.Data;
using Shared.Operational.Persistence.Initializers;
using Shared.Operational.Persistence.Seeders;

namespace Shared.UnitTests.Operational.Persistence.Initializers;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class DatabaseInitializerTests
{
    private sealed class TestDbContext(DbContextOptions<TestDbContext> options)
        : DbContext(options), IApplicationDbContext
    {
    }

    private static ServiceProvider BuildProvider(Action<IServiceCollection> configure)
    {
        ServiceCollection services = new();
        services.AddSingleton<ILoggerFactory>(LoggerFactory.Create(b => { }));
        configure(services);
        return services.BuildServiceProvider();
    }

    private static (ServiceProvider Provider, Mock<ILogger> LoggerMock) BuildProviderWithLogger(
        Action<IServiceCollection> configure)
    {
        ServiceCollection services = new();

        Mock<ILogger> loggerMock = new();
        loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        Mock<ILoggerFactory> factoryMock = new();
        factoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        services.AddSingleton<ILoggerFactory>(factoryMock.Object);

        configure(services);

        return (services.BuildServiceProvider(), loggerMock);
    }

    // ── Migration Tests ──────────────────────────────────────────

    [Fact(DisplayName = "InitializeAsync with a single DbContext should attempt migration")]
    public async Task InitializeAsync_WithSingleDbContext_ShouldAttemptMigration()
    {
        ServiceProvider provider = BuildProvider(services =>
        {
            services.AddScoped<IApplicationDbContext>(_ =>
            {
                DbContextOptionsBuilder<TestDbContext> builder = new();
                builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
                return new TestDbContext(builder.Options);
            });
        });

        Func<Task> act = () => provider.InitializeDatabaseAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "InitializeAsync with multiple DbContexts should not throw")]
    public async Task InitializeAsync_WithMultipleDbContexts_ShouldNotThrow()
    {
        ServiceProvider provider = BuildProvider(services =>
        {
            services.AddScoped<IApplicationDbContext>(_ =>
            {
                DbContextOptionsBuilder<TestDbContext> builder = new();
                builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
                return new TestDbContext(builder.Options);
            });
            services.AddScoped<IApplicationDbContext>(_ =>
            {
                DbContextOptionsBuilder<TestDbContext> builder = new();
                builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
                return new TestDbContext(builder.Options);
            });
        });

        Func<Task> act = () => provider.InitializeDatabaseAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Non-DbContext IApplicationDbContext implementation should be skipped")]
    public async Task InitializeAsync_NonDbContextImplementation_ShouldBeSkipped()
    {
        ServiceProvider provider = BuildProvider(services =>
        {
            services.AddScoped(_ => Mock.Of<IApplicationDbContext>());
        });

        Func<Task> act = () => provider.InitializeDatabaseAsync();

        await act.Should().NotThrowAsync();
    }

    // ── Seeder Tests ─────────────────────────────────────────────

    [Fact(DisplayName = "InitializeAsync with runSeeders=false should skip seeders and log")]
    public async Task InitializeAsync_WhenRunSeedersFalse_ShouldNotSeed()
    {
        (ServiceProvider provider, Mock<ILogger> loggerMock) = BuildProviderWithLogger(
            services =>
            {
                services.AddScoped<IDataSeeder>(_ => Mock.Of<IDataSeeder>());
            });

        await provider.InitializeDatabaseAsync(runSeeders: false);

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.Is<EventId>(e => e.Id == 266),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "InitializeAsync with no seeders should log and return")]
    public async Task InitializeAsync_WithNoSeeders_ShouldLogAndReturn()
    {
        (ServiceProvider provider, Mock<ILogger> loggerMock) = BuildProviderWithLogger(
            services => { });

        await provider.InitializeDatabaseAsync();

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.Is<EventId>(e => e.Id == 258),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "InitializeAsync with single seeder should execute it")]
    public async Task InitializeAsync_WithSingleSeeder_ShouldExecuteIt()
    {
        Mock<IDataSeeder> seederMock = new();
        seederMock.Setup(s => s.Order).Returns(1);
        seederMock
            .Setup(s => s.SeedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        ServiceProvider provider = BuildProvider(services =>
        {
            services.AddScoped<IDataSeeder>(_ => seederMock.Object);
        });

        await provider.InitializeDatabaseAsync();

        seederMock.Verify(s => s.SeedAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "InitializeAsync with multiple seeders should execute in order")]
    public async Task InitializeAsync_WithMultipleSeeders_ShouldExecuteInOrder()
    {
        List<string> executionOrder = [];
        Mock<IDataSeeder> seeder1 = new();
        seeder1.Setup(s => s.Order).Returns(2);
        seeder1
            .Setup(s => s.SeedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Callback(() => executionOrder.Add("Seeder2"));

        Mock<IDataSeeder> seeder2 = new();
        seeder2.Setup(s => s.Order).Returns(1);
        seeder2
            .Setup(s => s.SeedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Callback(() => executionOrder.Add("Seeder1"));

        Mock<IDataSeeder> seeder3 = new();
        seeder3.Setup(s => s.Order).Returns(3);
        seeder3
            .Setup(s => s.SeedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Callback(() => executionOrder.Add("Seeder3"));

        ServiceProvider provider = BuildProvider(services =>
        {
            services.AddScoped<IDataSeeder>(_ => seeder1.Object);
            services.AddScoped<IDataSeeder>(_ => seeder2.Object);
            services.AddScoped<IDataSeeder>(_ => seeder3.Object);
        });

        await provider.InitializeDatabaseAsync();

        executionOrder.Should().ContainInOrder("Seeder1", "Seeder2", "Seeder3");
    }

    [Fact(DisplayName = "InitializeAsync when seeder returns failure should log error")]
    public async Task InitializeAsync_WhenSeederReturnsFailure_ShouldLogError()
    {
        (ServiceProvider provider, Mock<ILogger> loggerMock) = BuildProviderWithLogger(
            services =>
            {
                Mock<IDataSeeder> seederMock = new();
                seederMock.Setup(s => s.Order).Returns(1);
                seederMock
                    .Setup(s => s.SeedAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Result.BadRequest(errors: [Error.Validation("TEST", "Something went wrong")]));
                services.AddScoped<IDataSeeder>(_ => seederMock.Object);
            });

        await provider.InitializeDatabaseAsync();

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.Is<EventId>(e => e.Id == 260),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "InitializeAsync when seeder throws should log and continue to next")]
    public async Task InitializeAsync_WhenSeederThrows_ShouldLogAndContinueToNext()
    {
        Mock<IDataSeeder> succeedingSeeder = new();
        succeedingSeeder.Setup(s => s.Order).Returns(2);
        succeedingSeeder
            .Setup(s => s.SeedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        ServiceProvider provider = BuildProvider(services =>
        {
            Mock<IDataSeeder> failingSeeder = new();
            failingSeeder.Setup(s => s.Order).Returns(1);
            failingSeeder
                .Setup(s => s.SeedAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Seeder crashed"));
            services.AddScoped<IDataSeeder>(_ => failingSeeder.Object);
            services.AddScoped<IDataSeeder>(_ => succeedingSeeder.Object);
        });

        await provider.InitializeDatabaseAsync();

        succeedingSeeder.Verify(s => s.SeedAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "InitializeAsync with mixed results should report counts")]
    public async Task InitializeAsync_MixedResults_ShouldReportCounts()
    {
        (ServiceProvider provider, Mock<ILogger> loggerMock) = BuildProviderWithLogger(
            services =>
            {
                Mock<IDataSeeder> successSeeder = new();
                successSeeder.Setup(s => s.Order).Returns(1);
                successSeeder
                    .Setup(s => s.SeedAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Result.Ok());

                Mock<IDataSeeder> failSeeder = new();
                failSeeder.Setup(s => s.Order).Returns(2);
                failSeeder
                    .Setup(s => s.SeedAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Result.BadRequest(errors: [Error.Validation("FAIL", "fail")]));

                services.AddScoped<IDataSeeder>(_ => successSeeder.Object);
                services.AddScoped<IDataSeeder>(_ => failSeeder.Object);
            });

        await provider.InitializeDatabaseAsync();

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.Is<EventId>(e => e.Id == 263),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // ── Parameter-Based Opt-Out Tests ────────────────────────────

    [Fact(DisplayName = "InitializeAsync with runMigrations=false should skip migrations and log")]
    public async Task InitializeAsync_WhenMigrationsDisabled_ShouldSkipAndLog()
    {
        (ServiceProvider provider, Mock<ILogger> loggerMock) = BuildProviderWithLogger(
            services =>
            {
                services.AddScoped<IApplicationDbContext>(_ =>
                {
                    DbContextOptionsBuilder<TestDbContext> builder = new();
                    builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
                    return new TestDbContext(builder.Options);
                });
            });

        await provider.InitializeDatabaseAsync(runMigrations: false);

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.Is<EventId>(e => e.Id == 265),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "InitializeAsync with runSeeders=false when seeders exist should skip")]
    public async Task InitializeAsync_WhenRunSeedersFalseWithSeeders_ShouldSkipAndLog()
    {
        (ServiceProvider provider, Mock<ILogger> loggerMock) = BuildProviderWithLogger(
            services =>
            {
                Mock<IDataSeeder> seederMock = new();
                seederMock.Setup(s => s.Order).Returns(1);
                services.AddScoped<IDataSeeder>(_ => seederMock.Object);
            });

        await provider.InitializeDatabaseAsync(runSeeders: false);

        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.Is<EventId>(e => e.Id == 266),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
