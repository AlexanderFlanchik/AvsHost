namespace Avs.StaticSiteHosting.Reports.Models
{
    public abstract class ReportSection
    {
    }

    public class TextSection : ReportSection
    {
        public string? Content { get; set; }
    }

    public class TableSection: ReportSection
    {
        public string[] Columns { get; private set; }
        public TableRow[] Rows { get; private set; }
        public TableRow? Totals { get; set; }

        public TableSection(string[] columns, TableRow[] rows)
        {
            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
            Rows = rows ?? throw new ArgumentNullException(nameof(rows));
        }
    }

    public class TableRow
    {
        /// <summary>
        /// Row cells
        /// </summary>
        public TableCell[]? Cells { get; set; }
    }

    public class TableCell
    {
        public object? Value { get; set; }
        public TableCellAlign Align { get; set; }
        public bool IsBold { get; set; }
        
        public TableCell Bold()
        {
            IsBold = true;
            return this;
        }

        public TableCell WithAlign(TableCellAlign align)
        {
            Align = align;
            return this;
        }

        public override string? ToString()
        {
            return Value?.ToString();
        }

        public static implicit operator TableCell(string value) => new() { Value = value };
        public static implicit operator TableCell(int value) => new() { Value = value };
        public static implicit operator TableCell(decimal value) => new() { Value = value };
        public static implicit operator TableCell(DateTime value) => new() { Value = value };
    }

    public enum TableCellAlign
    {
        Left,
        Center,
        Right
    }
}