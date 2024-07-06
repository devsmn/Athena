
SELECT	DocumentId,
		DocumentName,
		DocumentComment,
		FolderId,
		FolderName,
		Snippet,
		DocumentPageNumber
  FROM ( SELECT doc.DOC_ref			as DocumentId,
				doc.DOC_name		as DocumentName,
				doc.DOC_comment		as DocumentComment,
				FOLDER.FD_ref		as FolderId,
				FOLDER.FD_name		as FolderName,
				doc.DOC_name		as Snippet,
				0					as DocumentPageNumber
		   FROM DOCUMENT doc, 
			    doc_tag docTag, 
				FOLDER_DOC folderDoc,
				FOLDER folder
		  WHERE CASE WHEN @DOC_name		= '' then doc.DOC_name = doc.DOC_name else doc.DOC_name like @DOC_name END
		    AND folderDoc.DOC_ref		= DOC.DOC_ref
		    AND FOLDER.FD_ref			= folderDoc.FD_ref
		    AND docTag.DOC_ref			= doc.DOC_ref
		    AND docTag.TAG_ref			in (<<__replace__>>)

		  UNION
   
	     SELECT ch.DOC_ref			as DocumentId,
			    doc.DOC_name		as DocumentName,
			    doc.DOC_comment		as DocumentComment,
			    ch.FD_ref			as FolderId,
			    folder.FD_name		as FolderName,
			    replace(replace(snippet(CHAPTER, 3, '<b>', '</b>', '...', 6), CHAR(10), ' '), CHAR(13), ' ') as Snippet,
			    DOC_pageNr as DocumentPageNumber
		   FROM CHAPTER ch, 
				FOLDER folder, 
				DOCUMENT doc,
			    doc_tag docTag
		  WHERE CHAPTER				MATCH replace(@DOC_name, '%', '')
		    AND folder.FD_ref		= CAST(ch.FD_ref as INTEGER)
		    AND DOC.DOC_ref			= CAST(ch.DOC_ref as INTEGER)
			AND docTag.DOC_ref		= DOC.DOC_ref	
		    AND docTag.TAG_ref		in (<<__replace__>>)
		    AND @useFTS				= 1)
--ORDER BY rank;
   