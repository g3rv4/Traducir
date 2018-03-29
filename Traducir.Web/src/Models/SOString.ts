import SOStringSuggestion from './SOStringSuggestion'

export default interface SOString {
    Id: number;
    Key: string;
    OriginalString: string;
    Translation: string;
    Variant: string;
    CreationDate: Date;
    Suggestions: SOStringSuggestion[];
}