using System;
using System.Runtime.Serialization;

namespace Traducir.Core.Models.Services
{
    public class PaginatedResponse<T>
    {
        [DataMember(Name = "items")]
        public T[] Items { get; set; }

        [DataMember(Name = "has_more")]
        public bool HasMore { get; set; }

        [DataMember(Name = "quota_max")]
        public int QuotaMax { get; set; }

        [DataMember(Name = "quota_remaining")]
        public int QuotaRemaining { get; set; }
    }

    public abstract class BaseUser
    {
        [DataMember(Name = "account_id")]
        public int AccountId { get; set; }

        [DataMember(Name = "user_id")]
        public int UserId { get; set; }

        [DataMember(Name = "user_type")]
        public string UserType { get; set; }
    }

    public class User : BaseUser
    {
        [DataMember(Name = "display_name")]
        public string DisplayName { get; set; }

        [DataMember(Name = "is_employee")]
        public bool IsEmployee { get; set; }
    }

    public class NetworkUser : BaseUser
    {
        [DataMember(Name = "site_name")]
        public string SiteName { get; set; }

        [DataMember(Name = "site_url")]
        public string SiteUrl { get; set; }

        public string SiteDomain => new Uri(SiteUrl).Host;
    }
}