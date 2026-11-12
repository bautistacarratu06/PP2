using System.Data;
using UniqueConstraint = GestionEscolar.Data.Persistence.UniqueConstraint;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Data.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GestionEscolar.Data.Repositories;

public sealed class EfCommissionImportRepository(IDbContextFactory<CourseTrackingDbContext> contexts) : ICommissionImportRepository
{
    // Filas por SaveChanges: acota el tamaño del comando y la cantidad de entidades rastreadas.
    internal const int BatchSize = 200;

    private readonly IDbContextFactory<CourseTrackingDbContext> _contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));

    public async Task<IReadOnlyList<Section>> GetSectionsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await db.Sections
            .AsNoTracking()
            .OrderBy(section => section.Id)
            .Select(section => new { section.Id, section.Name })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(section => new Section(section.Id, section.Name)).ToList();
    }

    public async Task<IReadOnlyList<Student>> GetStudentsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await db.Students
            .AsNoTracking()
            .Select(student => new
            {
                student.Id,
                student.SectionId,
                student.Dni,
                student.LastName,
                student.FirstName,
                student.StudentNumber,
                student.Phone,
                student.Email,
                student.EnrolledOn
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(student => new Student
        {
            Id = student.Id,
            SectionId = student.SectionId,
            Dni = student.Dni,
            LastName = student.LastName,
            FirstName = student.FirstName,
            Name = student.FirstName + " " + student.LastName,
            StudentNumber = student.StudentNumber,
            Phone = student.Phone ?? string.Empty,
            Email = student.Email ?? string.Empty,
            EnrolledOn = student.EnrolledOn
        }).ToList();
    }

    public async Task<Section> CreateSectionAsync(string name, IReadOnlyList<Student> students, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(students);
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var section = new SectionEntity { Name = name };
        try
        {
            db.Sections.Add(section);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            foreach (var batch in students.Chunk(BatchSize))
                await SaveBatchAsync(db, section.Id, batch, cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException ex) when (UniqueConstraint.IsViolation(ex))
        {
            throw new ImportValidationException("Ya existe una comisión con ese nombre.");
        }

        return new Section(section.Id, section.Name);
    }

    public async Task ApplyAsync(
        int sectionId,
        IReadOnlyList<Student> inserts,
        IReadOnlyList<StudentContactChange> updates,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inserts);
        ArgumentNullException.ThrowIfNull(updates);
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await ApplyUpdatesAsync(db, updates, cancellationToken).ConfigureAwait(false);
        foreach (var batch in inserts.Chunk(BatchSize))
            await SaveBatchAsync(db, sectionId, batch, cancellationToken).ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<ImportResult> ApplyWithReportAsync(
        int sectionId,
        IReadOnlyList<NumberedStudent> inserts,
        IReadOnlyList<StudentContactChange> updates,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inserts);
        ArgumentNullException.ThrowIfNull(updates);
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await ApplyUpdatesAsync(db, updates, cancellationToken).ConfigureAwait(false);

        var errors = new List<RowError>();
        var inserted = 0;
        foreach (var batch in inserts.Chunk(BatchSize))
            inserted += await InsertBatchReportingAsync(db, transaction, sectionId, batch, errors, cancellationToken).ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new ImportResult(inserted, updates.Count, errors);
    }

    public async Task<ImportResult> BulkApplyAsync(
        int sectionId,
        IReadOnlyList<NumberedStudent> inserts,
        IReadOnlyList<StudentContactChange> updates,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inserts);
        ArgumentNullException.ThrowIfNull(updates);
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await ApplyUpdatesAsync(db, updates, cancellationToken).ConfigureAwait(false);

        var connection = (SqlConnection)db.Database.GetDbConnection();
        var sqlTransaction = (SqlTransaction)transaction.GetDbTransaction();
        var importId = Guid.NewGuid();
        await StageRowsAsync(connection, sqlTransaction, importId, inserts, cancellationToken).ConfigureAwait(false);
        var errors = await MergeStagedRowsAsync(connection, sqlTransaction, importId, sectionId, cancellationToken).ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new ImportResult(inserts.Count - errors.Count, updates.Count, errors);
    }

    private static async Task ApplyUpdatesAsync(
        CourseTrackingDbContext db,
        IReadOnlyList<StudentContactChange> updates,
        CancellationToken cancellationToken)
    {
        foreach (var batch in updates.Chunk(BatchSize))
        {
            var ids = batch.Select(update => update.StudentId).ToList();
            var stored = await db.Students
                .Where(student => ids.Contains(student.Id))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (var update in batch)
            {
                var student = stored.Single(item => item.Id == update.StudentId);
                student.LastName = update.LastName;
                student.FirstName = update.FirstName;
                student.Phone = EmptyToNull(update.Phone);
                student.Email = EmptyToNull(update.Email);
            }

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            db.ChangeTracker.Clear();
        }
    }

    // Guarda un lote; si falla, la excepción sube y la transacción externa no se confirma.
    private static async Task SaveBatchAsync(
        CourseTrackingDbContext db,
        int sectionId,
        IEnumerable<Student> batch,
        CancellationToken cancellationToken)
    {
        db.Students.AddRange(batch.Select(student => ToEntity(sectionId, student)));
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        db.ChangeTracker.Clear();
    }

    // Intenta el lote entero (un DNI repetido dentro del lote lo detecta EF antes de ir a la base); si falla, vuelve al punto de guardado y lo reintenta fila por fila
    // para dejar las válidas y anotar las rechazadas con su línea.
    private static async Task<int> InsertBatchReportingAsync(
        CourseTrackingDbContext db,
        IDbContextTransaction transaction,
        int sectionId,
        NumberedStudent[] batch,
        List<RowError> errors,
        CancellationToken cancellationToken)
    {
        const string savepoint = "import_batch";
        await transaction.CreateSavepointAsync(savepoint, cancellationToken).ConfigureAwait(false);
        try
        {
            await SaveBatchAsync(db, sectionId, batch.Select(item => item.Student), cancellationToken).ConfigureAwait(false);
            return batch.Length;
        }
        catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
        {
            db.ChangeTracker.Clear();
            await transaction.RollbackToSavepointAsync(savepoint, cancellationToken).ConfigureAwait(false);
        }

        var inserted = 0;
        foreach (var item in batch)
        {
            if (await TryInsertRowAsync(db, transaction, sectionId, item, errors, cancellationToken).ConfigureAwait(false))
                inserted++;
        }

        return inserted;
    }

    private static async Task<bool> TryInsertRowAsync(
        CourseTrackingDbContext db,
        IDbContextTransaction transaction,
        int sectionId,
        NumberedStudent item,
        List<RowError> errors,
        CancellationToken cancellationToken)
    {
        const string savepoint = "import_row";
        await transaction.CreateSavepointAsync(savepoint, cancellationToken).ConfigureAwait(false);
        try
        {
            await SaveBatchAsync(db, sectionId, [item.Student], cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (DbUpdateException ex)
        {
            db.ChangeTracker.Clear();
            await transaction.RollbackToSavepointAsync(savepoint, cancellationToken).ConfigureAwait(false);
            errors.Add(new RowError(item.Line, Describe(ex)));
            return false;
        }
    }

    private static string Describe(DbUpdateException exception) =>
        UniqueConstraint.IsViolation(exception)
            ? "El DNI o el legajo ya existe."
            : "No se pudo guardar la fila.";

    private static async Task StageRowsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        Guid importId,
        IReadOnlyList<NumberedStudent> inserts,
        CancellationToken cancellationToken)
    {
        var table = new DataTable();
        foreach (var (name, type) in StagingColumns)
            table.Columns.Add(name, type);

        foreach (var item in inserts)
        {
            var student = item.Student;
            table.Rows.Add(
                importId,
                item.Line,
                student.Dni,
                student.StudentNumber,
                student.LastName,
                student.FirstName,
                (object?)EmptyToNull(student.Phone) ?? DBNull.Value,
                (object?)EmptyToNull(student.Email) ?? DBNull.Value,
                student.EnrolledOn.Date);
        }

        using var copy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
        {
            DestinationTableName = "dbo.student_import_staging",
            BatchSize = 1000
        };
        foreach (var (name, _) in StagingColumns)
            copy.ColumnMappings.Add(name, name);

        await copy.WriteToServerAsync(table, cancellationToken).ConfigureAwait(false);
    }

    private static readonly (string Name, Type Type)[] StagingColumns =
    [
        ("import_id", typeof(Guid)),
        ("line", typeof(int)),
        ("dni", typeof(string)),
        ("student_number", typeof(string)),
        ("last_name", typeof(string)),
        ("first_name", typeof(string)),
        ("phone", typeof(string)),
        ("email", typeof(string)),
        ("enrolled_on", typeof(DateTime))
    ];

    // Todo set-based en la base: detecta conflictos (contra alumnos guardados y contra filas
    // anteriores del mismo archivo), inserta el resto, limpia el staging y devuelve los conflictos.
    private const string MergeSql =
        """
        SET NOCOUNT ON;
        SELECT st.line,
               CAST(CASE WHEN d.dni IS NOT NULL OR EXISTS (
                        SELECT 1 FROM dbo.student_import_staging o
                        WHERE o.import_id = st.import_id AND o.line < st.line AND o.dni = st.dni)
                    THEN 1 ELSE 0 END AS bit) AS dni_clash,
               CAST(CASE WHEN n.id IS NOT NULL OR EXISTS (
                        SELECT 1 FROM dbo.student_import_staging o
                        WHERE o.import_id = st.import_id AND o.line < st.line AND o.student_number = st.student_number)
                    THEN 1 ELSE 0 END AS bit) AS number_clash
        INTO #conflicts
        FROM dbo.student_import_staging st
        LEFT JOIN dbo.student d ON d.dni = st.dni
        LEFT JOIN dbo.student n ON n.section_id = @section_id AND n.student_number = st.student_number
        WHERE st.import_id = @import_id;

        DELETE FROM #conflicts WHERE dni_clash = 0 AND number_clash = 0;

        INSERT INTO dbo.student (dni, section_id, student_number, last_name, first_name, phone, email, enrolled_on)
        SELECT st.dni, @section_id, st.student_number, st.last_name, st.first_name, st.phone, st.email, st.enrolled_on
        FROM dbo.student_import_staging st
        WHERE st.import_id = @import_id
          AND NOT EXISTS (SELECT 1 FROM #conflicts c WHERE c.line = st.line);

        DELETE FROM dbo.student_import_staging WHERE import_id = @import_id;

        SELECT line, dni_clash, number_clash FROM #conflicts ORDER BY line;
        DROP TABLE #conflicts;
        """;

    private static async Task<List<RowError>> MergeStagedRowsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        Guid importId,
        int sectionId,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(MergeSql, connection, transaction) { CommandTimeout = 120 };
        command.Parameters.Add(new SqlParameter("@import_id", SqlDbType.UniqueIdentifier) { Value = importId });
        command.Parameters.Add(new SqlParameter("@section_id", SqlDbType.Int) { Value = sectionId });

        var errors = new List<RowError>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            errors.Add(new RowError(reader.GetInt32(0), ConflictMessage(reader.GetBoolean(1), reader.GetBoolean(2))));

        return errors;
    }

    private static string ConflictMessage(bool dniClash, bool numberClash) =>
        (dniClash, numberClash) switch
        {
            (true, true) => "El DNI y el legajo ya existen.",
            (true, false) => "El DNI ya existe.",
            _ => "El legajo ya existe."
        };

    private static StudentEntity ToEntity(int sectionId, Student student) => new()
    {
        SectionId = sectionId,
        Dni = student.Dni,
        StudentNumber = student.StudentNumber,
        LastName = student.LastName,
        FirstName = student.FirstName,
        Phone = EmptyToNull(student.Phone),
        Email = EmptyToNull(student.Email),
        EnrolledOn = student.EnrolledOn.Date
    };

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
