using FluentMigrator;
using FluentMigrator.Builders.Create;

namespace CrowdParlay.Users.Infrastructure.Persistence.Migrations;

[Migration(202307190001)]
public class AddUsernameNormalized_202307190001 : Migration
{
    public override void Up()
    {
        Create.Column("username_normalized").OnTable("users").AsString().Nullable().Unique();
        
        Execute.Sql(StoredFunction);
        Execute.Sql(TriggerFunction);
        Execute.Sql(Trigger);
    }

    public override void Down()
    {
        Delete.Index("UQ_username_normalized").OnTable("users");
        
        Delete.Column("username_normalized").FromTable("users");
        
        Execute.Sql("DROP TRIGGER IF EXISTS normalize_username_trigger ON users;");

        Execute.Sql("DROP FUNCTION IF EXISTS normalize_username(NVARCHAR)");
    }

    
    private string StoredFunction = """
CREATE OR REPLACE FUNCTION normalize_username(username TEXT) 
RETURNS TEXT as $$
DECLARE
    last_char VARCHAR := '';
    username_normalized VARCHAR := '';
    i INT;
    current_char TEXT;
    char_map JSONB := '{
        "0": "O",
        "1": "L",
        "I": "L",
        "3": "E",
        "4": "A",
        "5": "S",
        "6": "B",
        "8": "B",
        "9": "G",
        "W": "V",
        "Q": "P",
        "D": "B"
    }';
BEGIN
    username := UPPER(username);
    
    -- replace chars and remove duplicates
    FOR i IN 1..LENGTH(username) LOOP
        current_char := SUBSTRING(username FROM i FOR 1);
        IF char_map ? current_char THEN
            current_char := (char_map ->> current_char)::TEXT;
        END IF;
        IF current_char <> last_char THEN
            username_normalized := username_normalized || current_char;
        END IF;
        last_char := current_char;
    END LOOP;

    RETURN username_normalized;
END;
$$ LANGUAGE plpgsql;
""";

    private string TriggerFunction = """
CREATE OR REPLACE FUNCTION normalize_username_trigger_function()
RETURNS TRIGGER AS
$$
BEGIN
    IF TG_OP = 'INSERT' THEN
        NEW.username_normalized := normalize_username(NEW.username);
    ELSIF TG_OP = 'UPDATE' THEN
        IF NEW.username <> OLD.username THEN
            NEW.username_normalized := normalize_username(NEW.username);
        END IF;
    END IF;
    RETURN NEW;
END;
$$
LANGUAGE plpgsql;
""";

    private string Trigger = """
CREATE TRIGGER normalize_username_trigger
BEFORE INSERT OR UPDATE ON users
FOR EACH ROW
EXECUTE FUNCTION normalize_username_trigger_function();
""";
}