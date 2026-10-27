using System.Text;

namespace dWebShop.DataMigration;

/// <summary>
/// Reads table rows straight out of a mysqldump / MariaDB dump file, so data can
/// be recovered without restoring the old database anywhere. Column names come
/// from each table's CREATE TABLE block; rows from its INSERT INTO statements.
/// </summary>
public class SqlDumpReader
{
    private readonly string _sql;

    public SqlDumpReader(string path) => _sql = File.ReadAllText(path, Encoding.UTF8);

    /// <summary>All rows of <paramref name="table"/>, keyed by column name. NULL becomes null.</summary>
    public List<Dictionary<string, string?>> Rows(string table)
    {
        var columns = Columns(table);
        var rows = new List<Dictionary<string, string?>>();
        var marker = $"INSERT INTO `{table}` VALUES ";
        var at = 0;
        while ((at = _sql.IndexOf(marker, at, StringComparison.Ordinal)) >= 0)
        {
            at += marker.Length;
            foreach (var values in ParseTuples(ref at))
            {
                if (values.Count != columns.Count)
                    throw new FormatException($"{table}: row has {values.Count} values but the table has {columns.Count} columns.");
                var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < columns.Count; i++) row[columns[i]] = values[i];
                rows.Add(row);
            }
        }
        return rows;
    }

    private List<string> Columns(string table)
    {
        var start = _sql.IndexOf($"CREATE TABLE `{table}` (", StringComparison.Ordinal);
        if (start < 0) throw new InvalidOperationException($"Table {table} not found in dump.");
        var end = _sql.IndexOf(") ENGINE", start, StringComparison.Ordinal);
        return _sql[start..end].Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.StartsWith('`'))
            .Select(l => l[1..l.IndexOf('`', 1)])
            .ToList();
    }

    // Parses "(a,'b',NULL),(…),…;" starting at pos; leaves pos after the ';'.
    private IEnumerable<List<string?>> ParseTuples(ref int pos)
    {
        var tuples = new List<List<string?>>();
        while (pos < _sql.Length)
        {
            var c = _sql[pos];
            if (c == ';') { pos++; break; }
            if (c != '(') { pos++; continue; }

            pos++; // past '('
            var values = new List<string?>();
            while (true)
            {
                values.Add(ReadValue(ref pos));
                var sep = _sql[pos++];
                if (sep == ')') break;
                if (sep != ',') throw new FormatException($"Unexpected '{sep}' in dump at offset {pos - 1}.");
            }
            tuples.Add(values);
        }
        return tuples;
    }

    private string? ReadValue(ref int pos)
    {
        if (_sql[pos] == '\'')
        {
            var sb = new StringBuilder();
            pos++;
            while (true)
            {
                var c = _sql[pos++];
                if (c == '\\')
                {
                    var e = _sql[pos++];
                    sb.Append(e switch { 'n' => '\n', 'r' => '\r', 't' => '\t', '0' => '\0', 'Z' => '\x1a', _ => e });
                }
                else if (c == '\'')
                {
                    if (_sql[pos] == '\'') { sb.Append('\''); pos++; } // '' escape
                    else break;
                }
                else sb.Append(c);
            }
            return sb.ToString();
        }

        var startPos = pos;
        while (_sql[pos] != ',' && _sql[pos] != ')') pos++;
        var raw = _sql[startPos..pos];
        return raw == "NULL" ? null : raw;
    }
}
