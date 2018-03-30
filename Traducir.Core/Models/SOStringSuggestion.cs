using System;
using Traducir.Core.Models.Enums;

namespace Traducir.Core.Models
{
    public class SOStringSuggestion
    {
        public int Id { get; set; }
        public int StringId { get; set; }
        public string Suggestion { get; set; }
        public StringSuggestionState State { get; set; }
        public string CreatedByName { get; set; }
        public int CreatedById { get; set; }
        public string LastStateUpdatedByName { get; set; }
        public int? LastStateUpdatedById { get; set; }
        public DateTime CreationDate { get; set; }
    }
}