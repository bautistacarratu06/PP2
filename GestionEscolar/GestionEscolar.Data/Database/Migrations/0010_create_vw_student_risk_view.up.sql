USE course_tracking;
GO
CREATE OR ALTER VIEW dbo.vw_student_risk
AS
WITH attendance AS (
    SELECT ar.student_id,
           COUNT_BIG(*) AS attendance_records,
           SUM(CASE WHEN ar.status = N'presente' THEN 1 ELSE 0 END) AS present_count
    FROM dbo.attendance_record AS ar
    GROUP BY ar.student_id
), submissions AS (
    SELECT s.student_id,
           COUNT_BIG(*) AS submission_records,
           SUM(CASE WHEN s.status = N'entregó' THEN 1 ELSE 0 END) AS submitted_count
    FROM dbo.submission AS s
    GROUP BY s.student_id
), signals AS (
    SELECT st.id AS student_id,
           CAST(a.present_count AS DECIMAL(10,4)) / NULLIF(a.attendance_records, 0) AS attendance_pct,
           CAST(s.submitted_count AS DECIMAL(10,4)) / NULLIF(s.submission_records, 0) AS submission_pct,
           a.attendance_records,
           s.submission_records,
           a.present_count,
           s.submitted_count
    FROM dbo.student AS st
    LEFT JOIN attendance AS a ON a.student_id = st.id
    LEFT JOIN submissions AS s ON s.student_id = st.id
), scored AS (
    SELECT *,
           CASE
               WHEN attendance_pct IS NULL AND submission_pct IS NULL THEN NULL
               WHEN attendance_pct IS NULL THEN submission_pct * 100.0
               WHEN submission_pct IS NULL THEN attendance_pct * 100.0
               ELSE (attendance_pct * 0.6 + submission_pct * 0.4) * 100.0
           END AS score
    FROM signals
)
SELECT student_id,
       attendance_pct * 100.0 AS attendance_pct,
       submission_pct * 100.0 AS submission_pct,
       score,
       CASE WHEN score IS NULL THEN NULL ELSE ROUND(100.0 - score, 0) END AS risk,
       attendance_records,
       submission_records,
       present_count,
       submitted_count
FROM scored;
GO


USE course_tracking;
GO

-- ======================================================================
-- SCRIPT DE POBLACIÓN DE DATOS MASIVOS PARA ENTORNO DE PRUEBAS
-- ======================================================================

-- 1. Insertar Sections (4 Comisiones)
INSERT INTO dbo.section (name) 
VALUES 
    (N'Mañana - 1A'), 
    (N'Tarde - 1B'),
    (N'Noche - 1C'),
    (N'Sábados - 1D');
GO

