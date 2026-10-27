using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GestionEscolar.Data.Persistence;

internal static class UniqueConstraint
{
    public static bool IsViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException sql && sql.Number is 2601 or 2627)
                return true;
        }

        return false;
    }
}
