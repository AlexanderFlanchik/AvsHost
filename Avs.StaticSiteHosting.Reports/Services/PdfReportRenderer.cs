using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Avs.StaticSiteHosting.Reports.Models;
using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Reports.Contracts;
using iText.Layout.Properties;

namespace Avs.StaticSiteHosting.Reports.Services
{
    public class PdfReportRenderer : ReportRendererBase<Document>, IReportRenderer
    {       
        public ReportFormat Format => ReportFormat.PDF;

        public Task<byte[]> RenderAsync(Report report)
        {
            using var ms = new MemoryStream();

            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer.SetSmartMode(true));
            var document = new Document(pdf, PageSize.A4);

            document.SetMargins(20f, 20f, 20f, 20f);

            if (!string.IsNullOrEmpty(report.Title))
            {
                document.Add(new Paragraph(report.Title).SetFontSize(12f).SetBold());
            }
            
            // Rendering of all report sections
            foreach (var section in report.Sections)
            {
                var renderer = _sectionRenderers[section.GetType()];
                if (renderer is null)
                {
                    continue;
                }

                renderer(section, document);
            }

            document.Close();

            return Task.FromResult(ms.ToArray());
        }

        protected override void RenderTextSection(ReportSection section, Document document)
        {
            if (section is not TextSection textSection)
            {
                return;
            }

            document.Add(new Paragraph(textSection.Content).SetFontSize(10f));
        }

        protected override void RenderTableSection(ReportSection section, Document document)
        {
            if (section is not TableSection tableSection)
            {
                return;
            }

            var columns = tableSection.Columns;
            var table = new Table(columns.Length);

            // Header
            foreach (var column in columns)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(column).SetBold()));
            }

            // Table body
            foreach (var row in tableSection.Rows)
            {
                table.StartNewRow();
                foreach (var (rowCell, cell) in from rowCell in row.Cells
                                                let cell = new Cell()
                                                select (rowCell, cell))
                {
                    cell.Add(new Paragraph(rowCell?.Value is not null ? rowCell.Value.ToString() : string.Empty));
                    table.AddCell(cell);
                }
            }

            if (tableSection.Totals is not null)
            {
                table.StartNewRow();

                foreach (var totalCell in tableSection.Totals.Cells!) 
                { 
                    var cell = new Cell();
                    if (totalCell is not null)
                    {
                        var paragraph = new Paragraph(totalCell.Value is not null ? totalCell.Value.ToString() : string.Empty);
                        paragraph = paragraph.SetFontSize(12f);
                        
                        if (totalCell.IsBold)
                        {
                            paragraph = paragraph.SetBold();
                        }

                        switch (totalCell.Align)
                        {
                            case TableCellAlign.Center:
                                paragraph = paragraph.SetTextAlignment(TextAlignment.CENTER);
                                break;
                            case TableCellAlign.Right:
                                paragraph = paragraph.SetTextAlignment(TextAlignment.RIGHT);
                                break;
                        }
  
                        cell.Add(paragraph);
                    }

                    table.AddCell(cell);
                }
            }

            document.Add(table);
        }
    }
}