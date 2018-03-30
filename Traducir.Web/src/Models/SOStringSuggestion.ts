export default interface SOStringSuggestion {
    id: number;
    suggestion: string;
    state: StringSuggestionState;
    createdById: number;
    createdBy: string;
    creationDate: Date;
}

export enum StringSuggestionState {
    Created = 1,
    ApprovedByTrustedUser = 2,
    ApprovedByReviewer = 3,
    Rejected = 4,
    DeletedByOwner = 5,
    DismissedByOtherString = 6
}