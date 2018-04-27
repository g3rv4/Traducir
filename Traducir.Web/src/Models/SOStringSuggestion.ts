import ISOStringSuggestionHistory from "./SOStringSuggestionHistory";

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

    // these are retrieved... sometimes :)
    originalString: string;
    histories: ISOStringSuggestionHistory[];
}

export enum StringSuggestionState {
    Created = 1,
    ApprovedByTrustedUser = 2,
    ApprovedByReviewer = 3,
    Rejected = 4,
    DeletedByOwner = 5,
    DismissedByOtherString = 6
}

export function suggestionStateToString(state: StringSuggestionState): string {
    switch (state) {
        case StringSuggestionState.Created:
            return "Created";
        case StringSuggestionState.ApprovedByTrustedUser:
            return "Approved by trusted user";
        case StringSuggestionState.ApprovedByReviewer:
            return "Approved by reviewer";
        case StringSuggestionState.Rejected:
            return "Rejected";
        case StringSuggestionState.DeletedByOwner:
            return "Deleted by owner";
        case StringSuggestionState.DismissedByOtherString:
            return "Dismissed by other string";
    }
    return "Unknown";
}
