using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Rmvvml.Sample
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public static WindowsControlViewModel WindowsControlViewModel { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var windowsControl = FindResource("WindowsControl") as WindowsControl;
            WindowsControlViewModel = windowsControl.DataContext as WindowsControlViewModel;

            WindowsControlViewModel.ItemsSource.Add(new MainWindowVM());
        }
    }
}
