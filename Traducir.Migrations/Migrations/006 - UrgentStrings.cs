using SimpleMigrations;

namespace Traducir.Migrations.Migrations
{
    [Migration(6, "Strings can now be marked as urgent")]
    public class UrgentStrings : Migration
    {
        protected override void Down()
        {
            throw new System.NotImplementedException();
        }

        protected override void Up()
        {
            Execute(@"
If dbo.fnColumnExists('Strings', 'IsUrgent') = 0
Begin
    Alter Table Strings Add IsUrgent Bit Not Null Default(0)
End

If Not Exists (Select 1 From StringHistoryTypes Where Id = 6)
Begin
    Insert Into StringHistoryTypes (Id, Name, Description)
    values (6, 'MadeUrgent', 'The string was marked as urgent')
End

If Not Exists (Select 1 From StringHistoryTypes Where Id = 7)
Begin
    Insert Into StringHistoryTypes (Id, Name, Description)
    values (7, 'MadeNotUrgent', 'The string was marked as not urgent')
End
");
        }
    }
}