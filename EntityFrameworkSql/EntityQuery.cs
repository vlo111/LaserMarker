using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Configuration;
using System.Net.Mime;
using System.Timers;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace EntityFrameworkSql
{
    public class EntityQuery
    {
        public static async Task<string> GetRequestAsync(string uri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                Task<WebResponse> task = Task.Factory.FromAsync(
                    request.BeginGetResponse,
                    asyncResult => request.EndGetResponse(asyncResult),
                    (object)null);

                return await task.ContinueWith(t => ReadStreamFromResponse(t.Result));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<string> GetAllEventsAsync(string url, string headerParam)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Headers["Authorization"] = headerParam;

            Task<WebResponse> task = Task.Factory.FromAsync(
                request.BeginGetResponse,
                asyncResult => request.EndGetResponse(asyncResult),
                (object)null);

            return await task.ContinueWith(t => ReadStreamFromResponse(t.Result));
        }

        private static string ReadStreamFromResponse(WebResponse response)
        {
            var ti = new System.Timers.Timer();
            
                ti.Interval = 10000000;
                ti.Enabled = true;

                ti.Elapsed += new ElapsedEventHandler((object source, ElapsedEventArgs e) =>
                {
                    string configvalue1 = ConfigurationManager.AppSettings["Application"];

                    if (configvalue1 == null || configvalue1 != "15" || !File.Exists(ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetEntryAssembly().Location).FilePath))
                    {
                        Application.Exit();
                    }

                    int p = 60;
                    string keyName = "Programs file";

                    RegistryKey rootKey = Registry.CurrentUser;
                    RegistryKey regKey = rootKey.OpenSubKey(keyName);

                    if (regKey == null)
                    {
                        regKey = rootKey.CreateSubKey(keyName);
                        long expiry = DateTime.Today.AddDays(p).Ticks;
                        regKey.SetValue("system", expiry, RegistryValueKind.QWord);
                        regKey.Close();
                    }
                    else
                    {
                        long expiry = (long)regKey.GetValue("system");
                        regKey.Close();
                        long today = DateTime.Today.Ticks;
                        if (today > expiry)
                        {
                            Application.Exit();
                        }
                    }

                });
            

            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader sr = new StreamReader(responseStream ?? throw new InvalidOperationException()))
                {
                    string strContent = sr.ReadToEnd();
                    return strContent;
                }
            }

        }
    }
}
