using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Data.SqlClient; // Requiere instalar NuGet Microsoft.Data.SqlClient

namespace GestionEscolar.Domain.Data
{
    class DatabaseInstance
    {
        // 1. Connection string pointing to your local SQL Server instance
        private readonly string _connectionString =
            "Server=.\\SQLEXPRESS;Database=course_tracking;Trusted_Connection=True;TrustServerCertificate=True;";

        // Private constructor: prevents direct instantiation with "new DatabaseInstance()" from outside
        private DatabaseInstance() { }

        // Static variable with Singleton type, that holds the only instance of the class "_instance"
        private static DatabaseInstance? _instance;

        // Creates an instance called "_lock" with "object" type, which is used for locking the critical section of code that creates the instance of the class.
        private static readonly object _lock = new object();

        // Creates a public static method called "GetInstance" that returns the only instance of the class (Double-Check Locking).
        public static DatabaseInstance GetInstance(string value)
        {
            if (_instance == null)
            {
                // Uses "lock" (reserved word) to look at the _lock object value
                lock (_lock)
                {
                    // If the _instance variable is null, creates a new instance of the class and assigns it to the _instance variable
                    if (_instance == null)
                    {
                        _instance = new DatabaseInstance();
                        _instance.Value = value;
                    }
                }
            }
            return _instance;
        }

        // Property used to store an instance identifier
        public string Value { get; set; } = string.Empty;

        // Method that returns a new SqlConnection object for executing queries
        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
        // Returns true if a connection can be opened against the database
        public bool TestConnection()
        {
            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();
                return true;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        // Lists the user tables of the database as "schema.table"
        public List<string> ListTables()
        {
            var tables = new List<string>();
            const string sql =
                @"SELECT TABLE_SCHEMA, TABLE_NAME
                  FROM INFORMATION_SCHEMA.TABLES
                  WHERE TABLE_TYPE = 'BASE TABLE'
                  ORDER BY TABLE_SCHEMA, TABLE_NAME";

            using SqlConnection conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                tables.Add($"{reader.GetString(0)}.{reader.GetString(1)}");

            return tables;
        }
    }
}
