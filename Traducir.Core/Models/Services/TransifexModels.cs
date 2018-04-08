using System.Linq;
using System.Runtime.Serialization;
using Traducir.Core.Helpers;

namespace Traducir.Core.Models.Services
{
    public class TransifexString
    {
        [DataMember(Name = "key")]
        public string Key { get; set; }

        private string _NormalizedKey;
        public string NormalizedKey => _NormalizedKey ?? (_NormalizedKey = Key.ToNormalizedKey());

        [DataMember(Name = "reviewed")]
        public bool Reviewed { get; set; }

        [DataMember(Name = "source_string")]
        public string Source { get; set; }

        [DataMember(Name = "translation")]
        public string UnreviewedTranslation { get; set; }

        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        public string Variant => Comment.HasValue()? Comment : null;
        public string Translation => Reviewed ? UnreviewedTranslation : null;
    }

    public class TransifexStringToPush
    {
        [IgnoreDataMember]
        public string Key { get; set; }

        [DataMember(Name = "source_entity_hash")]
        public string SourceEntityHash => $"{Key}:".CalculateMd5();

        [DataMember(Name = "reviewed")]
        public bool Reviewed => true;

        [DataMember(Name = "translation")]
        public string Translation { get; set; }
    }
}