select DOC.DOC_ref as Id,
	   DOC.DOC_name as Name,
	   DOC.DOC_comment as Comment,
	   DOC.DOC_thumbnail as ThumbnailString,
	   DOC.DOC_pdf as PdfString,
	   DOC.DOC_isPinned as IsPinnedInteger
  from DOCUMENT doc,
	   FOLDER_DOC FDDOC
 where FDDOC.FD_ref = @FD_ref
   and doc.DOC_ref = FDDOC.DOC_ref