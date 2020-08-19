using System.Threading.Tasks;
using DevExpress.Utils.Extensions;
using DevExpress.XtraLayout;
using Telerik.WinControls.UI;
using static System.Threading.Tasks.Task;

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
        private readonly List<Tuple<string, StringBuilder>> _competitor;

        bool _doWorkRun = false;

        bool _doWorkTest = false;

        private bool _initRun = false;

        public UpdateEzdData(List<Tuple<string, StringBuilder>> competitor)
        {
            _competitor = competitor;

            this.MaximumSize = CurrentUIData.RightPanelSize;
            this.MinimumSize = CurrentUIData.RightPanelSize;

            InitializeComponent();

            this.flyoutPanel1.OwnerControl = CurrentUIData.RightLayoutControl;

            this.flyoutPanel1.MaximumSize = CurrentUIData.RightPanelSize;
            this.flyoutPanel1.MinimumSize = CurrentUIData.RightPanelSize;

            this.flyoutPanel1.ShowPopup();

            ElementCreator();
        }

        #region Plus and Minus btn click event

        private void ChangeFont(ReopositoryEzdFile.ModeFontSize mode, int index)
        {
            var img = ReopositoryEzdFile.FontSize(
                _competitor[index].Item1,
                mode);

            CurrentData.EzdImage = Images.MakeImageTransparent(img);

            CurrentData.EzdPictureBox.Refresh();

            LaserMarker.laserMarker.OpenPreview();
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
            //if (!_initRun) 
            //    return;

            try
            {
                var text = (TextEdit)sender;

                var comp = new Tuple<string, string>(text.Properties.NullText, text.Text);

                CurrentData.EzdImage = ReopositoryEzdFile.UpdateCustomEzd(comp);

                CurrentData.EzdPictureBox.Refresh();

                LaserMarker.laserMarker.OpenPreview();
            }
            catch (Exception)
            {
                XtraMessageBox.Show(@"Данные с этим номером не найдены", "Информация", MessageBoxButtons.OK);
            }
        }

        private void RunBtn_Click(object sender, EventArgs e)
        {
            var btn = (SimpleButton) sender;

            if (_doWorkTest)
            {
                _doWorkTest = false;
            }

            if (btn.Text == "RUN")
            {
                if (XtraMessageBox.Show("Вы действительно хотите гравировать?", "Сообщения", MessageBoxButtons.YesNo) ==
                    DialogResult.Yes)
                {
                    _doWorkRun = true;

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
                        XtraMessageBox.Show("Гравировка уже идет", "Информация", MessageBoxButtons.OK);
                    }
                }
            }
            else
            {
                _doWorkRun = false;

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
            try
            {
                if (_doWorkRun)
                {
                    ReopositoryEzdFile.Mark();
                }

                if (!ReopositoryEzdFile.IsMarking())
                {
                    _doWorkRun = false;
                    this.testBtn.Tag = "redMarkContour";

                    this.testBtn.Enabled = true;

                    this.testBtn.Appearance.BackColor = Color.FromArgb(192, 0, 0);

                    this.testBtn.Cursor = Cursors.Hand;

                    this.runBtn.Text = "RUN";
                    this.runBtn.Appearance.BackColor = Color.FromArgb(0, 192, 192);
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void TestBtn_Click(object sender, EventArgs e)
        {
            var btn = (SimpleButton) sender;
            try
            {
                if (_doWorkRun)
                {
                    if (XtraMessageBox.Show("Вы действительно хотите простановить гравировку?", "Сообщения",
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        _doWorkRun = false;
                    }
                    else
                    {
                        return;
                    }
                }

                if (btn.Text == "TEST")
                {
                    _doWorkTest = true;

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
                    _doWorkTest = false;

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
                XtraMessageBox.Show(ex.Message, "Информация", MessageBoxButtons.OK);
            }
        }

        private void TestRedMarkBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (_doWorkTest)
            {
                Thread.Sleep(100);
                ReopositoryEzdFile.RedMark();
            }
        }

        private void TestRedMarkContourBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (_doWorkTest)
            {
                Thread.Sleep(100);
                ReopositoryEzdFile.RedMarkContour();
            }
        }

        private void ElementCreator()
        {
            _initRun = false;
            try
            {
                var competitorCount = 60 + _competitor.Count;
                // Text
                var textEditSize = new Size
                {
                    Height = 40 - 5,
                    Width = this.Width - (int)(this.Width / 2.5) - 5
                };

                // Text`s layout
                var middleLayoutItemSize = new Size
                {
                    Height = 40,
                    Width = this.Width - (int)(this.Width / 2.5)
                };

                // Button`s layout
                var layoutItemSize = new Size
                {
                    Height = 40,
                    Width = 42
                };

                var textEdits = this.layoutControl2.Controls
                    .OfType<TextEdit>();

                var textEditsList = textEdits as TextEdit[] ?? textEdits.ToArray();
                var stop = new System.Diagnostics.Stopwatch();

                Parallel.ForEach(source: textEditsList, body: item =>
                {
                    _ = item.BeginInvoke(method: new Action(() =>
                    {
                        if (item.TabIndex >= competitorCount && item.TabIndex <= 67)
                            item.BackColor = Color.FromArgb(red: 240, green: 240, blue: 240);

                        if (item.TabIndex < competitorCount)
                        {
                            var text = _competitor[index: item.TabIndex - 60].Item2.ToString();
                            item.Text = text;

                            item.MinimumSize = textEditSize;
                            item.MaximumSize = textEditSize;
                            item.Size = textEditSize;

                            item.Properties.NullText = _competitor[index: item.TabIndex - 60].Item1;
                        }
                    }));
                });

                var control = this.layoutControlGroup3.Items.AsParallel();

                control.ForAll(item =>
                {
                    this.flyoutPanel1.BeginInvoke(new Action(() =>
                    {
                        if (item?.Tag == null)
                            return;

                        var tag = Convert.ToInt16(value: item.Tag);

                        if (tag >= 60 && tag < competitorCount)
                        {
                            item.MaxSize = middleLayoutItemSize;
                            item.MinSize = middleLayoutItemSize;
                            item.Size = middleLayoutItemSize;
                        }

                        if ((tag - 40 >= 60 && tag - 40 < competitorCount) ||
                            (tag - 50 >= 60 && tag - 50 < competitorCount))
                        {
                            item.MaxSize = layoutItemSize;
                            item.MinSize = layoutItemSize;
                            item.Size = layoutItemSize;
                        }

                        return;
                    }));

                });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void UpdateEzdData_Load(object sender, EventArgs e)
        {
            _initRun = true;
        }
    }
}