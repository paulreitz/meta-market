namespace MarketApi.Providers
{
    public interface IStorageProvider
    {
        Task SaveFileAsync(string relativePath, Stream fileStream);
        Task<bool> FileExistsAsync(string relativePath);
        Task DeleteFileAsync(string relativePath);
    }
}
