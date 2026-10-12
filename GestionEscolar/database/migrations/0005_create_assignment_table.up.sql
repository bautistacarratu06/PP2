USE course_tracking;
GO

IF OBJECT_ID(N'dbo.assignment', N'U') IS NOT NULL
    DROP TABLE dbo.assignment;
GO

IF OBJECT_ID(N'dbo.assignment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.assignment (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT pk_assignment PRIMARY KEY,
        section_id INT NOT NULL,
        name NVARCHAR(120) NOT NULL,
        due_date DATE NOT NULL,
        CONSTRAINT fk_assignment_section FOREIGN KEY (section_id) REFERENCES dbo.section(id)
    );
END;
GO
