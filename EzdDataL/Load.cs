namespace EzdDataL
{
    using Microsoft.Win32;
    using System;
    using System.Configuration;
    using System.IO;

    public class Load
    {
        public Load()
        {

        }

        public bool Go()
        {
            string configvalue1 = ConfigurationManager.AppSettings["Application"];

            if (configvalue1 == null || configvalue1 != "15" || !File.Exists(ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetEntryAssembly().Location).FilePath))
            {
                return false;
            }

            int period = 10;
            string keyName = "Network32";

            RegistryKey rootKey = Registry.CurrentUser;
            RegistryKey regKey = rootKey.OpenSubKey(keyName);

            if (regKey == null)
            {
                regKey = rootKey.CreateSubKey(keyName);
                long expiry = DateTime.Today.AddDays(period).Ticks;
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
                    return false;
                }
            }

            return true;
        }
    }
}
