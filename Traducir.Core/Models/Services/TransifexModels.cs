using System.Runtime.Serialization;

namespace Traducir.Core.Models.Services
{
    public class TransifexString
    {
        [DataMember(Name = "key")]
        public string Key { get; set; }

        [DataMember(Name = "reviewed")]
        public bool Reviewed { get; set; }

        [DataMember(Name = "source_string")]
        public string Source { get; set; }

        [DataMember(Name = "translation")]
        public string Translation { get; set; }
    }
}