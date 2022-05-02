USE Sinphinity;
GO
-- Truncate the log by changing the database recovery model to SIMPLE.
ALTER DATABASE Sinphinity
SET RECOVERY SIMPLE;
GO
-- Shrink the truncated log file to 1 MB.
DBCC SHRINKFILE (Sinphinity_Log, 1);
GO
-- Reset the database recovery model.
ALTER DATABASE Sinphinity
SET RECOVERY FULL;
GO