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

export function userTypeToString(userType: UserType) {
    switch (userType) {
        case UserType.Banned:
            return 'Banned';
        case UserType.User:
            return "User";
        case UserType.TrustedUser:
            return "Trusted User";
        case UserType.Reviewer:
            return "Reviewer";
    }
}