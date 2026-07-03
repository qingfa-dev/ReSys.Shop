namespace Module.Catalog.Domain.Products;

public static partial class ProductLoggers
{
    #region CRUD

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Debug,
        Message = "[Product.Created]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Created(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Debug,
        Message = "[Product.Updated]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Updated(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Debug,
        Message = "[Product.Deleted]: {Name} ({Id}) by {ActionBy}")]
    public static partial void Deleted(ILogger logger, string Name, Guid Id, string? ActionBy = "System");

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Debug,
        Message = "[Product.StatusChanged]: {Name} ({Id}) → {NewStatus} by {ActionBy}")]
    public static partial void StatusChanged(ILogger logger, string Name, Guid Id, ProductStatus NewStatus, string? ActionBy = "System");

    #endregion

    #region Storefront

    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Warning,
        Message = "[Storefront.ProductNotFound]: Product with slug '{Slug}' was not found")]
    public static partial void StorefrontProductNotFoundBySlug(ILogger logger, string Slug);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Warning,
        Message = "[Storefront.ProductNotFound]: Product with ID '{ProductId}' was not found")]
    public static partial void StorefrontProductNotFoundById(ILogger logger, Guid ProductId);

    [LoggerMessage(
        EventId = 5003,
        Level = LogLevel.Information,
        Message = "[Storefront.ProductDetailLoaded]: Product '{Slug}' ({Id}) loaded")]
    public static partial void StorefrontProductDetailLoaded(ILogger logger, string Slug, Guid Id);

    [LoggerMessage(
        EventId = 5004,
        Level = LogLevel.Information,
        Message = "[Storefront.RelatedProductsFound]: Found {Count} related products for product {ProductId}")]
    public static partial void StorefrontRelatedProductsFound(ILogger logger, int Count, Guid ProductId);

    [LoggerMessage(
        EventId = 5005,
        Level = LogLevel.Information,
        Message = "[Storefront.NoTaxonsFound]: Product {ProductId} has no taxon classifications")]
    public static partial void StorefrontNoTaxonsFound(ILogger logger, Guid ProductId);

    #endregion
}
