namespace GestionEscolar.Core.Models;

// Comisión (dbo.section, migración 0002): id + nombre único, p. ej. "Mañana - 1A".
public sealed record Section(int Id, string Name)
{
    // Lo usa el Picker para mostrar el texto.
    public override string ToString() => Name;
}
