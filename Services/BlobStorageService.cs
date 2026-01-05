using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;

namespace ChickenChap.Api.Infrastructure;

public sealed class BlobStorageService : IStorageService
{
    private readonly BlobStorageOptions _opts;
    private readonly BlobContainerClient _containerClient;
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(IOptions<BlobStorageOptions> opts)
    {
        _opts = opts.Value;
        _blobServiceClient = new BlobServiceClient(opts.Value.ConnectionString);
        _containerClient = _blobServiceClient.GetBlobContainerClient(opts.Value.ContainerName);
        
        _containerClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public async Task<List<string>> UploadFilesAsync(string folderPath, IEnumerable<IFormFile> files)
    {
        var uploadedUrls = new List<string>();

        foreach (var file in files)
        {
            if (file.Length == 0)
                continue;

            var blobName = $"{folderPath}/{Guid.NewGuid()}-{file.FileName}";
            var blobClient = _containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            uploadedUrls.Add(blobClient.Uri.GetLeftPart(UriPartial.Path));
        }

        return uploadedUrls;
    }

    public async Task DeleteFileAsync(string blobUrl)
    {
        try
        {
            var blobName = ExtractBlobName(blobUrl);
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteAsync();
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return;
        }
    }

    public async Task DeleteFilesAsync(IEnumerable<string> blobUrls)
    {
        foreach (var url in blobUrls)
        {
            await DeleteFileAsync(url);
        }
    }

    public string GetSasUrl(string blobUrl)
    {
        try
        {
            var blobName = ExtractBlobName(blobUrl);
            var blobClient = _containerClient.GetBlobClient(blobName);
            
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerClient.Name,
                BlobName = System.Uri.UnescapeDataString(blobClient.Name),
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(24)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate SAS URL for: {blobUrl}. Error: {ex.Message}", ex);
        }
    }

    private string ExtractBlobName(string blobUrl)
    {
        try
        {
            var uri = new Uri(blobUrl);
            var pathWithoutQuery = uri.AbsolutePath;
            
            var segments = pathWithoutQuery.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            if (segments.Length < 2)
                throw new InvalidOperationException($"Invalid blob URL: {blobUrl}");

            // Skip first segment (container name)
            var blobPathSegments = segments.Skip(1)
                .Select(s => System.Uri.UnescapeDataString(s))
                .ToList();
            
            // ✅ FIX: Remove ALL redundant container names (not just one)
            while (blobPathSegments.Count > 0 && 
                   blobPathSegments[0].Equals(_opts.ContainerName, StringComparison.OrdinalIgnoreCase))
            {
                blobPathSegments = blobPathSegments.Skip(1).ToList();
            }

            var blobName = string.Join("/", blobPathSegments);
            
            if (string.IsNullOrWhiteSpace(blobName))
                throw new InvalidOperationException($"Could not extract blob name from URL: {blobUrl}");

            return blobName;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to extract blob name from URL: {blobUrl}. Error: {ex.Message}", ex);
        }
    }
}
