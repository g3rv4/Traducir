import SOStringSuggestion from './SOStringSuggestion'

export default interface SOString {
    id: number;
    key: string;
    originalString: string;
    translation: string;
    variant: string;
    creationDate: Date;
    suggestions: SOStringSuggestion[];
}