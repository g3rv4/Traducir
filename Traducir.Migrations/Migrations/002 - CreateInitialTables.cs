using SimpleMigrations;

namespace Traducir.Migrations.Migrations
{
    [Migration(2, "Create initial tables")]
    public class CreateInitialTables : Migration
    {
        protected override void Up()
        {
            Execute(@"
If dbo.fnTableExists('Strings') = 0
Begin
  Create Table dbo.Strings
  (
    Id Int Not Null Identity (1, 1),
    [Key] VarChar(255) Not Null,
    NormalizedKey VarChar(255) Not Null,
    OriginalString NVarChar(Max) Not Null,
    Translation NVarChar(Max) Null,
    NeedsPush Bit Not Null Default(0),
    Variant VarChar(255) Null,
    CreationDate DateTime Not Null,
    DeletionDate DateTime Null

    Constraint PK_Strings Primary Key Clustered (Id Asc),
    Constraint IX_Strings_Key UNIQUE ([Key]),
    Constraint IX_Strings_NormalizedKey UNIQUE (NormalizedKey)
  )
  On [Primary]
End

If dbo.fnIndexExists('Strings', 'IX_Strings_NormalizedKey_Filtered') = 0
Begin
  Create Index IX_Strings_NormalizedKey_Filtered On Strings (NormalizedKey)
  Include (DeletionDate)
  Where DeletionDate Is Null
End

If dbo.fnTableExists('Users') = 0
Begin
  Create Table dbo.Users
  (
    Id Int Not Null,
    DisplayName NVarChar(150) Not Null,
    IsModerator Bit Not Null,
    IsTrusted Bit Not Null,
    IsReviewer Bit Not Null,
    IsBanned Bit Not Null,
    CreationDate DateTime Not Null,
    LastSeenDate DateTime Null

    Constraint PK_Users Primary Key Clustered (Id Asc)
  )
  On [Primary]
End

If dbo.fnTableExists('StringSuggestions') = 0
Begin
  Create Table dbo.StringSuggestions
  (
    Id Int Not Null Identity (1, 1),
    StringId Int Not Null Constraint FK_StringSuggestions_String Foreign Key References Strings (Id),
    Suggestion NVarChar(Max),
    StateId TinyInt Not Null,
    CreatedById Int Not Null Constraint FK_StringSuggestions_CreatedBy Foreign Key References Users (Id),
    LastStateUpdatedById Int Null Constraint FK_StringSuggestions_LastStateUpdatedById Foreign Key References Users (Id),
    CreationDate DateTime Not Null,
    LastStateUpdatedDate DateTime Null

    Constraint PK_StringSuggestions Primary Key Clustered (Id Asc)
  )
  On [Primary]
End

If dbo.fnTableExists('StringSuggestionHistory') = 0
Begin
  Create Table dbo.StringSuggestionHistory
  (
    Id Int Not Null Identity (1, 1),
    StringSuggestionId Int Not Null Constraint FK_StringSuggestionHistory_StringSuggestion Foreign Key References StringSuggestions (Id),
    HistoryTypeId TinyInt Not Null,
    Comment NVarChar(100) Null,
    UserId Int Null Constraint FK_StringSuggestionHistory_User Foreign Key References Users (Id),
    CreationDate datetime Not Null

    Constraint PK_StringSuggestionHistory Primary Key Clustered (Id Asc)
  )
  On [Primary]
End

If dbo.fnTableExists('StringHistory') = 0
Begin
  Create Table dbo.StringHistory
  (
    Id Int Not Null Identity (1, 1),
    StringId Int Not Null Constraint FK_StringHistory_String Foreign Key References Strings (Id),
    HistoryTypeId TinyInt Not Null,
    Comment NVarChar(100) Null,
    UserId Int Null Constraint FK_StringHistory_User Foreign Key References Users (Id),
    CreationDate datetime Not Null

    Constraint PK_StringHistory Primary Key Clustered (Id Asc)
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