DELETE
  FROM DOC_TAG
WHERE DOC_ref = @DOC_ref 
  AND TAG_ref = @TAG_ref;