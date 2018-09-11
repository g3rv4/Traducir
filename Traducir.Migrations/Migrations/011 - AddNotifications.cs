using SimpleMigrations;

namespace Traducir.Migrations.Migrations
{
    [Migration(11, "Add Notifications")]
    public class AddNotifications : Migration
    {
        protected override void Up()
        {
            Execute(@"
If dbo.fnColumnExists('Users', 'NextNotificationUrgentStrings') = 0
Begin
    Alter Table Users Add NextNotificationUrgentStrings DateTime Null;
End

If dbo.fnColumnExists('Users', 'NextNotificationSuggestionsAwaitingApproval') = 0
Begin
    Alter Table Users Add NextNotificationSuggestionsAwaitingApproval DateTime Null;
End

If dbo.fnColumnExists('Users', 'NextNotificationSuggestionsAwaitingReview') = 0
Begin
    Alter Table Users Add NextNotificationSuggestionsAwaitingReview DateTime Null;
End

If dbo.fnColumnExists('Users', 'NextNotificationStringsPushedToTransifex') = 0
Begin
    Alter Table Users Add NextNotificationStringsPushedToTransifex DateTime Null;
End

If dbo.fnColumnExists('Users', 'NextNotificationSuggestionsApproved') = 0
Begin
    Alter Table Users Add NextNotificationSuggestionsApproved DateTime Null;
End

If dbo.fnColumnExists('Users', 'NextNotificationSuggestionsRejected') = 0
Begin
    Alter Table Users Add NextNotificationSuggestionsRejected DateTime Null;
End

If dbo.fnColumnExists('Users', 'NextNotificationSuggestionsReviewed') = 0
Begin
    Alter Table Users Add NextNotificationSuggestionsReviewed DateTime Null;
End

If dbo.fnColumnExists('Users', 'NextNotificationSuggestionsOverriden') = 0
Begin
    Alter Table Users Add NextNotificationSuggestionsOverriden DateTime Null;
End

If dbo.fnColumnExists('Users', 'NotificationDetails') = 0
Begin
    Alter Table Users Add NotificationDetails VarChar(Max) Null;
End

If dbo.fnColumnExists('Users', 'NotificationsIntervalId') = 0
Begin
    Alter Table Users Add NotificationsIntervalId TinyInt Not Null Default(1440); -- days
End

If dbo.fnColumnExists('Users', 'NotificationsIntervalValue') = 0
Begin
    Alter Table Users Add NotificationsIntervalValue SmallInt Not Null Default(7);
End

If dbo.fnColumnExists('Users', 'NotificationsIntervalMinutes') = 0
Begin
    Alter Table Users Add NotificationsIntervalMinutes Int Not Null Default(10080); -- 7 days (the default)
End
");
        }

        protected override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}