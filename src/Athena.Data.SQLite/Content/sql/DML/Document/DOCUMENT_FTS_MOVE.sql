UPDATE CHAPTER
   SET FD_ref	= @FD_ref_new
 WHERE FD_ref	= @FD_ref_old
   AND DOC_ref	= @DOC_ref;