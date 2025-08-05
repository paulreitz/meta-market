namespace MarketApi.Providers
{
    public class CloudStorageProvider : IStorageProvider
    {
        private readonly IConfiguration _config;

        public CloudStorageProvider(IConfiguration config)
        {
            _config = config;
        }

        public async Task SaveFileAsync(string relativePath, Stream fileStream)
        {
            await Task.CompletedTask;
        }

        public async Task<bool> FileExistsAsync(string relativePath)
        {
            return await Task.FromResult(false);
        }

        public async Task DeleteFileAsync(string relativePath)
        {
            await Task.CompletedTask;
        }
    }
}
