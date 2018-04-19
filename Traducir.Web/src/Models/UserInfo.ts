import { UserType } from "./UserType";

export default interface UserInfo {
    name: string;
    userType: UserType;
    canSuggest: boolean;
    canReview: boolean;
    canManageUsers: boolean;
    id: number;
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