export enum NotificationInterval {
    Minutes = 1,
    Hours = 60,
    Days = 1440
}

export interface INotificationSettings {
    notifyUrgentStrings: boolean;
    notifySuggestionsAwaitingApproval: boolean;
    notifySuggestionsAwaitingReview: boolean;
    notifyStringsPushedToTransifex: boolean;
    notifySuggestionsApproved: boolean;
    notifySuggestionsRejected: boolean;
    notifySuggestionsReviewed: boolean;
    notifySuggestionsOverriden: boolean;
    notificationsInterval: NotificationInterval;
    notificationsIntervalValue: number;
}
