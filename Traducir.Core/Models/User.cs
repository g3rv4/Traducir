using System;
using Traducir.Core.Models.Enums;

namespace Traducir.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public bool IsModerator { get; set; }
        public bool IsBanned { get; set; }
        public bool IsTrusted { get; set; }
        public bool IsReviewer { get; set; }
        public UserType UserType
        {
            get
            {
                if (IsBanned)
                {
                    return UserType.Banned;
                }
                if (IsReviewer)
                {
                    return UserType.Reviewer;
                }
                if (IsTrusted)
                {
                    return UserType.TrustedUser;
                }
                return UserType.User;
            }
        }
        public DateTime CreationDate { get; set; }
        public DateTime? LastSeenDate { get; set; }
    }
}