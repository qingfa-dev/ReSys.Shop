using Shared.Application.Domain.Concerns.Parameterizable;
using Shared.Application.Domain.Models;

namespace Shared.UnitTests.Application.Domain.Concerns.Parameterizable;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Concerns")]
public class ParameterizableExtensionsTests
{
    private class TestEntity : Entity, IParameterizable
    {
        public string Name { get; set; } = string.Empty;
        public string? Presentation { get; set; }
    }

    [Fact(DisplayName = "WhereNameEquals should filter by exact name match")]
    public void WhereNameEquals_ShouldFilterByName()
    {
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Name = "hello_world" },
            new() { Name = "other" }
        }.AsQueryable();

        var result = data.WhereNameEquals("hello_world").ToList();

        result.Should().ContainSingle().Which.Name.Should().Be("hello_world");
    }

    [Fact(DisplayName = "WhereNameEquals should return empty when no match")]
    public void WhereNameEquals_NoMatch_ShouldReturnEmpty()
    {
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Name = "hello_world" }
        }.AsQueryable();

        var result = data.WhereNameEquals("nope").ToList();

        result.Should().BeEmpty();
    }

    [Fact(DisplayName = "WhereNotId should exclude entity with matching Id")]
    public void WhereNotId_ShouldExcludeMatchingId()
    {
        var targetId = Guid.NewGuid();
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Id = targetId, Name = "exclude" },
            new() { Id = Guid.NewGuid(), Name = "keep" }
        }.AsQueryable();

        var result = data.WhereNotId<TestEntity, Guid>(targetId).ToList();

        result.Should().ContainSingle().Which.Name.Should().Be("keep");
    }

    [Fact(DisplayName = "WhereDuplicateName should return matches for given name")]
    public void WhereDuplicateName_ShouldFindDuplicates()
    {
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Name = "duplicate" },
            new() { Name = "unique" }
        }.AsQueryable();

        var result = data.WhereDuplicateName<TestEntity, Guid>("duplicate").ToList();

        result.Should().ContainSingle().Which.Name.Should().Be("duplicate");
    }

    [Fact(DisplayName = "WhereDuplicateName should exclude own Id when excludeId provided")]
    public void WhereDuplicateName_WithExcludeId_ShouldExcludeOwn()
    {
        var currentId = Guid.NewGuid();
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Id = currentId, Name = "my-name" },
            new() { Id = Guid.NewGuid(), Name = "other-name" }
        }.AsQueryable();

        var result = data.WhereDuplicateName<TestEntity, Guid>("my-name", currentId).ToList();

        result.Should().BeEmpty();
    }

    [Fact(DisplayName = "WhereDuplicateName with default excludeId should not exclude anything")]
    public void WhereDuplicateName_DefaultExcludeId_ShouldNotExclude()
    {
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Name = "same-name" },
            new() { Name = "same-name" },
            new() { Name = "other" }
        }.AsQueryable();

        var result = data.WhereDuplicateName<TestEntity, Guid>("same-name").ToList();

        result.Should().HaveCount(2);
    }

    private sealed class MockEntity : TestEntity
    {
        public MockEntity(Guid id)
        {
            Id = id;
        }
    }
}
