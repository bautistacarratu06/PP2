using Microsoft.Data.SqlClient;

namespace GestionEscolar.Data;

// Código heredado: crea la tabla Student si no existe. Hoy el esquema sale de
// database/migrations, así que esto no se usa; queda por compatibilidad.
// TODO: borrarlo cuando el equipo confirme que alcanza con las migraciones.
public class ExecuteDataBase
{
    private readonly DatabaseInstance _db;

    public ExecuteDataBase(DatabaseInstance db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public void CreateStudentTable()
    {
        const string createTableSql =
            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Student')
            BEGIN
                CREATE TABLE Student
                (
                    id INT IDENTITY(1,1),
                    dni VARCHAR(20) PRIMARY KEY,
                    first_name VARCHAR(50),
                    last_name VARCHAR(50),
                    email VARCHAR(120),
                    phone VARCHAR(30),
                    created_at DATETIME DEFAULT GETDATE()
                );
            END";

        using SqlConnection conn = _db.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(createTableSql, conn);
        cmd.ExecuteNonQuery();
    }
}
