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