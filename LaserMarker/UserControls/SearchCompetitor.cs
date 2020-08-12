using DevExpress.Utils.Extensions;
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
        private Competitors _competitors;

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

            CurrentData.Preview?.ShowSearch(this.Height);
        }

        private async void searchControl1_TextChanged(object sender, EventArgs e)
        {
            var search = this.searchControl.Text;

            if (!string.IsNullOrEmpty(this.searchControl.Text))
            {
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
        }

        private async Task loadPrestatieGetCompetitorAsync(ListView list, CancellationToken token, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                await Task.Delay(500, token).ConfigureAwait(true);
                try
                {
                    var task = await Queries.GetRequestAsync(
                        $@"http://openeventor.ru/event/{CurrentApiData.Token}/plugins/engraver/get?search={this.searchControl.Text}");

                    if (task == null)
                    {
                        return;
                    }
                    this._competitors = JsonConvert.DeserializeObject<Competitors>(task);

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
        }

        private void UpdateListView(string search)
        {
            this.listView1.Items.Clear();

            var listItem = new List<ListViewItem>();

            if (this._competitors.CompetitorList == null || this._competitors.CompetitorList.Count <= 0)
            {
                return;
            }

            var competitors = this._competitors.CompetitorList.Where(
                list => list.Values
                    .Any(l => l
                        .ToLower()
                        .Contains(search.Trim()
                            .ToLower())));

            var searchedCompetitorList = competitors.ToList();

            // Columns
            if (this.listView1.Columns.Count <= 0)
            {
                ICollection<ColumnHeader> columns = new List<ColumnHeader>();

                searchedCompetitorList.FirstOrDefault()
                    ?.Keys.ForEach(key =>
                    {
                        columns.Add(new ColumnHeader() {Text = key, Width = this.listView1.Width / 100 * 20});
                    });

                this.listView1.Columns.AddRange(columns.ToArray());
            }

            //Items
            searchedCompetitorList.ForEach(
                p => { listItem.Add(new ListViewItem(p.Values.ToArray())); });

            this.listView1.Items.AddRange(listItem.ToArray());

            SearchCompetitorPreview.UpdateLData(searchedCompetitorList, this.searchControl.Text);
        }

        private void KeyBtns_Click(object sender, EventArgs e)
        {
            var btn = (DevExpress.XtraEditors.SimpleButton) sender;

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
                || this._competitors.CompetitorList == null
                || this._competitors.CompetitorList.Count <= 0)
            {
                return;
            }

            if (this.listView1.SelectedItems.Count > 0)
            {
                // Get bib from listView/API
                var index = this.listView1.SelectedIndices[0];

                int bibIndex = 0;

                foreach (ColumnHeader column in this.listView1.Columns)
                {
                    if (column.Text == @"bib")
                    {
                        bibIndex = column.DisplayIndex;
                    }
                }

                var currentBib = this.listView1.Items[index].SubItems[bibIndex].Text;

                this._bib_text.Text = currentBib;

                var selectedCompotitor = new Dictionary<string, string>();

                foreach (var dict in this._competitors.CompetitorList)
                {
                    foreach (KeyValuePair<string, string> keyValuePair in dict)
                    {
                        if (keyValuePair.Key == "bib")
                        {
                            if (keyValuePair.Value == currentBib)
                            {
                                selectedCompotitor = dict;
                            }
                        }
                    }
                }

                CurrentData.EzdImage = ReopositoryEzdFile.UpdateEzdApi(selectedCompotitor);

                CurrentData.EzdPictureBox.Refresh();
            }

            CloseSearch();

            CurrentData.Preview?.UpdateImage(CurrentUIData.PanelImages.ToImage());
        }

        private void simpleButton12_Click(object sender, EventArgs e)
        {
            CloseSearch();
        }

        private void CloseSearch()
        {
            CurrentData.Preview?.CloseSearch();

            this.Close();
        }
    }
}