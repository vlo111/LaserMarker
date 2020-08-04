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

namespace LaserMarker.UserControls
{
    using API;

    using global::LaserMarker.Helper;

    public partial class SearchCompetitorPreview : XtraUserControl
    {
        public static List<CompetitorData> _competitors;

        public static string _searchText;

        public SearchCompetitorPreview()
        {
            InitializeComponent();  
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateData();
        }

        public void UpdateData()
        {
            if (_competitors != null)
            {
                this.listView11.Items.Clear();

                var listItem = new List<ListViewItem>();

                _competitors.ForEach(
                    p =>
                        {
                            listItem.Add(new ListViewItem(new[] { p.Bib, p.FirstName, p.LastName, p.BirthYear }));
                        });

                this.listView11.Items.AddRange(listItem.ToArray());
            }

            if (!string.IsNullOrEmpty(_searchText))
            {
                this.searchControl.Text = _searchText;
            }
        }

        public static void UpdateLData(List<CompetitorData> searchedCompetitorList, string searchControlText)
        {
            _competitors = searchedCompetitorList;
            _searchText = searchControlText;
        }
    }
}
