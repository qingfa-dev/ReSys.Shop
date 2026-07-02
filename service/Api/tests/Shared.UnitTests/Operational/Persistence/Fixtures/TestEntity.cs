using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Domain.Concerns.Versionable;
using Shared.Application.Domain.Models;

namespace Shared.UnitTests.Operational.Persistence.Fixtures;

public class TestAuditableEntity : Entity, IAuditable
{
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
}

public class TestVersionedEntity : Entity, IVersionable
{
    public uint Version { get; set; }
}

public class TestSoftDeletedEntity : Entity, ISoftDeletable
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}


public class TestNonAuditableEntity : Entity;

public class TestNonVersionedEntity : Entity;

public class TestNonSoftDeletedEntity : Entity;

