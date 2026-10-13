USE course_tracking;
GO

IF OBJECT_ID(N'dbo.setting', N'U') IS NOT NULL
    DROP TABLE dbo.setting;
GO

IF OBJECT_ID(N'dbo.setting', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.setting (
        setting_key NVARCHAR(40) NOT NULL CONSTRAINT pk_setting PRIMARY KEY,
        setting_value INT NOT NULL,
        CONSTRAINT ck_setting_value CHECK (setting_value BETWEEN 0 AND 100)
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.setting WHERE setting_key = N'high_threshold')
    INSERT INTO dbo.setting (setting_key, setting_value) VALUES (N'high_threshold', 50);
IF NOT EXISTS (SELECT 1 FROM dbo.setting WHERE setting_key = N'medium_threshold')
    INSERT INTO dbo.setting (setting_key, setting_value) VALUES (N'medium_threshold', 25);
GO
