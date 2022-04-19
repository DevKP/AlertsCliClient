using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlertUkrZen.Extensions
{
    public static class StringBuilderExtensions
    {
        public static void AppendRows(this StringBuilder stringBuilder, IEnumerable<IEnumerable<Field>> rows, string separator)
        {
            foreach (var row in rows)
            {
                stringBuilder.AppendTableRow(row, separator);
                stringBuilder.AppendNewLine();
            }
        }

        public static void AppendNewLine(this StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine();
        }

        public static void AppendTableRow(this StringBuilder stringBuilder, IEnumerable<Field> row, string separator)
        {
            var paddedFields = GetRightPaddedFields(row);

            stringBuilder.Append(separator.TrimStart());
            stringBuilder.Append(string.Join(separator, paddedFields));
            stringBuilder.Append(separator.TrimEnd());
        }

        private static IEnumerable<string> GetRightPaddedFields(IEnumerable<Field> row)
        {
            return row.Select(field => field.Data.PadRight(field.Padding));
        }

        public static void AppendRowsDivider(this StringBuilder stringBuilder, char symbol, int width)
        {
            stringBuilder.Append(' ');
            stringBuilder.AppendLine(new string(symbol, width - 1));
        }
    }
}