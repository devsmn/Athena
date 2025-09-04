SELECT tag.TAg_ref as Id,
	   tag.TAg_name as Name,
	   tag.TAg_comment as Comment,
	   tag.TAG_backgroundColor as BackgroundColor,
	   tag.TAG_textColor as TextColor
  FROM TAG tag
 WHERE CASE WHEN @TAG_ref = -1 THEN tag.TAG_ref = tag.TAG_ref ELSE tag.TAG_ref = @TAG_ref END;