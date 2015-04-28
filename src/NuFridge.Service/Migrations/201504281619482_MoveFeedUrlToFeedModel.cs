namespace NuFridge.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MoveFeedUrlToFeedModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Feeds", "Host", c => c.String(maxLength: 4000));
            AddColumn("dbo.Feeds", "Port", c => c.Int(nullable: false));
            AddColumn("dbo.Feeds", "VirtualDirectory", c => c.String(maxLength: 4000));
            AddColumn("dbo.Feeds", "SynchronizeOnStart", c => c.Boolean(nullable: false));
            AddColumn("dbo.Feeds", "EnablePackageFileWatcher", c => c.Boolean(nullable: false));
            AddColumn("dbo.Feeds", "GroupPackageFilesById", c => c.Boolean(nullable: false));
            AddColumn("dbo.Feeds", "AllowPackageOverwrite", c => c.Boolean(nullable: false));
            DropColumn("dbo.Feeds", "GroupId");
            DropColumn("dbo.Feeds", "RunPackageCleaner");
            DropColumn("dbo.Feeds", "KeepXNumberOfPackageVersions");
            DropTable("dbo.FeedGroups");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.FeedGroups",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 4000),
                        Name = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Feeds", "KeepXNumberOfPackageVersions", c => c.Int(nullable: false));
            AddColumn("dbo.Feeds", "RunPackageCleaner", c => c.Boolean(nullable: false));
            AddColumn("dbo.Feeds", "GroupId", c => c.String(maxLength: 4000));
            DropColumn("dbo.Feeds", "AllowPackageOverwrite");
            DropColumn("dbo.Feeds", "GroupPackageFilesById");
            DropColumn("dbo.Feeds", "EnablePackageFileWatcher");
            DropColumn("dbo.Feeds", "SynchronizeOnStart");
            DropColumn("dbo.Feeds", "VirtualDirectory");
            DropColumn("dbo.Feeds", "Port");
            DropColumn("dbo.Feeds", "Host");
        }
    }
}