-- 2. Insertar Students (20 estudiantes distribuidos en las 4 comisiones)
INSERT INTO dbo.student (dni, section_id, student_number, last_name, first_name, phone, email) 
VALUES 
    -- Comisión 1A (Mañana) - IDs 1 al 5
    ('38452716', 1, N'LEG-1001', N'Fernández', N'Martina', N'1100001001', N'martina.fernandez@example.com'),
    ('41263895', 1, N'LEG-1002', N'González', N'Tomás', N'1100001002', N'tomas.gonzalez@example.com'),
    ('39741528', 1, N'LEG-1003', N'Rodríguez', N'Lucía', N'1100001003', N'lucia.rodriguez@example.com'),
    ('42581937', 1, N'LEG-1004', N'López', N'Facundo', N'1100001004', N'facundo.lopez@example.com'),
    ('36829451', 1, N'LEG-1005', N'Martínez', N'Camila', N'1100001005', N'camila.martinez@example.com'),
    ('40173682', 1, N'LEG-1006', N'Pérez', N'Benjamín', N'1100001006', N'benjamin.perez@example.com'),
    ('43915267', 1, N'LEG-1007', N'García', N'Valentina', N'1100001007', N'valentina.garcia@example.com'),
    ('37684129', 1, N'LEG-1008', N'Sánchez', N'Joaquín', N'1100001008', N'joaquin.sanchez@example.com'),
    ('41829563', 1, N'LEG-1009', N'Romero', N'Agustina', N'1100001009', N'agustina.romero@example.com'),
    ('39276418', 1, N'LEG-1010', N'Sosa', N'Franco', N'1100001010', N'franco.sosa@example.com'),
    ('43158724', 1, N'LEG-1011', N'Torres', N'Julieta', N'1100001011', N'julieta.torres@example.com'),
    ('35941862', 1, N'LEG-1012', N'Álvarez', N'Santiago', N'1100001012', N'santiago.alvarez@example.com'),
    ('40627391', 1, N'LEG-1013', N'Ruiz', N'Florencia', N'1100001013', N'florencia.ruiz@example.com'),
    ('38715946', 1, N'LEG-1014', N'Ramírez', N'Ignacio', N'1100001014', N'ignacio.ramirez@example.com'),
    ('42369158', 1, N'LEG-1015', N'Acosta', N'Catalina', N'1100001015', N'catalina.acosta@example.com'),
    ('37182649', 1, N'LEG-1016', N'Medina', N'Bruno', N'1100001016', N'bruno.medina@example.com'),
    ('41495827', 1, N'LEG-1017', N'Herrera', N'Victoria', N'1100001017', N'victoria.herrera@example.com'),
    ('39518274', 1, N'LEG-1018', N'Domínguez', N'Gabriel', N'1100001018', N'gabriel.dominguez@example.com'),
    ('42837615', 1, N'LEG-1019', N'Castro', N'Emilia', N'1100001019', N'emilia.castro@example.com'),
    ('36294187', 1, N'LEG-1020', N'Morales', N'Nahuel', N'1100001020', N'nahuel.morales@example.com'),
    ('40962731', 1, N'LEG-1021', N'Ortiz', N'Paula', N'1100001021', N'paula.ortiz@example.com'),
    ('38159426', 1, N'LEG-1022', N'Silva', N'Leandro', N'1100001022', N'leandro.silva@example.com'),
    ('43621859', 1, N'LEG-1023', N'Rojas', N' Abril', N'1100001023', N'abril.rojas@example.com'),
    ('37462918', 1, N'LEG-1024', N'Benítez', N'Federico', N'1100001024', N'federico.benitez@example.com'),
    ('41736285', 1, N'LEG-1025', N'Vega', N'Milagros', N'1100001025', N'milagros.vega@example.com'),
    ('39085147', 1, N'LEG-1026', N'Aguirre', N'Ramiro', N'1100001026', N'ramiro.aguirre@example.com'),
    ('42719536', 1, N'LEG-1027', N'Pereyra', N'Antonella', N'1100001027', N'antonella.pereyra@example.com'),
    ('36571842', 1, N'LEG-1028', N'Godoy', N'Lisandro', N'1100001028', N'lisandro.godoy@example.com'),
    ('40285913', 1, N'LEG-1029', N'Molina', N'Rocío', N'1100001029', N'rocio.molina@example.com'),
    ('38847125', 1, N'LEG-1030', N'Ferreyra', N'Juan Cruz', N'1100001030', N'juancruz.ferreyra@example.com'),
    
    -- Comisión 1B (Tarde) - IDs 6 al 10
