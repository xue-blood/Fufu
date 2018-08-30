using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
    }
}