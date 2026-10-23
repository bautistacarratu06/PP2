USE course_tracking;
GO

-- Comentado: queda deshabilitado hasta separar los scripts DDL de los DML.
-- IF OBJECT_ID(N'dbo.submission', N'U') IS NOT NULL
--     DROP TABLE dbo.submission;
GO

IF OBJECT_ID(N'dbo.submission', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.submission (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT pk_submission PRIMARY KEY,
        assignment_id INT NOT NULL,
        student_id INT NOT NULL,
        status NVARCHAR(12) NOT NULL,
        CONSTRAINT ck_submission_status CHECK (status IN (N'entregó', N'no entregó')),
        CONSTRAINT uq_submission_assignment_student UNIQUE (assignment_id, student_id),
        CONSTRAINT fk_submission_assignment FOREIGN KEY (assignment_id) REFERENCES dbo.assignment(id),
        CONSTRAINT fk_submission_student FOREIGN KEY (student_id) REFERENCES dbo.student(id)
    );
END;
GO
