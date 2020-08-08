using System.Windows.Forms;

namespace BLL
{
    public class ScreenSize
    {
        public static Screen GetSecondaryScreen()
        {
            if (Screen.AllScreens.Length == 1)
            {
                return null;
            }

            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.Primary == false)
                {
                    return screen;
                }
            }

            return null;
        }

        public static int PrimaryWidth()
        {
            int width = 0;

            foreach (var screen in Screen.AllScreens)
            {
                if (screen.Primary == true)
                {
                    width = screen.Bounds.Width;
                    break;
                }
            }

            return width;
        }

        public static int PrimaryHeight()
        {
            int height = 0;

            foreach (var screen in Screen.AllScreens)
            {
                if (screen.Primary == true)
                {
                    height = screen.Bounds.Height;
                    break;
                }
            }

            return height;
        }
    }

}
