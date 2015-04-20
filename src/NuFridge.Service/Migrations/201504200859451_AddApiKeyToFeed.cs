namespace NuFridge.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddApiKeyToFeed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Feeds", "ApiKey", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Feeds", "ApiKey");
        }
    }
}
