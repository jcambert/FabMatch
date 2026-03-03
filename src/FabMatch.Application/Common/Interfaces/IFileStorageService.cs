namespace FabMatch.Application.Common.Interfaces;

/// <summary>
/// Abstraction over a blob / file storage backend (local disk, S3, Azure Blob, etc.).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file and returns the storage key used to retrieve it later.
    /// </summary>
    /// <param name="stream">File content stream.</param>
    /// <param name="fileName">Original filename (used to derive extension/content-type).</param>
    /// <param name="folder">Logical folder / prefix (e.g. "plans", "avatars").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Storage key that uniquely identifies the saved file.</returns>
    Task<string> SaveAsync(
        Stream stream,
        string fileName,
        string folder,
        CancellationToken ct = default);

    /// <summary>Retrieves a stored file as a byte array.</summary>
    /// <param name="storageKey">Key returned by <see cref="SaveAsync"/>.</param>
    Task<byte[]> GetAsync(string storageKey, CancellationToken ct = default);

    /// <summary>Retrieves a stored file as a readable stream.</summary>
    Task<Stream> GetStreamAsync(string storageKey, CancellationToken ct = default);

    /// <summary>Deletes a stored file.</summary>
    Task DeleteAsync(string storageKey, CancellationToken ct = default);

    /// <summary>Returns a public or signed URL for the file, if supported by the backend.</summary>
    Task<string?> GetPublicUrlAsync(string storageKey, CancellationToken ct = default);
}
