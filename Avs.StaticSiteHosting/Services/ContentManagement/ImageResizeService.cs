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
            var imageFormat = _formatManager.FindFormatByMimeType(contentType);
            if (imageFormat == null)
            {
                throw new InvalidOperationException("Invalid image type.");
            }

            var buffer = new MemoryStream();
            await inputStream.CopyToAsync(buffer).ConfigureAwait(false);
            buffer.Position = 0;

            var currentImage = await Image.LoadWithFormatAsync(buffer).ConfigureAwait(false);
            if (currentImage.Image.Width > maxWidth)
            {
                // resize
                var aspect = (decimal)currentImage.Image.Width / currentImage.Image.Height;
                var newWidth = maxWidth;
                var newHeight = (int)Math.Round(newWidth / aspect, 0);
                currentImage.Image.Mutate(o => o.Resize(new ResizeOptions { Size = new Size(newWidth, newHeight) }));

                var ms = new MemoryStream();
                currentImage.Image.Save(ms, imageFormat);
                ms.Position = 0;
                buffer.Dispose();

                return ms;
            }

            buffer.Position = 0;

            return buffer;
        }
    }
}