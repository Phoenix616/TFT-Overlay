using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using TFT_Overlay.Properties;

namespace TFT_Overlay
{
    public partial class App : Application
    {
        private void AutoUpdater(object sender, StartupEventArgs e) {


            if (!checkAndUpdate("Phoenix616", "fork", "A new fork update is available.\nWould you like to download V%version%?"))
            {
                checkAndUpdate("Just2good", "master", "A new official update is available.\nWould you like to download V%version%?\nThis will revert fork changes but include new features!");
            }
        }

        private bool checkAndUpdate(string user, string branchPath, string updateMessage) {
            string versionExtra = "";
            if (!branchPath.Equals("master")) {
                versionExtra = "-" + branchPath;
            }
            string currentVersion = Version.version;

            using (WebClient client = new WebClient())
            {
                try
                {
                    string htmlCode = client.DownloadString($"https://raw.githubusercontent.com/{user}/TFT-Overlay/{branchPath}/Version.cs");
                    int versionFind = htmlCode.IndexOf("public static string version = ");
                    string version = htmlCode.Substring(versionFind + 32, 5);
                    if (compareSemanticVersion(currentVersion, version) > 0 && Settings.Default.AutoUpdate)
                    {
                        var result = MessageBox.Show(updateMessage.Replace("%version%", version), "TFT Overlay Update Available", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            string link = $"https://github.com/{user}/TFT-Overlay/releases/download/V" + version + versionExtra + "/TFT.Overlay.V" + version + versionExtra + ".zip";
                            ServicePointManager.Expect100Continue = true;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            client.DownloadFile(new Uri(link), "TFTOverlay.zip");

                            var res = MessageBox.Show("The zip file was downloaded to your local directory, please extract and use the updated version instead.\nDo you want to open your local directory?",
                                "Success", MessageBoxButton.YesNo, MessageBoxImage.Information);
                            if (res == MessageBoxResult.Yes)
                            {
                                Process.Start(Directory.GetCurrentDirectory());
                            }
                            return true;
                        }
                        else if (result == MessageBoxResult.No && branchPath.Length > 0)
                        {
                            Settings.FindAndUpdate("AutoUpdate", false);
                        }
                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex);
                    MessageBox.Show(ex.ToString(), "An error occured", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return false;
        }

        private int compareSemanticVersion(string currentVersion, string version) {
            int[] curVer = parseSemanticVersion(currentVersion);
            int[] ver = parseSemanticVersion(version);

            for (int i = 0; i < curVer.Length && i < ver.Length; i++) {
                if (curVer[i] < ver[i]) {
                    return 1;
                } else if (curVer[i] > ver[i]) {
                    return -1;
                }
            }

            if (curVer.Length < ver.Length) {
                return 1;
            } else if (curVer.Length > ver.Length) {
                return -1;
            }
            return 0;
        }

        private int[] parseSemanticVersion(string version) {
            string[] split = version.Split('.');
            int[] ver = new int[split.Length];
            for (int i = 0; i < ver.Length; i++) {
                if (!int.TryParse(split[i], out ver[i])) {
                    ver[i] = 0;
                }
            }
            return ver;
        }
    }
}