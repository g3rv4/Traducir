using System;
using Traducir.Core.Helpers;

namespace Traducir.Core.Models
{
    public class SODumpString
    {
        public int Id { get; set; }
        public int LocaleId { get; set; }
        public string Hash { get; set; }
        public string NormalizedHash => Hash.ToNormalizedKey();
        public string Translation { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime LastSeenDate { get; set; }
        public string TranslationOverride { get; set; }
    }
}