('39162784', 2, N'LEG-2001', N'Quiroga', N'Agustín', N'1100002001', N'agustin.quiroga@example.com'),
    ('42738156', 2, N'LEG-2002', N'Vázquez', N'Carolina', N'1100002002', N'carolina.vazquez@example.com'),
    ('36592418', 2, N'LEG-2003', N'Cabrera', N'Pablo', N'1100002003', N'pablo.cabrera@example.com'),
    ('40851729', 2, N'LEG-2004', N'Figueroa', N'Antonella', N'1100002004', N'antonella.figueroa@example.com'),
    ('38274615', 2, N'LEG-2005', N'Carrizo', N'Matías', N'1100002005', N'matias.carrizo@example.com'),
    ('41956328', 2, N'LEG-2006', N'Correa', N'Brenda', N'1100002006', N'brenda.correa@example.com'),
    ('37481592', 2, N'LEG-2007', N'Ponce', N'Gonzalo', N'1100002007', N'gonzalo.ponce@example.com'),
    ('43269751', 2, N'LEG-2008', N'Bravo', N'Juliana', N'1100002008', N'juliana.bravo@example.com'),
    ('39628417', 2, N'LEG-2009', N'Muñoz', N'Esteban', N'1100002009', N'esteban.munoz@example.com'),
    ('41572863', 2, N'LEG-2010', N'Paz', N'Micaela', N'1100002010', N'micaela.paz@example.com'),
    ('36915284', 2, N'LEG-2011', N'Villalba', N'Ramón', N'1100002011', N'ramon.villalba@example.com'),
    ('42381765', 2, N'LEG-2012', N'Roldán', N'Candela', N'1100002012', N'candela.roldan@example.com'),
    ('38529471', 2, N'LEG-2013', N'Peralta', N'Iván', N'1100002013', N'ivan.peralta@example.com'),
    ('40168257', 2, N'LEG-2014', N'Leiva', N'Celeste', N'1100002014', N'celeste.leiva@example.com'),
    ('43825196', 2, N'LEG-2015', N'Andrada', N'Federico', N'1100002015', N'federico.andrada@example.com'),
    ('37741629', 2, N'LEG-2016', N'Escobar', N'Natalia', N'1100002016', N'natalia.escobar@example.com'),
    ('41279538', 2, N'LEG-2017', N'Vera', N'Cristian', N'1100002017', N'cristian.vera@example.com'),
    ('39416825', 2, N'LEG-2018', N'Campos', N'Lorena', N'1100002018', N'lorena.campos@example.com'),
    ('42651973', 2, N'LEG-2019', N'Navarro', N'Damián', N'1100002019', N'damian.navarro@example.com'),
    ('36184752', 2, N'LEG-2020', N'Giménez', N'Belén', N'1100002020', N'belen.gimenez@example.com'),
    ('40729361', 2, N'LEG-2021', N'Franco', N'Kevin', N'1100002021', N'kevin.franco@example.com'),
    ('38962514', 2, N'LEG-2022', N'Miranda', N'Ailén', N'1100002022', N'ailen.miranda@example.com'),
    ('43417682', 2, N'LEG-2023', N'Ríos', N'Leandro', N'1100002023', N'leandro.rios@example.com'),
    ('37295841', 2, N'LEG-2024', N'Lucero', N'Melina', N'1100002024', N'melina.lucero@example.com'),
    ('41862735', 2, N'LEG-2025', N'Godoy', N'Adrián', N'1100002025', N'adrian.godoy@example.com'),
    ('39847162', 2, N'LEG-2026', N'Bustos', N'Noelia', N'1100002026', N'noelia.bustos@example.com'),
    ('42916387', 2, N'LEG-2027', N'Oliva', N'Lisandro', N'1100002027', N'lisandro.oliva@example.com'),
    ('36358291', 2, N'LEG-2028', N'Coronel', N'Florencia', N'1100002028', N'florencia.coronel@example.com'),
    ('40581736', 2, N'LEG-2029', N'Mansilla', N'Julián', N'1100002029', N'julian.mansilla@example.com'),
    ('38624957', 2, N'LEG-2030', N'Toledo', N'Carla', N'1100002030', N'carla.toledo@example.com'),
    
    -- Comisión 1C (Noche) - IDs 11 al 15
