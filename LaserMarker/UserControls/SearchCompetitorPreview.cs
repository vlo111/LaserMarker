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

    public partial class SearchCompetitorPreview : Form//: XtraUserControl
    {
        public static List<CompetitorData> _competitors;

        public static List<CompetitorData> _tempCompetitors;

        public static string _searchText;

        static System.Timers.Timer timer = new System.Timers.Timer(1000);

        public SearchCompetitorPreview(int height)
        {
            InitializeComponent();

            this.listView11.Columns.AddRange(
                new ColumnHeader[]
                    {
                        new ColumnHeader() { Text = @"Bib", Width = this.listView11.Width / 100 * 20 },
                        new ColumnHeader() { Text = @"First Name", Width = this.listView11.Width / 100 * 30  },
                        new ColumnHeader() { Text = @"Last Name", Width = this.listView11.Width / 100 * 30  },
                        new ColumnHeader() { Text = @"Birth year", Width = this.listView11.Width / 100 * 20  }
                    });


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
                if (_tempCompetitors != null && _tempCompetitors.OrderBy(p => p.Bib).SequenceEqual(_competitors.OrderBy(p => p.Bib)))
                {

                }
                else
                {
                    if (this.listView11.InvokeRequired)
                    {
                        this.listView11.BeginInvoke(new Action(() =>
                        {
                            if (_competitors != null)
                            {
                                this.listView11.Items.Clear();

                                var listItem = new List<ListViewItem>();

                                foreach (var competitor in _competitors)
                                {
                                    listItem.Add(new ListViewItem(new[] {
                                competitor.Bib,
                                competitor.FirstName,
                                competitor.LastName,
                                competitor.BirthYear }));

                                }

                                this.listView11.Items.AddRange(listItem.ToArray());
                            }

                            if (!string.IsNullOrEmpty(_searchText))
                            {
                                this.searchControl.Text = _searchText;
                            }
                        }));
                    }

                }
            }

            _tempCompetitors = _competitors;

        }

          
        public static void UpdateLData(List<CompetitorData> searchedCompetitorList, string searchControlText)
        {
            _competitors = searchedCompetitorList;

            _searchText = searchControlText;
        }
    }
}
