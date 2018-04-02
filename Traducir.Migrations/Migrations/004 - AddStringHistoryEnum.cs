using SimpleMigrations;

namespace Traducir.Migrations.Migrations
{
    [Migration(4, "Add the string history types enum")]
    public class AddStringHistoryEnum : Migration
    {
        protected override void Down()
        {
            throw new System.NotImplementedException();
        }

        protected override void Up()
        {
            Execute(@"
If dbo.fnTableExists('StringHistoryTypes') = 0
Begin
  Create Table dbo.StringHistoryTypes
  (
    Id TinyInt Not Null,
    Name VarChar(100) Not Null,
    Description VarChar(255) Not Null

    Constraint PK_StringHistoryTypes Primary Key Clustered (Id Asc)
  )
  On [Primary]
End

-- Insert the current types

If Not Exists (Select 1 From StringHistoryTypes Where Id = 1)
Begin
    Insert Into StringHistoryTypes (Id, Name, Description)
    values (1, 'Created', 'The string was imported for the first time')
End

If Not Exists (Select 1 From StringHistoryTypes Where Id = 2)
Begin
    Insert Into StringHistoryTypes (Id, Name, Description)
    values (2, 'Updated', 'The string was updated from Transifex')
End

If Not Exists (Select 1 From StringHistoryTypes Where Id = 3)
Begin
    Insert Into StringHistoryTypes (Id, Name, Description)
    values (3, 'Deleted', 'The string was deleted from Transifex')
End

If Not Exists (Select 1 From StringHistoryTypes Where Id = 4)
Begin
    Insert Into StringHistoryTypes (Id, Name, Description)
    values (4, 'Undeleted', 'The string came back to life on Transifex')
End

-- ready for the FK!
If dbo.fnConstraintExists('StringHistory', 'FK_StringHistory_StringHistoryTypes') = 0
Begin
      Alter Table StringHistory With Check Add Constraint [FK_StringHistory_StringHistoryTypes]
        Foreign Key (HistoryTypeId)
        References StringHistoryTypes (Id);
End
");
        }
    }
}