('39174826', 3, N'LEG-3001', N'Cardozo', N'Federico', N'1100003001', N'federico.cardozo@example.com'),
    ('42615387', 3, N'LEG-3002', N'Benítez', N'Mariana', N'1100003002', N'mariana.benitez@example.com'),
    ('37462951', 3, N'LEG-3003', N'Paredes', N'Lisandro', N'1100003003', N'lisandro.paredes@example.com'),
    ('40831765', 3, N'LEG-3004', N'Verón', N'Antonella', N'1100003004', N'antonella.veron@example.com'),
    ('38519472', 3, N'LEG-3005', N'Barrera', N'Emiliano', N'1100003005', N'emiliano.barrera@example.com'),
    ('41762839', 3, N'LEG-3006', N'Coronel', N'Luciana', N'1100003006', N'luciana.coronel@example.com'),
    ('36295184', 3, N'LEG-3007', N'Valdez', N'Ramiro', N'1100003007', N'ramiro.valdez@example.com'),
    ('43917265', 3, N'LEG-3008', N'Vega', N'Bianca', N'1100003008', N'bianca.vega@example.com'),
    ('39681527', 3, N'LEG-3009', N'Suárez', N'Facundo', N'1100003009', N'facundo.suarez@example.com'),
    ('42153768', 3, N'LEG-3010', N'Agüero', N'Milena', N'1100003010', N'milena.aguero@example.com'),
    ('37824619', 3, N'LEG-3011', N'Barrios', N'Joaquín', N'1100003011', N'joaquin.barrios@example.com'),
    ('41389527', 3, N'LEG-3012', N'Velasco', N'Camila', N'1100003012', N'camila.velasco@example.com'),
    ('38746195', 3, N'LEG-3013', N'Zárate', N'Ignacio', N'1100003013', N'ignacio.zarate@example.com'),
    ('43062817', 3, N'LEG-3014', N'Bermúdez', N'Julieta', N'1100003014', N'julieta.bermudez@example.com'),
    ('36951742', 3, N'LEG-3015', N'Quiñones', N'Sebastián', N'1100003015', N'sebastian.quinones@example.com'),
    ('40218653', 3, N'LEG-3016', N'Ferreyra', N'Agustina', N'1100003016', N'agustina.ferreyra@example.com'),
    ('39472518', 3, N'LEG-3017', N'Páez', N'Leandro', N'1100003017', N'leandro.paez@example.com'),
    ('42835196', 3, N'LEG-3018', N'Villagra', N'Natalia', N'1100003018', N'natalia.villagra@example.com'),
    ('37168245', 3, N'LEG-3019', N'Roldán', N'Matías', N'1100003019', N'matias.roldan@example.com'),
    ('41529763', 3, N'LEG-3020', N'Chávez', N'Pilar', N'1100003020', N'pilar.chavez@example.com'),
    ('38391627', 3, N'LEG-3021', N'Farias', N'Bruno', N'1100003021', N'bruno.farias@example.com'),
    ('43726185', 3, N'LEG-3022', N'Maldonado', N'Carolina', N'1100003022', N'carolina.maldonado@example.com'),
    ('40653821', 3, N'LEG-3023', N'Bustamante', N'Iván', N'1100003023', N'ivan.bustamante@example.com'),
    ('35982746', 3, N'LEG-3024', N'Rivarola', N'Candela', N'1100003024', N'candela.rivarola@example.com'),
    ('42479136', 3, N'LEG-3025', N'Carrera', N'Gonzalo', N'1100003025', N'gonzalo.carrera@example.com'),
    ('39251864', 3, N'LEG-3026', N'Santillán', N'Verónica', N'1100003026', N'veronica.santillan@example.com'),
    ('41086329', 3, N'LEG-3027', N'Frías', N'Tobías', N'1100003027', N'tobias.frias@example.com'),
    ('37629518', 3, N'LEG-3028', N'Escudero', N'Mora', N'1100003028', N'mora.escudero@example.com'),
    ('43581762', 3, N'LEG-3029', N'Cabral', N'Nahuel', N'1100003029', N'nahuel.cabral@example.com'),
    ('38862471', 3, N'LEG-3030', N'Lucero', N'Valeria', N'1100003030', N'valeria.lucero@example.com'),
    
    -- Comisión 1D (Sábados) - IDs 16 al 20
    ('39182647', 4, N'LEG-4001', N'Imhoff', N'Agustina', N'1100004001', N'agustina.imhoff@example.com'),
    ('42751938', 4, N'LEG-4002', N'Vergará', N'Martín', N'1100004002', N'martin.vergara@example.com'),
    ('36518294', 4, N'LEG-4003', N'Bianchi', N'Luciana', N'1100004003', N'luciana.bianchi@example.com'),
    ('40876315', 4, N'LEG-4004', N'Giménez', N'Franco', N'1100004004', N'franco.gimenez@example.com'),
    ('38251946', 4, N'LEG-4005', N'Ponce', N'Julieta', N'1100004005', N'julieta.ponce@example.com'),
    ('41983725', 4, N'LEG-4006', N'Villalba', N'Santiago', N'1100004006', N'santiago.villalba@example.com'),
    ('37429681', 4, N'LEG-4007', N'Peralta', N'Micaela', N'1100004007', N'micaela.peralta@example.com'),
    ('43258176', 4, N'LEG-4008', N'Barreto', N'Benicio', N'1100004008', N'benicio.barreto@example.com'),
    ('39617284', 4, N'LEG-4009', N'Funes', N'Camila', N'1100004009', N'camila.funes@example.com'),
    ('41593627', 4, N'LEG-4010', N'Godoy', N'Julián', N'1100004010', N'julian.godoy@example.com'),
    ('36928415', 4, N'LEG-4011', N'Maidana', N'Rocío', N'1100004011', N'rocio.maidana@example.com'),
    ('42376518', 4, N'LEG-4012', N'Nieva', N'Agustín', N'1100004012', N'agustin.nieva@example.com'),
    ('38561729', 4, N'LEG-4013', N'Soria', N'Emilia', N'1100004013', N'emilia.soria@example.com'),
    ('40195836', 4, N'LEG-4014', N'Ledesma', N'Federico', N'1100004014', N'federico.ledesma@example.com'),
    ('43816257', 4, N'LEG-4015', N'Paz', N'Milagros', N'1100004015', N'milagros.paz@example.com'),
    ('37752914', 4, N'LEG-4016', N'Casco', N'Bruno', N'1100004016', N'bruno.casco@example.com'),
    ('41268395', 4, N'LEG-4017', N'Olmos', N'Catalina', N'1100004017', N'catalina.olmos@example.com'),
    ('39481726', 4, N'LEG-4018', N'Bustos', N'Ramiro', N'1100004018', N'ramiro.bustos@example.com'),
    ('42635179', 4, N'LEG-4019', N'Pereyra', N'Antonella', N'1100004019', N'antonella.pereyra@example.com'),
    ('36197528', 4, N'LEG-4020', N'Galarza', N'Facundo', N'1100004020', N'facundo.galarza@example.com'),
    ('40761853', 4, N'LEG-4021', N'Lezcano', N'Valentina', N'1100004021', N'valentina.lezcano@example.com'),
    ('38925741', 4, N'LEG-4022', N'Falcón', N'Juan Manuel', N'1100004022', N'juanmanuel.falcon@example.com'),
    ('43481627', 4, N'LEG-4023', N'Cuello', N'Bianca', N'1100004023', N'bianca.cuello@example.com'),
    ('37264195', 4, N'LEG-4024', N'Aguilera', N'Lisandro', N'1100004024', N'lisandro.aguilera@example.com'),
    ('41857236', 4, N'LEG-4025', N'Farías', N'Celeste', N'1100004025', N'celeste.farias@example.com'),
    ('39812657', 4, N'LEG-4026', N'Lucero', N'Tobías', N'1100004026', N'tobias.lucero@example.com'),
    ('42975163', 4, N'LEG-4027', N'Moyano', N'Pilar', N'1100004027', N'pilar.moyano@example.com'),
    ('36381749', 4, N'LEG-4028', N'Bogado', N'Kevin', N'1100004028', N'kevin.bogado@example.com'),
    ('40529618', 4, N'LEG-4029', N'Coronel', N'Mora', N'1100004029', N'mora.coronel@example.com'),
    ('38647152', 4, N'LEG-4030', N'Riquelme', N'Dante', N'1100004030', N'dante.riquelme@example.com');
