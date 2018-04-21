using Traducir.Core.Models.Enums;

namespace Traducir.Api.ViewModels.Account
{
    public class ChangeUserTypeViewModel
    {
        public int UserId { get; set; }

        public UserType UserType { get; set; }
    }
}