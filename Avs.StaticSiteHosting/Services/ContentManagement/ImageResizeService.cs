using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using System.IO;

namespace Avs.StaticSiteHosting.Web.Services.ContentManagement
{
    public class ImageResizeService
    {
        private readonly ImageFormatManager _formatManager;

        public ImageResizeService()
        {
            _formatManager = new ImageFormatManager();
            _formatManager.AddImageFormat(PngFormat.Instance);
            _formatManager.AddImageFormat(JpegFormat.Instance);
            _formatManager.AddImageFormat(BmpFormat.Instance);
            _formatManager.AddImageFormat(GifFormat.Instance);
        }

        public async Task<Stream> GetResizedImageStreamAsync(Stream inputStream, string contentType, int maxWidth)
        {
            if (! _formatManager.TryFindFormatByMimeType(contentType, out var imageFormat))
            {
                throw new InvalidOperationException("Invalid image type.");
            }

            var buffer = new MemoryStream();
            await inputStream.CopyToAsync(buffer).ConfigureAwait(false);
            buffer.Position = 0;

            var currentImage = Image.Load(buffer);
            if (currentImage.Width > maxWidth)
            {
                // resize
                var aspect = (decimal)currentImage.Width / currentImage.Height;
                var newWidth = maxWidth;
                var newHeight = (int)Math.Round(newWidth / aspect, 0);
                currentImage.Mutate(o => o.Resize(new ResizeOptions { Size = new Size(newWidth, newHeight) }));

                var ms = new MemoryStream();
                await currentImage.SaveAsync(ms, imageFormat).ConfigureAwait(false);
                
                ms.Position = 0;
                buffer.Dispose();

                return ms;
            }

            buffer.Position = 0;

            return buffer;
        }
    }
}