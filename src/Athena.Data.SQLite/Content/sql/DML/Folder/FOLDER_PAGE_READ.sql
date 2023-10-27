select PG.PG_ref as Id,
	   PG.PG_title as Title,
	   PG.PG_comment as Comment
  from PAGE PG,
	   FOLDER_PAGE FDPG
 where FDPG.FD_ref = @FD_ref
   and PG.PG_ref = FDPG.PG_ref;