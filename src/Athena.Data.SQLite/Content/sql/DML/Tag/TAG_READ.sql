SELECT tag.TAg_ref as Id,
	   tag.TAg_name as Name,
	   tag.TAg_comment as Comment
  FROM TAG tag
 WHERE CASE WHEN @TAG_ref = -1 THEN tag.TAG_ref = tag.TAG_ref ELSE tag.TAG_ref = @TAG_ref END;



