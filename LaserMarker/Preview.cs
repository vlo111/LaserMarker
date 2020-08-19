
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
        private SearchCompetitorPreview searchPreviewPopup;

        private float _bgWidth;
        private float _bgHeight;
        private float _ezdWidth;
        private float _ezdHeight;
        private float bgX;
        private float bgY;

        private float BgWidth;
        private float BgHeight;

        private float ezdX;
        private float ezdY;

        public bool showEzd = false;

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

        public Preview(Size primaryPanelSize, float bgZoom, float fgZoom,
            float bgx, float bgy, float ezdx, float ezdy,
            float bgHeight, float bgWidth, float ezdHeight, float ezdWidth)
        {
            InitializeComponent();

            this._bgWidth = bgWidth;

            this._bgHeight = bgHeight;

            this._ezdWidth = ezdWidth;

            this._ezdHeight = ezdHeight;

            this.fg_zoom = fgZoom;

            this.zoom = bgZoom;

            this.imgx = bgx;

            this.imgy = bgy;

            this.fg_imgx = ezdx;

            this.fg_imgy = ezdy;

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

            this.foregroundPictureBox.Parent = this.backgroundPictureBox;

            ConfigurationPreview(primaryPanelSize);

            backgroundPictureBox.Paint += new PaintEventHandler(imageBox_Paint);

            foregroundPictureBox.Paint += new PaintEventHandler(EzdImageBox_Paint);
        }

        private void ConfigurationPreview(Size primaryPanelSize)
        {
            //  if (this.Height != (float)primaryPanelSize.Height)
            {
                // this.Width = 1920;
                // this.Height = 1080;
                float zoomC = (this.Height / (float)primaryPanelSize.Height);
                this.zoom = this.zoom * zoomC;
                this.fg_zoom = this.fg_zoom * zoomC;


                int fx = (primaryPanelSize.Width / 2) - (int)fg_imgx;
                int fy = (primaryPanelSize.Height / 2) - (int)fg_imgy;
                int bx = (primaryPanelSize.Width / 2) - (int)imgx;
                int by = (primaryPanelSize.Height / 2) - (int)imgy;

                this.imgx += (float)(bx - (float)(bx * zoomC)) + ((this.Width - primaryPanelSize.Width) / 2);
                this.fg_imgx += (float)(fx - (float)(fx * zoomC)) + ((this.Width - primaryPanelSize.Width) / 2);
                this.imgy += (float)(by - (float)(by * zoomC)) + ((this.Height - primaryPanelSize.Height) / 2);
                this.fg_imgy += (float)(fy - (float)(fy * zoomC)) + ((this.Height - primaryPanelSize.Height) / 2);

            }

        }
        public void UpdateImage(Size primaryPanelSize, float bgZoom, float fgZoom,
            float bgx, float bgy, float ezdx, float ezdy,
            float bgHeight, float bgWidth, float ezdHeight, float ezdWidth)
        {
            this._bgWidth = bgWidth;

            this._bgHeight = bgHeight;

            this._ezdWidth = ezdWidth;

            this._ezdHeight = ezdHeight;

            this.fg_zoom = fgZoom;

            this.zoom = bgZoom;

            this.imgx = bgx;

            this.imgy = bgy;

            this.fg_imgx = ezdx;
            this.fg_imgy = ezdy;

            ConfigurationPreview(primaryPanelSize);

            this.backgroundPictureBox.Refresh();

            this.foregroundPictureBox.Refresh();
        }

        public void ShowSearch(int height)
        {
            searchPreviewPopup = new SearchCompetitorPreview(height);

            searchPreviewPopup.Show();
        }
        public void CloseSearch()
        {
            searchPreviewPopup.Close();
        }

        private void imageBox_Paint(object sender, PaintEventArgs e)
        {
            float N2BgWidth = (_bgWidth * zoom);
            float N2BgHeight = ((_bgHeight / _bgWidth) * N2BgWidth);

            if (CurrentData.BgImage != null)
            {
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                e.Graphics.DrawImage(CurrentData.BgImage, (int)imgx, (int)imgy, (int)N2BgWidth, (int)N2BgHeight);
            }
        }

        private void EzdImageBox_Paint(object sender, PaintEventArgs e)
        {
            float N2ezdWidth = (_ezdWidth * fg_zoom);
            float N2ezdHeight = ((_ezdHeight / _ezdWidth) * N2ezdWidth);

            if (CurrentData.EzdImage != null)
            {
                if (showEzd)
                {
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    e.Graphics.DrawImage(CurrentData.EzdImage, (int)fg_imgx, (int)fg_imgy, (int)N2ezdWidth, (int)N2ezdHeight);
                }
            }
        }

        public void RefreshEzd()
        {
            this.foregroundPictureBox.Refresh();
        }
    }
}