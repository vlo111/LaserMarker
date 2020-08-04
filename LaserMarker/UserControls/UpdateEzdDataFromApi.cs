namespace LaserMarker.UserControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Data;
    using System.Text;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using DevExpress.XtraEditors;
    using DevExpress.XtraLayout;
    using Newtonsoft.Json;
    using API;
    using EzdDataControl;
    using System.IO;
    using System.Threading;

    using global::LaserMarker.State;

    public partial class UpdateEzdDataFromApi : DevExpress.XtraEditors.XtraUserControl
    {
        private Competitor _competitor;

        private LaserMarker _form;

        bool doWorkRun = false;
        bool doWorkTest = false;

        public UpdateEzdDataFromApi(LaserMarker form)
        {
            try
            {
                EzdDataL.Load data = new EzdDataL.Load();

                if (!data.Go())
                {
                    //if (new CustomMessage().ShowDialog() >= 0)
                    //{
                    //    Application.Exit();
                    //}

                    Application.Exit();
                }
            }
            catch (Exception)
            {
                Application.Exit();
            }

            _form = form;

            this.MaximumSize = CurrentUIData.RightPanelSize;
            this.MinimumSize = CurrentUIData.RightPanelSize;

            InitializeComponent();

            this.marginUpEmptySpace.MinSize = new Size(0, CurrentUIData.RightPanelSize.Height / 6);

            this.flyoutPanel1.OwnerControl = CurrentUIData.RightLayoutControl;

            this.flyoutPanel1.MaximumSize = CurrentUIData.RightPanelSize;
            this.flyoutPanel1.MinimumSize = CurrentUIData.RightPanelSize;

            this.flyoutPanel1.ShowPopup();
        }

        private void HidePopupBtn_Click(object sender, EventArgs e)
        {
            this.flyoutPanel1.HidePopup();
        }

        private void KeyBtns_Click(object sender, EventArgs e)
        {
            var btn = (DevExpress.XtraEditors.SimpleButton)sender;

            if (btn.Text == "C")
            {
                searchTextEdit.Text = searchTextEdit.Text.Remove(searchTextEdit.Text.Length - 1);
            }
            else
            {
                searchTextEdit.Text = searchTextEdit.Text + btn.Text;
            }
        }

        private async void OkSimpleButton_Click(object sender, EventArgs e)
        {
            try
            {
                var task = await Queries.GetRequestAsync(
                  $@"http://openeventor.ru/api/event/{CurrentApiData.Token}/get_competitor/bib/{this.searchTextEdit.Text}");

                this._competitor = JsonConvert.DeserializeObject<Competitor>(task);

                ReopositoryEzdFile.LoadImage(CurrentData.EzdName);

                CurrentData.EzdImage = ReopositoryEzdFile.UpdateEzdApi(_competitor.CompetitorData, CurrentData.EzdImage.Width, CurrentData.EzdImage.Height);
                CurrentData.EzdPictureBox.Refresh();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Данные с этим номером не найдены", "Information", MessageBoxButtons.OK);
            }
        }

        private void SearchSimpleButton_Click(object sender, EventArgs e)
        {
            CustomFlyoutDialog.ShowForm(_form, null, new SearchCompetitor(this.searchTextEdit));

            // new SearchCompetitor(this.searchTextEdit).Show();
        }

        private void editEzdBtn_Click(object sender, EventArgs e)
        {
            var ezdObjects = EzdDataControl.ReopositoryEzdFile.GetEzdData();

            new UpdateEzdData(ezdObjects);
        }

        private void RunBtn_Click(object sender, EventArgs e)
        {
            var btn = (SimpleButton)sender;

            if (doWorkTest)
            {
                doWorkTest = false;
            }

            if (btn.Text == "RUN")
            {
                if (XtraMessageBox.Show("Вы действительно хотите гравировать?", "Сообщения", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    doWorkRun = true;

                    if (!runBackgroundWorker.IsBusy)
                    {
                        runBackgroundWorker.RunWorkerAsync();
                        btn.Text = "STOP";
                        btn.Appearance.BackColor = Color.FromArgb(192, 0, 0);

                        this.testBtn.Enabled = false;
                        this.testBtn.Cursor = Cursors.No;
                    }
                    else
                    {
                        XtraMessageBox.Show("Гравировка уже идет", "Information", MessageBoxButtons.OK);
                    }
                }
            }
            else
            {
                doWorkRun = false;

                ReopositoryEzdFile.StopMark();
                btn.Text = "RUN";
                btn.Appearance.BackColor = Color.FromArgb(0, 192, 192);

                this.testBtn.Enabled = true;
                this.testBtn.Cursor = Cursors.No;
            }
        }

        private void RunBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (doWorkRun)
            {
                ReopositoryEzdFile.Mark();
            }

            if (!ReopositoryEzdFile.IsMarking())
            {
                this.runBtn.Text = "RUN";
                this.runBtn.Appearance.BackColor = Color.FromArgb(0, 192, 192);
            }
        }

        private void TestBtn_Click(object sender, EventArgs e)
        {
            var btn = (SimpleButton)sender;
            try
            {
                if (doWorkRun)
                {
                    if (XtraMessageBox.Show("Вы действительно хотите простановить гравировку?", "Сообщения", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        doWorkRun = false;
                    }
                    else
                    {
                        return;
                    }
                }

                if (btn.Text == "TEST")
                {
                    doWorkTest = true;
                    if (btn.Tag.ToString() == "redMark")
                    {
                        if (!testRedMarkBackgroundWorker.IsBusy)
                        {
                            testRedMarkBackgroundWorker.RunWorkerAsync();
                        }

                        btn.Tag = "redMarkContour";
                    }
                    else if (btn.Tag.ToString() == "redMarkContour")
                    {
                        if (!testRedMarkContourBackgroundWorker.IsBusy)
                        {
                            testRedMarkContourBackgroundWorker.RunWorkerAsync();
                        }

                        btn.Tag = "redMark";
                    }

                    btn.BackColor = btn.BackColor = Color.FromArgb(192, 0, 0);
                    btn.Text = "STOP TEST";
                }
                else if (btn.Text == "STOP TEST")
                {
                    doWorkTest = false;
                    if (btn.Tag.ToString() == "redMark")
                    {
                        testRedMarkBackgroundWorker.WorkerSupportsCancellation = true;

                        btn.Tag = "redMarkContour";
                    }
                    else if (btn.Tag.ToString() == "redMarkContour")
                    {
                        testRedMarkContourBackgroundWorker.WorkerSupportsCancellation = true;

                        btn.Tag = "redMark";
                    }
                    btn.BackColor = Color.FromArgb(0, 192, 192);
                    btn.Text = "TEST";
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Information", MessageBoxButtons.OK);
            }
        }

        private void TestRedMarkBackgroundWorkerr_DoWork(object sender, DoWorkEventArgs e)
        {
            while (doWorkTest)
            {
                Thread.Sleep(100);
                ReopositoryEzdFile.RedMark();
            }
        }

        private void TestRedMarkContourBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (doWorkTest)
            {
                Thread.Sleep(100);
                ReopositoryEzdFile.RedMarkContour();
            }
        }
    }

}
