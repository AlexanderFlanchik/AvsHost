using Avs.StaticSiteHosting.Reports.Models;
using Avs.StaticSiteHosting.Reports.Services;

namespace Avs.StaticSiteHosting.Reports.Tests
{
    public class ReportRendererTests
    {
        [Fact]
        public async Task RenderPdfReport_IsSuccessful()
        {
            // Arrange
            var report = CreateReport();
            var renderer = new PdfReportRenderer();

            // Act
            var reportContent = await renderer.RenderAsync(report);

            // Assert
            Assert.NotNull(reportContent);
            Assert.NotEmpty(reportContent);
        }

        [Fact]
        public async Task RenderXlsxReport_IsSuccessful()
        {
            // Arrange
            var report = CreateReport();
            var renderer = new XlsxReportRenderer();

            // Act
            var reportContent = await renderer.RenderAsync(report);
            
            // Assert
            Assert.NotNull(reportContent);
            Assert.NotEmpty(reportContent);
        }

        private Report CreateReport()
        {
            var report = new Report();
            report.Title = "Test Report";
            report.Sections.Add(new TextSection() { Content = "I am a simple text section with some text!" });
            
            var rows = new List<TableRow>();
            for (var i = 1; i <= 100; i++)
            {
                var row = new TableRow();
                row.Cells = new object[] { i, i + 1, i + 2 }; 
                rows.Add(row);
            }

            report.Sections.Add(
                new TableSection(
                    new[] { "First Column", "Second Column", "Third Column" },
                    rows.ToArray()
               ));
            
            return report;
        }
    }
}