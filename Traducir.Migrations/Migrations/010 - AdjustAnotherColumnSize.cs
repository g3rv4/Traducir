using SimpleMigrations;

namespace Traducir.Migrations.Migrations
{
    [Migration(10, "Adjust StringSuggestionHistory Comment size")]
    public class AdjustAnotherColumnSize : Migration
    {
        protected override void Down()
        {
            throw new System.NotImplementedException();
        }

        protected override void Up()
        {
            Execute("Alter Table StringSuggestionHistory Alter Column Comment NVarChar(Max) Null");
        }
    }
}