SELECT doc.DOC_pdf as PdfString
  FROM DOCUMENT doc
 WHERE doc.DOC_ref = @DOC_ref;