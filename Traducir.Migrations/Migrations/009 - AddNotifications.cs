using SimpleMigrations;

namespace Traducir.Migrations.Migrations
{
    [Migration(9, "Add Notifications")]
    public class AddNotifications : Migration
    {
        protected override void Up()
        {
            Execute(@"
If dbo.fnTableExists('NotificationTypes') = 0
Begin
  Create Table dbo.NotificationTypes
  (
    Id TinyInt Not Null,
    Name VarChar(100) Not Null,
    Description VarChar(255) Not Null

    Constraint PK_NotificationTypes Primary Key Clustered (Id Asc)
  )
  On [Primary]
End

If Not Exists (Select 1 From NotificationTypes Where Id = 1)
Begin
    Insert Into NotificationTypes (Id, Name, Description)
    values (1, 'UrgentStrings', 'There are urgent strings')
End

If Not Exists (Select 1 From NotificationTypes Where Id = 2)
Begin
    Insert Into NotificationTypes (Id, Name, Description)
    values (2, 'SuggestionsAwaitingApproval', 'There are suggestions awaiting approval')
End

If Not Exists (Select 1 From NotificationTypes Where Id = 3)
Begin
    Insert Into NotificationTypes (Id, Name, Description)
    values (3, 'SuggestionsAwaitingReview', 'There are suggestings awaiting review')
End

If Not Exists (Select 1 From NotificationTypes Where Id = 4)
Begin
    Insert Into NotificationTypes (Id, Name, Description)
    values (4, 'StringsPushedToTransifex', 'Strings pushed to Transifex')
End

If Not Exists (Select 1 From NotificationTypes Where Id = 5)
Begin
    Insert Into NotificationTypes (Id, Name, Description)
    values (5, 'SuggestionsUpdated', 'Your suggestions were processed')
End

If dbo.fnTableExists('Notifications') = 0
Begin
  Create Table dbo.Notifications
  (
    Id Int Not Null Identity (1, 1),
    UserId Int Not Null Constraint FK_Notifications_User Foreign Key References Users (Id),

    Data VarChar(Max) Not Null,
    NotificationTypeId TinyInt Not Null Constraint FK_Notifications_NotificationType Foreign Key References NotificationTypes (Id),

    CreationDate DateTime Not Null,
    SentDate DateTime Null

    Constraint PK_Notifications Primary Key Clustered (Id Asc)
  )
  On [Primary]
End

If dbo.fnColumnExists('Users', 'NotifyUrgentStrings') = 0
Begin
    Alter Table Users Add NotifyUrgentStrings Bit Not Null Default(0);
End

If dbo.fnColumnExists('Users', 'NotifySuggestionsAwaitingApproval') = 0
Begin
    Alter Table Users Add NotifySuggestionsAwaitingApproval Bit Not Null Default(0);
End

If dbo.fnColumnExists('Users', 'NotifySuggestionsAwaitingReview') = 0
Begin
    Alter Table Users Add NotifySuggestionsAwaitingReview Bit Not Null Default(0);
End

If dbo.fnColumnExists('Users', 'NotifyStringsPushedToTransifex') = 0
Begin
    Alter Table Users Add NotifyStringsPushedToTransifex Bit Not Null Default(0);
End

If dbo.fnColumnExists('Users', 'NotifySuggestionsApproved') = 0
Begin
    Alter Table Users Add NotifySuggestionsApproved Bit Not Null Default(0);
End

If dbo.fnColumnExists('Users', 'NotifySuggestionsRejected') = 0
Begin
    Alter Table Users Add NotifySuggestionsRejected Bit Not Null Default(0);
End

If dbo.fnColumnExists('Users', 'NotifySuggestionsReviewed') = 0
Begin
    Alter Table Users Add NotifySuggestionsReviewed Bit Not Null Default(0);
End

If dbo.fnColumnExists('Users', 'NotifySuggestionsOverriden') = 0
Begin
    Alter Table Users Add NotifySuggestionsOverriden Bit Not Null Default(0);
End

If dbo.fnColumnExists('Users', 'LastNotificationSentDate') = 0
Begin
    Alter Table Users Add LastNotificationSentDate DateTime Null;
End

If dbo.fnColumnExists('Users', 'NotificationDetails') = 0
Begin
    Alter Table Users Add NotificationDetails VarChar(Max) Null;
End
");
        }

        protected override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}