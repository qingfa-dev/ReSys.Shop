namespace Module.Catalog.Features.Storefront.Products.SearchByImage;

/// <summary>
/// Defines the use case for searching products by image upload.
/// </summary>
public static partial class SearchByImage
{
    public sealed record Command(IFormFile Image) : ICommand<Response>;

    public sealed class QueryHandler : ICommandHandler<Command, Response>
    {
        /// <summary>
        /// Handles the image search command (scaffold — delegates to inference service in production).
        /// </summary>
        /// <param name="command">The command containing the uploaded image.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A placeholder response indicating scaffold mode.</returns>
        // Contract: pre=command!=null, post=result!=null
        public Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Note: In a production setup, this would:
            // 1. Save the uploaded image to temp storage
            // 2. Call the inference service (sidecar) to get embedding vector
            // 3. Use the vector for similarity search via pgvector
            //
            // For now, this is a scaffold that demonstrates the endpoint wiring.
            // The actual inference integration requires the running sidecar service.

            var image = command.Image;

            // Check: Image file is required.
            if (image is null || image.Length == 0)
                return Task.FromResult<Result<Response>>(new Response { Items = [] });

            // Placeholder: In production, call inference client to generate vector.
            return Task.FromResult<Result<Response>>(new Response
            {
                Message = "Image search requires inference service. Upload received but no embedding generated in scaffold mode."
            });
        }
    }
}
