
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
        private float _bgZoom;
        private float _fgZoom;
        private float _bgImgx;
        private float _bgImgy;
        private float _fgImgx;
        private float _fgImgy;

        public Preview()
        {
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

            this.foregroundPictureBox.Paint += this.ForegroundImageBox_Paint;

            this.backgroundPictureBox.Paint += this.BackgroundImageBox_Paint;

            this.CreateBgPictureBoxImage();

            this.CreateEzdPictureBoxImage();
        }

        public void UpdateImage(Bitmap image, Size primaryPanel, float bgZoom, float fgZoom, float bgx,float bgy, float ezdx, float ezdy)
        {

            var zoomC = this.panel1.Width / primaryPanel.Width;

            var zoom1 = bgZoom * zoomC;

            var zoom2 = fgZoom * zoomC;

            var bgWidth = 

            this._fgZoom = fgZoom;
            this._bgZoom = bgZoom;

            this._bgImgx = bgx;
            this._bgImgy = bgy;

            this._fgImgx = ezdx;
            this._fgImgy= ezdy;

            //this.backgroundPictureBox.Image = CurrentData.BgImage; //  image.Scale();

            //this.foregroundPictureBox.Image = CurrentData.EzdImage;
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

        #region Create image for PictureBox

        private void CreateEzdPictureBoxImage()
        {
            if (CurrentData.EzdImage == null)
            {
                CurrentData.EzdImage = new Bitmap(10, 10);
            }

            var fg = this.CreateGraphics();

            // Fit width
            this._fgZoom = (this.foregroundPictureBox.Width / (float)CurrentData.EzdImage.Width)
                           * (CurrentData.EzdImage.HorizontalResolution / fg.DpiX);

            this.foregroundPictureBox.Refresh();
        }

        private void CreateBgPictureBoxImage()
        {
            if (CurrentData.BgImage == null)
            {
                CurrentData.BgImage = new Bitmap(10, 10);
            }

            var g = this.CreateGraphics();

            // Fit width
            this._bgZoom = (this.backgroundPictureBox.Width / (float)CurrentData.BgImage.Width)
                           * (CurrentData.BgImage.HorizontalResolution / g.DpiX);

            this.backgroundPictureBox.Refresh();
        }

        #endregion Create/Delete image from PictureBox

        private void ForegroundImageBox_Paint(object sender, PaintEventArgs e)
        {
            if (CurrentData.EzdImage == null)
            {
                return;
            }

            e.Graphics.InterpolationMode = InterpolationMode.Low;
            e.Graphics.ScaleTransform(this._fgZoom, this._fgZoom);
            e.Graphics.DrawImage(CurrentData.EzdImage, this._fgImgx, this._fgImgy);
        }

        private void BackgroundImageBox_Paint(object sender, PaintEventArgs e)
        {
            if (CurrentData.BgImage == null)
            {
                return;
            }

            e.Graphics.InterpolationMode = InterpolationMode.Low;
            e.Graphics.ScaleTransform(this._bgZoom, this._bgZoom);
            e.Graphics.DrawImage(CurrentData.BgImage, this._bgImgx, this._bgImgy);
        }

    }
}
