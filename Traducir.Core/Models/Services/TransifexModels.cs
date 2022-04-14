using System.Runtime.Serialization;
using Traducir.Core.Helpers;

namespace Traducir.Core.Models.Services
{
    public class TransifexString
    {
        private string _normalizedKey;

        [DataMember(Name = "key")]
        public string Key { get; set; }

        public string NormalizedKey => _normalizedKey ?? (_normalizedKey = Key.ToNormalizedKey());

        [DataMember(Name = "source_string")]
        public string Source { get; set; }

        [DataMember(Name = "translation")]
        public string Translation { get; set; }

        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        public string Variant => Comment.HasValue() ? Comment : null;
    }

    public class TransifexStringToPush
    {
        [IgnoreDataMember]
        public string Key { get; set; }

        [DataMember(Name = "source_entity_hash")]
        public string SourceEntityHash => $"{Key}:".CalculateMd5();

        [DataMember(Name = "translation")]
        public string Translation { get; set; }
    }
}