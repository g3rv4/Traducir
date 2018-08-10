using SimpleMigrations;

namespace Traducir.Migrations.Migrations
{
    [Migration(9, "Adjust StringHistory Comment size")]
    public class AdjustStringHistoryCommentSize : Migration
    {
        protected override void Down()
        {
            throw new System.NotImplementedException();
        }

        protected override void Up()
        {
            Execute("Alter Table StringHistory Alter Column Comment NVarChar(Max) Null");
        }
    }
}