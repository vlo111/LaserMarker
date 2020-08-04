namespace LaserMarker.State
{
    using System.Drawing;
    using System.Windows.Forms;

    public class CurrentData
    {
        public static Preview Preview { get; set; }

        public static Image EzdImage { get; set; }

        public static string EzdName { get; set; }

        public static string BgName { get; set; }

        public static string FullImageName { get; set; }

        public static Image FullImage { get; set; }

        public static Image BgImage { get; set; }

        public static PictureBox EzdPictureBox { get; set; }

        public static void Clear()
        {
            EzdImage = null;
            EzdName = null;
            BgImage = null;
            BgName = null;
            FullImageName = null;
        }
    }
}