namespace ChickenChap.Api.Infrastructure;

public interface IStorageService
{
    /// <summary>
    /// Upload multiple files and return their blob URLs
    /// </summary>
    Task<List<string>> UploadFilesAsync(string folderPath, IEnumerable<IFormFile> files);

    /// <summary>
    /// Delete a single blob by URL
    /// </summary>
    Task DeleteFileAsync(string blobUrl);

    /// <summary>
    /// Delete multiple blobs
    /// </summary>
    Task DeleteFilesAsync(IEnumerable<string> blobUrls);

    /// <summary>
    /// ✅ NEW: Generate fresh SAS URL for downloading image
    /// </summary>
    string GetSasUrl(string blobUrl);
}
