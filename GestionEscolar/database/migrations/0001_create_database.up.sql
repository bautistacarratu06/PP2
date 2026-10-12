IF DB_ID(N'course_tracking') IS NULL
BEGIN
    CREATE DATABASE course_tracking;
END;
GO
USE course_tracking;
GO

IF DB_ID(N'course_tracking') IS NULL
    THROW 50000, 'Database course_tracking could not be created.', 1;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'dbo')
    EXEC(N'CREATE SCHEMA dbo');
GO

-- Database creation is intentionally separated from table creation so later files can run in order.
USE course_tracking;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'section' AND schema_id = SCHEMA_ID(N'dbo'))
    PRINT N'Ready to create course_tracking objects.';
GO

-- The final statement keeps this file valid when executed by sqlcmd as a standalone migration.
USE course_tracking;
GO

-- No application tables are created in this step.
PRINT N'0001_create_database completed.';
