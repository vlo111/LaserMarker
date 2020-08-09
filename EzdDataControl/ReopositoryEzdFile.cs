namespace EzdDataControl
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using SDK;
    using BLL;

    public static class ReopositoryEzdFile
    {
        private static readonly int ezdWidth;

        private static readonly int Eheight;

        private static readonly int EheightForGetPrev;

        private static readonly double Ex;

        private static readonly double Ey;

        private static readonly double Escale;

        private static readonly string EshowWorkSpace;

        static ReopositoryEzdFile()
        {
            var filePath = Application.StartupPath + @"\EZCAD.CFG";

            IList<string> lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                var value = line.Substring(line.IndexOf("=", StringComparison.Ordinal) + 1,
                    line.Length - line.IndexOf("=", StringComparison.Ordinal) - 1);
                //.Replace(".", ",");

                if (line.Contains("WORKSPACEWIDTH"))
                {
                    ezdWidth = int.Parse(value, System.Globalization.NumberStyles.Any);
                }
                else if (line.Contains("WORKSPACEHEIGHT"))
                {
                    Eheight = int.Parse(value, System.Globalization.NumberStyles.Any);
                }
                else if (line.Contains("LBCORNERX"))
                {
                    Ex = int.Parse(value, System.Globalization.NumberStyles.Any);
                }
                else if (line.Contains("LBCORNERY"))
                {
                    Ey = int.Parse(value, System.Globalization.NumberStyles.Any);
                }
                else if (line.Contains("SHOWWORKSPACE"))
                {
                    EshowWorkSpace = "0";
                    lines[i] = $@"SHOWWORKSPACE={EshowWorkSpace}";
                }

                if (ezdWidth > 0 && Eheight > 0 && Ex > 0 && Ey > 0
                    && !string.IsNullOrEmpty(EshowWorkSpace))
                {
                    break;
                }
            }

            File.WriteAllLines(filePath, lines);

            Escale = 1d / (ScreenSize.PrimaryWidth() / (double) ezdWidth);

            EheightForGetPrev = (int) ((ScreenSize.PrimaryWidth() / (double) ezdWidth) * (double) Eheight);
        }

        public static string Initialize(string path, bool mode)
        {
            var errMessage = JczLmc.Initialize(path, mode);

            if (errMessage == 0)
            {
                return string.Empty;
            }
            else if (errMessage == 1)
            {
                return "Now have a working EZCAD";
            }
            else if (errMessage > 1)
            {
                JczLmc.Close();

                JczLmc.Initialize(path, !mode);

                return string.Empty;
            }

            return string.Empty;
        }

        public static void LoadImage(string fileName)
        {
            JczLmc.ClearLibAllEntity();

            // load ezd
            JczLmc.LoadEzdFile(fileName);
        }

        public static List<Tuple<string, StringBuilder>> GetEzdData()
        {
            var count = JczLmc.GetEntityCount();

            var ezdObjects = new List<Tuple<string, StringBuilder>>();

            var names = new List<string>();

            for (int i = 0; i < count; i++)
            {
                names.Add(JczLmc.GetEntityNameByIndex(i));
            }

            names = names.Where(p => { return !string.IsNullOrEmpty(p); }).Distinct().ToList();

            foreach (var name in names)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    var str = new StringBuilder();

                    JczLmc.GetTextByName(name, str);

                    ezdObjects.Add(new Tuple<string, StringBuilder>(name, str));
                }
            }

            return ezdObjects;
        }

        public static Image UpdateCustomEzd(Tuple<string, string> ezdObj, int width, int height)
        {
            JczLmc.ChangeTextByName(ezdObj.Item1, ezdObj.Item2);

            var img = GetImagePreview();

            return img;
        }

        public static Image UpdateEzdApi(Dictionary<string, string> competitor, int width, int height)
        {
            string ezdObj;

            for (var i = 0; i < JczLmc.GetEntityCount(); i++)
            {
                ezdObj = JczLmc.GetEntityNameByIndex(i);

                JczLmc.ChangeTextByName(ezdObj,
                    competitor.Keys
                        .Any(p => p == ezdObj) ? competitor[ezdObj] : "");
            }

            var img = GetImagePreview();

            return img;
        }

        public static Image LoadAndGetImage(string fileName)
        {
            JczLmc.LoadEzdFile(fileName);

            return GetImagePreview();
        }

        public static Image FontSize(string entName, ModeFontSize mode, int width, int heght)
        {
            StringBuilder font = new StringBuilder();
            double height_ezd = 0;
            double width_ezd = 0;
            double angle = 0;
            double space = 0;
            double line_space = 0;
            bool bBold = false;
            int nTextAlign = 0;
            bool bItalic = false;

            int nTextSpaceMode = 0;
            double dTextSpace = 0;
            double dNullCharWidthRatio = 0;

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

            if (mode == ModeFontSize.Reduce)
            {
                height_ezd = height_ezd - 0.0052d;
                width_ezd = width_ezd - 0.0052d;
            }
            else
            {
                height_ezd = height_ezd + 0.0052d;
                width_ezd = width_ezd + 0.0052d;
            }


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

            return GetImagePreview();
        }

        public static Image GetImagePreview()
        {
            var image = JczLmc.GetCurPreviewImage3(
                ScreenSize.PrimaryWidth(),
                EheightForGetPrev,
                Ex,
                Ey,
                Escale);

            return Images.MakeImageTransparent(image);
        }

        public static int Mark()
        {
            return JczLmc.Mark(false);
        }

        public static bool IsMarking()
        {
            return JczLmc.IsMarking();
        }

        public static void StopMark()
        {
            int nErr = JczLmc.StopMark();
        }

        public static void RedMarkContour()
        {
            int nErr = JczLmc.RedMarkContour();
        }

        public static void RedMark()
        {
            int nErr = JczLmc.RedMark();
        }

        public enum ModeFontSize
        {
            Reduce,
            Zoom
        }
    }
}