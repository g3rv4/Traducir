namespace Traducir.Core.Models.Enums
{
    public enum StringHistoryType : int
    {
        Created = 1,
        Updated = 2,
        Deleted = 3,
        Undeleted = 4,
        TranslationUpdatedFromDump = 5,
        MadeUrgent = 6,
        MadeNotUrgent = 7,
        Ignored = 8,
        UnIgnored = 9,
    }
}