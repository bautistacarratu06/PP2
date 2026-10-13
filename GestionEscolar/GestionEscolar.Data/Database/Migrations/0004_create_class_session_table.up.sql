USE course_tracking;
GO

IF OBJECT_ID(N'dbo.class_session', N'U') IS NOT NULL
    DROP TABLE dbo.class_session;
GO

IF OBJECT_ID(N'dbo.class_session', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.class_session (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT pk_class_session PRIMARY KEY,
        section_id INT NOT NULL,
        session_date DATE NOT NULL,
        CONSTRAINT uq_class_session_section_date UNIQUE (section_id, session_date),
        CONSTRAINT fk_class_session_section FOREIGN KEY (section_id) REFERENCES dbo.section(id)
    );
END;
GO
