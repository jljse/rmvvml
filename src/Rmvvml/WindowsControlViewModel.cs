using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Rmvvml
{
    /// <summary>
    /// ViewModelのルート
    /// </summary>
    public class WindowsControlViewModel : ViewModelBase
    {
        #region ItemsSource
        ObservableCollection<ViewModelBase> _ItemsSource = new ObservableCollection<ViewModelBase>();
        public ObservableCollection<ViewModelBase> ItemsSource
        {
            get { return _ItemsSource; }
            set { Set(nameof(ItemsSource), ref _ItemsSource, value); }
        }
        #endregion

        public MessageBoxResult ShowMessage(MessageBoxWindowViewModel vm)
        {
            ItemsSource.Add(vm);
            return vm.Result;
        }

        public async Task<MessageBoxResult> ShowMessageAsync(MessageBoxWindowViewModel vm)
        {
            return await Task.Run<MessageBoxResult>(() =>
            {
                ItemsSource.Add(vm);
                return vm.Result;
            });
        }
    }

}
