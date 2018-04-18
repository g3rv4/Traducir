import { UserType } from "./UserType";

export default interface User {
    id: number;
    displayName: string;
    userType: UserType;
    creationDate: string;
    lastSeenDate: string;
    isModerator: boolean;
}