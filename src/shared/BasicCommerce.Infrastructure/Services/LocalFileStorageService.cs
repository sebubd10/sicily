using BasicCommerce.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BasicCommerce.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadDirectory;
    private readonly string _baseUrl;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _uploadDirectory = configuration["FileStorage:UploadDirectory"] ?? "wwwroot/uploads";
        _baseUrl = (configuration["FileStorage:BaseUrl"] ?? "http://localhost:5001").TrimEnd('/');
    }

    public async Task<(string Url, string StoredFileName)> UploadAsync(
        Guid tenantId, Guid productId, Stream fileStream, string originalFileName, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid()}{ext}";
        var relativePath = Path.Combine("products", tenantId.ToString(), storedFileName);
        var fullPath = Path.Combine(_uploadDirectory, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fs = File.Create(fullPath);
        await fileStream.CopyToAsync(fs, ct);

        var url = $"{_baseUrl}/uploads/{relativePath.Replace('\\', '/')}";
        return (url, storedFileName);
    }

    public Task DeleteAsync(string storedFileName, CancellationToken ct = default)
    {
        var searchRoot = _uploadDirectory;
        var files = Directory.GetFiles(searchRoot, storedFileName, SearchOption.AllDirectories);
        foreach (var file in files)
            File.Delete(file);
        return Task.CompletedTask;
    }
}
