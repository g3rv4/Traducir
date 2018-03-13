using SimpleMigrations;

namespace Traducir.Migrations.Migrations
{
    [Migration(2, "Create initial tables")]
    public class CreateInitialTables : Migration
    {
        protected override void Up()
        {
            Execute(@"
If dbo.fnTableExists('SOStrings') = 0
Begin
  Create Table dbo.SOStrings
  (
    Id Int Not Null Primary Key (Id) Identity (1, 1),
    [Key] VarChar(255) Not Null,
    NormalizedKey VarChar(255) Not Null,
    OriginalString NVarChar(Max) Not Null,
    Translation NVarChar(Max) Null,
    CreationDate DateTime Not Null

    Constraint IX_SOStrings_Key UNIQUE ([Key]),
    Constraint IX_SOStrings_NormalizedKey UNIQUE (NormalizedKey)
  )
  On [Primary]
End

If dbo.fnTableExists('Users') = 0
Begin
  Create Table dbo.Users
  (
    Id Int Not Null Primary Key (Id) Identity (1, 1),
    AccountId Int Not Null,
    UserId Int Not Null,
    DisplayName NVarChar(150) Not Null,
    IsModerator Bit Not Null,
    CreationDate datetime Not Null

    Constraint IX_Users_AccountId UNIQUE (AccountId),
    Constraint IX_Users_UserId UNIQUE (UserId)
  )
  On [Primary]
End

If dbo.fnTableExists('SOStringSuggestions') = 0
Begin
  Create Table dbo.SOStringSuggestions
  (
    Id Int Not Null Primary Key (Id) Identity (1, 1),
    SOStringId Int Not Null Constraint FK_SOStringSuggestions_SoString Foreign Key References SOStrings (Id),
    Suggestion NVarChar(Max),
    CreatedById Int Not Null Constraint FK_SOStringSuggestions_CreatedBy Foreign Key References Users (Id),
    ApprovedBy Int Null Constraint FK_SOStringSuggestions_ApprovedBy Foreign Key References Users (Id),
    RejectedBy Int Null Constraint FK_SOStringSuggestions_RejectedBy Foreign Key References Users (Id),
    CreationDate datetime Not Null,
    UpdateDate DateTime Null
  )
  On [Primary]
End

If dbo.fnTableExists('SOStringHistory') = 0
Begin
  Create Table dbo.SOStringHistory
  (
    Id Int Not Null Primary Key (Id) Identity (1, 1),
    SOStringId Int Not Null Constraint FK_SOStringHistory_SoString Foreign Key References SOStrings (Id),
    HistoryTypeId TinyInt Not Null,
    UserId Int Not Null Constraint FK_SOStringHistory_User Foreign Key References Users (Id),
    CreationDate datetime Not Null
  )
  On [Primary]
End");
        }

        protected override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}