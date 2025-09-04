UPDATE FOLDER
SET FD_name = @FD_name, FD_comment = @FD_comment, FD_isPinnedInt = @FD_isPinnedInt
WHERE FD_ref = @FD_ref;