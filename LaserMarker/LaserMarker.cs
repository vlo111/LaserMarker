namespace LaserMarker
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    using API;

    using DevExpress.XtraEditors;

    using EzdDataL;

    using global::LaserMarker.DataAccess;
    using global::LaserMarker.State;
    using global::LaserMarker.UserControls;

    using Newtonsoft.Json;

    using PictureControl;

    using SDK;

    public partial class LaserMarker : Form
    {
        private static List<Tuple<string, StringBuilder>> updatedEzdObjects;

        #region Path Field

        private readonly string iconPath = Directory.GetCurrentDirectory() + @"\icon\";

        #endregion

        private int currentPEindex = 0;

        private List<Tuple<string, StringBuilder>> ezdObjects;

        #region PictureBox Field

        #region Fields for BG picture

        private Point _bg_mouseDown;

        private int _bgStartx; // offset of image when mouse was pressed

        private int _bgStarty;

        private int _bgImgx; // current offset of image

        private int _bgImgy;

        private bool _bgMousepressed; // true as long as left mousebutton is pressed

        private float _bgZoom;

        #endregion Fields for BG picture

        #region Fields for FG picture

        private Point _fg_mouseDown;

        // offset of image when mouse was pressed
        private int _fgStartx;

        private int _fgStarty;

        // current offset of image
        private int _fgImgx;

        private int _fgImgy;

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

        #region Initial

        public LaserMarker()
        {
            this.InitializeComponent();

            Initial();

            this.foregroundPictureBox.Parent = this.backgroundPictureBox;

            this.foregroundPictureBox.Paint += this.ForegroundImageBox_Paint;
            this.backgroundPictureBox.Paint += this.BackgroundImageBox_Paint;
        }

        private void Initial()
        {
            try
            {
                // Connect sdk
                var err = JczLmc.Initialize(Application.StartupPath, true);

                this.rightpanel.Width = SystemInformation.VirtualScreen.Width / 100 * 30;

                this.panel1.Height = SystemInformation.VirtualScreen.Height
                                         - (SystemInformation.VirtualScreen.Height / 100 * 30);

                var userDataDtos = UserDataRepository.GetAllUser();

                if (userDataDtos.Count > 0)
                {
                    // init top pictures
                    this.InitialTopPictures(userDataDtos);

                    // init current pictures
                    var currentData = userDataDtos.LastOrDefault();

                    this.InitialCurrentData(currentData);

                    this.CreateBgPictureBoxImage();

                    this.CreateEzdPictureBoxImage();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
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

                this.currentPEindex = i;

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

        private void InitialCurrentData(UserDataDto currentData)
        {
            CurrentData.EzdPictureBox = this.foregroundPictureBox;

            this.currentPEindex = (int)currentData.Sequence;

            this.UpdateImageFromDB(currentData);

            CurrentUIData.RightLayoutControl = this.rightLayoutControl;

            CurrentUIData.WindowSize = new Size(
                SystemInformation.VirtualScreen.Width,
                SystemInformation.VirtualScreen.Height);

            CurrentUIData.RightPanelSize = new Size(this.rightpanel.Width, SystemInformation.VirtualScreen.Height);

            this.urlTextEdit.Text = currentData.Login;

            this.passwordTextEdit.Text = currentData.Password;

            this.urlTextEdit.Text = currentData.Url;

            CurrentApiData.Token = currentData.Token;

            // Initial preview
            CurrentData.Preview = new Preview(this.panel1.ToImage()) { StartPosition = FormStartPosition.Manual };

            CurrentData.Preview.Show();
        }

        #endregion

        private void LaserMarker_Load(object sender, EventArgs e)
        {
            try
            {
                Load data = new EzdDataL.Load();

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
        }

        #region Login Event

        private async void LoginBtn_Click(object sender, EventArgs e)
        {
            try
            {
                // Get all events
                var base64HeaderValue = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes($@"{this.loginTextEdit.Text}:{this.passwordTextEdit.Text}"));

                const string Url = @"http://openeventor.ru/api/get_events";

                var task = await Queries.GetAllEventsAsync(Url, base64HeaderValue);

                var result = JsonConvert.DeserializeObject<Eventor>(task);

                CustomFlyoutDialog.ShowForm(this, null, new GetEvents(result));

                this.urlTextEdit.Text = $@"http://openeventor.ru/api/event/{CurrentApiData.Token}/get_event";
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(
                    "Прове  рте логин/пароль или нет доступ к апи, проверьте интернет соединения",
                    "Error",
                    MessageBoxButtons.OK);

                if (CurrentData.EzdImage != null)
                {
                    ezdObjects = EzdDataControl.ReopositoryEzdFile.GetEzdData();

                    updatedEzdObjects = new List<Tuple<string, StringBuilder>>();

                    new UpdateEzdData(ezdObjects);
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
                    if (!string.IsNullOrEmpty(CurrentApiData.Token))
                    {
                        new UpdateEzdDataFromApi(this);
                    }
                    else
                    {
                        ezdObjects = EzdDataControl.ReopositoryEzdFile.GetEzdData();

                        updatedEzdObjects = new List<Tuple<string, StringBuilder>>();

                        new UpdateEzdData(ezdObjects);
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                }
            }
            else
            {
                XtraMessageBox.Show("Пожалуйста, выберите ezd файл", "Error", MessageBoxButtons.OK);
            }
        }

        #endregion

        #region Create image for PictureBox

        private void CreateEzdPictureBoxImage()
        {
            var fg = this.CreateGraphics();

            // Fit width
            this._fgZoom = (this.foregroundPictureBox.Width / (float)CurrentData.EzdImage.Width)
                           * (CurrentData.EzdImage.HorizontalResolution / fg.DpiX);

            this.foregroundPictureBox.Refresh();
        }

        private void CreateBgPictureBoxImage()
        {
            var g = this.CreateGraphics();

            // Fit width
            this._bgZoom = (this.backgroundPictureBox.Width / (float)CurrentData.BgImage.Width)
                           * (CurrentData.BgImage.HorizontalResolution / g.DpiX);

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
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                this.Scale(e, ScaleMode.Plus);
            }
            else if (e.Delta < 0)
            {
                this.Scale(e, ScaleMode.Minus);
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
                    this._bgImgx = (int)(this._bgStartx + (deltaX / this._bgZoom));
                    this._bgImgy = (int)(this._bgStarty + (deltaY / this._bgZoom));

                    this.backgroundPictureBox.Refresh();
                }

                if (fgCheckBox.Checked)
                {
                    // the distance the mouse has been moved since mouse was pressed
                    var deltaX = mousePosNow.X - this._fg_mouseDown.X;
                    var deltaY = mousePosNow.Y - this._fg_mouseDown.Y;

                    // calculate new offset of image based on the current zoom factor
                    this._fgImgx = (int)(this._fgStartx + (deltaX / this._fgZoom));
                    this._fgImgy = (int)(this._fgStarty + (deltaY / this._fgZoom));

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
            e.Graphics.ScaleTransform(this._fgZoom, this._fgZoom);
            e.Graphics.DrawImage(CurrentData.EzdImage, this._fgImgx, this._fgImgy);

            this.PositionLabel.Text =
                $@"ezd X - {this._fgImgx}, Y - {this._fgImgy} | bg X - {this._bgImgx}, Y - {this._bgImgy}";
        }

        private void BackgroundImageBox_Paint(object sender, PaintEventArgs e)
        {
            if (CurrentData.BgImage == null)
            {
                return;
            }

            e.Graphics.InterpolationMode = InterpolationMode.Low;
            e.Graphics.ScaleTransform(this._bgZoom, this._bgZoom);
            e.Graphics.DrawImage(CurrentData.BgImage, this._bgImgx, this._bgImgy);
        }

        #endregion General FG BG Pictures Event

        #region Minus/Plus Event

        private void MunisBtn_Click(object sender, EventArgs e)
        {
            this.Scale(e, ScaleMode.Minus);
        }

        private void PlusBtn_Click(object sender, EventArgs e)
        {
            this.Scale(e, ScaleMode.Plus);
        }

        #endregion Minus/Plus Event

        private void Scale(EventArgs e, ScaleMode scaleMode)
        {
            if (!(e is MouseEventArgs mouse))
            {
                return;
            }

            var mousePosNow = mouse.Location;

            float oldzoom;
            int x;
            int y;
            int oldimagex;
            int oldimagey;
            int newimagex;
            int newimagey;
            if (this.bgCheckBox.Checked)
            {
                oldzoom = this._bgZoom;

                switch (scaleMode)
                {
                    case ScaleMode.Plus:
                        this._bgZoom += this._bgZoom / 100f * 5f;
                        break;
                    case ScaleMode.Minus:
                        this._bgZoom -= this._bgZoom / 100f * 5f;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(scaleMode), scaleMode, null);
                }

                // Where location of the mouse in the pictureframe
                x = mousePosNow.X - this.backgroundPictureBox.Location.X;
                y = mousePosNow.Y - this.backgroundPictureBox.Location.Y;

                // Where in the IMAGE is it now
                oldimagex = (int)(x / oldzoom);
                oldimagey = (int)(y / oldzoom);

                // Where in the IMAGE will it be when the new zoom i made
                newimagex = (int)(x / this._bgZoom);
                newimagey = (int)(y / this._bgZoom);

                // Where to move image to keep focus on one point
                this._bgImgx = newimagex - oldimagex + this._bgImgx;
                this._bgImgy = newimagey - oldimagey + this._bgImgy;

                // calls imageBox_Paint
                this.backgroundPictureBox.Refresh();
            }

            if (!this.fgCheckBox.Checked)
            {
                return;
            }

            oldzoom = this._fgZoom;

            switch (scaleMode)
            {
                case ScaleMode.Plus:
                    this._fgZoom += this._fgZoom / 100f * 5f;
                    break;
                case ScaleMode.Minus:
                    this._fgZoom -= this._fgZoom / 100f * 5f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scaleMode), scaleMode, null);
            }

            x = mousePosNow.X - this.foregroundPictureBox.Location.X;
            y = mousePosNow.Y - this.foregroundPictureBox.Location.Y;

            oldimagex = (int)(x / oldzoom);
            oldimagey = (int)(y / oldzoom);

            newimagex = (int)(x / this._fgZoom);
            newimagey = (int)(y / this._fgZoom);

            // Where to move image to keep focus on one point
            this._fgImgx = newimagex - oldimagex + this._fgImgx;
            this._fgImgy = newimagey - oldimagey + this._fgImgy;

            // calls imageBox_Paint
            this.foregroundPictureBox.Refresh();
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

                                CurrentData.EzdImage = EzdDataControl.ReopositoryEzdFile.LoadImage(
                                    ofd.FileName,
                                    this.foregroundPictureBox.Width,
                                    this.foregroundPictureBox.Height);

                                CurrentData.EzdName = ofd.FileName;

                                this.CreateEzdPictureBoxImage();

                                break;
                            }

                        case UploadType.image when ofd.FileName != null:
                            {
                                this.bgImageLbl.Text = Path.GetFileName(ofd.FileName);

                                CurrentData.BgImage = Image.FromFile(ofd.FileName);

                                CurrentData.BgName = ofd.FileName;

                                this.CreateBgPictureBoxImage();

                                break;
                            }

                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
            }
        }

        #endregion

        #region Delete/Save

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (CurrentData.BgImage != null && CurrentData.EzdImage != null)
            {
                if (this.currentPEindex < 10)
                {
                    CurrentData.FullImageName = $@"FullImage{DateTime.Now.Ticks}.jpg";

                    CurrentData.FullImage = this.panel1.ToImage();

                    this.SaveImageDB();

                    this.UpdateTopPictures();

                    CurrentData.Preview?.Close();

                    // Screen preview
                    if (Screen.AllScreens.Length > 1)
                    {
                        CurrentData.Preview =
                            new Preview(this.panel1.ToImage()) { StartPosition = FormStartPosition.CenterParent };

                        CurrentData.Preview.Show();
                    }
                }
            }
            else
            {
                XtraMessageBox.Show("Выберите обложку или ezd файл", "Error", MessageBoxButtons.OK);
            }
        }

        private void UpdateTopPictures()
        {
            // Top button, Tab index 40 - 49
            var selectedImage = this.layoutControl2.Controls.OfType<PictureEdit>()
                .Where(c => c.TabIndex == this.currentPEindex + 40).Select(c => c).First();

            selectedImage.Image = this.panel1.ToImage();
            selectedImage.Cursor = Cursors.Hand;
            selectedImage.Properties.ReadOnly = false;
            selectedImage.Tag = "filled";

            // next plus
            if (this.currentPEindex < 9)
            {
                var selectedNextImage = this.layoutControl2.Controls.OfType<PictureEdit>()
                    .Where(c => c.TabIndex == (currentPEindex + 1) + 40).Select(c => c).First();

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
                        Sequence = currentPEindex,
                        Login = this.loginTextEdit.Text,
                        Password = this.passwordTextEdit.Text,
                        Url = this.urlTextEdit.Text,
                        BgImageName = CurrentData.BgName,
                        EzdImageName = CurrentData.EzdName,
                        FullImageName = CurrentData.FullImageName,
                        FullImage = CurrentData.FullImage.ToBytes(),
                        BgImagePosX = this._bgImgx,
                        BgImagePosY = this._bgImgy,
                        BgImageScale = this._bgZoom,
                        EzdImageScale = this._fgZoom,
                        EzdImagePosX = this._fgImgx,
                        EzdImagePosY = this._fgImgy
                    });
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            if (CurrentData.BgImage != null || CurrentData.EzdImage != null)
            {
                if (XtraMessageBox.Show("Вы действительно хотите удалить?", "Сообщения", MessageBoxButtons.YesNo)
                    == DialogResult.Yes)
                {
                    CurrentData.Preview?.Close();

                    if (currentPEindex < 10)
                    {
                        UserDataRepository.DeleteByTabIndex(currentPEindex);

                        CurrentData.Clear();

                        var userDataDtos = UserDataRepository.GetAllUser();

                        this.ResetTopPictures();

                        this.InitialTopPictures(userDataDtos);

                        this.bgImageLbl.Text = " ";
                        this.ezdFileLbl.Text = " ";

                        this.backgroundPictureBox.Refresh();
                        this.foregroundPictureBox.Refresh();
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
            var picture = (PictureEdit)sender;

            this.currentPEindex = picture.TabIndex - 40;

            if ((string)picture.Tag == @"filled")
            {
                if (!picture.Properties.ReadOnly)
                {
                    var userDto = UserDataRepository.GetByTabIndex(this.currentPEindex);

                    this.UpdateImageFromDB(userDto);

                    this.foregroundPictureBox.Refresh();

                    this.backgroundPictureBox.Refresh();
                }
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

        private void UpdateImageFromDB(UserDataDto currentData)
        {
            // bg
            if (!File.Exists(currentData.BgImageName))
            {
                XtraMessageBox.Show(@"Подложка не найдено", "Error", MessageBoxButtons.OK);

                return;
            }
            else
            {
                CurrentData.BgImage = Image.FromFile(currentData.BgImageName);

                CurrentData.BgName = currentData.BgImageName;

                this._bgImgx = (int)currentData.BgImagePosX;

                this._bgImgy = (int)currentData.BgImagePosY;

                this._bgZoom = (float)currentData.BgImageScale;
            }

            // Ezd
            if (!File.Exists(currentData.EzdImageName))
            {
                XtraMessageBox.Show(@"Ezd файл не найдено", "Error", MessageBoxButtons.OK);
            }
            else
            {
                CurrentData.EzdImage = CurrentData.EzdImage = EzdDataControl.ReopositoryEzdFile.LoadImage(
                                           currentData.EzdImageName,
                                           this.foregroundPictureBox.Width,
                                           this.foregroundPictureBox.Height);

                CurrentData.EzdName = currentData.EzdImageName;

                this._fgImgx = (int)currentData.EzdImagePosX;

                this._fgImgy = (int)currentData.EzdImagePosY;

                this._fgZoom = (float)currentData.EzdImageScale;
            }
        }
    }
}