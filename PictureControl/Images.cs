namespace PictureControl
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows.Forms;

    public static class Images
    {
        public static Bitmap MakeImageTransparent(this Image image)
        {
            var bitmap = (Bitmap)image;

            bitmap.MakeTransparent(Color.White);

            return bitmap;
        }

        public static Bitmap ToImage(this Panel control)
        {
            int width = control.Size.Width;
            int height = control.Size.Height;

            var bmp = new Bitmap(width, height);
            control.DrawToBitmap(bmp, new Rectangle(0, 0, width, height));

            return bmp;
        }

        public static byte[] ToBytes(this Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, ImageFormat.Bmp);
                return ms.ToArray();
            }
        }

        public static Image ToImage(this byte[] imageBytes)
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                return Bitmap.FromStream(ms);
            }
        }
    }
}
