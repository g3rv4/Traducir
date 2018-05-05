import ISOStringSuggestion from "./SOStringSuggestion";

export default interface ISOString {
    id: number;
    key: string;
    familyKey: string;
    originalString: string;
    translation: string;
    variant: string;
    creationDate: string;
    suggestions: ISOStringSuggestion[];
    isUrgent: boolean;
    isIgnored: boolean;
    touched?: boolean;
}
