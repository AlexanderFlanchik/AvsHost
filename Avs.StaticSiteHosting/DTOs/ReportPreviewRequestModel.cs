using Avs.StaticSiteHosting.Reports.Common;
using System.Collections.Generic;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class ReportPreviewRequestModel
    {
        public IDictionary<string, object> Filter { get; set; }
        public ReportType ReportType { get; set; }
    }
}