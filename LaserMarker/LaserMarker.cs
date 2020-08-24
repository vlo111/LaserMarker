using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using API;
using BLL;
using DevExpress.XtraEditors;
using Request = EntityFrameworkSql.EntityQuery;
using global::LaserMarker.DataAccess;
using global::LaserMarker.State;
using global::LaserMarker.UserControls;
using Newtonsoft.Json;

namespace LaserMarker
{
    using EZD = EzdDataControl.ReopositoryEzdFile;

    public partial class LaserMarker : Form
    {
        public static LaserMarker laserMarker;

        #region Path Field

        private readonly string iconPath = Directory.GetCurrentDirectory() + @"\icon\";

        #endregion

        private int _currentPEindex = 0;

        private List<Tuple<string, StringBuilder>> _ezdObjects;

        #region PictureBox Field

        public float BgHeight { get; set; }
        public float BgWidth { get; set; }
        public float ezdWidth { get; set; }
        public float ezdHeight { get; set; }
        public float NBgHeight { get; set; }
        public float NBgWidth { get; set; }
        public float NezdWidth { get; set; }
        public float NezdHeight { get; set; }

        #region Fields for BG picture

        private Point _bg_mouseDown;

        private float _bgStartx; // offset of image when mouse was pressed

        private float _bgStarty;

        private float _bgImgx; // current offset of image

        private float _bgImgy;

        private bool _bgMousepressed; // true as long as left mousebutton is pressed

        private float _bgZoom;

        #endregion Fields for BG picture

        #region Fields for FG picture

        private Point _fg_mouseDown;

        // offset of image when mouse was pressed
        private float _fgStartx;

        private float _fgStarty;

        // current offset of image
        private float _fgImgx;

        private float _fgImgy;

        // true as long as left mousebutton is pressed
        private bool _fg_mousepressed;

        private float _fgZoom;

        #endregion Fields for FG picture

        #endregion PictureBox Field

        private enum UploadType
        {
            /// <summary>
            /// The image.
            /// </summary>
            image = 1,

            /// <summary>
            /// The ezd.
            /// </summary>
            Ezd
        }

        private enum ScaleMode
        {
            /// <summary>
            /// The plus.
            /// </summary>
            Plus,

            /// <summary>
            /// The minus.
            /// </summary>
            Minus
        }

        private enum ScalePMCenter
        {
            Scroll,
            Minus,
            Plus
        }

        #region Initial

        public LaserMarker()
        {
            this.InitializeComponent();

            this.foregroundPictureBox.Parent = this.backgroundPictureBox;

            this.foregroundPictureBox.Paint += this.ForegroundImageBox_Paint;
            this.backgroundPictureBox.Paint += this.BackgroundImageBox_Paint;

            Initial();

            laserMarker = this;
        }

