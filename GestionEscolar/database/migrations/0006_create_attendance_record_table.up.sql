USE course_tracking;
GO

IF OBJECT_ID(N'dbo.attendance_record', N'U') IS NOT NULL
    DROP TABLE dbo.attendance_record;
GO

IF OBJECT_ID(N'dbo.attendance_record', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.attendance_record (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT pk_attendance_record PRIMARY KEY,
        class_session_id INT NOT NULL,
        student_id INT NOT NULL,
        status NVARCHAR(10) NOT NULL,
        corrected_at DATETIME2(0) NULL,
        CONSTRAINT ck_attendance_record_status CHECK (status IN (N'presente', N'ausente')),
        CONSTRAINT uq_attendance_record_session_student UNIQUE (class_session_id, student_id),
        CONSTRAINT fk_attendance_record_class_session FOREIGN KEY (class_session_id) REFERENCES dbo.class_session(id),
        CONSTRAINT fk_attendance_record_student FOREIGN KEY (student_id) REFERENCES dbo.student(id)
    );
END;
GO
