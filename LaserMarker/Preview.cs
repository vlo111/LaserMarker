
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

        public Preview(Bitmap image)
        {
            InitializeComponent();

            // Get the second monitor screen
            Screen screen = ScreenSize.GetSecondaryScreen();

            if (screen != null)
            {
                // Important !
                this.StartPosition = FormStartPosition.Manual;

                // set the location to the top left of the second screen
                this.Location = screen.WorkingArea.Location;

                // set it fullscreen
                this.Size = new Size(screen.WorkingArea.Width, screen.WorkingArea.Height);
            }
            
            this.pictureEdit1.Image = image.Scale();
        }

        private void RunBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ReopositoryEzdFile.Mark();
        }

        private void RunBtn_Click(object sender, EventArgs e)
        {
            var btn = (SimpleButton)sender;

            if (btn.Name == "Гравировать")
            {
                if (XtraMessageBox.Show("Вы действительно хотите гравировать?", "Сообщения", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (!runBackgroundWorker.IsBusy)
                    {
                        runBackgroundWorker.RunWorkerAsync();
                        btn.Name = "Стоп";
                        btn.BackColor = Color.FromArgb(192, 0, 0);
                    }
                    else
                    {
                        XtraMessageBox.Show("Гравировка уже идет", "Information", MessageBoxButtons.OK);
                    }
                }
            }
            else
            {
                ReopositoryEzdFile.StopMark();
                btn.Name = "Гравировать";
                btn.BackColor = Color.FromArgb(0, 192, 192);
            }
        }

        public void ShowSearch()
        {
            // CustomFlyoutDialog.ShowForm(this, null, new SearchCompetitorPreview());

            //using (var searchPreviewPopup = new SearchCompetitorPreview())
            //{
                var searchPreviewPopup = new SearchCompetitorPreview();
                searchPreviewPopup.Show();
            //}
        }
    }
}
