using System.Text;

namespace GestionEscolar.Core.Services;

// Arma registros CSV completos: si una comilla queda abierta, el registro sigue en la línea siguiente.
// StartLine es el número (desde 1) de la línea física donde empezó el último registro leído.
public sealed class CsvRecordReader
{
    private readonly TextReader _reader;
    private int _physicalLine;

    public CsvRecordReader(TextReader reader)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public int StartLine { get; private set; }

    public string? ReadRecord()
    {
        var first = _reader.ReadLine();
        if (first is null)
            return null;

        _physicalLine++;
        StartLine = _physicalLine;
        if (!HasOpenQuote(first))
            return first;

        var record = new StringBuilder(first);
        var open = true;
        while (open && _reader.ReadLine() is { } next)
        {
            _physicalLine++;
            record.Append('\n').Append(next);
            open = HasOpenQuote(record.ToString());
        }

        return record.ToString();
    }

    // Las comillas escapadas ("") cuentan de a dos, así que la paridad alcanza.
    private static bool HasOpenQuote(string text) => text.Count(character => character == '"') % 2 == 1;
}
