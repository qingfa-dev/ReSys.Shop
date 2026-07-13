using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Mappings;

/// <summary>
/// Maps between VariantImage domain entities and response DTOs.
/// </summary>
public static partial class VariantImageMapping
{
    /// <summary>
    /// Maps a VariantImage entity to a detail response DTO with all image attributes.
    /// </summary>
    /// <typeparam name="T">The response type deriving from <see cref="VariantImageDetailResponse"/>.</typeparam>
    /// <param name="entity">The variant image domain entity.</param>
    /// <returns>A detail response DTO populated from the entity.</returns>
    public static T MapToDetail<T>(this VariantImage entity) where T : VariantImageDetailResponse, new()
    {
        // Map: Domain entity fields to wire-format response DTO
        return new T
        {
            Id = entity.Id,
            VariantId = entity.VariantId,
            Url = entity.Url,
            Alt = entity.Alt,
            ContentType = entity.ContentType,
            FileName = entity.FileName,
            FileSize = entity.FileSize,
            Width = entity.Width,
            Height = entity.Height,
            DimensionsUnit = entity.DimensionsUnit,
            Position = entity.Position,
            Type = entity.Type.ToString(),
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }

    /// <summary>
    /// Maps a VariantImage entity to a download response DTO including the binary stream.
    /// </summary>
    /// <typeparam name="T">The response type deriving from <see cref="VariantImageDownloadResponse"/>.</typeparam>
    /// <param name="entity">The variant image domain entity.</param>
    /// <param name="stream">The binary stream from storage.</param>
    /// <returns>A download response DTO with entity metadata and the stream.</returns>
    public static T MapToDownload<T>(this VariantImage entity, Stream stream) where T : VariantImageDownloadResponse, new()
    {
        // Map: Domain entity fields + binary stream into download response
        return new T
        {
            Id = entity.Id,
            VariantId = entity.VariantId,
            Url = entity.Url,
            Alt = entity.Alt,
            ContentType = entity.ContentType,
            FileName = entity.FileName,
            FileSize = entity.FileSize,
            Width = entity.Width,
            Height = entity.Height,
            DimensionsUnit = entity.DimensionsUnit,
            Position = entity.Position,
            Type = entity.Type.ToString(),
            CreatedAtUtc = entity.CreatedAtUtc,
            Stream = stream,
        };
    }
}