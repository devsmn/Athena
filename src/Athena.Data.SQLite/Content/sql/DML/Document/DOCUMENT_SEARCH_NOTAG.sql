
select 
DocumentId,
DocumentName,
DocumentComment,
PageId,
PageTitle,
FolderId,
FolderName,
Snippet,
DocumentPageNumber

  from (


SELECT doc.DOC_ref as DocumentId,
	   doc.DOC_name as DocumentName,
	   doc.DOC_comment as DocumentComment,
	   page.PG_ref as PageId,
	   page.PG_title as PageTitle,
	   FOLDER.FD_ref as FolderId,
	   FOLDER.FD_name as FolderName,
	   doc.DOC_name as Snippet,
	   0 as DocumentPageNumber
  FROM DOCUMENT doc, PAGE_DOC pageDoc, PAGE page, FOLDER_PAGE folderPage, FOLDER folder
 WHERE CASE WHEN @DOC_name = '' then doc.DOC_name = doc.DOC_name else doc.DOC_name like @DOC_name END
   AND pageDoc.DOC_ref = DOC.DOC_ref
   AND PAGE.PG_ref = pageDoc.PG_ref
   AND folderPage.PG_ref = PAGE.PG_ref
   AND FOLDEr.FD_ref = folderPage.FD_ref
   UNION
   
   SELECT ch.DOC_ref as DocumentId,
	   doc.DOC_name as DocumentName,
	   doc.DOC_comment as DocumentComment,
	   ch.PG_ref as PageId,
	   page.PG_title as PageTitle,
	  ch.FD_ref as FolderId,
	   folder.FD_name as FolderName,
	   replace(replace(snippet(CHAPTER, 4, '<b>', '</b>', '...', 6), CHAR(10), ' '), CHAR(13), ' ') as Snippet,
	   DOC_pageNr as DocumentPageNumber
  FROM CHAPTER ch, FOLDER folder, PAGE page, DOCUMENT doc
 WHERE CHAPTER MATCH replace(@DOC_name, '%', '')
  AND folder.FD_ref = CAST(ch.FD_ref as INTEGER)
  aND page.PG_ref = CAST(ch.PG_ref as INTEGER)
  AND DOC.DOC_ref = CAST(ch.DOC_ref as INTEGER)
  AND @useFTS = 1)
--ORDER BY rank;
   