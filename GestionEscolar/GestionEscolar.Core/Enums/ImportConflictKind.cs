namespace GestionEscolar.Core.Enums;

public enum ConflictCriterion
{
    Dni,
    StudentNumber,
    Both
}

public enum ConflictOrigin
{
    Database,
    File,
    DatabaseAndFile
}

public enum ImportDecision
{
    Omit,
    Merge
}
