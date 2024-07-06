select FD.FD_ref as Id,
	   FD.FD_name as Name,
	   FD.FD_comment as Comment
  from FOLDER FD,
	   FOLDER_FOLDER FDFD
 where FDFD.FD_refParent = @FD_refParent
   and FD.FD_ref = FDFD.FD_ref;