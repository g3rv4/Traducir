export default interface SOStringSuggestion {
    Id: number;
    Suggestion: string;
    State: StringSuggestionState;
    CreatedBy: string;
    CreationDate: Date;
}

export enum StringSuggestionState {
    Created = 1,
    ApprovedByTrustedUser = 2,
    ApprovedByReviewer = 3,
    Rejected = 4,
    DeletedByOwner = 5,
    DismissedByOtherString = 6
}