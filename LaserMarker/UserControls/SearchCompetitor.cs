using DevExpress.XtraBars.Docking2010.Customization;

namespace LaserMarker.UserControls
{
    using BLL;

    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using API;

    using DevExpress.XtraEditors;

    using EzdDataControl;
    using global::LaserMarker.State;

    using Newtonsoft.Json;

    using Telerik.WinControls.UI;

    public partial class SearchCompetitor : Form // : XtraUserControl
    {
        private CompetitorList _competitors;

        private TextEdit _bib_text;

        RadWaitingBar waitingBar;

        CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public SearchCompetitor(TextEdit bib)
        {
            _bib_text = bib;

            InitializeComponent();

            this.Width = ScreenSize.PrimaryWidth();

            this.Location = new Point(0, (ScreenSize.PrimaryHeight() - this.Height) / 2);

            waitingBar = new RadWaitingBar();
            waitingBar.AssociatedControl = this.layoutControl2;
            waitingBar.Size = new System.Drawing.Size(80, 80);
            waitingBar.WaitingStyle = Telerik.WinControls.Enumerations.WaitingBarStyles.LineRing;

            this.layoutControl2.Controls.Add(waitingBar);

            CurrentData.Preview.ShowSearch(this.Height);
        }

        private void SearchCompetitor_Load(object sender, EventArgs e)
        {
            this.listView1.Columns.AddRange(
                new ColumnHeader[]
                    {
                        new ColumnHeader() { Text = @"Bib", Width = this.listView1.Width / 100 * 20 },
                        new ColumnHeader() { Text = @"First Name", Width = this.listView1.Width / 100 * 30  },
                        new ColumnHeader() { Text = @"Last Name", Width = this.listView1.Width / 100 * 30  },
                        new ColumnHeader() { Text = @"Birth year", Width = this.listView1.Width / 100 * 20  }
                    });
        }

        private async void searchControl1_TextChanged(object sender, EventArgs e)
        {

            var search = this.searchControl.Text;

            if (string.IsNullOrEmpty(search))
            {
                return;
            }



            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
            }

            _tokenSource = new CancellationTokenSource();

            try
            {

                this.waitingBar.StartWaiting();

                await loadPrestatieGetCompetitorAsync(this.listView1, _tokenSource.Token, search);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task loadPrestatieGetCompetitorAsync(ListView list, CancellationToken token, string search)
        {
            await Task.Delay(500, token).ConfigureAwait(true);
            try
            {
                var task = await Queries.GetRequestAsync($@"http://openeventor.ru/event/{CurrentApiData.Token}/plugins/engraver/get?search={this.searchControl.Text}");

                this._competitors = JsonConvert.DeserializeObject<CompetitorList>(task);

                UpdateListView(search);

                this.waitingBar.StopWaiting();

                token.ThrowIfCancellationRequested();

            }
            catch (OperationCanceledException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UpdateListView(string search)
        {
            this.listView1.Items.Clear();

            var listItem = new List<ListViewItem>();

            if (this._competitors.CompetitorDatas == null || this._competitors.CompetitorDatas.Count <= 0)
            {
                return;
            }

            var competitors = this._competitors.CompetitorDatas.Where(
                p => p.LastName != null && p.LastName.ToLower().Contains(search.Trim().ToLower()));

            var searchedCompetitorList = competitors.ToList();

            SearchCompetitorPreview.UpdateLData(searchedCompetitorList, this.searchControl.Text);

            searchedCompetitorList.ForEach(
                   p =>
                   {
                       listItem.Add(new ListViewItem(new[] { p.Bib, p.FirstName, p.LastName, p.BirthYear }));
                   });

            this.listView1.Items.AddRange(listItem.ToArray());
        }

        private void KeyBtns_Click(object sender, EventArgs e)
        {
            var btn = (DevExpress.XtraEditors.SimpleButton)sender;

            if (btn.Text == "<")
            {
                if (searchControl.Text.Length <= 0)
                {
                    return;
                }

                searchControl.Text = searchControl.Text.Remove(searchControl.Text.Length - 1);
            }
            else
            {
                searchControl.Text = searchControl.Text + btn.Text;
            }
        }

        private void enterBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.searchControl.Text)
                || this._competitors == null
                || this._competitors.CompetitorDatas == null
                || this._competitors.CompetitorDatas.Count <= 0)
            {
                return;
            }

            if (this.listView1.SelectedItems.Count > 0)
            {
                var selectedCompotitor = this._competitors.CompetitorDatas.FirstOrDefault(p => p.Bib == this.listView1.SelectedItems[0].Text);

                this._bib_text.Text = selectedCompotitor.Bib;

                CurrentData.EzdImage = ReopositoryEzdFile.UpdateEzdApi(selectedCompotitor, CurrentData.EzdImage.Width, CurrentData.EzdImage.Height);
                
                CurrentData.EzdPictureBox.Refresh();
            }

            CloseSearch();

            CurrentData.Preview.UpdateImage(CurrentUIData.PanelImages.ToImage());
        }

        private void simpleButton12_Click(object sender, EventArgs e)
        {
            CloseSearch();
        }

        private void CloseSearch()
        {
            CurrentData.Preview.CloseSearch();

            this.Close();
        }
    }
}
