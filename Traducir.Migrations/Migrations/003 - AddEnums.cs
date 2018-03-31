using SimpleMigrations;

namespace Traducir.Migrations.Migrations
{
    [Migration(3, "Add enums to the database")]
    public class AddEnums : Migration
    {
        protected override void Down()
        {
            throw new System.NotImplementedException();
        }

        protected override void Up()
        {
            Execute(@"
If dbo.fnTableExists('StringSuggestionStates') = 0
Begin
  Create Table dbo.StringSuggestionStates
  (
    Id TinyInt Not Null,
    Name VarChar(100) Not Null,
    Description VarChar(255) Not Null

    Constraint PK_StringSuggestionStates Primary Key Clustered (Id Asc)
  )
  On [Primary]
End

If dbo.fnTableExists('StringSuggestionHistoryTypes') = 0
Begin
  Create Table dbo.StringSuggestionHistoryTypes
  (
    Id TinyInt Not Null,
    Name VarChar(100) Not Null,
    Description VarChar(255) Not Null

    Constraint PK_StringSuggestionHistoryTypes Primary Key Clustered (Id Asc)
  )
  On [Primary]
End

-- Insert the current states

If Not Exists (Select 1 From StringSuggestionStates Where Id = 1)
Begin
    Insert Into StringSuggestionStates (Id, Name, Description)
    values (1, 'Created', 'The suggestion is waiting for review')
End

If Not Exists (Select 1 From StringSuggestionStates Where Id = 2)
Begin
    Insert Into StringSuggestionStates (Id, Name, Description)
    values (2, 'ApprovedByTrustedUser', 'The suggestion is approved by a trusted user and waiting for a reviewer')
End

If Not Exists (Select 1 From StringSuggestionStates Where Id = 3)
Begin
    Insert Into StringSuggestionStates (Id, Name, Description)
    values (3, 'ApprovedByReviewer', 'The suggestion is approved and used as a translation')
End

If Not Exists (Select 1 From StringSuggestionStates Where Id = 4)
Begin
    Insert Into StringSuggestionStates (Id, Name, Description)
    values (4, 'Rejected', 'The suggestion is rejected. Nobody will ever discover it again')
End

If Not Exists (Select 1 From StringSuggestionStates Where Id = 5)
Begin
    Insert Into StringSuggestionStates (Id, Name, Description)
    values (5, 'DeletedByOwner', 'The suggestion was deleted by its owner')
End

If Not Exists (Select 1 From StringSuggestionStates Where Id = 6)
Begin
    Insert Into StringSuggestionStates (Id, Name, Description)
    values (6, 'DismissedByOtherString', 'The suggestion was dismissed by the approval of other suggestion')
End

-- ready for the FK!
If dbo.fnConstraintExists('StringSuggestions', 'FK_StringSuggestions_StringSuggestionStates') = 0
Begin
      Alter Table StringSuggestions With Check Add Constraint [FK_StringSuggestions_StringSuggestionStates]
        Foreign Key (StateId)
        References StringSuggestionStates (Id);
End

-- insert the current history types
If Not Exists (Select 1 From StringSuggestionHistoryTypes Where Id = 1)
Begin
    Insert Into StringSuggestionHistoryTypes (Id, Name, Description)
    values (1, 'Created', 'The suggestion was created')
End

If Not Exists (Select 1 From StringSuggestionHistoryTypes Where Id = 2)
Begin
    Insert Into StringSuggestionHistoryTypes (Id, Name, Description)
    values (2, 'ApprovedByTrusted', 'The suggestion was approved by a trusted user')
End

If Not Exists (Select 1 From StringSuggestionHistoryTypes Where Id = 3)
Begin
    Insert Into StringSuggestionHistoryTypes (Id, Name, Description)
    values (3, 'ApprovedByReviewer', 'The suggestion was approved by a reviewer')
End

If Not Exists (Select 1 From StringSuggestionHistoryTypes Where Id = 4)
Begin
    Insert Into StringSuggestionHistoryTypes (Id, Name, Description)
    values (4, 'RejectedByTrusted', 'The suggestion was rejected by a trusted user')
End

If Not Exists (Select 1 From StringSuggestionHistoryTypes Where Id = 5)
Begin
    Insert Into StringSuggestionHistoryTypes (Id, Name, Description)
    values (5, 'RejectedByReviewer', 'The suggestion was rejected by a reviewer')
End

If Not Exists (Select 1 From StringSuggestionHistoryTypes Where Id = 6)
Begin
    Insert Into StringSuggestionHistoryTypes (Id, Name, Description)
    values (6, 'DeletedByOwner', 'The suggestion was deleted by its owner')
End

If Not Exists (Select 1 From StringSuggestionHistoryTypes Where Id = 7)
Begin
    Insert Into StringSuggestionHistoryTypes (Id, Name, Description)
    values (7, 'DismissedByOtherString', 'The suggestion was dismissed because other suggestion was approved')
End

-- ready for the FK!
If dbo.fnConstraintExists('StringSuggestionHistory', 'FK_StringSuggestionHistory_StringSuggestionHistoryTypes') = 0
Begin
      Alter Table StringSuggestionHistory With Check Add Constraint [FK_StringSuggestionHistory_StringSuggestionHistoryTypes]
        Foreign Key (HistoryTypeId)
        References StringSuggestionHistoryTypes (Id);
End
");
        }
    }
}