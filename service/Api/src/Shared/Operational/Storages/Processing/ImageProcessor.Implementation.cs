using Shared.Operational.Storages.Models;

using SkiaSharp;

namespace Shared.Operational.Storages.Processing;

internal sealed partial class ImageProcessor(ILogger<ImageProcessor> logger) : IImageProcessor
{
    public Task<Result<Stream>> ProcessAsync(
        Stream inputStream,
        UploadOptions options,
        CancellationToken ct = default)
    {
        int? width = options.ResizeWidth;
        int? height = options.ResizeHeight;

        if (width is null && height is null && options.OutputFormat is null)
            return Task.FromResult(Result<Stream>.Ok(inputStream));

        ProcessingResizeMode mode = options.ResizeMode switch
        {
            Models.ResizeMode.Fit => ProcessingResizeMode.Fit,
            Models.ResizeMode.Fill => ProcessingResizeMode.Fill,
            Models.ResizeMode.Stretch => ProcessingResizeMode.Stretch,
            _ => ProcessingResizeMode.Stretch
        };

        bool maintainAspectRatio = options.MaintainAspectRatio;
        string? outputFormat = options.OutputFormat;

        if ((width is not null && width <= 0) || (height is not null && height <= 0))
            return Task.FromResult(ImageProcessorResult.Failure.InvalidDimensions(width, height));

        Loggers.LogProcessingStarted(logger, width, height, mode, maintainAspectRatio);

        try
        {
            SKEncodedImageFormat? sourceEncodedFormat;
            SKBitmap source;

            using (var codec = SKCodec.Create(inputStream))
            {
                if (codec is null)
                {
                    Loggers.LogProcessingFailed(logger, "Input stream does not contain a valid image.");
                    return Task.FromResult(ImageProcessorResult.Failure.InvalidImage);
                }

                sourceEncodedFormat = codec.EncodedFormat;
                source = SKBitmap.Decode(codec);
            }

            if (source is null)
            {
                Loggers.LogProcessingFailed(logger, "Input stream does not contain a valid image.");
                return Task.FromResult(ImageProcessorResult.Failure.InvalidImage);
            }

            using (source)
            {
                int sourceWidth = source.Width;
                int sourceHeight = source.Height;

                int targetWidth = width ?? (int)Math.Round(height!.Value * (double)sourceWidth / sourceHeight);
                int targetHeight = height ?? (int)Math.Round(width!.Value * (double)sourceHeight / sourceWidth);

                float drawX, drawY, drawW, drawH;

                if (!maintainAspectRatio || mode == ProcessingResizeMode.Stretch)
                {
                    drawX = 0;
                    drawY = 0;
                    drawW = targetWidth;
                    drawH = targetHeight;
                }
                else if (mode == ProcessingResizeMode.Fit)
                {
                    float scale = Math.Min((float)targetWidth / sourceWidth, (float)targetHeight / sourceHeight);
                    drawW = sourceWidth * scale;
                    drawH = sourceHeight * scale;
                    drawX = (targetWidth - drawW) / 2;
                    drawY = (targetHeight - drawH) / 2;
                }
                else
                {
                    float scale = Math.Max((float)targetWidth / sourceWidth, (float)targetHeight / sourceHeight);
                    drawW = sourceWidth * scale;
                    drawH = sourceHeight * scale;
                    drawX = (targetWidth - drawW) / 2;
                    drawY = (targetHeight - drawH) / 2;
                }

                using var target = new SKBitmap(targetWidth, targetHeight);
                using var canvas = new SKCanvas(target);
                canvas.Clear(SKColors.Transparent);

                using (var image = SKImage.FromBitmap(source))
                {
                    canvas.DrawImage(image, new SKRect(drawX, drawY, drawX + drawW, drawY + drawH), Constants.DefaultResampleOptions);
                }

                canvas.Flush();

                Func<SKImage, SKData>? encoder = ResolveEncoder(outputFormat, sourceEncodedFormat);

                using SKImage targetImage = SKImage.FromBitmap(target);
                using SKData encoded = encoder?.Invoke(targetImage) ?? Constants.FallbackEncoder(targetImage);

                var outputStream = new MemoryStream();
                encoded.SaveTo(outputStream);
                outputStream.Position = 0;

                Loggers.LogProcessingCompleted(logger, sourceWidth, sourceHeight, target.Width, target.Height, outputFormat ?? Constants.DefaultFormatLabel);

                return Task.FromResult(Result<Stream>.Ok(outputStream));
            }
        }
        catch (Exception ex)
        {
            Loggers.LogProcessingFailed(logger, ex.Message);
            return Task.FromResult(ImageProcessorResult.Failure.InvalidImage);
        }
    }

    private static Func<SKImage, SKData>? ResolveEncoder(string? outputFormat, SKEncodedImageFormat? sourceFormat)
    {
        if (outputFormat is not null)
            return Constants.FormatEncoders.TryGetValue(outputFormat, out Func<SKImage, SKData>? factory) ? factory : null;

        if (sourceFormat is null)
            return Constants.FallbackEncoder;

        string? formatKey = sourceFormat switch
        {
            SKEncodedImageFormat.Jpeg => "jpeg",
            SKEncodedImageFormat.Png => "png",
            SKEncodedImageFormat.Webp => "webp",
            _ => null
        };

        if (formatKey is null || !Constants.FormatEncoders.TryGetValue(formatKey, out Func<SKImage, SKData>? srcFactory))
            return Constants.FallbackEncoder;

        return srcFactory;
    }
}
