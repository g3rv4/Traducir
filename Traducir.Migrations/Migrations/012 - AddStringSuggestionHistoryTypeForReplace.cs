using SimpleMigrations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Traducir.Migrations.Migrations
{

    [Migration(12, "Add new suggestion history type")]
    class AddSuggestionHistoryTypeForReplace: Migration
    {
        protected override void Up()
        {
            Execute(@"
Insert into StringSuggestionHistoryTypes
Values (8, 'ReplacedByOwner', 'The suggestion was replaced by its owner');
");

        }

        protected override void Down()
        {
            Execute(@"
Delete
From StringSuggestionHistoryTypes
Where id = 8;");
        }

    }
}
