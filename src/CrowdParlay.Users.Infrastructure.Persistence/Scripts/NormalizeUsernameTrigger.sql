CREATE TRIGGER normalize_username_trigger
    BEFORE INSERT OR UPDATE ON users
    FOR EACH ROW
EXECUTE FUNCTION normalize_username_trigger_function();