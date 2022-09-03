using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.ContentManagement
{
    public interface IPagePreviewService
    {
        Task<string> GetPagePreviewAsync(string contentId);
    }

    public class PagePreviewService : IPagePreviewService
    {
        private readonly IContentManager _contentManager;

        public PagePreviewService(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async Task<string> GetPagePreviewAsync(string contentId)
        {
            var (_, _, contentStream) = await _contentManager.GetContentFileAsync(contentId);
            using (contentStream)
            using (var streamReader = new StreamReader(contentStream))
            {
                var content = await streamReader.ReadToEndAsync();
                if (string.IsNullOrEmpty(content))
                {
                    return null;
                }

                var bodyIndex = content.IndexOf("</body>");
                var script = @"<script>for (let a of document.body.getElementsByTagName('a')) { a.href='#'; }</script>";
                var part1 = content.Substring(0, bodyIndex);
                var part2 = content.Substring(bodyIndex);

                var sb = new StringBuilder(part1);
                sb.AppendLine();
                sb.Append(script);
                sb.AppendLine();
                sb.Append(part2);

                return sb.ToString();
            }
        }
    }
}