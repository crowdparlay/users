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
END
$$ LANGUAGE plpgsql;

CREATE TRIGGER normalize_username_trigger
    BEFORE INSERT OR UPDATE
    ON users
    FOR EACH ROW
EXECUTE FUNCTION normalize_username_trigger_function();