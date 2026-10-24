using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace WebResourceManager.Images
{
    internal enum IconFitMode
    {
        /// <summary>Scale the whole image to fit, padding the short side with transparency.</summary>
        Fit,

        /// <summary>Fill the square, cropping the long side from the centre.</summary>
        Crop
    }

    /// <summary>
    /// Makes square icon bitmaps from a source image and encodes them as PNG or a
    /// multi-size ICO.
    /// </summary>
    internal static class IconResizer
    {
        public static readonly int[] StandardSizes = { 16, 24, 32, 48, 64, 80, 128 };

        public const string OpenFilter = "Images (*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.ico)|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.ico|All files (*.*)|*.*";

        /// <summary>
        /// Loads an image without keeping the file locked. For an .ico the largest
        /// image in the file is used.
        /// </summary>
        public static Bitmap Load(string path)
        {
            var bytes = File.ReadAllBytes(path);
            using (var stream = new MemoryStream(bytes))
            {
                if (path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                {
                    var png = LargestPngEntry(bytes);
                    if (png != null)
                    {
                        using (var pngStream = new MemoryStream(png))
                        {
                            return CopyOf(pngStream);
                        }
                    }

                    using (var icon = new Icon(stream, 256, 256))
                    {
                        return icon.ToBitmap();
                    }
                }

                return CopyOf(stream);
            }
        }

        private static Bitmap CopyOf(Stream stream)
        {
            using (var image = Image.FromStream(stream))
            {
                // Copy into a 32-bit bitmap so the stream can close and transparency is kept
                var copy = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
                copy.SetResolution(96, 96);
                using (var g = Graphics.FromImage(copy))
                {
                    g.DrawImage(image, 0, 0, image.Width, image.Height);
                }
                return copy;
            }
        }

        /// <summary>
        /// The largest image in an .ico if it is stored as a PNG, otherwise null
        /// (the Icon class handles the classic bitmap entries).
        /// </summary>
        private static byte[] LargestPngEntry(byte[] ico)
        {
            if (ico.Length < 6 || BitConverter.ToInt16(ico, 2) != 1)
            {
                return null;
            }

            var count = BitConverter.ToInt16(ico, 4);
            int bestSize = -1, bestOffset = 0, bestLength = 0;
            for (var n = 0; n < count; n++)
            {
                var entry = 6 + 16 * n;
                if (entry + 16 > ico.Length)
                {
                    return null;
                }

                var width = ico[entry] == 0 ? 256 : ico[entry];
                if (width > bestSize)
                {
                    bestSize = width;
                    bestLength = BitConverter.ToInt32(ico, entry + 8);
                    bestOffset = BitConverter.ToInt32(ico, entry + 12);
                }
            }

            var isPng = bestSize > 0 && bestOffset >= 0 && bestLength > 8 && bestOffset + bestLength <= ico.Length &&
                ico[bestOffset] == 0x89 && ico[bestOffset + 1] == (byte)'P' && ico[bestOffset + 2] == (byte)'N' && ico[bestOffset + 3] == (byte)'G';
            if (!isPng)
            {
                return null;
            }

            var png = new byte[bestLength];
            Buffer.BlockCopy(ico, bestOffset, png, 0, bestLength);
            return png;
        }

        public static Bitmap Resize(Image source, int size, IconFitMode mode)
        {
            var result = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            result.SetResolution(96, 96);

            RectangleF src;
            RectangleF dest;
            if (mode == IconFitMode.Crop)
            {
                var side = Math.Min(source.Width, source.Height);
                src = new RectangleF((source.Width - side) / 2f, (source.Height - side) / 2f, side, side);
                dest = new RectangleF(0, 0, size, size);
            }
            else
            {
                var scale = Math.Min((float)size / source.Width, (float)size / source.Height);
                var w = source.Width * scale;
                var h = source.Height * scale;
                src = new RectangleF(0, 0, source.Width, source.Height);
                dest = new RectangleF((size - w) / 2f, (size - h) / 2f, w, h);
            }

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
                g.DrawImage(source, ToPoints(dest), src, GraphicsUnit.Pixel, attributes);
            }

            return result;
        }

        public static byte[] ToPng(Image image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Builds an .ico holding each image as a PNG entry (supported by Windows
        /// Vista and later and by every current browser). Images must be 256px or smaller.
        /// </summary>
        public static byte[] ToIco(IEnumerable<Bitmap> images)
        {
            var entries = images.OrderBy(i => i.Width).Select(i => new { i.Width, i.Height, Png = ToPng(i) }).ToList();

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((short)0);              // Reserved
                writer.Write((short)1);              // Type: icon
                writer.Write((short)entries.Count);

                var offset = 6 + 16 * entries.Count;
                foreach (var entry in entries)
                {
                    writer.Write((byte)(entry.Width >= 256 ? 0 : entry.Width));
                    writer.Write((byte)(entry.Height >= 256 ? 0 : entry.Height));
                    writer.Write((byte)0);           // No palette
                    writer.Write((byte)0);           // Reserved
                    writer.Write((short)1);          // Colour planes
                    writer.Write((short)32);         // Bits per pixel
                    writer.Write(entry.Png.Length);
                    writer.Write(offset);
                    offset += entry.Png.Length;
                }

                foreach (var entry in entries)
                {
                    writer.Write(entry.Png);
                }

                writer.Flush();
                return stream.ToArray();
            }
        }

        private static PointF[] ToPoints(RectangleF rect)
        {
            // DrawImage with ImageAttributes takes the destination as three corner points
            return new[] { new PointF(rect.Left, rect.Top), new PointF(rect.Right, rect.Top), new PointF(rect.Left, rect.Bottom) };
        }
    }
}
