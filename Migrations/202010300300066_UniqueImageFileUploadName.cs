namespace MVC_CSS_EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UniqueImageFileUploadName : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProductImages", "FileName", c => c.String(maxLength: 100));
            CreateIndex("dbo.ProductImages", "FileName", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.ProductImages", new[] { "FileName" });
            AlterColumn("dbo.ProductImages", "FileName", c => c.String());
        }
    }
}
