namespace Traducir.Api.Models.Enums
{
    public enum SuggestionCreationResult : int
    {
        CreationOk = 1,
        InvalidStringId = 2,
        SuggestionEqualsOriginal = 3,
        EmptySuggestion = 4,
        SuggestionAlreadyThere = 5,
        TooFewVariables = 6,
        TooManyVariables = 7,
        DatabaseError = 8
    }
}

