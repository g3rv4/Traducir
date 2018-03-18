namespace Traducir.ViewModels.Strings
{
    public class QueryViewModel
    {
        public string SourceRegex { get; set; }
        public string TranslationRegex { get; set; }
        public bool? WithoutTranslation { get; set; }
        public bool? WithPendingTranslation { get; set; }
    }
}