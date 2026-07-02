using Shared.Application.Domain.Models;

namespace Shared.UnitTests.Application.Domain.Models;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Concerns")]
public class EntityTests
{
    private sealed class TestEntity : Entity
    {
    }

    private sealed class TestEntityString : Entity<string>
    {
        public void SetIdString(string id) => SetId(id);
    }

    [Fact(DisplayName = "Entity Constructor should auto-generate a non-empty Guid Id")]
    public void Constructor_ShouldAutoGenerateGuidId()
    {
        var entity = new TestEntity();
        entity.Id.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Equals should return true when two entities have the same Id")]
    public void Equals_SameId_ShouldReturnTrue()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity { Id = id };
        var entity2 = new TestEntity { Id = id };

        entity1.Equals(entity2).Should().BeTrue();
    }

    [Fact(DisplayName = "Equals should return false when two entities have different Ids")]
    public void Equals_DifferentId_ShouldReturnFalse()
    {
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        entity1.Equals(entity2).Should().BeFalse();
    }

    [Fact(DisplayName = "Equals should return false when comparing to null")]
    public void Equals_Null_ShouldReturnFalse()
    {
        var entity = new TestEntity();
        entity.Equals(null).Should().BeFalse();
    }

    [Fact(DisplayName = "Equals should return false when comparing to a different type")]
    public void Equals_DifferentType_ShouldReturnFalse()
    {
        var entity = new TestEntity();
        entity.Equals("not-an-entity").Should().BeFalse();
    }

    [Fact(DisplayName = "ReferenceEquals should short-circuit and return true")]
    public void Equals_SameReference_ShouldReturnTrue()
    {
        var entity = new TestEntity();
        entity.Equals(entity).Should().BeTrue();
    }

    [Fact(DisplayName = "Equals should return false when Id is null on one or both sides")]
    public void Equals_NullId_ShouldReturnFalse()
    {
        var entity1 = new TestEntityString();
        var entity2 = new TestEntityString();

        entity1.Equals(entity2).Should().BeFalse();
    }

    [Fact(DisplayName = "GetHashCode should return same hash for equal Ids")]
    public void GetHashCode_SameId_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var entity1 = new TestEntity { Id = id };
        var entity2 = new TestEntity { Id = id };

        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    [Fact(DisplayName = "GetHashCode should return different hash for different Ids")]
    public void GetHashCode_DifferentId_ShouldNotBeEqual()
    {
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        entity1.GetHashCode().Should().NotBe(entity2.GetHashCode());
    }

    [Fact(DisplayName = "GetHashCode should return 0 when Id is null")]
    public void GetHashCode_NullId_ShouldReturnZero()
    {
        var entity = new TestEntityString();
        entity.GetHashCode().Should().Be(0);
    }

    [Fact(DisplayName = "operator == should return true when both entities are null")]
    public void OperatorEquals_BothNull_ShouldReturnTrue()
    {
        TestEntity? a = null;
        TestEntity? b = null;

        (a == b).Should().BeTrue();
    }

    [Fact(DisplayName = "operator == should return false when one entity is null")]
    public void OperatorEquals_OneNull_ShouldReturnFalse()
    {
        var a = new TestEntity();
        TestEntity? b = null;

        (a == b).Should().BeFalse();
        (b == a).Should().BeFalse();
    }

    [Fact(DisplayName = "operator == should return true when both entities have same Id")]
    public void OperatorEquals_SameId_ShouldReturnTrue()
    {
        var id = Guid.NewGuid();
        var a = new TestEntity { Id = id };
        var b = new TestEntity { Id = id };

        (a == b).Should().BeTrue();
    }

    [Fact(DisplayName = "operator == should return false when entities have different Ids")]
    public void OperatorEquals_DifferentId_ShouldReturnFalse()
    {
        var a = new TestEntity();
        var b = new TestEntity();

        (a == b).Should().BeFalse();
    }

    [Fact(DisplayName = "operator != should return false when both entities have same Id")]
    public void OperatorNotEquals_SameId_ShouldReturnFalse()
    {
        var id = Guid.NewGuid();
        var a = new TestEntity { Id = id };
        var b = new TestEntity { Id = id };

        (a != b).Should().BeFalse();
    }

    [Fact(DisplayName = "operator != should return true when entities have different Ids")]
    public void OperatorNotEquals_DifferentId_ShouldReturnTrue()
    {
        var a = new TestEntity();
        var b = new TestEntity();

        (a != b).Should().BeTrue();
    }
}
