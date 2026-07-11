using Hangfire;
using Hangfire.States;

using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Services;
using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Mappings;

using Shared.Operational.Storages.Models;
using Shared.Operational.Storages.Services;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Upload;

/// <summary>
/// Defines the use case for uploading a variant image.
/// </summary>
public static partial class UploadVariantImage
{
    public sealed record Command(Guid VariantId, Request Request) : ICommand<Response>;

    /// <summary>
    /// Handles image upload: validates variant, uploads to storage, creates entity, persists.
    /// </summary>
    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        IStorageService storageService,
        IBackgroundJobClient? backgroundJobClient,
        ILogger<CommandHandler> logger,
        ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        // Guard: Only allow known image extensions via storage validator pipeline
        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

        /// <summary>
        /// Executes the upload pipeline: variant check → storage upload → entity creation → persistence.
        /// </summary>
        /// <param name="command">The command containing variant ID and upload request.</param>
        /// <param name="cancellationToken">Propagates cancellation notification.</param>
        /// <returns>A created result with the new image detail, or a failure result.</returns>
        // Contract: pre=command!=null, post=result!=null
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var (variantId, request) = command;

            // Check: Parent variant must exist before accepting image uploads
            var variantExists = await dbContext.Set<Variant>()
                .AnyAsync(x => x.Id == variantId, cancellationToken);
            if (!variantExists)
                return VariantResult.Errors.NotFound(variantId);

            // Initialize: Storage subdirectory scoped to variant for organisational isolation
            var subdirectory = $"catalog/variants/{variantId}/images";
            var options = new UploadOptions
            {
                ScanForMalware = true,
                GenerateHash = true
            };

            // Acquire: Read stream from the uploaded file for storage pipeline
            await using var stream = request.File.OpenReadStream();

            // Call: Storage service upload pipeline — validates, scans, encrypts, stores
            var uploadResult = await storageService.UploadAsync(
                new UploadRequest(
                    Key: $"{subdirectory}/{request.File.FileName}", 
                    Content: stream,
                    ContentType: request.File.ContentType, 
                    Options: options),
                ct: cancellationToken);
            if (uploadResult.IsFailure)
                return uploadResult.Errors;

            var fileResult = uploadResult.Value;

            // Parse: Convert request type string to domain enum, fall back to Default
            var imageType = Enum.TryParse<VariantImageType>(request.Type, ignoreCase: true, out var parsedType)
                ? parsedType
                : VariantImageType.Default;

            // Create: VariantImage domain entity from storage result metadata
            var fileName = Path.GetFileName(fileResult.Key);
            var createResult = Module.Catalog.Domain.Products.Variants.Images.VariantImageMethod.Create(
                contentType: request.File.ContentType,
                fileName: fileName,
                fileSize: (int)fileResult.SizeBytes,
                url: fileResult.Uri?.ToString() ?? string.Empty,
                storagePath: fileResult.Key,
                position: request.Position,
                alt: request.Alt,
                type: imageType,
                variantId: variantId);
            if (createResult.IsFailure)
                return createResult.Errors;

            var image = createResult.Value;

            dbContext.Set<VariantImage>().Add(image);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record image creation event for observability
            VariantImageLoggers.Created(logger, Id: image.Id, VariantId: image.VariantId ?? Guid.Empty, FileName: image.FileName, ActionBy: currentUser.UserName);

            // Enqueue: Trigger background embedding generation for search-type images
            if (imageType == VariantImageType.Search)
            {
                var modelName = VariantImageConstant.Defaults.DefaultEmbeddingModel;
                backgroundJobClient?.Create<IEmbeddingOrchestrator>(
                    orchestrator => orchestrator.GenerateAndPersistAsync(image.Id, modelName, CancellationToken.None),
                    new EnqueuedState());
            }

            // Map: Return created image as detail DTO with 201 response
            return Result<Response>.Created(
                image.MapToDetail<Response>(),
                VariantImageResult.Success.Created(image.Id));
        }
    }
}
