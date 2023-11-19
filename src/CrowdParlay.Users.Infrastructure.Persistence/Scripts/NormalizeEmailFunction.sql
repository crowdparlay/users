CREATE OR REPLACE FUNCTION normalize_email(email TEXT)
    RETURNS TEXT as
$$
BEGIN
    RETURN UPPER(email);
END
$$ LANGUAGE plpgsql;