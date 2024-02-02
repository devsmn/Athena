SELECT DOC_ref as DocumentId,
	   DOC_pageNr as DocumentPageNumber,
	   FD_ref as FolderId,
	   PG_ref as PageId,
	   replace(replace(snippet(CHAPTER, 4, '<b>', '</b>', '...', 12), CHAR(10), ' '), CHAR(13), ' ') as Snippet
  FROM CHAPTER
 WHERE CHAPTER MATCH @CHP_text
ORDER BY rank;