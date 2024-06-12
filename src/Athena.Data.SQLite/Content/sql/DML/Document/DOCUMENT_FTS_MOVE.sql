UPDATE CHAPTER
   SET PG_ref = @PG_ref_new
 WHERE PG_ref = @PG_ref_old
   AND DOC_ref = @DOC_ref;