        private void Initial()
        {
            try
            {
                this.rightpanel.Width = ScreenSize.PrimaryWidth() / 100 * 30;

                this.panel1.Height = ScreenSize.PrimaryHeight() - (ScreenSize.PrimaryHeight() / 100 * 30);

                CurrentUIData.RightLayoutControl = this.rightLayoutControl;

                CurrentUIData.RightPanelSize = new Size(this.rightpanel.Width, ScreenSize.PrimaryHeight());

                CurrentUIData.PanelImages = this.panel1;

                EZD.AutoWidthEzd = false;

                CurrentData.EzdPictureBox = this.foregroundPictureBox;

                // Connect sdk
                var errMessage = EZD.Initialize(Application.StartupPath, false);

                if (!string.IsNullOrEmpty(errMessage))
                {
                    XtraMessageBox.Show(errMessage, @"Ошибка", MessageBoxButtons.OK);
                }

                var userDataDtos = UserDataRepository.GetAllUser();

                if (userDataDtos.Count > 0)
                {
                    // init top pictures
                    this.InitialTopPictures(userDataDtos);

                    // init current pictures
                    var currentData = userDataDtos.LastOrDefault();

                    if (currentData != null)
                    {
                        this.InitialCurrentDataFromUser(currentData);

                        this.CreateBgPictureBoxImage(false);

                        this.CreateEzdPictureBoxImage(false);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, @"Ошибка", MessageBoxButtons.OK);
            }
        }

        private void InitialTopPictures(IReadOnlyList<UserDataDto> userDataDtos)
        {
            for (var i = 0; i < userDataDtos.Count; i++)
            {
                var topImages = this.layoutControl2.Controls.OfType<PictureEdit>().ToArray();

                var image = topImages.Where(c => c.TabIndex == i + 40).Select(c => c).First();

                image.Image = userDataDtos[i].FullImage.ToImage();

                image.Properties.ReadOnly = false;

                image.Cursor = Cursors.Hand;

                image.Tag = @"filled";

                this._currentPEindex = i;

                if (i + 1 != userDataDtos.Count)
                {
                    continue;
                }

                // last plus
                var lastImage = topImages.Where(c => c.TabIndex == (i + 1) + 40).Select(c => c).First();

                lastImage.Image = Image.FromFile($@"{this.iconPath}plus.png");

                lastImage.Cursor = Cursors.Hand;

                lastImage.Properties.ReadOnly = false;

                lastImage.Tag = @"next";
            }
        }

        private void InitialCurrentDataFromUser(UserDataDto currentData)
        {
            this._currentPEindex = (int)currentData.Sequence;

            this.UpdateImageFromDb(currentData);

            this.urlTextEdit.Text = currentData.Login;

            this.passwordTextEdit.Text = currentData.Password;

            this.urlTextEdit.Text = currentData.UrlSport;

            CurrentApiData.Token = currentData.Token;
        }

        #endregion

        #region Login Event

        private async void LoginBtn_Click(object sender, EventArgs e)
        {
            try
            {
                // Get all events
                var base64HeaderValue = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes($@"{this.loginTextEdit.Text}:{this.passwordTextEdit.Text}"));

                const string Url = @"http://openeventor.ru/api/get_events";

                var task = await Request.GetAllEventsAsync(Url, base64HeaderValue);

                var result = JsonConvert.DeserializeObject<Eventor>(task);

                CustomFlyoutDialog.ShowForm(this, null, new GetEvents(result));

                this.urlTextEdit.Text = $@"http://openeventor.ru/api/event/{CurrentApiData.Token}/get_event";
            }
            catch (Exception)
            {
                XtraMessageBox.Show(
                    @"Проверте логин/пароль или нет доступ к апи, проверьте интернет соединения",
                    "Ошибка",
                    MessageBoxButtons.OK);

                if (CurrentData.EzdImage != null)
                {
                    _ezdObjects = EZD.GetEzdData();

                    new UpdateEzdData(_ezdObjects);
                }
            }
        }

        #endregion

        #region ShowDialog Event

        private void DialogUpdateEzd_Click(object sender, EventArgs e)
        {
            if (CurrentData.EzdImage != null)
            {
                try
                {
                    this.bgCheckBox.Checked = false;

                    this.fgCheckBox.Checked = false;

                    if (!string.IsNullOrEmpty(CurrentApiData.Token))
                    {
                        new UpdateEzdDataFromApi(this);
                    }
                    else
                    {
                        _ezdObjects = EZD.GetEzdData();

                        new UpdateEzdData(_ezdObjects);
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK);
                }
            }
            else
            {
                XtraMessageBox.Show(@"Пожалуйста, выберите ezd файл", @"Ошибка", MessageBoxButtons.OK);
            }
        }

        #endregion

        #region Create image for PictureBox

        private void CreateEzdPictureBoxImage(bool switchZoom)
        {
            if (CurrentData.EzdImage == null)
            {
                CurrentData.EzdImage = new Bitmap(10, 10);
            }

            var fg = this.CreateGraphics();

            ezdWidth = CurrentData.EzdImage.Width;
            ezdHeight = CurrentData.EzdImage.Height;
            if (switchZoom)
            {
                this._fgZoom = (this.foregroundPictureBox.Width / (float)CurrentData.EzdImage.Width)
                               * (CurrentData.EzdImage.HorizontalResolution / fg.DpiX);

                //Выставлем по центру
                this._fgImgx = (foregroundPictureBox.Width / 2) - ((int)(ezdWidth * this._fgZoom) / 2);
                this._fgImgy = 0;
            }

            this.foregroundPictureBox.Refresh();
        }

