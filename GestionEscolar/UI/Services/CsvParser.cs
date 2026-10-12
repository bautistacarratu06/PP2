using System.Text;

namespace UI.Services;

public static class CsvParser
{
    public static List<string> ParseLine(string line)
    {
        var fields = new List<string>();

        var field = new StringBuilder();

        var isInsideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];

            var hasNextCharacter = i + 1 < line.Length;

            if (character == '"')
            {
                var isEscapedQuote =
                    isInsideQuotes &&
                    hasNextCharacter &&
                    line[i + 1] == '"';

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
            else
            {
                // comprueba si el carácter actual es una coma
                var isFieldSeparator = character == ',';

                // 1 coma solo separa campos cuando está fuera de las comillas
                var canSplitField = isFieldSeparator && !isInsideQuotes;

                if (canSplitField)
                {
                    // guarda el campo terminado y prepara el siguiente
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    // el carácter pertenece al contenido del campo actual
                    field.Append(character);
                }
            }
        }

        // guarda el último campo, ya que no necesariamente termina en coma
        fields.Add(field.ToString());

        return fields;
    }
}