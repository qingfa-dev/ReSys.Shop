using Shared.Application.Domain.Concerns.Sluggable;
using Shared.Application.Domain.Models;

namespace Shared.UnitTests.Application.Domain.Concerns.Sluggable;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Concerns")]
public class SluggableExtensionsTests
{
    private class TestEntity : Entity, ISluggable
    {
        public string Slug { get; set; } = string.Empty;
    }

    [Fact(DisplayName = "WhereSlugEquals should filter by slug")]
    public void WhereSlugEquals_ShouldFilterBySlug()
    {
        // Arrange
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Slug = "match" },
            new() { Slug = "no-match" }
        }.AsQueryable();

        // Act
        var result = data.WhereSlugEquals("match").ToList();

        // Assert
        result.Should().ContainSingle().Which.Slug.Should().Be("match");
    }

    [Fact(DisplayName = "WhereDuplicateSlug should find duplicates for new entities")]
    public void WhereDuplicateSlug_ForNewEntity_ShouldFindDuplicates()
    {
        // Arrange
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new() { Slug = "duplicate" },
            new() { Slug = "unique" }
        }.AsQueryable();

        // Act
        var result = data.WhereDuplicateSlug<TestEntity, Guid>("duplicate").ToList();

        // Assert
        result.Should().ContainSingle().Which.Slug.Should().Be("duplicate");
    }

    [Fact(DisplayName = "WhereDuplicateSlug should exclude own ID for updates")]
    public void WhereDuplicateSlug_ForUpdate_ShouldExcludeOwnId()
    {
        // Arrange
        var currentId = Guid.NewGuid();
        IQueryable<TestEntity> data = new List<TestEntity>
        {
            new MockEntity(currentId) { Slug = "my-slug" },
            new MockEntity(Guid.NewGuid()) { Slug = "other-slug" }
        }.AsQueryable();

        // Act
        var result = data.WhereDuplicateSlug("my-slug", currentId).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    private sealed class MockEntity : TestEntity
    {
        public MockEntity(Guid id)
        {
            Id = id;
        }
    }
}
