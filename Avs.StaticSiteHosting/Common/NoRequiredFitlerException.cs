using System;

namespace Avs.StaticSiteHosting.Web.Common;

/// <summary>
/// Represents an exception thrown in Reports logic when there are not some required filter set
/// </summary>
public class NoRequiredFitlerException : Exception
{
    /// <summary>
    /// Filter names
    /// </summary>
    public string[] FilterNames { get; init; }
    
    public NoRequiredFitlerException(string[] fiterNames)
    {
        FilterNames = fiterNames;
    }
}