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

        public User GetUserResponse(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var cdnBaseUrl = _config.GetValue<string>("CdnBaseUrl") ?? "";
            var fullPfpUrl = $"{cdnBaseUrl.TrimEnd('/')}/{user.PfpUrl.TrimStart('/')}";
            var fullBannerUrl = $"{cdnBaseUrl.TrimEnd('/')}/{user.BannerUrl.TrimStart('/')}";

            return new User
            {
                WalletAddress = user.WalletAddress,
                Username = user.Username,
                PfpUrl = fullPfpUrl,
                BannerUrl = fullBannerUrl,
                Bio = user.Bio,
                Website = user.Website,
                AdultContentEnabled = user.AdultContentEnabled,
                Joined = user.Joined,
            };
        }
    }
}
