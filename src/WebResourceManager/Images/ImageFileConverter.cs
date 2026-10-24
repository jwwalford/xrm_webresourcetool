using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using WebResourceManager.Models;

namespace WebResourceManager.Images
{
    /// <summary>
    /// Resizes and re-encodes uploaded images for the upload tool.
    /// </summary>
    internal static class ImageFileConverter
    {
        public const string OpenFilter = "Images (*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.ico;*.svg)|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.ico;*.svg|All files (*.*)|*.*";

        /// <summary>
        /// The web resource type for a file extension, or null when Dataverse has
        /// no image type for it (BMP), so the image has to be converted.
        /// </summary>
        public static WebResourceType? TypeForExtension(string extension)
        {
            switch ((extension ?? string.Empty).ToLowerInvariant())
            {
                case ".png": return WebResourceType.Png;
                case ".jpg":
                case ".jpeg": return WebResourceType.Jpg;
                case ".gif": return WebResourceType.Gif;
                case ".ico": return WebResourceType.Ico;
                case ".svg": return WebResourceType.Svg;
                default: return null;
            }
        }

        public static string ExtensionFor(WebResourceType type)
        {
            switch (type)
            {
                case WebResourceType.Jpg: return ".jpg";
                case WebResourceType.Gif: return ".gif";
                case WebResourceType.Ico: return ".ico";
                case WebResourceType.Svg: return ".svg";
                default: return ".png";
            }
        }

        public static string MimeType(WebResourceType type)
        {
            switch (type)
            {
                case WebResourceType.Jpg: return "image/jpeg";
                case WebResourceType.Gif: return "image/gif";
                case WebResourceType.Ico: return "image/x-icon";
                case WebResourceType.Svg: return "image/svg+xml";
                default: return "image/png";
            }
        }

        public static Bitmap Resize(Image source, int width, int height)
        {
            var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            result.SetResolution(96, 96);
            using (var g = Graphics.FromImage(result))
            using (var attributes = new ImageAttributes())
            {
                g.Clear(Color.Transparent);
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;

                // Stops the bicubic filter pulling a faint border in from outside the image
                attributes.SetWrapMode(WrapMode.TileFlipXY);
                g.DrawImage(source, new Rectangle(0, 0, width, height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
            }
            return result;
        }

        /// <summary>
        /// Encodes a bitmap as PNG, JPG or GIF. JPG has no transparency, so
        /// transparent areas are filled with the background colour first.
        /// </summary>
        public static byte[] Encode(Bitmap image, WebResourceType type, int jpgQuality, Color jpgBackground)
        {
            using (var stream = new MemoryStream())
            {
                switch (type)
                {
                    case WebResourceType.Jpg:
                        using (var flat = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb))
                        {
                            flat.SetResolution(96, 96);
                            using (var g = Graphics.FromImage(flat))
                            {
                                g.Clear(jpgBackground);
                                g.DrawImage(image, 0, 0, image.Width, image.Height);
                            }

                            var codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                            using (var parameters = new EncoderParameters(1))
                            {
                                parameters.Param[0] = new EncoderParameter(Encoder.Quality, (long)Math.Max(1, Math.Min(100, jpgQuality)));
                                flat.Save(stream, codec, parameters);
                            }
                        }
                        break;
                    case WebResourceType.Gif:
                        image.Save(stream, ImageFormat.Gif);
                        break;
                    case WebResourceType.Ico:
                        return IconResizer.ToIco(new[] { image });
                    default:
                        image.Save(stream, ImageFormat.Png);
                        break;
                }

                return stream.ToArray();
            }
        }

        public static string FormatSize(long bytes)
        {
            if (bytes < 1024)
            {
                return bytes + " bytes";
            }

            return bytes < 1024 * 1024
                ? (bytes / 1024.0).ToString("0.#") + " KB"
                : (bytes / (1024.0 * 1024.0)).ToString("0.##") + " MB";
        }
    }
}
