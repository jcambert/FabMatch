namespace FabMatch.Domain.Enums;

/// <summary>Supported file formats for technical plans (blueprints).</summary>
public enum PlanFormat
{
    /// <summary>PNG raster image.</summary>
    Png = 1,

    /// <summary>JPEG raster image.</summary>
    Jpeg = 2,

    /// <summary>PDF document (converted to images before AI analysis).</summary>
    Pdf = 3
}
