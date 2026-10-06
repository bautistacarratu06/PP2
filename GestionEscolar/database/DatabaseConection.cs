using System;
using System.IO;
using Microsoft.Data.SqlClient;
using GestionEscolar.Domain.Data; // allows to use DatabaseInstance class

class ExecuteDataBase
{
    static void Main(string[] args)
    {
        // Pull singletone instance of DatabaseInstance
        DatabaseInstance db = DatabaseInstance.GetInstance("PrincipalConexion");

        // SQL query to create the Student table if it doesn't exist
        string createTableSql = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Student')
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

        // Open and execute the SQL command to create the table
        using (SqlConnection conn = db.GetConnection())
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(createTableSql, conn))
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("Singleton conected and 'Student' table created.");
            }
        }
    }
}