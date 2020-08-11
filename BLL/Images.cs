namespace BLL
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

        public static Image Scale(this Image img)
        {
            int width;

            int height;

            int coefficientW;

            // size second screen
            var screenWidth = ScreenSize.GetSecondaryScreen().Bounds.Width;

            var screenHeight = ScreenSize.GetSecondaryScreen().Bounds.Height;

            var imgNewHeight = img.Height;

            if (img.Width < screenWidth)
            {
                coefficientW = (int)(img.Width / (double)screenWidth * 100);

                width = img.Width + (screenWidth * coefficientW / 100);

                height = imgNewHeight + (screenWidth * coefficientW / 100);
            }
            else
            {
                coefficientW = (int)(img.Width / (double)screenWidth * 100);

                width = img.Width - (screenWidth * coefficientW / 100);

                height = imgNewHeight - (screenHeight * coefficientW / 100);
            }

            if (height > screenHeight)
            {
                var reduce = (int)((double)screenHeight / (double)height * 100d);

                width = width * reduce / 100;
                height = height * reduce / 100;
            }

            Bitmap bmp = new Bitmap(img, width, height);

            Graphics graphics = Graphics.FromImage(bmp);

            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            return bmp;
        }
    }
}
