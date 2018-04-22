export default interface ISOStringSuggestion {
    id: number;
    stringId: number;
    suggestion: string;
    state: StringSuggestionState;
    createdById: number;
    createdByName: string;
    creationDate: string;
    lastStateUpdatedById: number;
    lastStateUpdatedByName: string;
}

export enum StringSuggestionState {
    Created = 1,
    ApprovedByTrustedUser = 2,
    ApprovedByReviewer = 3,
    Rejected = 4,
    DeletedByOwner = 5,
    DismissedByOtherString = 6
}
