UPDATE FOLDER_DOC
   SET FD_ref	= @FD_ref_new
 WHERE DOC_ref	= @DOC_ref
   AND FD_ref	= @FD_ref_old;