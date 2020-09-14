using System;

namespace Avs.StaticSiteHosting
{
    /// <summary>
    /// General exception which can be thrown by StaticSiteMiddleware.
    /// </summary>
    public class StaticSiteProcessingException : Exception
    {
        public string ErrorTitle { get; set; }
        public int HttpStatusCode { get; set; }
        public StaticSiteProcessingException(string message) : base(message) {}

        public StaticSiteProcessingException(int httpStatusCode, string message) : this(message)
        {
            HttpStatusCode = httpStatusCode;
        }

        public StaticSiteProcessingException(int httpStatusCode, string errorTitle, string message) 
            : this(httpStatusCode, message)
        {
            ErrorTitle = errorTitle;
        }
    }
}