import { UserType } from "./UserType";

export default interface IUser {
    id: number;
    displayName: string;
    userType: UserType;
    creationDate: string;
    lastSeenDate: string;
    isModerator: boolean;
}
