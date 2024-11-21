using System;

namespace Avs.StaticSiteHosting.Web.Common
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string errorMessage) : base(errorMessage) { }
    }
}