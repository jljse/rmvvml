using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rmvvml.Sample
{
    class MainWindowVM : ViewModelBase
    {
        public MainWindowVM()
        {
            for(var i = 0; i < 100; ++i)
            {
                DataGridSource.Add(new ItemVM());
            }
        }

        #region ShowChildCommand
        RelayCommand _ShowChildCommand;
        public RelayCommand ShowChildCommand
        {
            get
            {
                return _ShowChildCommand ?? (_ShowChildCommand = new RelayCommand(OnShowChildCommand));
            }
        }
        void OnShowChildCommand()
        {
            App.WindowsControlViewModel.ItemsSource.Add(new ChildWindowVM());
        }
        #endregion

        #region IsClosingAccepted
        bool _IsClosingAccepted = false;
        public bool IsClosingAccepted
        {
            get { return _IsClosingAccepted; }
            set { Set(nameof(IsClosingAccepted), ref _IsClosingAccepted, value); }
        }
        #endregion

        #region QueryClosingCommand
        RelayCommand _QueryClosingCommand;
        public RelayCommand QueryClosingCommand
        {
            get
            {
                return _QueryClosingCommand ?? (_QueryClosingCommand = new RelayCommand(OnQueryClosingCommand));
            }
        }
        async void OnQueryClosingCommand()
        {
            // 確認ダイアログ
            var result = await App.WindowsControlViewModel.ShowMessageAsync(
                new MessageBoxWindowViewModel { Caption = "msg", Message = "are you sure?", Button = System.Windows.MessageBoxButton.YesNo });
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                // QueryClosingBehaviorを使う場合、必ずVM側から能動的に閉じる流れになる
                IsClosingAccepted = true;
                App.WindowsControlViewModel.ItemsSource.Remove(this);
            }
        }
        #endregion

        #region DataGridSource
        public ObservableCollection<ItemVM> DataGridSource { get; } = new ObservableCollection<ItemVM>();
        #endregion
    }

    class ItemVM : ViewModelBase
    {

    }
}
