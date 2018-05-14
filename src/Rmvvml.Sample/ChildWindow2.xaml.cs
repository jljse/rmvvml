using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Rmvvml.Sample
{
    /// <summary>
    /// ChildWindow2.xaml の相互作用ロジック
    /// </summary>
    public partial class ChildWindow2 : Window
    {
        public ChildWindow2()
        {
            InitializeComponent();
            var template = new DataTemplate();
            
        }
    }

    public class ChildWindow2VM : ViewModelBase, IShowDialog
    {
        public bool IsShowDialog => true;

        public object Owner { get; set; }

        #region OpenCommand
        RelayCommand _OpenCommand;
        public RelayCommand OpenCommand
        {
            get
            {
                return _OpenCommand ?? (_OpenCommand = new RelayCommand(OnOpenCommand));
            }
        }

        void OnOpenCommand()
        {
            App.WindowsControlViewModel.ItemsSource.Add(new ChildWindow2VM());
        }
        #endregion
    }

    [ContentProperty("Test")]
    public class TestClass
    {
        public List<Behavior> Test { get; set; }
    }
}
