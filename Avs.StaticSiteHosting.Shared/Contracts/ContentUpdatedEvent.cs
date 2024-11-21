namespace Avs.StaticSiteHosting.Shared.Contracts
{
    public class ContentUpdatedEvent
    {
        public SiteContentInfo SiteContent { get; set; } = default!;
    }
}