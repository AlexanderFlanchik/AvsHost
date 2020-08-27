namespace Avs.StaticSiteHosting.Common
{
    public class PaginationParameters
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SortField { get; set; }
        public SortOrder SortOrder { get; set; }
    }

    public enum SortOrder
    {
        Asc,
        Desc,
        None
    }
}