        private void CreateBgPictureBoxImage(bool switchZoom)
        {
            if (CurrentData.BgImage == null)
            {
                CurrentData.BgImage = new Bitmap(10, 10);
            }

            var g = this.CreateGraphics();

            // Fit width
            BgWidth = CurrentData.BgImage.Width;
            BgHeight = CurrentData.BgImage.Height;

            if (switchZoom)
            {
                this._bgZoom = (this.backgroundPictureBox.Width / (float)CurrentData.BgImage.Width)
                               * (CurrentData.BgImage.HorizontalResolution / g.DpiX);

                //Выставлем по центру
                this._bgImgy = (backgroundPictureBox.Height / 2) - ((int)(BgHeight * this._bgZoom) / 2);
                this._bgImgx = 0;
            }

            this.backgroundPictureBox.Refresh();
        }

        #endregion Create/Delete image from PictureBox


        #region Picturebox

        #region General FG BG Pictures Event

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            if ((msg.Msg != WM_KEYDOWN) && (msg.Msg != WM_SYSKEYDOWN))
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            if (this.bgCheckBox.Checked)
            {
                switch (keyData)
                {
                    case Keys.Right:
                        this._bgImgx -= (int)(this.backgroundPictureBox.Width * 0.03F / this._bgZoom);
                        this.backgroundPictureBox.Refresh();
                        break;

                    case Keys.Left:
                        this._bgImgx += (int)(this.backgroundPictureBox.Width * 0.03F / this._bgZoom);
                        this.backgroundPictureBox.Refresh();
                        break;

                    case Keys.Down:
                        this._bgImgy -= (int)(this.backgroundPictureBox.Height * 0.03F / this._bgZoom);
                        this.backgroundPictureBox.Refresh();
                        break;

                    case Keys.Up:
                        this._bgImgy += (int)(this.backgroundPictureBox.Height * 0.03F / this._bgZoom);
                        this.backgroundPictureBox.Refresh();
                        break;

                    case Keys.PageDown:
                        this._bgImgy -= (int)(this.backgroundPictureBox.Height * 0.20F / this._bgZoom);
                        this.backgroundPictureBox.Refresh();
                        break;

                    case Keys.PageUp:
                        this._bgImgy += (int)(this.backgroundPictureBox.Height * 0.20F / this._bgZoom);
                        this.backgroundPictureBox.Refresh();
                        break;
                    case Keys.Oemplus:
                        this.Scale(ScaleMode.Plus);
                        break;
                    case Keys.OemMinus:
                        this.Scale(ScaleMode.Minus);
                        break;
                }
            }

            if (!this.fgCheckBox.Checked)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            switch (keyData)
            {
                case Keys.Right:
                    this._fgImgx -= (int)(this.foregroundPictureBox.Width * 0.03F / this._fgZoom);
                    this.foregroundPictureBox.Refresh();
                    break;

                case Keys.Left:
                    this._fgImgx += (int)(this.foregroundPictureBox.Width * 0.03F / this._fgZoom);
                    this.foregroundPictureBox.Refresh();
                    break;

                case Keys.Down:
                    this._fgImgy -= (int)(this.foregroundPictureBox.Height * 0.03F / this._fgZoom);
                    this.foregroundPictureBox.Refresh();
                    break;

                case Keys.Up:
                    this._fgImgy += (int)(this.foregroundPictureBox.Height * 0.03F / this._fgZoom);
                    this.foregroundPictureBox.Refresh();
                    break;

                case Keys.PageDown:
                    this._fgImgy -= (int)(this.foregroundPictureBox.Height * 0.20F / this._fgZoom);
                    this.foregroundPictureBox.Refresh();
                    break;

                case Keys.PageUp:
                    this._fgImgy += (int)(this.foregroundPictureBox.Height * 0.20F / this._fgZoom);
                    this.foregroundPictureBox.Refresh();
                    break;
                case Keys.Oemplus:
                    this.Scale(ScaleMode.Plus);
                    break;
                case Keys.OemMinus:
                    this.Scale(ScaleMode.Minus);
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            MouseEventArgs mouse = e as MouseEventArgs;
            Point mousePosNow = mouse.Location;

            float oldimagex = 0;
            float oldimagey = 0;

            if (this.bgCheckBox.Checked)
            {
                float oldzoom = this._bgZoom;

                int x = mousePosNow.X - (int)this._bgImgx;
                int y = mousePosNow.Y - (int)this._bgImgy;


                if (e.Delta > 0)
                {
                    this._bgZoom = (float)(this._bgZoom * 1.1);
                    oldimagex = (float)(x - (x * 1.1));
                    oldimagey = (float)(y - (y * 1.1));
                }

                else if (e.Delta < 0)
                {
                    this._bgZoom = (float)(this._bgZoom / 1.1);
                    oldimagex = (float)(x - (x / 1.1));
                    oldimagey = (float)(y - (y / 1.1));
                }


                this._bgImgx += oldimagex;
                this._bgImgy += oldimagey;

                backgroundPictureBox.Refresh();
            }

            if (this.fgCheckBox.Checked)
            {
                float oldzoom = this._fgZoom;

                int x = mousePosNow.X - (int)this._fgImgx;
                int y = mousePosNow.Y - (int)this._fgImgy;


                if (e.Delta > 0)
                {
                    this._fgZoom = (float)(this._fgZoom * 1.1);

                    oldimagex = (float)(x - (x * 1.1));
                    oldimagey = (float)(y - (y * 1.1));
                }

                else if (e.Delta < 0)
                {
                    if (ezdWidth > 10 && ezdHeight > 10)
                        this._fgZoom = (float)(this._fgZoom / 1.1);

                    oldimagex = (float)(x - (x / 1.1));
                    oldimagey = (float)(y - (y / 1.1));
                }


                this._fgImgx += oldimagex;
                this._fgImgy += oldimagey;

                foregroundPictureBox.Refresh();
            }
        }

