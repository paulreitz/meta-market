using MarketApi.Models;
using Microsoft.Extensions.Configuration;

namespace MarketApi.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _config;

        public UserService(IConfiguration config)
        {
            _config = config;
        }

        public User GetUserResponse(User entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var cdnBaseUrl = _config.GetValue<string>("CdnBaseUrl") ?? "";
            var fullPfpUrl = $"{cdnBaseUrl.TrimEnd('/')}/{entity.PfpUrl.TrimStart('/')}";
            var fullBannerUrl = $"{cdnBaseUrl.TrimEnd('/')}/{entity.BannerUrl.TrimStart('/')}";

            return new User
            {
                WalletAddress = entity.WalletAddress,
                Username = entity.Username,
                PfpUrl = fullPfpUrl,
                BannerUrl = fullBannerUrl,
                Bio = entity.Bio,
                Website = entity.Website,
                AdultContentEnabled = entity.AdultContentEnabled,
                Joined = entity.Joined,
            };
        }
    }
}
