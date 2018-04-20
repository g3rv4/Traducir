namespace Traducir.Api.ViewModels.Strings
{
    public class CreateSuggestionViewModel
    {
        public int StringId { get; set; }
        public string Suggestion { get; set; }
        public bool Approve { get; set; }
        public bool RawString { get; set; }
    }
}