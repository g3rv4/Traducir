using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Traducir.Core.Helpers;
using Traducir.Core.Models.Enums;

namespace Traducir.Web.Net.ViewModels
{
    public class LayoutViewModel
    {
        public LayoutViewModel(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            var httpContext = httpContextAccessor.HttpContext;
            var user = httpContext.User;

            ConfigName = configuration.GetValue<string>("FRIENDLY_NAME");
            CurrentPathAndQuery = httpContext.Request.Path + httpContext.Request.QueryString;
            var userName = user.GetClaim<string>(ClaimType.Name);
            IsLoggedIn = userName != null;

            if (IsLoggedIn)
            {
                UserId = user.GetClaim<int>(ClaimType.Id);
                var userType = user.GetClaim<UserType>(ClaimType.UserType);
                var userIsModerator = user.GetClaim<string>(ClaimType.IsModerator) == "1";
                var userTypeForDisplay = userType.ToDisplayString();
                UserInfo = $"{userName} ({userTypeForDisplay}) {(userIsModerator ? "♦" : null)}";
            }
            else
            {
                UserInfo = null;
            }
        }

        public string ConfigName { get; }

        public int UserId { get; }

        public bool IsLoggedIn { get; }

        public string CurrentPathAndQuery { get; }

        public string UserInfo { get; }
    }
}
