UPDATE TAG
SET TAG_name = @TAG_name, TAG_comment = @TAG_comment, TAG_modDate = @TAG_modDate, TAG_backgroundColor = @TAG_backgroundColor, TAG_textColor = @TAG_textColor
WHERE TAG_ref = @TAG_ref;