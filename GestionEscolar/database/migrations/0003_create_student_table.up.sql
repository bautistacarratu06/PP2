USE course_tracking;
GO

IF OBJECT_ID(N'dbo.student', N'U') IS NOT NULL
    DROP TABLE dbo.student;
GO

IF OBJECT_ID(N'dbo.student', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.student (
        dni VARCHAR(20) NOT NULL CONSTRAINT pk_student PRIMARY KEY,
        section_id INT NOT NULL,
        student_number NVARCHAR(20) NOT NULL,
        last_name NVARCHAR(80) NOT NULL,
        first_name NVARCHAR(80) NOT NULL,
        phone NVARCHAR(30) NULL,
        email NVARCHAR(120) NULL,
        created_at DATETIME2(0) NOT NULL CONSTRAINT df_student_created_at DEFAULT SYSDATETIME(),
        CONSTRAINT uq_student_section_student_number UNIQUE (section_id, student_number),
        CONSTRAINT fk_student_section FOREIGN KEY (section_id) REFERENCES dbo.section(id)
    );
END;
GO
