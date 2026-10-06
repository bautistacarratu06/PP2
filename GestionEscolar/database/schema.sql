-----------------
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name='BDstudents')
BEGIN
 CREATE DATABASE BDstudents;
END
GO

---------------------------

USE BDstudents;
GO

---------------------------
CREATE TABLE Student
(
id int IDENTITY(1,1),
dni varchar(20) PRIMARY KEY,
first_name varchar(50),
last_name varchar(50),
email varchar(120),
phone varchar(30),
created_at datetime default GETDATE()
)

