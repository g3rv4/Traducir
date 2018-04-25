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

        [DataMember(Name = "reviewed")]
        public bool Reviewed { get; set; }

        [DataMember(Name = "source_string")]
        public string Source { get; set; }

        [DataMember(Name = "translation")]
        public string UnreviewedTranslation { get; set; }

        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        public string Variant => Comment.HasValue() ? Comment : null;

        public string Translation => Reviewed ? UnreviewedTranslation : null;
    }

    public class TransifexStringToPush
    {
        #pragma warning disable CA1822
        [DataMember(Name = "reviewed")]
        public bool Reviewed => true;
        #pragma warning restore CA1822

        [IgnoreDataMember]
        public string Key { get; set; }

        [DataMember(Name = "source_entity_hash")]
        public string SourceEntityHash => $"{Key}:".CalculateMd5();

        [DataMember(Name = "translation")]
        public string Translation { get; set; }
    }
}