namespace NuFridge.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FeedPackageCleanerColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Feeds", "RunPackageCleaner", c => c.Boolean(nullable: false));
            AddColumn("dbo.Feeds", "KeepXNumberOfPackageVersions", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Feeds", "KeepXNumberOfPackageVersions");
            DropColumn("dbo.Feeds", "RunPackageCleaner");
        }
    }
}
