
using System.Drawing.Drawing2D;
using LaserMarker.State;

namespace LaserMarker
{
    using BLL;

    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    using DevExpress.XtraEditors;

    using EzdDataControl;
    using global::LaserMarker.UserControls;

    public partial class Preview : Form
    {
        private float bgX;
        private float bgY;

        private float BgWidth;
        private float BgHeight;
        
        private float ezdX;
        private float ezdY;

        private float ezdWidth;
        private float ezdHeight;

        #region Fields BG

        Image imgBack;

        Point bg_mouseDown;
        int startx = 0; // offset of image when mouse was pressed
        int starty = 0;
        float imgx = 0; // current offset of image
        float imgy = 0;

        bool mousepressed = false;  // true as long as left mousebutton is pressed
        float zoom = 1;

        #endregion

        #region Fields FG

        Image imgEzd;
        Point fg_mouseDown;
        int fg_startx = 0; // offset of image when mouse was pressed
        int fg_starty = 0;
        float fg_imgx = 0; // current offset of image
        float fg_imgy = 0;

        bool fg_mousepressed = false;  // true as long as left mousebutton is pressed
        float fg_zoom = 1;

        #endregion

        public Preview(float bgZoom, float fgZoom, float bgx, float bgy, float ezdx, float ezdy)
        {
            this.fg_zoom = fgZoom;
            this.zoom = bgZoom;

            this.imgx = bgx;
            this.imgy = bgy;

            this.fg_imgx = ezdx;
            this.fg_imgy = ezdy;

            InitializeComponent();

            this.foregroundPictureBox.Parent = this.backgroundPictureBox;

            // Get the second monitor screen
            Screen screen = ScreenSize.GetSecondaryScreen();

            if (screen != null)
            {
                // Important
                this.StartPosition = FormStartPosition.CenterScreen;

                // set the location to the top left of the second screen
                this.Location = screen.WorkingArea.Location;

                // set it fullscreen
                this.Size = new Size(screen.WorkingArea.Width, screen.WorkingArea.Height);
            }

            // Fit width
            bgX = CurrentData.BgImage.Width;
            bgY = CurrentData.BgImage.Height;
            zoom = (float)(backgroundPictureBox.Width / bgX);
            //Выставлем по центру
            imgy = (backgroundPictureBox.Height / 2) - ((bgY * zoom) / 2);

            backgroundPictureBox.Paint += new PaintEventHandler(imageBox_Paint);



            Graphics fg = this.CreateGraphics();

            ezdX = CurrentData.EzdImage.Width;
            ezdY = CurrentData.EzdImage.Height;

            fg_zoom = (float)(foregroundPictureBox.Height / ezdY);
            //Выставлем по центру
            fg_imgx = (foregroundPictureBox.Width / 2) - ((ezdX * fg_zoom) / 2);





            foregroundPictureBox.Paint += new PaintEventHandler(EzdImageBox_Paint);

            foregroundPictureBox.Parent = backgroundPictureBox;

        }

        public void UpdateImage(float bgZoom, float fgZoom, float bgx, float bgy, float ezdx, float ezdy)
        {
            this.fg_zoom = fgZoom;
            this.zoom = bgZoom;

            this.imgx = bgx;
            this.imgy = bgy;

            this.fg_imgx = ezdx;
            this.fg_imgy = ezdy;

            //this.backgroundPictureBox.Image = CurrentData.BgImage; //  image.Scale();

            //this.foregroundPictureBox.Image = CurrentData.EzdImage;
            this.backgroundPictureBox.Refresh();

            this.foregroundPictureBox.Refresh();
        }

        public void ShowSearch(int height)
        {
            //searchPreviewPopup = new SearchCompetitorPreview(height);

            //searchPreviewPopup.Show();
        }
        public void CloseSearch()
        {
            //searchPreviewPopup.Close();
        }

        private void imageBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            BgWidth = (bgX * zoom);
            BgHeight = ((bgY / bgX) * BgWidth);

            e.Graphics.DrawImage(CurrentData.BgImage, (int)imgx, (int)imgy, (int)BgWidth, (int)BgHeight);
        }

        private void EzdImageBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            ezdWidth = (ezdX * fg_zoom);
            ezdHeight = ((ezdY / ezdX) * ezdWidth);

            e.Graphics.DrawImage(CurrentData.EzdImage, (int)fg_imgx, (int)fg_imgy, (int)ezdWidth, (int)ezdHeight);
        }
    }
}