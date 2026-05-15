namespace BasicCommerce.Application.Interfaces;

public interface IFileStorageService
{
    Task<(string Url, string StoredFileName)> UploadAsync(
        Guid tenantId, Guid productId, Stream fileStream, string originalFileName, CancellationToken ct = default);

    Task DeleteAsync(string storedFileName, CancellationToken ct = default);
}
