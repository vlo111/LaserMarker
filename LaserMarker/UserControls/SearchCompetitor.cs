using DevExpress.Utils.Extensions;
using DevExpress.XtraBars.Docking2010.Customization;
using EntityFrameworkSql;

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
    using Request = EntityFrameworkSql.EntityQuery;

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

        bool switching = true;

        private static DateTime time = new DateTime();

        private async void searchControl1_TextChanged(object sender, EventArgs e)
        {
            var search = this.searchControl.Text;
            try
            {
                if (!string.IsNullOrEmpty(this.searchControl.Text))
                {
                    if (string.IsNullOrEmpty(search))
                    {
                        return;
                    }

                    this.waitingBar.StartWaiting();

                    var mili = DateTime.Now.TimeOfDay.TotalMilliseconds - time.TimeOfDay.TotalMilliseconds;

                    if (mili > 0 && mili <= 500)
                    {
                        switching = false;
                    }

                    time = DateTime.Now;

                    if (_tokenSource != null)
                    {
                        _tokenSource.Cancel();
                    }

                    _tokenSource = new CancellationTokenSource();

                    _= Task.Delay(500, _tokenSource.Token).ConfigureAwait(true);

                    //_tokenSource.Token.ThrowIfCancellationRequested();

                    if (switching)
                    {
                        var task = await Request.GetRequestAsync(
                            $@"http://openeventor.ru/event/{CurrentApiData.Token}/plugins/engraver/get?search={search}");

                        if (task == null)
                        {
                            this.waitingBar.StopWaiting();
                            return;
                        }

                        this._competitors = JsonConvert.DeserializeObject<Competitors>(task);

                        UpdateListView(search);
                    }

                    switching = true;


                    this.waitingBar.StopWaiting();
                }
            }
            catch (Exception)
            {
                this.waitingBar.StopWaiting();
            }
        }

        private void UpdateListView(string search)
        {
            try
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
                            .Contains(this.searchControl.Text.Trim()
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

                SearchCompetitorPreview.UpdateLData(searchedCompetitorList, search);
            }
            catch (Exception e)
            {
                return;
            }
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

            LaserMarker.laserMarker.OpenPreview();
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