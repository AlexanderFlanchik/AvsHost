using Avs.StaticSiteHosting.Reports.Models;

namespace Avs.StaticSiteHosting.Reports.Services
{
    public abstract class ReportRendererBase<TReportDocument>
    {
        protected readonly IDictionary<Type, Action<ReportSection, TReportDocument>> _sectionRenderers =
           new Dictionary<Type, Action<ReportSection, TReportDocument>>();

        protected ReportRendererBase()
        {
            _sectionRenderers.Add(typeof(TextSection), RenderTextSection);
            _sectionRenderers.Add(typeof(TableSection), RenderTableSection);
        }

        protected abstract void RenderTextSection(ReportSection section, TReportDocument reportDocument);
        protected abstract void RenderTableSection(ReportSection section, TReportDocument reportDocument);
    }
}