using SimpleMigrations;

namespace Traducir.Migrations.Migrations
{
    [Migration(7, "Add UserHistory and friends")]
    public class UserHistory : Migration
    {
        protected override void Down()
        {
            throw new System.NotImplementedException();
        }

        protected override void Up()
        {
            Execute(@"
If dbo.fnTableExists('UserHistoryTypes') = 0
Begin
  Create Table dbo.UserHistoryTypes
  (
    Id TinyInt Not Null,
    Name VarChar(100) Not Null,
    Description VarChar(255) Not Null

    Constraint PK_UserHistoryTypes Primary Key Clustered (Id Asc)
  )
  On [Primary]
End

If Not Exists (Select 1 From UserHistoryTypes Where Id = 1)
Begin
    Insert Into UserHistoryTypes (Id, Name, Description)
    values (1, 'Banned', 'The user was banned')
End

If Not Exists (Select 1 From UserHistoryTypes Where Id = 2)
Begin
    Insert Into UserHistoryTypes (Id, Name, Description)
    values (2, 'BanLifted', 'The user ban was lifted')
End

If Not Exists (Select 1 From UserHistoryTypes Where Id = 3)
Begin
    Insert Into UserHistoryTypes (Id, Name, Description)
    values (3, 'MadeTrustedUser', 'The user was made trusted user')
End

If Not Exists (Select 1 From UserHistoryTypes Where Id = 4)
Begin
    Insert Into UserHistoryTypes (Id, Name, Description)
    values (4, 'DemotedToRegularUser', 'The trusted user was made a regular user')
End

If dbo.fnTableExists('UserHistory') = 0
Begin
  Create Table dbo.UserHistory
  (
    Id Int Not Null Identity (1, 1),
    UserId Int Not Null Constraint FK_UserHistory_User Foreign Key References Users (Id),
    HistoryTypeId TinyInt Not Null Constraint FK_UserHistory_UserHistoryType Foreign Key References UserHistoryTypes (Id),
    Comment NVarChar(100) Null,
    UpdatedById Int Null Constraint FK_UserHistory_UpdatedById Foreign Key References Users (Id),
    CreationDate datetime Not Null

    Constraint PK_UserHistory Primary Key Clustered (Id Asc)
  )
  On [Primary]
End");
        }
    }
}