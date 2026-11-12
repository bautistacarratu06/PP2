namespace UI.ViewModels;

// Lo que la pantalla Estudiantes le pasa al detalle por Shell: la fila, el nombre y el id de la comisión.
public sealed record StudentDetailQuery(string? SectionName, StudentRow? Row, int? SectionId)
{
    public static StudentDetailQuery Read(IDictionary<string, object> query)
    {
        var sectionName = query.TryGetValue(StudentDetailViewModel.SectionQueryKey, out var section)
            ? section as string
            : null;
        var row = query.TryGetValue(StudentDetailViewModel.StudentQueryKey, out var value)
            ? value as StudentRow
            : null;
        int? sectionId = query.TryGetValue(StudentDetailViewModel.SectionIdQueryKey, out var id) && id is int sid && sid > 0
            ? sid
            : null;
        return new StudentDetailQuery(sectionName, row, sectionId);
    }
}
