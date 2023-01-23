namespace Avs.StaticSiteHosting.Reports.Models
{
    /// <summary>
    /// Represents a general report with a title and sections.
    /// </summary>
    public class Report
    {
        /// <summary>
        /// Report title
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Report content as a set of report sections
        /// </summary>       
        public IList<ReportSection> Sections { get; } = new List<ReportSection>();
    }
}