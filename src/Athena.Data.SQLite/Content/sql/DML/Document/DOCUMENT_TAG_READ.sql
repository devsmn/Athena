select tag.TAG_ref as Id,
	   tag.TAG_name as Name,
	   tag.TAG_comment as Comment,
	   tag.TAG_backgroundColor as BackgroundColor,
	   tag.TAG_textColor as TextColor
  from TAG tag,
	   DOC_TAG doctag
 where doctag.DOC_ref = @DOC_ref
   and tag.TAG_ref = doctag.TAG_ref