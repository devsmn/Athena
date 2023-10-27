select DOC.DOC_ref as Id,
	   DOC.DOC_name as Name,
	   DOC.DOC_comment as Comment,
	   DOC.DOC_image as ImageString
  from DOCUMENT doc,
	   PAGE_DOC PGDOC
 where PGDOC.PG_ref = @PG_ref
   and doc.DOC_ref = PGDOC.DOC_ref;