        private void ForegroundPictureBox_MouseDown(object sender, MouseEventArgs mouse)
        {
            if (mouse.Button == MouseButtons.Left)
            {
                if (this.bgCheckBox.Checked)
                {
                    if (!this._bgMousepressed)
                    {
                        this._bgMousepressed = true;
                        this._bg_mouseDown = mouse.Location;
                        this._bgStartx = this._bgImgx;
                        this._bgStarty = this._bgImgy;
                    }
                }

                if (fgCheckBox.Checked)
                {
                    if (!this._fg_mousepressed)
                    {
                        this._fg_mousepressed = true;
                        this._fg_mouseDown = mouse.Location;
                        this._fgStartx = this._fgImgx;
                        this._fgStarty = this._fgImgy;
                    }
                }
            }
        }

        private void ForegroundPictureBox_MouseMove(object sender, MouseEventArgs mouse)
        {
            if (mouse.Button == MouseButtons.Left)
            {
                var mousePosNow = mouse.Location;

                if (this.bgCheckBox.Checked)
                {
                    // the distance the mouse has been moved since mouse was pressed
                    var deltaX = mousePosNow.X - this._bg_mouseDown.X;
                    var deltaY = mousePosNow.Y - this._bg_mouseDown.Y;

                    // calculate new offset of image based on the current zoom factor
                    this._bgImgx = (int)(this._bgStartx + (deltaX));
                    this._bgImgy = (int)(this._bgStarty + (deltaY));

                    this.backgroundPictureBox.Refresh();
                }

                if (fgCheckBox.Checked)
                {
                    // the distance the mouse has been moved since mouse was pressed
                    var deltaX = mousePosNow.X - this._fg_mouseDown.X;
                    var deltaY = mousePosNow.Y - this._fg_mouseDown.Y;

                    // calculate new offset of image based on the current zoom factor
                    this._fgImgx = (int)(this._fgStartx + (deltaX));
                    this._fgImgy = (int)(this._fgStarty + (deltaY));

                    this.foregroundPictureBox.Refresh();
                }
            }
        }

        private void ForegroundPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.bgCheckBox.Checked)
            {
                this._bgMousepressed = false;
            }