GO

-- 3. Insertar Class Sessions (3 clases por comisión)
INSERT INTO dbo.class_session (section_id, session_date) 
VALUES 
    -- Comisión 1A (IDs 1 al 3)
    (1, '2026-09-01'), (1, '2026-09-08'), (1, '2026-09-15'),
    -- Comisión 1B (IDs 4 al 6)
    (2, '2026-09-02'), (2, '2026-09-09'), (2, '2026-09-16'),
    -- Comisión 1C (IDs 7 al 9)
    (3, '2026-09-03'), (3, '2026-09-10'), (3, '2026-09-17'),
    -- Comisión 1D (IDs 10 al 12)
    (4, '2026-09-05'), (4, '2026-09-12'), (4, '2026-09-19');
GO

-- 4. Insertar Assignments (2 trabajos por comisión)
INSERT INTO dbo.assignment (section_id, name, due_date) 
VALUES 
    -- Comisión 1A (IDs 1 y 2)
    (1, N'TP1 - Fundamentos', '2026-09-10'), (1, N'TP2 - Base de Datos', '2026-09-20'),
    -- Comisión 1B (IDs 3 y 4)
    (2, N'TP1 - Lógica', '2026-09-11'), (2, N'TP2 - Programación POO', '2026-09-21'),
    -- Comisión 1C (IDs 5 y 6)
    (3, N'TP1 - Diseño UML', '2026-09-12'), (3, N'TP2 - Testing', '2026-09-22'),
    -- Comisión 1D (IDs 7 y 8)
    (4, N'TP1 - Proyecto Integrador', '2026-09-15'), (4, N'TP2 - Despliegue', '2026-09-25');
