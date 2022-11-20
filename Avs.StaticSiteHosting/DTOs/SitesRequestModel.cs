namespace Avs.StaticSiteHosting.Web.DTOs;

public record SiteRequestModel(int Page, int PageSize, string SortOrder, string SortField, string[] TagIds);