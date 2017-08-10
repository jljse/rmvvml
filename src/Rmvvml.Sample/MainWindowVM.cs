using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rmvvml.Sample
{
    class MainWindowVM : ViewModelBase
    {
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
    }
}
