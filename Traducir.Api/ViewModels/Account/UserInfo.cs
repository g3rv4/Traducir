using Traducir.Core.Models.Enums;

namespace Traducir.Api.ViewModels.Account
{
    public class UserInfo
    {
        public string Name { get; set; }
        public UserType UserType { get; set; }
        public bool CanSuggest { get; set; }
        public bool CanReview { get; set; }
        public bool CanManageUsers { get; set; }
        public int Id { get; set; }
    }
}