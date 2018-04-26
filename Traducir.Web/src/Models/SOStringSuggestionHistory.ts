export default interface ISOStringSuggestionHistory {
    id: number;
    historyType: SuggestionHistoryType;
    comment?: string;
    userId: number;
    userName: string;
    creationDate: string;
}

export enum SuggestionHistoryType {
    Created = 1,
    ApprovedByTrusted = 2,
    ApprovedByReviewer = 3,
    RejectedByTrusted = 4,
    RejectedByReviewer = 5,
    DeletedByOwner = 6,
    DismissedByOtherString = 7
}

export function suggestionHistoryTypeToString(historyType: SuggestionHistoryType): string {
    switch (historyType) {
        case SuggestionHistoryType.Created:
            return "Created";
        case SuggestionHistoryType.ApprovedByTrusted:
            return "Approved by trusted user";
        case SuggestionHistoryType.ApprovedByReviewer:
            return "Approved by reviewer";
        case SuggestionHistoryType.RejectedByTrusted:
            return "Rejected by trusted user";
        case SuggestionHistoryType.RejectedByReviewer:
            return "Rejected by reviewer";
        case SuggestionHistoryType.DeletedByOwner:
            return "Deleted by owner";
        case SuggestionHistoryType.DismissedByOtherString:
            return "Dismissed by other string";
    }
    return "Unknown";
}
