using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rmvvml.Sample
{
    /// <summary>
    /// MultiSelectorAttSample.xaml の相互作用ロジック
    /// </summary>
    public partial class MultiSelectorAttSample : Window
    {
        public MultiSelectorAttSample()
        {
            InitializeComponent();
        }
    }

    public class MultiSelectorAttSampleVM : ViewModelBase
    {
        public MultiSelectorAttSampleVM()
        {
            for(int i = 0; i < 20; ++i)
            {
                ItemsSource.Add(new MultiSelectorAttSampleItemVM() { Name = string.Format("item{0}", i) });
            }
        }

        #region ItemsSource
        ObservableCollection<MultiSelectorAttSampleItemVM> _ItemsSource = new ObservableCollection<MultiSelectorAttSampleItemVM>();
        public ObservableCollection<MultiSelectorAttSampleItemVM> ItemsSource
        {
            get { return _ItemsSource; }
            set { Set(nameof(ItemsSource), ref _ItemsSource, value); }
        }
        #endregion

        #region SelectedItems
        ObservableCollection<MultiSelectorAttSampleItemVM> _SelectedItems = new ObservableCollection<MultiSelectorAttSampleItemVM>();
        public ObservableCollection<MultiSelectorAttSampleItemVM> SelectedItems
        {
            get { return _SelectedItems; }
            set { Set(nameof(SelectedItems), ref _SelectedItems, value); }
        }
        #endregion

        #region RemoveCommand
        RelayCommand<MultiSelectorAttSampleItemVM> _RemoveCommand;
        public RelayCommand<MultiSelectorAttSampleItemVM> RemoveCommand
        {
            get
            {
                return _RemoveCommand ?? (_RemoveCommand = new RelayCommand<MultiSelectorAttSampleItemVM>(OnRemoveCommand));
            }
        }
        void OnRemoveCommand(MultiSelectorAttSampleItemVM parameter)
        {
            SelectedItems.Remove(parameter);
        }
        #endregion
    }

    public class MultiSelectorAttSampleItemVM : ViewModelBase
    {
        #region Name
        string _Name;
        public string Name
        {
            get { return _Name; }
            set { Set(nameof(Name), ref _Name, value); }
        }
        #endregion
    }
}
