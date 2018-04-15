using System;
using Traducir.Core.Helpers;

namespace Traducir.Core.Models
{
    public class SOString
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string OriginalString { get; set; }
        public string Translation { get; set; }
        private bool? _HasTranslation;
        public bool HasTranslation => _HasTranslation ?? (_HasTranslation = Translation.HasValue()).Value;
        public bool NeedsPush { get; set; }
        public bool IsUrgent { get; set; }
        public string Variant { get; set; }
        public DateTime CreationDate { get; set; }

        public SOStringSuggestion[] Suggestions { get; set; }
    }
}