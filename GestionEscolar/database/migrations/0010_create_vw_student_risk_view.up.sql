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
