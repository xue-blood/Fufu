using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

using Prop = WpfApp1.Properties;

namespace WpfApp1 {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private void Application_DispatcherUnhandledException ( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {
            // fix WPF Clipboard exception
            // https://stackoverflow.com/questions/12769264/openclipboard-failed-when-copy-pasting-data-from-wpf-datagrid
            var comException = e.Exception as System.Runtime.InteropServices.COMException;
            if (comException != null && comException.ErrorCode == -2147221040)
                e.Handled = true;
            else
                MessageBox.Show (e.Exception.Message);
            e.Handled = true;
        }

        public class AppRun {
            [STAThread]
            static void Main () {
                try {
                    UpdateVersion ();
                    App.Main ();

                    // 删除先前的
                    delLast ();
                }
                catch (Exception _e) { MessageBox.Show (_e.Message + "\n"); }
            }

            static void delLast () {
                var args = Environment.GetCommandLineArgs ();
                if (args.Length == 2) {
                    if (File.Exists (args[1]) && args[1] != Assembly.GetExecutingAssembly ().Location)
                        File.Delete (args[1]);
                }
            }

            public static void UpdateVersion ( bool manul = false ) {
                var bgw = new BackgroundWorker ();
                bgw.DoWork += ( _, __ ) => {

                    using (var client = new HttpClient ()) {
                        var response = client.GetAsync (Prop.Resources.VersionUrl).Result;
                        if (response.IsSuccessStatusCode) {
                            // by calling .Result you are synchronously reading the result
                            string responseString = response.Content.ReadAsStringAsync ().Result; Debug.WriteLine (responseString);
                            string[] ver = responseString.Split ('|');

                            // 比较版本
                            if (ver.Length == 3 && !string.IsNullOrEmpty (ver[0]) && !string.IsNullOrEmpty (ver[1]) && !string.IsNullOrEmpty (ver[2]) &&
                                ver[0] != Prop.Resources.Version) {
                                // 是否忽略的版本
                                if (ver[0] == Prop.Settings.Default.VersionSkip && !manul) return;

                                Application.Current.Dispatcher.Invoke (() => {
                                    var r = MessageBox.Show (ver[1] + "\n\n\n是否更新，点击 “取消” 忽略这个版本", "发现新版本 " + ver[0], MessageBoxButton.YesNoCancel);
                                    if (r == MessageBoxResult.Yes) doUpdate (ver[2].Trim (), ver[0]);
                                    else if (r == MessageBoxResult.Cancel) doSkip (ver[0]);
                                });
                            }
                        }
                    }
                };
                bgw.RunWorkerAsync ();
            }

            /// <summary>
            /// 更新
            /// </summary>
            static void doUpdate ( string url, string ver ) {
                var bgw = new BackgroundWorker ();
                bgw.DoWork += ( _, __ ) => {
                    // https error
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (var client = new WebClient ()) {
                        client.DownloadFile (url, Prop.Resources.ReleaseName + ver + ".exe");
                    }
                };

                bgw.RunWorkerCompleted += ( _, __ ) => {
                    if (MessageBox.Show ("更新成功 是否重新启动", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK) {

                        var dir = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);
                        System.Diagnostics.Process.Start (dir + "/" + Prop.Resources.ReleaseName + ver + ".exe", Assembly.GetExecutingAssembly ().Location);
                        Application.Current.Shutdown ();
                    }
                };
                bgw.RunWorkerAsync ();
            }

            /// <summary>
            /// 忽略这个版本
            /// </summary>
            static void doSkip ( string ver ) {
                Prop.Settings.Default.VersionSkip = ver;
            }
        }
    }
}
