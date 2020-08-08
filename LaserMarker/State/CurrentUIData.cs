using System.Windows.Forms;

namespace LaserMarker.State
{
    using System.Drawing;

    using DevExpress.XtraEditors;
    using DevExpress.XtraLayout;

    public class CurrentUIData
    {
        public static LayoutControl RightLayoutControl { get; set; }

        public static TextEdit BibText { get; set; }

        public static Size RightPanelSize { get; set; }

        public static Panel PanelImages { get; set; }
    }
}
