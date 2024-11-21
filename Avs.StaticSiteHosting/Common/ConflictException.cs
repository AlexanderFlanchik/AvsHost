using System;

namespace Avs.StaticSiteHosting.Web.Common
{
    public class ConflictException : Exception
    {
        public ConflictException(string errorMessage) : base(errorMessage) { }
    }
}