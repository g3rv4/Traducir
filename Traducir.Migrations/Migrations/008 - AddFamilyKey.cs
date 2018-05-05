using SimpleMigrations;

namespace Traducir.Migrations.Migrations
{
    [Migration(8, "Add FamilyKey to our Strings")]
    public class AddFamilyKey : Migration
    {
        protected override void Down()
        {
            throw new System.NotImplementedException();
        }

        protected override void Up()
        {
            Execute(@"
If dbo.fnColumnExists('Strings', 'FamilyKey') = 0
Begin
    Alter Table Strings Add FamilyKey Char(32) Null;
End

If dbo.fnColumnExists('Strings', 'IsIgnored') = 0
Begin
    Alter Table Strings Add IsIgnored Bit Not Null Default(0);
End
");
            // we need the previous transaction to be committed before updating the field :shrug:
            Execute(@"
If Exists (Select 1 From Strings Where FamilyKey Is Null)
Begin
    Update Strings Set FamilyKey = Left([Key], 32);
    Alter Table Strings Alter Column FamilyKey Char(32) Not Null;
End

If dbo.fnIndexExists('Strings', 'IX_Strings_FamilyKey') = 0
Begin
  Create NonClustered Index IX_Strings_FamilyKey On Strings (FamilyKey)
End

If Not Exists (Select 1 From StringHistoryTypes Where Id = 8)
Begin
    Insert Into StringHistoryTypes (Id, Name, Description)
    values (8, 'Ignored', 'The string was ignored')
End

If Not Exists (Select 1 From StringHistoryTypes Where Id = 9)
Begin
    Insert Into StringHistoryTypes (Id, Name, Description)
    values (9, 'UnIgnored', 'The string was unignored')
End
");
        }
    }
}