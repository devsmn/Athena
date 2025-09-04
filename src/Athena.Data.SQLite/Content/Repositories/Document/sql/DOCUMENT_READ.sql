 SELECT doc.DOC_ref as Id,
	   doc.DOC_name	as Name,
	   doc.DOC_comment as Comment,
	   doc.DOC_thumbnail as ThumbnailString,
	   doc.DOC_creationDate as CreationDate,
	   doc.DOC_modDate as ModDate,
	   doc.DOC_isPinned as IsPinnedInteger
  FROM DOCUMENT doc
 WHERE CASE WHEN @DOC_ref = -1 THEN doc.DOC_ref = doc.DOC_ref ELSE doc.DOC_ref = @DOC_ref END;