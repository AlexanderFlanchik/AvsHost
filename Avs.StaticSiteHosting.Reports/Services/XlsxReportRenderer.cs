using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Reports.Contracts;
using Avs.StaticSiteHosting.Reports.Models;

namespace Avs.StaticSiteHosting.Reports.Services
{
    public class XlsxReportRenderer : ReportRendererBase<ExcelWorksheet>, IReportRenderer
    {
        private int cellXindex = 1;
        private int cellYindex = 1;

        public ReportFormat Format => ReportFormat.XLSX;

        static XlsxReportRenderer()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }
        
        public async Task<byte[]> RenderAsync(Report report)
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Page 1");
            
            if (report.Title is not null)
            {
                var titleCell = ws.Cells[cellYindex++, cellXindex];
                titleCell.AutoFitColumns();
                titleCell.Style.Font.Bold = true;
                titleCell.Value = report.Title;
            }

            foreach (var section in report.Sections)
            {
                var renderer = _sectionRenderers[section.GetType()];
                if (renderer is null)
                {
                    continue;
                }

                renderer(section, ws);
            }

            var content = await package.GetAsByteArrayAsync();
            
            return content;
        }

        protected override void RenderTextSection(ReportSection section, ExcelWorksheet worksheet) 
        { 
            if (section is not TextSection textSection || string.IsNullOrEmpty(textSection.Content))
            {
                return;
            }

            worksheet.Cells[cellYindex++, cellXindex].Value = textSection.Content;
        }

        protected override void RenderTableSection(ReportSection section, ExcelWorksheet worksheet) 
        {
            if (section is not TableSection tableSection)
            {
                return;
            }
            
            var cx = cellXindex;
            var cy = ++cellYindex;
            foreach (var column in tableSection.Columns)
            {
                var headerCell = worksheet.Cells[cy, cx++];
                headerCell.Style.Font.Bold = true;
                headerCell.Value = column;
            }

            worksheet.Cells[cy, cellXindex, cy, cx].AutoFitColumns();

            foreach (var row in tableSection.Rows)
            {
                cy = ++cellYindex;
                cx = cellXindex;

                foreach (var rowCell in row.Cells)
                {
                    worksheet.Cells[cy, cx++].Value = rowCell is not null ? rowCell.ToString() : string.Empty;
                }
            }

            cellYindex += cy;
        }
    }
}