select tag.TAG_ref as Id,
	   tag.TAG_name as Name,
	   tag.TAG_comment as Comment
  from TAG tag,
	   DOC_TAG doctag
 where doctag.DOC_ref = @DOC_ref
   and tag.TAG_ref = doctag.TAG_ref