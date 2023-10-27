SELECT PG.PG_ref as Id,
	   PG.PG_title as Title,
	   PG.PG_comment as Comment
  FROM PAGE PG
 WHERE CASE WHEN @PG_ref = -1 THEN PG.PG_ref = PG.PG_ref ELSE PG.PG_ref = @PG_ref END;



