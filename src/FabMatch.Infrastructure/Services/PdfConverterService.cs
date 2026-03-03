using FabMatch.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using PDFtoImage;

namespace FabMatch.Infrastructure.Services;

/// <summary>
/// Converts PDF files to PNG images using the PDFtoImage library (based on Pdfium).
/// Implements <see cref="IPdfConverterService"/>.
/// </summary>
public sealed class PdfConverterService : IPdfConverterService
{
    private readonly ILogger<PdfConverterService> _logger;

    public PdfConverterService(ILogger<PdfConverterService> logger)
        => _logger = logger;

    /// <inheritdoc />
    public async Task<IReadOnlyList<byte[]>> ConvertToPngAsync(
        byte[] pdfBytes,
        int dpi = 150,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Converting PDF ({Bytes} bytes) to PNG at {Dpi} dpi", pdfBytes.Length, dpi);

        // PDFtoImage is a synchronous library; wrap in Task.Run to avoid blocking
        return await Task.Run(() =>
        {
            using var ms = new MemoryStream(pdfBytes);
            var pages = new List<byte[]>();

            // Render all pages as PNG using PDFtoImage
            var bitmaps = Conversion.ToImages(ms, dpi: dpi);
            foreach (var bitmap in bitmaps)
            {
                using var outStream = new MemoryStream();
                // Save as PNG
                bitmap.Encode(outStream, SkiaSharp.SKEncodedImageFormat.Png, 100);
                pages.Add(outStream.ToArray());
            }

            _logger.LogDebug("PDF converted to {Count} PNG pages", pages.Count);
            return (IReadOnlyList<byte[]>)pages;
        }, ct);
    }
}
