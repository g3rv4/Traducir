using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Traducir.Core.Helpers;
using Traducir.Core.Models.Enums;

namespace Traducir.Web.Net.ViewModels
{
    public class LayoutViewModel
    {
        public LayoutViewModel(IConfiguration configuration,  IHttpContextAccessor httpContextAccessor)
        {
            var httpContext = httpContextAccessor.HttpContext;
            var user = httpContext.User;

            ConfigName = configuration.GetValue<string>("FRIENDLY_NAME");
            UserName = user.GetClaim<string>(ClaimType.Name);
            UserType = user.GetClaim<UserType>(ClaimType.UserType);
            UserId = user.GetClaim<int>(ClaimType.Id);
            CurrentPathAndQuery = httpContext.Request.Path + httpContext.Request.QueryString;
        }

        public string ConfigName { get; }

        public string UserName { get; }

        public UserType UserType { get; }

        public int UserId { get; }

        public string UserTypeForDisplay =>
            UserType == UserType.TrustedUser ? "Trusted User" : UserType.ToString();

        public bool IsLoggedIn => UserName != null;

        public string CurrentPathAndQuery { get; }
    }
}
