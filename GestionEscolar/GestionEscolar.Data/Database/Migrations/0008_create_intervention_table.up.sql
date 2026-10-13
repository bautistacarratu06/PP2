USE course_tracking;
GO

IF OBJECT_ID(N'dbo.intervention', N'U') IS NOT NULL
    DROP TABLE dbo.intervention;
GO

IF OBJECT_ID(N'dbo.intervention', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.intervention (
        id INT IDENTITY(1,1) NOT NULL CONSTRAINT pk_intervention PRIMARY KEY,
        student_id INT NOT NULL,
        intervention_date DATE NOT NULL CONSTRAINT df_intervention_date DEFAULT CAST(SYSDATETIME() AS DATE),
        channel NVARCHAR(20) NOT NULL,
        outcome NVARCHAR(60) NOT NULL,
        note NVARCHAR(500) NULL,
        CONSTRAINT ck_intervention_channel CHECK (channel IN (N'Llamada', N'WhatsApp', N'Correo', N'Presencial')),
        CONSTRAINT ck_intervention_outcome CHECK (outcome IN (N'Contactado - se comprometió a volver', N'Contactado - sin novedades', N'No contestó', N'Abandonó confirmado')),
        CONSTRAINT fk_intervention_student FOREIGN KEY (student_id) REFERENCES dbo.student(id)
    );
END;
GO
