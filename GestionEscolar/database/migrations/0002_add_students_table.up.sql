---------------------------

USE StudentManagementDB;
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
);

USE StudentManagementDB;
GO

INSERT INTO Student
    (dni, first_name, last_name, email, phone)
VALUES
    ('EST-2024-001', 'María', 'Fernández', 'maria.fernandez@ejemplo.com', '+54 11 5555-0101'),
    ('EST-2024-002', 'Juan', 'Pérez', 'juan.perez@ejemplo.com', '+54 11 5555-0102'),
    ('EST-2024-003', 'Lucía', 'Gómez', 'lucia.gomez@ejemplo.com', '+54 11 5555-0103'),
    ('EST-2024-004', 'Santiago', 'Rodríguez', 'santiago.rodriguez@ejemplo.com', '+54 11 5555-0104'),
    ('EST-2024-005', 'Valentina', 'Díaz', 'valentina.diaz@ejemplo.com', '+54 11 5555-0105');
GO