namespace UI.ViewModels;

public enum StudentsActionKind
{
    Attendance,
    Submissions,
    Deactivate
}

// Una acción masiva de la paleta: tipo, título y, si pide confirmar, el texto según la cantidad elegida.
public sealed record ActionPaletteItem(StudentsActionKind Kind, string Title, Func<int, string>? ConfirmText = null);
