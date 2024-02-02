SELECT fd.FD_ref as Id,
	   fd.FD_name as Name,
	   fd.FD_comment as Comment,
	   fd.FD_isPinnedInt as IsPinnedInt,
	   fd.FD_creationDate as CreationDate
  FROM FOLDER fd
 WHERE CASE WHEN @FD_ref = -1 THEN fd.FD_ref = fd.FD_ref ELSE fd.FD_ref = @FD_ref END;



