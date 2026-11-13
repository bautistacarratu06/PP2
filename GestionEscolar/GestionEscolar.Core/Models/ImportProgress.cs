namespace GestionEscolar.Core.Models;

// Etapas de una importación larga, en el orden en que pasan.
public enum ImportStage
{
    Reading,
    Checking,
    Saving,
    Finishing
}

// Avance de una importación. Total en 0 = todavía no se sabe cuántas filas son.
public readonly record struct ImportProgress(ImportStage Stage, int Done, int Total);
