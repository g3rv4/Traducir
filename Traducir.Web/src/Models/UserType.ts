export enum UserType {
    Unknown = 0,
    Banned = 1,
    User = 2,
    TrustedUser = 3,
    Reviewer = 4
}

export function userTypeToString(userType: UserType): string{
    switch(userType){
        case UserType.Unknown:
            return 'Unknown';
        case UserType.Banned:
            return 'Banned';
        case UserType.User:
            return 'User';
        case UserType.TrustedUser:
            return 'Trusted User';
        case UserType.Reviewer:
            return 'Reviewer';
    }
}