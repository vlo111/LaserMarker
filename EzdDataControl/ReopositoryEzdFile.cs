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
        public static bool AutoWidthEzd { get; set; }

        private static readonly int EzdWidth;

        private static readonly int Eheight;

        private static readonly int EheightForGetPrev;

        private static readonly double Ex;

        private static readonly double Ey;

        private static readonly double Escale;

        private static readonly string EshowWorkSpace;

        public enum ModeFontSize
        {
            Reduce,
            Zoom
        }

        static ReopositoryEzdFile()
        {
            var filePath = Application.StartupPath + @"\EZCAD.CFG";

            IList<string> lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                var value = line.Substring(line.IndexOf("=", StringComparison.Ordinal) + 1,
                    line.Length - line.IndexOf("=", StringComparison.Ordinal) - 1);//.Replace(".", ",");

                if (line.Contains("WORKSPACEWIDTH"))
                {
                    EzdWidth = int.Parse(value, System.Globalization.NumberStyles.Any);
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

                if (EzdWidth > 0 && Eheight > 0 && Ex > 0 && Ey > 0
                    && !string.IsNullOrEmpty(EshowWorkSpace))
                {
                    break;
                }
            }

            File.WriteAllLines(filePath, lines);

            Escale = 1d / (ScreenSize.PrimaryWidth() / (double) EzdWidth);

            EheightForGetPrev = (int) ((ScreenSize.PrimaryWidth() / (double) EzdWidth) * (double) Eheight);
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

        public static Image UpdateCustomEzd(Tuple<string, string> ezdObj)
        {
            JczLmc.ChangeTextByName(ezdObj.Item1, ezdObj.Item2);

            var img = GetImagePreview();

            return img;
        }

        public static Image UpdateEzdApi(Dictionary<string, string> competitor)
        {
            string ezdObj;

            var entCount = JczLmc.GetEntityCount();

            for (var i = 0; i < entCount; i++)
            {
                ezdObj = JczLmc.GetEntityNameByIndex(i);

                if (competitor.Keys.Any(p => p == ezdObj))
                {
                    if (AutoWidthEzd)
                    {
                        UpdateEzdWithResize(competitor, ezdObj);
                    }
                    else
                    {
                        JczLmc.ChangeTextByName(ezdObj, competitor[ezdObj]);
                    }
                }
                else
                {
                    JczLmc.ChangeTextByName(ezdObj, "");
                }
            }

            var img = GetImagePreview();

            return img;
        }

        private static void UpdateEzdWithResize(Dictionary<string, string> competitor, string ezdObj)
        {
            double minx = 0;
            double miny = 0;

            double maxx = 0;
            double maxy = 0;

            double dz = 0;


            double minx1 = 0;
            double miny1 = 0;

            double maxx1 = 0;
            double maxy1 = 0;

            double dz1 = 0;
            JczLmc.GetEntSize(ezdObj, ref minx, ref miny, ref maxx, ref maxy, ref dz);

            JczLmc.ChangeTextByName(ezdObj, competitor[ezdObj]);

            JczLmc.GetEntSize(ezdObj, ref minx1, ref miny1, ref maxx1, ref maxy1, ref dz1);


            if ((maxx - minx) < (maxx1 - minx1))
            {
                var font = new FontEzd();

                font.InitialData(ezdObj);

                font.width_ezd = font.width_ezd * ((maxx - minx) / (maxx1 - minx1));

                font.UpdateData(ezdObj);
            }
        }

        public static Image LoadAndGetImage(string fileName)
        {
            JczLmc.LoadEzdFile(fileName);

            return GetImagePreview();
        }

        public static Image FontSize(string entName, ModeFontSize mode)
        {
            var font = new FontEzd();

            font.InitialData(entName);

            if (mode == ModeFontSize.Reduce)
            {
                font.height_ezd = font.height_ezd - 0.0052d;
                font.width_ezd = font.width_ezd - 0.0052d;
            }
            else
            {
                font.height_ezd = font.height_ezd + 0.0052d;
                font.width_ezd = font.width_ezd + 0.0052d;
            }

            font.UpdateData(entName);

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

        public static bool CheckSameEntName()
        {
            string ezdObj;

            var entCount = JczLmc.GetEntityCount();

            var listEntName = new List<string>();

            for (var i = 0; i < entCount; i++)
            {
                ezdObj = JczLmc.GetEntityNameByIndex(i);

                listEntName.Add(ezdObj);
            }

            return listEntName.Distinct().Count() != listEntName.Count;
        }
    }
}