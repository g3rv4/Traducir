using SimpleMigrations;

namespace Traducir.Migrations.Migrations
{
    [Migration(5, "Add string history enum: TranslationUpdatedFromDump")]
    public class AddEnumTranslationUpdatedFromDump : Migration
    {
        protected override void Down()
        {
            throw new System.NotImplementedException();
        }

        protected override void Up()
        {
            Execute(@"
If Not Exists (Select 1 From StringHistoryTypes Where Id = 5)
Begin
    Insert Into StringHistoryTypes (Id, Name, Description)
    values (5, 'TranslationUpdatedFromDump', 'The translation was updated from the SO DB dump')
End
");
        }
    }
}