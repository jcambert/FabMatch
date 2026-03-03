using FabMatch.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FabMatch.Infrastructure.Services;

/// <summary>
/// Local disk-based file storage implementation.
/// In production, replace with an Azure Blob or S3 backed implementation.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _opts;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IOptions<FileStorageOptions> opts,
        ILogger<LocalFileStorageService> logger)
    {
        _opts = opts.Value;
        _logger = logger;
        Directory.CreateDirectory(_opts.BasePath);
    }

    /// <inheritdoc />
    public async Task<string> SaveAsync(
        Stream stream,
        string fileName,
        string folder,
        CancellationToken ct = default)
    {
        var dirPath = Path.Combine(_opts.BasePath, folder);
        Directory.CreateDirectory(dirPath);

        var uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var key = Path.Combine(folder, uniqueName);
        var fullPath = Path.Combine(_opts.BasePath, key);

        await using var fs = File.Create(fullPath);
        await stream.CopyToAsync(fs, ct);

        _logger.LogDebug("Saved file {Key} ({Length} bytes)", key, fs.Length);
        return key;
    }

    /// <inheritdoc />
    public async Task<byte[]> GetAsync(string storageKey, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_opts.BasePath, storageKey);
        return await File.ReadAllBytesAsync(fullPath, ct);
    }

    /// <inheritdoc />
    public Task<Stream> GetStreamAsync(string storageKey, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_opts.BasePath, storageKey);
        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_opts.BasePath, storageKey);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string?> GetPublicUrlAsync(string storageKey, CancellationToken ct = default)
    {
        // Local storage does not provide public URLs.
        // Return a relative URL that the web app can serve from the storage path.
        return Task.FromResult<string?>($"/storage/{storageKey.Replace('\\', '/')}");
    }
}

/// <summary>Configuration options for local file storage.</summary>
public sealed class FileStorageOptions
{
    public const string Section = "FileStorage";

    /// <summary>Absolute or relative base directory for storing uploaded files.</summary>
    public string BasePath { get; set; } = "uploads";
}
