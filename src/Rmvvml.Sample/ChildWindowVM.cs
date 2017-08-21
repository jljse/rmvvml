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

        #region RadioSelectedValue
        RadioValues _RadioSelectedValue;
        public RadioValues RadioSelectedValue
        {
            get { return _RadioSelectedValue; }
            set { Set(nameof(RadioSelectedValue), ref _RadioSelectedValue, value); }
        }
        #endregion

        #region FocusTextBoxAction
        public AttachedPropertyAction FocusTextBoxAction { get; } = new AttachedPropertyAction();
        #endregion

        #region CallButtonCommand
        RelayCommand _CallButtonCommand;
        public RelayCommand CallButtonCommand
        {
            get
            {
                return _CallButtonCommand ?? (_CallButtonCommand = new RelayCommand(OnCallButtonCommand));
            }
        }
        async void OnCallButtonCommand()
        {
            await FocusTextBoxAction.Invoke();
        }
        #endregion
    }

    enum RadioValues
    {
        AAA,
        BBB,
        CCC,
    }
}