GO

-- 5. Insertar Attendance Records (Registro de asistencias masivo)
INSERT INTO dbo.attendance_record (class_session_id, student_id, status) 
VALUES 
    -- Sesiones 1A (Clase 1, 2 y 3) para estudiantes 1 al 5
    (1, 1, N'presente'), (1, 2, N'presente'), (1, 3, N'presente'), (1, 4, N'presente'), (1, 5, N'ausente'),
    (2, 1, N'presente'), (2, 2, N'ausente'),  (2, 3, N'presente'), (2, 4, N'presente'), (2, 5, N'ausente'),
    (3, 1, N'presente'), (3, 2, N'presente'), (3, 3, N'ausente'),  (3, 4, N'presente'), (3, 5, N'ausente'),

    -- Sesiones 1B (Clase 4, 5 y 6) para estudiantes 6 al 10
    (4, 6, N'presente'), (4, 7, N'presente'), (4, 8, N'ausente'),  (4, 9, N'presente'), (4, 10, N'presente'),
    (5, 6, N'presente'), (5, 7, N'presente'), (5, 8, N'presente'), (5, 9, N'ausente'),  (5, 10, N'presente'),
    (6, 6, N'presente'), (6, 7, N'ausente'),  (6, 8, N'presente'), (6, 9, N'presente'), (6, 10, N'presente'),

    -- Sesiones 1C (Clase 7, 8 y 9) para estudiantes 11 al 15
    (7, 11, N'presente'), (7, 12, N'presente'), (7, 13, N'presente'), (7, 14, N'presente'), (7, 15, N'presente'),
    (8, 11, N'ausente'),  (8, 12, N'presente'), (8, 13, N'presente'), (8, 14, N'ausente'),  (8, 15, N'presente'),
    (9, 11, N'ausente'),  (9, 12, N'ausente'),  (9, 13, N'presente'), (9, 14, N'presente'), (9, 15, N'presente'),

    -- Sesiones 1D (Clase 10, 11 y 12) para estudiantes 16 al 20
    (10, 16, N'presente'), (10, 17, N'ausente'),  (10, 18, N'presente'), (10, 19, N'presente'), (10, 20, N'ausente'),
    (11, 16, N'presente'), (11, 17, N'ausente'),  (11, 18, N'ausente'),  (11, 19, N'presente'), (11, 20, N'ausente'),
    (12, 16, N'presente'), (12, 17, N'presente'), (12, 18, N'presente'), (12, 19, N'presente'), (12, 20, N'ausente');
