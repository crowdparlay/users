using FluentMigrator;
using FluentMigrator.Builders.Create;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(202307190001)]
public class AddUsernameNormalized_202307190001 : Migration
{
    public override void Up()
    {
        Create.Column("username_normalized").OnTable("users").AsString(25);
        
        Execute.EmbeddedScript("CrowdParlay.Users.Infrastructure.Persistence.Migrations.NormalizeUsername.sql");
        Execute.EmbeddedScript("CrowdParlay.Users.Infrastructure.Persistence.Migrations.NormalizeUsernameTrigger.sql");
        Execute.EmbeddedScript("CrowdParlay.Users.Infrastructure.Persistence.Migrations.NormalizeUsernameTriggerFunction.sql");
        
        Create.Index("users_username_normalized_idx").OnTable("users").OnColumn("username_normalized").Unique();
    }

    public override void Down()
    {
        Delete.Index("users_username_normalized_idx").OnTable("users");
        
        Delete.Column("username_normalized").FromTable("users");
        
        Execute.Sql("DROP TRIGGER IF EXISTS normalize_username_trigger ON users;");

        Execute.Sql("DROP FUNCTION IF EXISTS normalize_username(TEXT)");
        
        Execute.Sql("DROP FUNCTION IF EXISTS normalize_username_trigger_function()");
    }
}