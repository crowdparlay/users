using FluentMigrator;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(202310110001)]
public class AddEmailNormalized_202310110001 : Migration
{
    public override void Up()
    {
        Create.Column("email_normalized").OnTable("users").AsString(25);
        Create.Index("users_email_normalized_idx").OnTable("users").OnColumn("email_normalized").Unique();
        
        Execute.EmbeddedScript("CrowdParlay.Users.Infrastructure.Persistence.Scripts.NormalizeEmailFunction.sql");
        Execute.EmbeddedScript("CrowdParlay.Users.Infrastructure.Persistence.Scripts.NormalizeEmailTrigger.sql");
    }

    public override void Down()
    {
        Delete.Index("users_email_normalized_idx").OnTable("users");
        Delete.Column("email_normalized").FromTable("users");
        
        Execute.Sql("DROP TRIGGER IF EXISTS normalize_email_trigger ON users");
        Execute.Sql("DROP FUNCTION IF EXISTS normalize_email(TEXT)");
        Execute.Sql("DROP FUNCTION IF EXISTS normalize_email_trigger_function()");
    }
}