export default interface UserInfo {
    name: string;
    userType: UserType;
    canSuggest: boolean;
    canReview: boolean;
}

export enum UserType {
    Unknown = 0,
    Banned = 1,
    User = 2,
    TrustedUser = 3,
    Reviewer = 4
}