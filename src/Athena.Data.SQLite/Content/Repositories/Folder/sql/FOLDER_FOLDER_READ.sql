select FD.FD_ref as Id,
	   FD.FD_name as Name,
	   FD.FD_comment as Comment,
	   fd.FD_isPinnedInt as IsPinnedInt,
	   fd.FD_creationDate as CreationDate
  from FOLDER FD,
	   FOLDER_FOLDER FDFD
 where FDFD.FD_refParent = @FD_refParent
   and FD.FD_ref = FDFD.FD_ref;