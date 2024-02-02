UPDATE PAGE
SET PG_title = @PG_title, PG_comment = @PG_comment, PG_modDate = @PG_modDate
WHERE PG_ref = @PG_ref;