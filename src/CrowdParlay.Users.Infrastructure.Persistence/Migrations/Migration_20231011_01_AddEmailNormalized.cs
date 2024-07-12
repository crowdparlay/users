using FluentMigrator;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(2023101101)]
public class Migration_20231011_01_AddEmailNormalized : Migration
{
    public override void Up()
    {
        Create.Column("email_normalized").OnTable("users").AsString(50).NotNullable();
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