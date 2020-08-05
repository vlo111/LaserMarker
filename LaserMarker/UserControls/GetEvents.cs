namespace LaserMarker.UserControls
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using DevExpress.XtraEditors;
    using Telerik.WinControls.UI;
    using ControlScreen;
    using System.Drawing;
    using API;

    public partial class GetEvents : DevExpress.XtraEditors.XtraUserControl
    {
        private readonly IList<Event> _data;

        RadWaitingBar waitingBar;

        public GetEvents(Eventor eventor)
        {
            InitializeComponent();
            
            this.Width = ScreenSize.PrimaryWidth();

            this._data = eventor.Events.Select(p => new Event { Token = p.Token, Id = p.Id, Name = p.Name }).ToList();

            if (eventor.Events != null)
            {
                this.listBoxControl1.DataSource = this._data;
                this.listBoxControl1.DisplayMember = "Name";
            }

            //waitingBar = new RadWaitingBar();
            //waitingBar.AssociatedControl = this.layoutControl21;
            //waitingBar.Size = new System.Drawing.Size(80, 80);
            //waitingBar.WaitingStyle = Telerik.WinControls.Enumerations.WaitingBarStyles.LineRing;

            //this.layoutControl21.Controls.Add(waitingBar);
        }

        private void KeyBtns_Click(object sender, EventArgs e)
        {
            var btn = (DevExpress.XtraEditors.SimpleButton)sender;
            if (btn.Text == "<")
            {
                if (string.IsNullOrEmpty(this.searchEventControl.Text))
                {
                    return;
                }

                this.searchEventControl.Text = this.searchEventControl.Text.Remove(this.searchEventControl.Text.Length - 1);
            }
            else
            {
                this.searchEventControl.Text = this.searchEventControl.Text + btn.Text;
            }

            this.listBoxControl1.Items.Clear();

            Event[] search = this._data.Where(p => p.Name.ToLower().Contains(this.searchEventControl.Text.Trim().ToLower()))
                .ToArray();

            this.listBoxControl1.DataSource = search;
        }

        private void enterBtn_Click(object sender, EventArgs e)
        {
            var data = (Event)this.listBoxControl1.SelectedItem;

            if (data != null)
            {
                State.CurrentApiData.Token = data.Token;

                ((SimpleButton)sender).DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }

        private void GetEvents_Load(object sender, EventArgs e)
        {
            //this.MaximumSize = new Size(CurrentUIData.WindowSize.Width, CurrentUIData.WindowSize.Height - (CurrentUIData.WindowSize.Height / 3));

            //this.Height = CurrentUIData.WindowSize.Height - (CurrentUIData.WindowSize.Height / 3);
            // this.Width = CurrentUIData.WindowSize.Width;

            //this.Left = 0;
            //this.Top = (CurrentUIData.WindowSize.Height - this.Height) / 2;
        }

    }
}
