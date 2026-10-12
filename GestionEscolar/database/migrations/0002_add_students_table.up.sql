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
    ('40000000', 'María', 'Fernández', 'maria.fernandez@ejemplo.com', '+54 11 5555-0101'),
    ('41000000', 'Juan', 'Pérez', 'juan.perez@ejemplo.com', '+54 11 5555-0102'),
    ('42000000', 'Lucía', 'Gómez', 'lucia.gomez@ejemplo.com', '+54 11 5555-0103'),
    ('43000000', 'Santiago', 'Rodríguez', 'santiago.rodriguez@ejemplo.com', '+54 11 5555-0104'),
    ('44000000', 'Valentina', 'Díaz', 'valentina.diaz@ejemplo.com', '+54 11 5555-0105');
GO