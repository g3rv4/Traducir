using System;

namespace Traducir.Core.Models
{
    public class SOString
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string OriginalString { get; set; }
        public string Translation { get; set; }
        public bool NeedsPush { get; set; }
        public string Variant { get; set; }
        public DateTime CreationDate { get; set; }

        public SOStringSuggestion[] Suggestions { get; set; }
    }
}