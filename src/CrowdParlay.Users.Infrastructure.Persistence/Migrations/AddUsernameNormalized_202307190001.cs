using FluentMigrator;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(202307190001)]
public class AddUsernameNormalized_202307190001 : Migration
{
    public override void Up()
    {
        Create.Column("username_normalized").OnTable("users").AsString().Nullable();

        Create.Index("UQ_username_normalized").OnTable("users").OnColumn("username_normalized").Unique();
    }

    public override void Down()
    {
        Delete.Index("UQ_username_normalized").OnTable("users");
        
        Delete.Column("username_normalized").FromTable("users");
    }
}