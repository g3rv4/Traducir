import { UserType } from "./UserType";

export default interface IUserInfo {
    name: string;
    userType: UserType;
    canSuggest: boolean;
    canReview: boolean;
    canManageUsers: boolean;
    id: number;
}
