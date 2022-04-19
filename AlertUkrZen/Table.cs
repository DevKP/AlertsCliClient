using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlertUkrZen.Extensions;
using Ardalis.GuardClauses;

namespace AlertUkrZen
{
    public class Table
    {
        private readonly List<List<Field>> _rows;
        private readonly int _columnsNumber;
        private readonly bool _havingHeaders;

        private const char RowsDividerSymbol = '-';
        private const string Separator  = " | ";

        public IEnumerable<IEnumerable<Field>> Rows => 
            _havingHeaders ? _rows.Skip(1) : _rows;

        public Table(int columns)
        {
            _columnsNumber = columns;
            _rows = new List<List<Field>>();
        }

        public Table(int columns, string[] headers) : this(columns)
        {
            Guard.Against.OutOfRange(headers.Length, nameof(headers), columns, columns);

            _rows = _rows.Prepend(headers.Select(header => new Field(header)).ToList()).ToList();
            _havingHeaders = true;
        }

        public Table(int columns, IEnumerable<IEnumerable<string>> rows, string[] headers) : this(columns, headers)
        {
            AddRange(rows);
        }

        public Table(int columns, IEnumerable<IEnumerable<string>> rows) : this(columns)
        {
            AddRange(rows);
        }

        private void Add(IEnumerable<Field> fields)
        {
            var fieldsList = fields.ToList();

            Guard.Against.OutOfRange(fieldsList.Count, nameof(fields), _columnsNumber, _columnsNumber);

            _rows.Add(fieldsList);

            CalculatePaddings();
        }

        private void AddRange(IEnumerable<IEnumerable<Field>> rows)
        {
            _rows.AddRange(rows.Select(row =>
            {
                var rowList = row.ToList();

                Guard.Against.OutOfRange(rowList.Count, nameof(rows), _columnsNumber, _columnsNumber);

                return rowList;
            }));

            CalculatePaddings();
        }

        public void Add(IEnumerable<string> fields)
        {
            Add(fields.Select(f => new Field(f)));
        }

        public void AddRange(IEnumerable<IEnumerable<string>> rows)
        {
            var rowsList = rows.Select(r => r.Select(f => new Field(f)));
            AddRange(rowsList);
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            var width = CalculateTableWidth();

            stringBuilder.AppendRowsDivider(RowsDividerSymbol, width);

            if (_havingHeaders)
            {
                stringBuilder.AppendRows(_rows.Take(1), Separator);
                stringBuilder.AppendRowsDivider(RowsDividerSymbol, width);
                stringBuilder.AppendRows(_rows.Skip(1), Separator);
            }
            else
            {
                stringBuilder.AppendRows(_rows, Separator);
            }

            stringBuilder.AppendRowsDivider(RowsDividerSymbol, width);

            return stringBuilder.ToString();
        }

        private int CalculateTableWidth()
        {
            return _rows.First().Sum(field => field.Padding) +
                   (Separator.Length * _columnsNumber);
        }

        private void CalculatePaddings()
        {
            for (var columnId = 0; columnId < _columnsNumber; columnId++)
            {
                var maxLength = _rows.Select(row => row[columnId].Data.Length).Max();
                _rows.ForEach(row => row[columnId].Padding = maxLength);
            }
        }
    }
}