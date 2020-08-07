namespace LaserMarker.UserControls
{
    using BLL;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    using DevExpress.XtraEditors;

    using EzdDataControl;

    using global::LaserMarker.State;

    public partial class UpdateEzdData : XtraUserControl
    {
        List<TextEdit> textEdits = new List<TextEdit>();

        List<Tuple<string, StringBuilder>> _competitor;

        bool doWorkRun = false;

        bool doWorkTest = false;

        public UpdateEzdData(List<Tuple<string, StringBuilder>> competitor)
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

            _competitor = competitor;

            this.MaximumSize = CurrentUIData.RightPanelSize;
            this.MinimumSize = CurrentUIData.RightPanelSize;

            InitializeComponent();

            try
            {
                this.flyoutPanel1.OwnerControl = CurrentUIData.RightLayoutControl;

                this.flyoutPanel1.MaximumSize = CurrentUIData.RightPanelSize;
                this.flyoutPanel1.MinimumSize = CurrentUIData.RightPanelSize;

                this.flyoutPanel1.ShowPopup();

                // Text
                var textEditSize = new Size();
                textEditSize.Height = 40 - 5;
                textEditSize.Width = this.Width - (int)(this.Width / 2.5) - 5;

                // Text`s layout
                var middlelayoutItemSize = new Size();
                middlelayoutItemSize.Height = 40;
                middlelayoutItemSize.Width = this.Width - (int)(this.Width / 2.5);

                // Button`s layout
                var layoutItemSize = new Size();
                layoutItemSize.Height = 40;
                layoutItemSize.Width = 42;

                for (int index = 60; index < (60 + competitor.Count); index++)
                {
                    #region Initial TextEdits

                    var textEdit = this.layoutControl2.Controls.OfType<TextEdit>()
                        .Where(c => c.TabIndex == index)
                        .Select(c => c)
                        .First();

                    textEdit.Text = competitor[index - 60].Item2.ToString();

                    textEdit.MinimumSize = textEditSize;
                    textEdit.MaximumSize = textEditSize;
                    textEdit.Size = textEditSize;

                    textEdit.Properties.NullText = competitor[index - 60].Item1;

                    textEdits.Add(textEdit);

                    #endregion

                    #region Initial TextEditLayout

                    var layoutItem = layoutControlGroup3.Items
                        .Where(p =>
                        {
                            if (p.Tag != null)
                            {
                                p.Tag = Convert.ToString(p.Tag);

                                string layout_index = index.ToString();

                                bool tag = (string)p.Tag == layout_index;

                                return tag;
                            }
                            return false;
                        })
                        .Select(c => c)
                        .First();

                    layoutItem.MaxSize = middlelayoutItemSize;
                    layoutItem.MinSize = middlelayoutItemSize;
                    layoutItem.Size = middlelayoutItemSize;

                    #endregion

                    #region Initial Button`s layouts

                    var pluslayout = layoutControlGroup3.Items
                        .Where(p =>
                        {
                            if (p.Tag != null)
                            {
                                p.Tag = Convert.ToString(p.Tag);

                                string layout_index = (index + 40).ToString();

                                bool tag = (string)p.Tag == layout_index;

                                return tag;
                            }
                            return false;
                        })
                        .Select(c => c)
                        .First();

                    var minuslayout = layoutControlGroup3.Items
                        .Where(p =>
                        {
                            if (p.Tag != null)
                            {
                                p.Tag = Convert.ToString(p.Tag);

                                string layout_index = (index + 50).ToString();

                                bool tag = (string)p.Tag == layout_index;

                                return tag;
                            }
                            return false;
                        })
                        .Select(c => c)
                        .First();

                    pluslayout.MaxSize = layoutItemSize;
                    pluslayout.MinSize = layoutItemSize;
                    pluslayout.Size = layoutItemSize;

                    minuslayout.MaxSize = layoutItemSize;
                    minuslayout.MinSize = layoutItemSize;
                    minuslayout.Size = layoutItemSize;

                    #endregion
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #region Plus and Minus btn click event

        private void ChangeFont(ReopositoryEzdFile.ModeFontSize mode, int index)
        {
            var img = ReopositoryEzdFile.FontSize(
                _competitor[index].Item1,
                mode,
                CurrentData.EzdImage.Width,
                CurrentData.EzdImage.Height);

            CurrentData.EzdImage = Images.MakeImageTransparent(img);

            CurrentData.EzdPictureBox.Refresh();
        }

        private void obj1PlusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Zoom, 0);
        }

        private void obj2PlusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Zoom, 1);
        }

        private void obj3PlusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Zoom, 2);
        }

        private void obj4PlusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Zoom, 3);
        }

        private void obj5PlusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Zoom, 4);
        }

        private void obj6PlusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Zoom, 5);
        }

        private void obj7PlusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Zoom, 6);
        }

        private void obj8PlusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Zoom, 7);
        }

        private void obj1MinusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Reduce, 0);
        }

        private void obj2MinusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Reduce, 1);
        }

        private void obj3MinusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Reduce, 2);
        }

        private void obj4MinusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Reduce, 3);
        }

        private void obj5MinusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Reduce, 4);
        }

        private void obj6MinusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Reduce, 5);
        }

        private void obj7MinusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Reduce, 6);
        }

        private void obj8MinusBtn_Click(object sender, EventArgs e)
        {
            ChangeFont(ReopositoryEzdFile.ModeFontSize.Reduce, 7);
        }

        #endregion

        private void HidePopupBtn_Click(object sender, EventArgs e)
        {
            this.flyoutPanel1.HidePopup();
        }

        private void obj1TextEdit_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var text = (TextEdit)sender;

                var comp = new Tuple<string, string>(text.Properties.NullText, text.Text);

                CurrentData.EzdImage = ReopositoryEzdFile.UpdateCustomEzd(comp, CurrentData.EzdImage.Width, CurrentData.EzdImage.Height);
                
                CurrentData.EzdPictureBox.Refresh();
            }
            catch (Exception)
            {
                XtraMessageBox.Show(@"Данные с этим номером не найдены", "Information", MessageBoxButtons.OK);
            }
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
                        this.testBtn.BackColor = Color.LightSalmon;
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

                this.testBtn.Appearance.BackColor = Color.LightSalmon;

                
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
                doWorkRun = false;
                this.testBtn.Tag = "redMarkContour";

                this.testBtn.Enabled = true;

                this.testBtn.Appearance.BackColor = Color.FromArgb(192, 0, 0);

                this.testBtn.Cursor = Cursors.Hand;

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
                    }
                    else if (btn.Tag.ToString() == "redMarkContour")
                    {
                        testRedMarkContourBackgroundWorker.WorkerSupportsCancellation = true;
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