using System.Drawing;

namespace LaserMarker.UserControls
{
    using API;
    using System;
    using System.Collections.Generic;
    using DevExpress.XtraEditors;
    using BLL;
    using System.Windows.Forms;
    using System.Linq;

    public partial class SearchCompetitorPreview : Form //: XtraUserControl
    {
        public static Competitors _competitors;

        public Competitors _tempCompetitors;

        public static string _searchText;

        static System.Timers.Timer timer = new System.Timers.Timer(1000);

        public SearchCompetitorPreview(int height)
        {
            InitializeComponent();

            // Get the second monitor screen
            Screen screen = ScreenSize.GetSecondaryScreen();

            if (screen != null)
            {
                // Important
                this.StartPosition = FormStartPosition.Manual;

                // set the location to the top left of the second screen
                this.Location = screen.WorkingArea.Location;

                // set it fullscreen
                this.Size = new Size(screen.WorkingArea.Width, height);

                this.Location = new Point(this.Location.X, (screen.Bounds.Height - height) / 2);
            }

            timer.Elapsed += Timer_Elapsed;

            timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_competitors != null)
            {
                if (_tempCompetitors == null ||
                    !_tempCompetitors.CompetitorList.SequenceEqual(_competitors.CompetitorList))
                {
                    if (this.listView11.InvokeRequired)
                    {
                        this.listView11.BeginInvoke(new Action(() =>
                        {
                            if (_competitors != null)
                            {
                                this.listView11.Items.Clear();

                                var listItem = new List<ListViewItem>();

                                // Columns
                                if (this.listView11.Columns.Count <= 0)
                                {
                                    ICollection<ColumnHeader> columns = new List<ColumnHeader>();

                                    var dictionary = _competitors.CompetitorList.FirstOrDefault();

                                    if (dictionary != null)
                                        foreach (var key in dictionary.Keys)
                                        {
                                            columns.Add(new ColumnHeader()
                                                {Text = key, Width = this.listView11.Width / 100 * 20});
                                        }

                                    this.listView11.Columns.AddRange(columns.ToArray());
                                }

                                //Items
                                _competitors.CompetitorList.ForEach(
                                    p => { listItem.Add(new ListViewItem(p.Values.ToArray())); });

                                this.listView11.Items.AddRange(listItem.ToArray());
                            }

                            if (!string.IsNullOrEmpty(_searchText))
                            {
                                this.searchControl.Text = _searchText;
                            }

                            _tempCompetitors = _competitors;
                        }));
                    }
                }
            }
        }


        public static void UpdateLData(List<Dictionary<string, string>> searchedCompetitorList,
            string searchControlText)
        {
            if (_competitors == null)
            {
                _competitors = new Competitors();

                _competitors.CompetitorList = new List<Dictionary<string, string>>();
            }

            _competitors.CompetitorList = searchedCompetitorList;

            _searchText = searchControlText;
        }
    }
}