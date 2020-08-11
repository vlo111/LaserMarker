
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

        public Preview()
        {
            InitializeComponent();

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
        }

        public void UpdateImage(Bitmap image)
        {
            this.pictureEdit1.Image = image.Scale();
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
    }
}
