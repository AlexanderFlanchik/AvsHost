namespace Avs.StaticSiteHosting.Shared.Contracts
{
    public class SiteError
    {
        public string? Id { get; set; }
        public string? SiteOwnerId { get; set; }
        public string Error { get; set; } = default!;
        public int Statuscode { get; set; }
    }
}