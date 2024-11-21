using System;

namespace Avs.StaticSiteHosting.Web.Common
{
    public class ReportPreviewException : Exception
    {
        public ReportPreviewException(string errorMessage) 
            : base(errorMessage) 
        { }
    }
}