using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.Entities;
using Shared.Application.Domain.Concerns.Sluggable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Concerns.Versionable;
using Shared.Application.Domain.Models;
using Shared.Operational.Persistence.Configurations.Models;

namespace Shared.UnitTests.Operational.Persistence.Configurations.Models;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public class EntityModelConfigurationTests
{
    private sealed class TestEntityOnly : Entity;

    private sealed class TestAuditableEntity : Entity, IAuditable
    {
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset? ModifiedAtUtc { get; set; }
        public String? CreatedBy { get; set; }
        public String? ModifiedBy { get; set; }
    }

    private sealed class TestVersionableEntity : Entity, IVersionable
    {
        public UInt32 Version { get; set; }
    }

    private sealed class TestSoftDeletableEntity : Entity, ISoftDeletable
    {
        public Boolean IsDeleted { get; set; }
        public DateTimeOffset? DeletedAtUtc { get; set; }
        public String? DeletedBy { get; set; }
    }

    private sealed class TestSluggableEntity : Entity, ISluggable
    {
        public String Slug { get; set; } = String.Empty;
    }

    private sealed class TestFullEntity : Entity, IAuditable, IVersionable, ISoftDeletable, ISluggable
    {
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset? ModifiedAtUtc { get; set; }
        public String? CreatedBy { get; set; }
        public String? ModifiedBy { get; set; }
        public UInt32 Version { get; set; }
        public Boolean IsDeleted { get; set; }
        public DateTimeOffset? DeletedAtUtc { get; set; }
        public String? DeletedBy { get; set; }
        public String Slug { get; set; } = String.Empty;
    }

    private static IModel BuildModelWithConfiguration(Type[] entityTypes)
    {
        ModelBuilder modelBuilder = new();

        foreach (Type entityType in entityTypes)
        {
            modelBuilder.Entity(entityType);
        }

        EntityModelConfiguration.ConfigureModel(modelBuilder, isNpgsql: false);

        return modelBuilder.FinalizeModel();
    }

    public class EntityConfiguration
    {
        [Fact]
        public void ShouldConfigureIdAsValueGeneratedNever()
        {
            IModel model = BuildModelWithConfiguration([typeof(TestEntityOnly)]);
            IEntityType entityType = model.FindEntityType(typeof(TestEntityOnly))!;
            IReadOnlyProperty idProperty = entityType.FindProperty(nameof(IEntity.Id))!;

            idProperty.ValueGenerated.Should().Be(ValueGenerated.Never);
        }
    }

    public class AuditableConfiguration
    {
        [Fact]
        public void ShouldConfigureCreatedAtUtcAsRequired()
        {
            IModel model = BuildModelWithConfiguration([typeof(TestAuditableEntity)]);
            IEntityType entityType = model.FindEntityType(typeof(TestAuditableEntity))!;
            IReadOnlyProperty property = entityType.FindProperty(nameof(IAuditable.CreatedAtUtc))!;

            property.IsNullable.Should().BeFalse();
        }

        [Fact]
        public void ShouldConfigureCreatedByWithMaxLength()
        {
            IModel model = BuildModelWithConfiguration([typeof(TestAuditableEntity)]);
            IEntityType entityType = model.FindEntityType(typeof(TestAuditableEntity))!;
            IReadOnlyProperty property = entityType.FindProperty(nameof(IAuditable.CreatedBy))!;

            property.GetMaxLength().Should().Be(AuditableConstant.Constraints.MaxCreatedByLength);
        }

        [Fact]
        public void ShouldConfigureModifiedByWithMaxLength()
        {
            IModel model = BuildModelWithConfiguration([typeof(TestAuditableEntity)]);
            IEntityType entityType = model.FindEntityType(typeof(TestAuditableEntity))!;
            IReadOnlyProperty property = entityType.FindProperty(nameof(IAuditable.ModifiedBy))!;

            property.GetMaxLength().Should().Be(AuditableConstant.Constraints.MaxModifiedByLength);
        }
    }

