namespace Module.Dashboard.Features.Admin.Shared.Models;

/// <summary>Activity feed item source type.</summary>
public enum ActivityType
{
    Order,
    Stock
}

/// <summary>Activity feed item status.</summary>
public enum ActivityStatus
{
    Draft,
    Placed,
    Canceled,
    Expired,
    Completed
}