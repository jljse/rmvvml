using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.ObjectModel;

namespace Rmvvml.Sample
{
    class ChildWindowVM : ViewModelBase
    {
        public ChildWindowVM()
        {
            MultiSelecterSource.Add(new BoxVM { Text = "111" });
            MultiSelecterSource.Add(new BoxVM { Text = "222" });
            MultiSelecterSource.Add(new BoxVM { Text = "333" });
            MultiSelecterSource.Add(new BoxVM { Text = "444" });
        }

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
        public InjectedViewAction FocusTextBoxAction { get; } = new InjectedViewAction();
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

        #region Check1
        string _Check1;
        [Required]
        public string Check1
        {
            get { return _Check1; }
            set { Set(nameof(Check1), ref _Check1, value); }
        }
        #endregion

        #region Check2
        string _Check2;
        [RegularExpression("[a-z]*")]
        public string Check2
        {
            get { return _Check2; }
            set { Set(nameof(Check2), ref _Check2, value); }
        }
        #endregion

        #region Check3
        string _Check3;

        [MaxLength(5)]
        public string Check3
        {
            get { return _Check3; }
            set { Set(nameof(Check3), ref _Check3, value); }
        }

        #endregion

        #region MultiSelecterSource
        public ObservableCollection<BoxVM> MultiSelecterSource { get; } = new ObservableCollection<BoxVM>();
        #endregion
    }

    class BoxVM : ViewModelBase
    {
        #region Text
        string _Text;
        public string Text
        {
            get { return _Text; }
            set { Set(nameof(Text), ref _Text, value); }
        }
        #endregion

        #region IsSelected
        bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { Set(nameof(IsSelected), ref _IsSelected, value); }
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
