UPDATE DOCUMENT
SET DOC_name = @DOC_name, DOC_comment = @DOC_comment, DOC_modDate = @DOC_modDate, DOC_isPinned = @DOC_isPinned
WHERE DOC_ref = @DOC_ref;