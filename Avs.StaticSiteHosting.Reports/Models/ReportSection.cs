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

        public TableSection(string[] columns, TableRow[] rows)
        {
            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
            Rows = rows ?? throw new ArgumentNullException(nameof(rows));
        }
    }

    public class TableRow
    {
        public object[]? Cells { get; set; }
    }
}