            if (this.fgCheckBox.Checked)
            {
                this._fg_mousepressed = false;
            }
        }

        private void ForegroundImageBox_Paint(object sender, PaintEventArgs e)
        {
            if (CurrentData.EzdImage == null)
            {
                return;
            }

            e.Graphics.InterpolationMode = InterpolationMode.Low;

            NezdWidth = (ezdWidth * this._fgZoom);
            NezdHeight = ((ezdHeight / ezdWidth) * NezdWidth);

            e.Graphics.DrawImage(CurrentData.EzdImage, (int)this._fgImgx, (int)this._fgImgy, (int)NezdWidth,
                (int)NezdHeight);
        }

        private void BackgroundImageBox_Paint(object sender, PaintEventArgs e)
        {
            if (CurrentData.BgImage == null)
            {
                return;
            }

            NBgWidth = (BgWidth * this._bgZoom);
            NBgHeight = ((BgHeight / (float)BgWidth) * NBgWidth);

            e.Graphics.DrawImage(CurrentData.BgImage, (int)this._bgImgx, (int)this._bgImgy, (int)NBgWidth,
                (int)NBgHeight);

            labelControl1.Text = "X: " + this._bgImgx + " Y: " + this._bgImgy;
        }

        #endregion General FG BG Pictures Event

        #region Minus/Plus Event

        private void MunisBtn_Click(object sender, EventArgs e)
        {
            Scale(ScaleMode.Minus);
        }

        private void PlusBtn_Click(object sender, EventArgs e)
        {
            Scale(ScaleMode.Plus);
        }

        #endregion Minus/Plus Event

        private void Scale(ScaleMode scaleMode)
        {
            float oldimagex = 0;
            float oldimagey = 0;

            if (this.bgCheckBox.Checked)
            {
                float oldzoom = this._bgZoom;

                int x = this.panel1.Width / 2 - (int)this._bgImgx;
                int y = this.panel1.Height / 2 - (int)this._bgImgy;


                if (scaleMode == ScaleMode.Plus)
                {
                    this._bgZoom = (float)(this._bgZoom * 1.1);
                    oldimagex = (float)(x - (x * 1.1));
                    oldimagey = (float)(y - (y * 1.1));
                }

                else if (scaleMode == ScaleMode.Minus)
                {
                    this._bgZoom = (float)(this._bgZoom / 1.1);
                    oldimagex = (float)(x - (x / 1.1));
                    oldimagey = (float)(y - (y / 1.1));
                }


                this._bgImgx += oldimagex;
                this._bgImgy += oldimagey;

                backgroundPictureBox.Refresh();
            }

            if (this.fgCheckBox.Checked)
            {
                float oldzoom = this._fgZoom;

                int x = this.panel1.Width / 2 - (int)this._fgImgx;
                int y = this.panel1.Height / 2 - (int)this._fgImgy;


                if (scaleMode == ScaleMode.Plus)
                {
                    this._fgZoom = (float)(this._fgZoom * 1.1);

                    oldimagex = (float)(x - (x * 1.1));
                    oldimagey = (float)(y - (y * 1.1));
                }

                else if (scaleMode == ScaleMode.Minus)
                {
                    if (ezdWidth > 10 && ezdHeight > 10)
                        this._fgZoom = (float)(this._fgZoom / 1.1);

                    oldimagex = (float)(x - (x / 1.1));
                    oldimagey = (float)(y - (y / 1.1));
                }


                this._fgImgx += oldimagex;
                this._fgImgy += oldimagey;

                foregroundPictureBox.Refresh();
            }
        }

        #endregion Picturebox

        #region Upload

        private void UploadBGBtn_Click(object sender, EventArgs e)
        {
            this.Upload(UploadType.image);
        }

        private void UploadEzdBtn_Click(object sender, EventArgs e)
        {
            this.Upload(UploadType.Ezd);
        }

        private void Upload(UploadType type)
        {
            try
            {
                // take filter type
                var filter = type == UploadType.Ezd
                    ? @"EZD file (*.ezd) | *.ezd"
                    : @"Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";

                using (var ofd = new OpenFileDialog { Multiselect = false, ValidateNames = true, Filter = filter })
                {
                    if (ofd.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    switch (type)
                    {
                        case UploadType.Ezd when ofd.FileName != null:
                            {
                                this.ezdFileLbl.Text = Path.GetFileName(ofd.FileName);

                                CurrentData.EzdImage = EZD.LoadAndGetImage(
                                    ofd.FileName);

                                if (EZD.CheckSameEntName())
                                {
                                    XtraMessageBox.Show(
                                        @"В файле существует одноименный поля. Автоподстройка будет работать для одного из этих полей",
                                        @"Предупреждение", MessageBoxButtons.OK);
                                }

                                CurrentData.EzdName = ofd.FileName;

                                this.CreateEzdPictureBoxImage(true);

                                break;
                            }

                        case UploadType.image when ofd.FileName != null:
                            {
                                this.bgImageLbl.Text = Path.GetFileName(ofd.FileName);

                                CurrentData.BgImage = Image.FromFile(ofd.FileName);

                                CurrentData.BgName = ofd.FileName;

                                this.CreateBgPictureBoxImage(true);

                                break;
                            }

                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK);
            }
        }

        #endregion

        #region Delete/Save

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurrentData.BgImage != null && CurrentData.EzdImage != null)
                {
                    if (this._currentPEindex < 10)
                    {
                        if (CurrentData.EzdImage != null)
                        {
                            if (CurrentData.Preview != null)
                            {
                                this.OpenPreview();

                                CurrentData.Preview.showEzd = false;

                                CurrentData.Preview.RefreshEzd();
                            }
                        }

                        CurrentData.FullImageName = $@"FullImage{DateTime.Now.Ticks}.jpg";

                        CurrentData.FullImage = this.panel1.ToImage();

                        this.SaveImageDB();

                        this.UpdateTopPictures();
                    }
                }
                else
                {
                    XtraMessageBox.Show("Выберите обложку или ezd файл", "Error", MessageBoxButtons.OK);
                }
            }
            catch (Exception)
            {
            }
        }

        private void UpdateTopPictures()
        {
            // Top button, Tab index 40 - 49
            var selectedImage = this.layoutControl2.Controls.OfType<PictureEdit>()
                .Where(c => c.TabIndex == this._currentPEindex + 40).Select(c => c).First();

            selectedImage.Image = this.panel1.ToImage();
            selectedImage.Cursor = Cursors.Hand;
            selectedImage.Properties.ReadOnly = false;
            selectedImage.Tag = "filled";

            // next plus
            if (this._currentPEindex < 9)
            {
                var selectedNextImage = this.layoutControl2.Controls.OfType<PictureEdit>()
                    .Where(c => c.TabIndex == (_currentPEindex + 1) + 40).Select(c => c).First();

                if (selectedNextImage.Properties.ReadOnly)
                {
                    selectedNextImage.Image = Image.FromFile($@"{iconPath}plus.png");
                    selectedNextImage.Cursor = Cursors.Hand;
                    selectedNextImage.Properties.ReadOnly = false;
                    selectedNextImage.Tag = "next";
                }
            }
        }

        private void SaveImageDB()
        {
            UserDataRepository.Insert(
                new UserDataDto
                {
                    Token = CurrentApiData.Token,
                    Sequence = _currentPEindex,
                    Login = this.loginTextEdit.Text,
                    Password = this.passwordTextEdit.Text,
                    UrlSport = this.urlTextEdit.Text,
                    BgImageName = CurrentData.BgName,
                    EzdImageName = CurrentData.EzdName,
                    FullImageName = CurrentData.FullImageName,
                    FullImage = CurrentData.FullImage.ToBytes(),
                    BgImagePosX = (long)this._bgImgx,
                    BgImagePosY = (long)this._bgImgy,
                    BgImageScale = this._bgZoom,
                    EzdImageScale = this._fgZoom,
                    EzdImagePosX = (long)this._fgImgx,
                    EzdImagePosY = (long)this._fgImgy
                });
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            if (CurrentData.BgImage != null || CurrentData.EzdImage != null)
            {
                if (XtraMessageBox.Show("Вы действительно хотите удалить?", "Сообщения", MessageBoxButtons.YesNo)
                    == DialogResult.Yes)
                {
                    if (_currentPEindex < 10)
                    {
                        UserDataRepository.DeleteByTabIndex(_currentPEindex);

                        CurrentData.Clear();

                        var userDataDtos = UserDataRepository.GetAllUser();

                        this.ResetTopPictures();

                        this.InitialTopPictures(userDataDtos);

                        this.bgImageLbl.Text = " ";

                        this.ezdFileLbl.Text = " ";

                        this.backgroundPictureBox.Refresh();
                        this.foregroundPictureBox.Refresh();

                        CurrentData.Preview?.Close();

                        CurrentData.Preview = null;

                        this.OpenPreview();
                    }
                }
            }
            else
            {
                XtraMessageBox.Show("Вы что хотите удалить?", "Предупреждение", MessageBoxButtons.OK);
            }
        }

        #endregion

        #region TopPictures Event

        private void TopPictureEdits_Click(object sender, EventArgs e)
        {
            try
            {
                var picture = (PictureEdit)sender;

                this._currentPEindex = picture.TabIndex - 40;

                if ((string)picture.Tag == @"filled")
                {
                    if (!picture.Properties.ReadOnly)
                    {
                        var userDto = UserDataRepository.GetByTabIndex(this._currentPEindex);

                        this.UpdateImageFromDb(userDto);

                        this.foregroundPictureBox.Refresh();

                        this.backgroundPictureBox.Refresh();
                    }
                }

                if (CurrentData.EzdImage != null)
                {
                    if (CurrentData.Preview != null)
                    {
                        this.OpenPreview();

                        CurrentData.Preview.showEzd = false;

                        CurrentData.Preview.RefreshEzd();
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        #endregion

        private void ResetTopPictures()
        {
            var topImages = this.layoutControl2.Controls.OfType<PictureEdit>().ToArray();

            foreach (var pictureEdit in topImages)
            {
                pictureEdit.ReadOnly = true;
                pictureEdit.Cursor = Cursors.Default;
                pictureEdit.Image = null;
            }
        }

        private void UpdateImageFromDb(UserDataDto currentData)
        {
            // bg
            if (!File.Exists(currentData.BgImageName))
            {
                XtraMessageBox.Show(@"Подложка не найдено", "Ошибка", MessageBoxButtons.OK);
            }
            else
            {
                CurrentData.BgImage = Image.FromFile(currentData.BgImageName);

                CurrentData.BgName = currentData.BgImageName;

                //this._bgStartx = (int)currentData.BgImagePosStartX;

                //this._bgStarty = (int)currentData.BgImagePosStartY;

                this._bgImgx = (int)currentData.BgImagePosX;

                this._bgImgy = (int)currentData.BgImagePosY;

                this._bgZoom = (float)currentData.BgImageScale;
            }

            // Ezd
            if (!File.Exists(currentData.EzdImageName))
            {
                XtraMessageBox.Show(@"Ezd файл не найдено", @"Ошибка", MessageBoxButtons.OK);
            }
            else
            {
                CurrentData.EzdImage = EZD.LoadAndGetImage(
                    currentData.EzdImageName);

                CurrentData.EzdName = currentData.EzdImageName;

                //this._fgStartx = (int)currentData.EzdImagePosStartX;

                //this._fgStarty = (int)currentData.EzdImagePosStartY;

                this._fgImgx = (int)currentData.EzdImagePosX;

                this._fgImgy = (int)currentData.EzdImagePosY;

                this._fgZoom = (float)currentData.EzdImageScale;
            }
        }

        public void OpenPreview()
        {
            // Screen preview
            if (Screen.AllScreens.Length > 1)
            {
                if (CurrentData.Preview == null)
                {
                    CurrentData.Preview?.Close();

                    CurrentData.Preview = new Preview(this.panel1.Size, this._bgZoom, this._fgZoom, this._bgImgx,
                        this._bgImgy, this._fgImgx, this._fgImgy, this.BgHeight, this.BgWidth, this.ezdHeight,
                        this.ezdWidth);

                    CurrentData.Preview.showEzd = false;

                    CurrentData.Preview.RefreshEzd();

                    CurrentData.Preview.Show();
                }
                else
                {
                    CurrentData.Preview.showEzd = true;

                    CurrentData.Preview.RefreshEzd();

                    CurrentData.Preview.UpdateImage(this.panel1.Size, this._bgZoom, this._fgZoom, this._bgImgx,
                        this._bgImgy, this._fgImgx, this._fgImgy, this.BgHeight, this.BgWidth, this.ezdHeight,
                        this.ezdWidth);
                }
            }
        }

        private void LaserMarker_Load(object sender, EventArgs e)
        {
            if (CurrentData.EzdImage != null)
            {
                this.OpenPreview();
            }
        }

        private void autoWidthEzdCB_CheckedChanged(object sender, EventArgs e)
        {
            EZD.AutoWidthEzd = this.autoWidthEzdCB.Checked;
        }
    }
}