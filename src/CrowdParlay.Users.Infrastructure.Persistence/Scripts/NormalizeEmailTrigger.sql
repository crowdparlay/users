CREATE OR REPLACE FUNCTION normalize_email_trigger_function()
    RETURNS TRIGGER AS
$$
BEGIN
    IF TG_OP = 'INSERT' THEN
        NEW.email_normalized := normalize_email(NEW.email);
    ELSIF TG_OP = 'UPDATE' THEN
        IF NEW.email <> OLD.email THEN
            NEW.email_normalized := normalize_email(NEW.email);
        END IF;
    END IF;
RETURN NEW;
END
$$ LANGUAGE plpgsql;

CREATE TRIGGER normalize_email_trigger
    BEFORE INSERT OR UPDATE
    ON users
    FOR EACH ROW
EXECUTE FUNCTION normalize_email_trigger_function();