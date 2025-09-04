 SELECT doc.DOC_ref as Id,
	   doc.DOC_name	as Name,
	   doc.DOC_comment as Comment,
	   doc.DOC_thumbnail as ThumbnailString,
	   doc.DOC_creationDate as CreationDate,
	   doc.DOC_modDate as ModDate
  FROM DOCUMENT doc
 ORDER BY doc.DOC_creationDate desc
 LIMIT @limit;