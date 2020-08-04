using System.Collections.Generic;
using System.Linq;

namespace LaserMarker.Helper
{
    using System.Windows.Forms;

    using API;

    public static class ControlSearch
    {
        public static ListView listView1;

        static ControlSearch()
        {
            listView1 = new ListView();

            listView1.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listView1.HideSelection = false;
            listView1.Location = new System.Drawing.Point(12, 12);
            listView1.MultiSelect = false;
            listView1.Name = "listView1";
            listView1.Size = new System.Drawing.Size(812, 148);
            listView1.Sorting = System.Windows.Forms.SortOrder.Ascending;
            listView1.TabIndex = 74;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = System.Windows.Forms.View.Details;

            listView1.Columns.AddRange(
                new ColumnHeader[]
                    {
                        new ColumnHeader() { Text = @"Bib", Width = 50 },
                        new ColumnHeader() { Text = @"First Name", Width = 150 },
                        new ColumnHeader() { Text = @"Last Name", Width = 150 },
                        new ColumnHeader() { Text = @"Birth year", Width = 100 }
                    });
        }

        public static void UpdateListView(string search, CompetitorList competitors)
        {
            listView1.Items.Clear();

            var listItem = new List<ListViewItem>();

            if (competitors.CompetitorDatas == null || competitors.CompetitorDatas.Count <= 0)
            {
                return;
            }

            var competitor = competitors.CompetitorDatas.Where(
                p => p.LastName != null && p.LastName.ToLower().Contains(search.Trim().ToLower()));

            var searchedCompetitorList = competitor.ToList();

            searchedCompetitorList.ForEach(
                p => { listItem.Add(new ListViewItem(new[] { p.Bib, p.FirstName, p.LastName, p.BirthYear })); });

            listView1.Items.AddRange(listItem.ToArray());
        }
    }
}