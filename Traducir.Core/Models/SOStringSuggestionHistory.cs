using System;
using Traducir.Core.Models.Enums;

namespace Traducir.Core.Models
{
    public class SOStringSuggestionHistory
    {
        public int Id { get; set; }

        public int StringSuggestionId { get; set; }

        public StringSuggestionHistoryType HistoryType { get; set; }

        public string Comment { get; set; }

        public int UserId { get; set; }

        public DateTime CreationDate { get; set; }

        public string UserName { get; set; }
    }
}