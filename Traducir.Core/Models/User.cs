using System;
using Traducir.Core.Models.Enums;

namespace Traducir.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public bool IsModerator { get; set; }
        public UserType UserType { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastSeenDate { get; set; }
    }
}