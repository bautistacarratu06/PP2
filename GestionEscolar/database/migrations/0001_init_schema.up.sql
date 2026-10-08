-----------------
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name='StudentManagementDB')
BEGIN
 CREATE DATABASE StudentManagementDB;
END
GO