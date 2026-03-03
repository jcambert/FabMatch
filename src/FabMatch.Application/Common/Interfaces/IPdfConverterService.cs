namespace FabMatch.Application.Common.Interfaces;

/// <summary>
/// Converts PDF documents to PNG/JPEG images for AI analysis.
/// </summary>
public interface IPdfConverterService
{
    /// <summary>
    /// Converts each page of a PDF to a PNG image.
    /// </summary>
    /// <param name="pdfBytes">Raw PDF file bytes.</param>
    /// <param name="dpi">Target resolution (default 150 dpi gives a good balance of quality/size).</param>
    /// <returns>An ordered list of PNG byte arrays, one per page.</returns>
    Task<IReadOnlyList<byte[]>> ConvertToPngAsync(byte[] pdfBytes, int dpi = 150, CancellationToken ct = default);
}
