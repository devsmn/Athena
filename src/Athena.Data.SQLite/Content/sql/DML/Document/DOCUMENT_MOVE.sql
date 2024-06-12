UPDATE PAGE_DOC
SET PG_ref = @PG_ref_new
WHERE DOC_ref = @DOC_ref
  AND PG_ref = @PG_ref_old;