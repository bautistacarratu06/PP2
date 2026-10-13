USE course_tracking;
GO

IF OBJECT_ID(N'dbo.section', N'U') IS NOT NULL
    DROP TABLE dbo.section;
GO

IF OBJECT_ID(N'dbo.section', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.section (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT pk_section PRIMARY KEY,
        name NVARCHAR(20) NOT NULL CONSTRAINT uq_section_name UNIQUE
    );
END;
GO
