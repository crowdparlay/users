using FluentMigrator;
using FluentMigrator.Builders.Create;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(2023071901)]
public class Migration_20230719_01_AddUsernameNormalized : Migration
{
    public override void Up()
    {
        Create.Column("username_normalized").OnTable("users").AsString(25);
        Create.Index("users_username_normalized_idx").OnTable("users").OnColumn("username_normalized").Unique();
        
        Execute.EmbeddedScript("CrowdParlay.Users.Infrastructure.Persistence.Scripts.NormalizeUsernameFunction.sql");
        Execute.EmbeddedScript("CrowdParlay.Users.Infrastructure.Persistence.Scripts.NormalizeUsernameTrigger.sql");
    }

    public override void Down()
    {
        Delete.Index("users_username_normalized_idx").OnTable("users");
        Delete.Column("username_normalized").FromTable("users");
        
        Execute.Sql("DROP TRIGGER IF EXISTS normalize_username_trigger ON users");
        Execute.Sql("DROP FUNCTION IF EXISTS normalize_username(TEXT)");
        Execute.Sql("DROP FUNCTION IF EXISTS normalize_username_trigger_function()");
    }
}