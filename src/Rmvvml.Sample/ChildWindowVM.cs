using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rmvvml.Sample
{
    class ChildWindowVM : ViewModelBase
    {
        #region CloseChildCommand
        RelayCommand _CloseChildCommand;
        public RelayCommand CloseChildCommand
        {
            get
            {
                return _CloseChildCommand ?? (_CloseChildCommand = new RelayCommand(OnCloseChildCommand));
            }
        }
        void OnCloseChildCommand()
        {
            App.WindowsControlViewModel.ItemsSource.Remove(this);
        }
        #endregion
    }
}
