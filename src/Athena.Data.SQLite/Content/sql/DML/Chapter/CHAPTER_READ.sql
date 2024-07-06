SELECT DOC_ref as DocumentId,
	   DOC_pageNr as DocumentPageNumber,
	   FD_ref as FolderId
	   replace(replace(snippet(CHAPTER, 3, '<b>', '</b>', '...', 12), CHAR(10), ' '), CHAR(13), ' ') as Snippet
  FROM CHAPTER
 WHERE CHAPTER MATCH @CHP_text
ORDER BY rank;