GO

-- 6. Insertar Submissions (Entregas de los TPs)
INSERT INTO dbo.submission (assignment_id, student_id, status) 
VALUES 
    -- Comisión 1A (TP1 id 1, TP2 id 2)
    (1, 1, N'entregó'), (1, 2, N'entregó'),    (1, 3, N'entregó'), (1, 4, N'entregó'), (1, 5, N'no entregó'),
    (2, 1, N'entregó'), (2, 2, N'no entregó'), (2, 3, N'entregó'), (2, 4, N'entregó'), (2, 5, N'no entregó'),

    -- Comisión 1B (TP1 id 3, TP2 id 4)
    (3, 6, N'entregó'), (3, 7, N'entregó'),    (3, 8, N'no entregó'), (3, 9, N'entregó'), (3, 10, N'entregó'),
    (4, 6, N'entregó'), (4, 7, N'entregó'),    (4, 8, N'entregó'),    (4, 9, N'entregó'), (4, 10, N'entregó'),

    -- Comisión 1C (TP1 id 5, TP2 id 6)
    (5, 11, N'entregó'),    (5, 12, N'entregó'), (5, 13, N'entregó'), (5, 14, N'entregó'),    (5, 15, N'entregó'),
    (6, 11, N'no entregó'), (6, 12, N'entregó'), (6, 13, N'entregó'), (6, 14, N'no entregó'), (6, 15, N'entregó'),

    -- Comisión 1D (TP1 id 7, TP2 id 8)
    (7, 16, N'entregó'), (7, 17, N'no entregó'), (7, 18, N'entregó'), (7, 19, N'entregó'), (7, 20, N'no entregó'),
    (8, 16, N'entregó'), (8, 17, N'entregó'),    (8, 18, N'entregó'), (8, 19, N'entregó'), (8, 20, N'no entregó');
GO

-- 7. Insertar Interventions (Seguimiento de alumnos con faltas/entregas pendientes)
INSERT INTO dbo.intervention (student_id, intervention_date, channel, outcome, note) 
VALUES 
    -- Estudiante 5 (Faltó a todo y no entregó nada)
    (5, '2026-09-09', N'WhatsApp', N'Contactado - se comprometió a volver', N'Dijo que arranca la próxima semana, tuvo temas de salud.'),
    (5, '2026-09-17', N'Llamada', N'Abandonó confirmado', N'Confirma que deja el curso por horarios laborales cruzados.'),
    
    -- Estudiante 11 (Venía bien, faltó últimas 2)
    (11, '2026-09-12', N'Correo', N'No contestó', N'Se envió correo automatizado por las faltas consecutivas.'),
    
    -- Estudiante 17 (Faltas intermitentes, no entregó TP1)
    (17, '2026-09-07', N'Presencial', N'Contactado - sin novedades', N'Se habló en el pasillo, dijo que se ponía al día con el TP1 pero aún no lo subió.'),
    
    -- Estudiante 20 (Faltó a todo y no entregó)
    (20, '2026-09-15', N'Llamada', N'No contestó', N'Buzón de voz. Se deja mensaje solicitando que se comunique con bedelía.'),
    (20, '2026-09-22', N'Correo', N'Abandonó confirmado', N'Responde al correo avisando que se muda de provincia y no puede continuar.');
GO