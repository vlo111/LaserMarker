using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK;

namespace EzdDataControl
{
    public class FontEzd
    {
        public StringBuilder font = new StringBuilder();
        public double height_ezd = 0;
        public double width_ezd = 0;
        public double angle = 0;
        public double space = 0;
        public double line_space = 0;
        public bool bBold = false;
        public int nTextAlign = 0;
        public bool bItalic = false;
        public int nTextSpaceMode = 0;
        public double dTextSpace = 0;
        public double dNullCharWidthRatio = 0;

        public FontEzd()
        {
                
        }

        public void InitialData(string entName)
        {

            int errGetText = JczLmc.GetTextEntParam4(entName,
                font,
                ref nTextSpaceMode,
                ref dTextSpace,
                ref height_ezd,
                ref width_ezd,
                ref angle,
                ref space,
                ref line_space,
                ref dNullCharWidthRatio,
                ref nTextAlign,
                ref bBold,
                ref bItalic);
        }

        public void UpdateData(string entName)
        {
            int errSetText = JczLmc.SetTextEntParam4(entName,
                font.ToString(),
                nTextSpaceMode,
                dTextSpace,
                height_ezd,
                width_ezd,
                angle,
                space,
                line_space,
                dNullCharWidthRatio,
                nTextAlign,
                bBold,
                bItalic);
        }
    }
}
