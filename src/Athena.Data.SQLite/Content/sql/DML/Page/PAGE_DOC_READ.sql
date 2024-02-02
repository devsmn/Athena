select DOC.DOC_ref as Id,
	   DOC.DOC_name as Name,
	   DOC.DOC_comment as Comment,
	   DOC.DOC_thumbnail as ThumbnailString,
	   DOC.DOC_pdf as PdfString
  from DOCUMENT doc,
	   PAGE_DOC PGDOC
 where PGDOC.PG_ref = @PG_ref
   and doc.DOC_ref = PGDOC.DOC_ref;