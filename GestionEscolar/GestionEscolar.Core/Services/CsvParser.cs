using System.Text;

namespace GestionEscolar.Core.Services;

public static class CsvParser
{
    public static List<string> ParseLine(string line)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        var isInsideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
                ReadQuote(line, ref i, field, ref isInsideQuotes);
            else
                ReadCharacter(line[i], field, fields, isInsideQuotes);
        }

        // guarda el último campo, ya que no necesariamente termina en coma
        fields.Add(field.ToString());
        return fields;
    }

    private static void ReadQuote(string line, ref int i, StringBuilder field, ref bool isInsideQuotes)
    {
        var isEscapedQuote = isInsideQuotes && i + 1 < line.Length && line[i + 1] == '"';
        if (isEscapedQuote)
        {
            field.Append('"');
            i++;
        }
        else
        {
            // abre o cierra el campo entrecomillado
            isInsideQuotes = !isInsideQuotes;
        }
    }

    private static void ReadCharacter(char character, StringBuilder field, List<string> fields, bool isInsideQuotes)
    {
        // una coma solo separa campos cuando está fuera de las comillas
        if (character == ',' && !isInsideQuotes)
        {
            fields.Add(field.ToString());
            field.Clear();
        }
        else
        {
            field.Append(character);
        }
    }
}