    public class VersionableConfiguration
    {
        [Fact]
        public void ShouldConfigureVersionAsConcurrencyToken()
        {
            IModel model = BuildModelWithConfiguration([typeof(TestVersionableEntity)]);
            IEntityType entityType = model.FindEntityType(typeof(TestVersionableEntity))!;
            IReadOnlyProperty property = entityType.FindProperty(nameof(IVersionable.Version))!;

            property.IsConcurrencyToken.Should().BeTrue();
        }
    }

    public class SoftDeletableConfiguration
    {
        [Fact]
        public void ShouldConfigureIsDeletedAsRequired()
        {
            IModel model = BuildModelWithConfiguration([typeof(TestSoftDeletableEntity)]);
            IEntityType entityType = model.FindEntityType(typeof(TestSoftDeletableEntity))!;
            IReadOnlyProperty property = entityType.FindProperty(nameof(ISoftDeletable.IsDeleted))!;

            property.IsNullable.Should().BeFalse();
        }

        [Fact]
        public void ShouldConfigureDeletedByWithMaxLength()
        {
            IModel model = BuildModelWithConfiguration([typeof(TestSoftDeletableEntity)]);
            IEntityType entityType = model.FindEntityType(typeof(TestSoftDeletableEntity))!;
            IReadOnlyProperty property = entityType.FindProperty(nameof(ISoftDeletable.DeletedBy))!;

            property.GetMaxLength().Should().Be(100);
        }

        [Fact]
        public void ShouldAddQueryFilter()
        {
            IModel model = BuildModelWithConfiguration([typeof(TestSoftDeletableEntity)]);
            IEntityType entityType = model.FindEntityType(typeof(TestSoftDeletableEntity))!;

            IQueryFilter? filter = entityType.GetDeclaredQueryFilters().FirstOrDefault();
            filter.Should().NotBeNull();
        }

        [Fact]
        public void QueryFilterShouldFilterOutDeletedEntities()
        {
            IModel model = BuildModelWithConfiguration([typeof(TestSoftDeletableEntity)]);
            IEntityType entityType = model.FindEntityType(typeof(TestSoftDeletableEntity))!;

            entityType.GetDeclaredQueryFilters().Should().HaveCount(1);
        }
    }

    public class SluggableConfiguration
    {
        [Fact]
        public void ShouldConfigureSlugAsRequired()
        {
            IModel model = BuildModelWithConfiguration([typeof(TestSluggableEntity)]);
            IEntityType entityType = model.FindEntityType(typeof(TestSluggableEntity))!;
            IReadOnlyProperty property = entityType.FindProperty(nameof(ISluggable.Slug))!;

            property.IsNullable.Should().BeFalse();
        }

        [Fact]
        public void ShouldConfigureSlugWithMaxLength()
        {
            IModel model = BuildModelWithConfiguration([typeof(TestSluggableEntity)]);
            IEntityType entityType = model.FindEntityType(typeof(TestSluggableEntity))!;
            IReadOnlyProperty property = entityType.FindProperty(nameof(ISluggable.Slug))!;

            property.GetMaxLength().Should().Be(SluggableConstant.Constraints.MaxSlugLength);
        }
    }

    public class CombinedConfiguration
    {
        [Fact]
        public void ShouldApplyMultipleConcernsToSingleEntity()
        {
            IModel model = BuildModelWithConfiguration([typeof(TestFullEntity)]);
            IEntityType entityType = model.FindEntityType(typeof(TestFullEntity))!;

            // Entity: Id is ValueGeneratedNever
            entityType.FindProperty(nameof(IEntity.Id))!.ValueGenerated.Should().Be(ValueGenerated.Never);

            // Auditable: CreatedBy has max length
            entityType.FindProperty(nameof(IAuditable.CreatedBy))!.GetMaxLength().Should().Be(
                AuditableConstant.Constraints.MaxCreatedByLength);

            // Versionable: Version is concurrency token
            entityType.FindProperty(nameof(IVersionable.Version))!.IsConcurrencyToken.Should().BeTrue();

            // SoftDeletable: query filter applied
            entityType.GetDeclaredQueryFilters().Should().NotBeEmpty();
        }
    }
}
