namespace NuFridge.Service.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeFeedGroupIdType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Feeds", "GroupId", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Feeds", "GroupId", c => c.Guid(nullable: false));
        }
    }
}
