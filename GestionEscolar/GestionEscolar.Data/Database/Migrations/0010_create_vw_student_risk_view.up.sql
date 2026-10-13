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
    ('410000', 1, N'LEG-1001', N'Doncic', N'Luka', N'1122334455', N'ldoncic@email.com'),
    ('420000', 1, N'LEG-1002', N'Irving', N'Kyrie', N'1133445566', N'kirving@email.com'),
    ('430000', 1, N'LEG-1003', N'Nowitzki', N'Dirk', N'1144556677', N'dnowitzki@email.com'),
    ('440000', 1, N'LEG-1004', N'Lively', N'Dereck', N'1155667788', N'dlively@email.com'),
    ('450000', 1, N'LEG-1005', N'Terry', N'Jason', N'1166778899', N'jterry@email.com'),
    
    -- Comisión 1B (Tarde) - IDs 6 al 10
    ('410300', 2, N'LEG-2001', N'García', N'Zagreus', N'1177889900', N'zgarcia@email.com'),
    ('424000', 2, N'LEG-2002', N'López', N'Melinoe', N'1188990011', N'mlopez@email.com'),
    ('435000', 2, N'LEG-2003', N'Martínez', N'Nyx', N'1199001122', N'nmartinez@email.com'),
    ('446000', 2, N'LEG-2004', N'Sánchez', N'Caronte', N'1100112233', N'csanchez@email.com'),
    ('452300', 2, N'LEG-2005', N'Gómez', N'Aquiles', N'1111223344', N'agomez@email.com'),
    
    -- Comisión 1C (Noche) - IDs 11 al 15
    ('411200', 3, N'LEG-3001', N'Thompson', N'Klay', N'1122112211', N'kthompson@email.com'),
    ('420300', 3, N'LEG-3002', N'Washington', N'PJ', N'1133223322', N'pwashington@email.com'),
    ('430700', 3, N'LEG-3003', N'Gafford', N'Daniel', N'1144334433', N'dgafford@email.com'),
    ('440900', 3, N'LEG-3004', N'Exum', N'Dante', N'1155445544', N'dexum@email.com'),
    ('450800', 3, N'LEG-3005', N'Kidd', N'Jason', N'1166556655', N'jkidd@email.com'),
    
    -- Comisión 1D (Sábados) - IDs 16 al 20
    ('410999', 4, N'LEG-4001', N'Silva', N'Marcos', N'1177667766', N'msilva@email.com'),
    ('420088', 4, N'LEG-4002', N'Herrera', N'Camila', N'1188778877', N'cherrera@email.com'),
    ('430077', 4, N'LEG-4003', N'Castro', N'Valentina', N'1199889988', N'vcastro@email.com'),
    ('445006', 4, N'LEG-4004', N'Romero', N'Lucas', N'1100990099', N'lromero@email.com'),
    ('450808', 4, N'LEG-4005', N'Díaz', N'Mateo', N'1110001000', N'mdiaz@email.com');
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