UPDATE TAG
SET TAG_name = @TAG_name, TAG_comment = @TAG_comment, TAG_modDate = @TAG_modDate
WHERE TAG_ref = @TAG_ref;