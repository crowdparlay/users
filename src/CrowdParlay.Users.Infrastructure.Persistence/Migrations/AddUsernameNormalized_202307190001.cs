using FluentMigrator;
using FluentMigrator.Builders.Create;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(202307190001)]
public class AddUsernameNormalized_202307190001 : Migration
{
    public override void Up()
    {
        Create.Column("username_normalized").OnTable("users").AsString().Nullable().Unique();
        
        Execute.Sql(Procedure);
        Execute.Sql(Trigger);
        
        Create.Index("UQ_username_normalized").OnTable("users").OnColumn("username_normalized").Unique();
    }

    public override void Down()
    {
        Delete.Index("UQ_username_normalized").OnTable("users");
        
        Delete.Column("username_normalized").FromTable("users");
        
        Execute.Sql("DROP TRIGGER IF EXISTS normalize_username_trigger ON users;");

        Execute.Sql("DROP FUNCTION IF EXISTS normalize_username(NVARCHAR)");
    }

    
    private string Procedure = $@"
CREATE OR REPLACE FUNCTION normalize_username()
RETURNS TRIGGER AS $$
DECLARE
    result VARCHAR := '';
    lastChar VARCHAR := '';
    i INT;
    c TEXT;
    characterReplacements JSONB := '{{
        ""0"": ""O"",
        ""1"": ""L"",
        ""I"": ""L"",
        ""3"": ""E"",
        ""4"": ""A"",
        ""5"": ""S"",
        ""6"": ""B"",
        ""8"": ""B"",
        ""9"": ""G"",
        ""W"": ""V"",
        ""Q"": ""P"",
        ""D"": ""B""
    }}';
BEGIN
    IF NEW.username = OLD.username THEN
        RETURN NEW;
    END IF;

    -- replace chars and remove duplicates
    FOR i IN 1..LENGTH(NEW.username) LOOP
        c := SUBSTRING(NEW.username FROM i FOR 1);
        IF characterReplacements::JSONB ? c THEN
            c := (characterReplacements ->> c)::TEXT;
        END IF;
        IF c <> lastChar THEN
            result := result || c;
        END IF;
        lastChar := c;
    END LOOP;

    NEW.username_normalized := result;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
";

    private string Trigger = """
CREATE TRIGGER normalize_username_trigger
BEFORE INSERT OR UPDATE ON users
FOR EACH ROW
EXECUTE FUNCTION normalize_username();
""";
}