﻿using GalaSoft.MvvmLight;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rmvvml.Sample
{
    /// <summary>
    /// WindowsControlSample.xaml の相互作用ロジック
    /// </summary>
    public partial class WindowsControlSample : Window
    {
        public WindowsControlSample()
        {
            InitializeComponent();
        }
    }

    public class WindowsControlSampleVM : ViewModelBase
    {
        #region WindowsControlCommand
        RelayCommand _WindowsControlCommand;
        public RelayCommand WindowsControlCommand
        {
            get
            {
                return _WindowsControlCommand ?? (_WindowsControlCommand = new RelayCommand(OnWindowsControlCommand));
            }
        }
        void OnWindowsControlCommand()
        {
            App.WindowsControlViewModel.ItemsSource.Add(new WindowsControlSampleChildVM() { Owner = this });
        }
        #endregion

        #region MessageBoxCommand
        RelayCommand _MessageBoxCommand;
        public RelayCommand MessageBoxCommand
        {
            get
            {
                return _MessageBoxCommand ?? (_MessageBoxCommand = new RelayCommand(OnMessageBoxCommand));
            }
        }
        async void OnMessageBoxCommand()
        {
            var result = await App.WindowsControlViewModel.ShowMessageAsync(new MessageBoxWindowViewModel()
            {
                Message = "message",
                Button = MessageBoxButton.YesNoCancel,
            });
            App.WindowsControlViewModel.ShowMessage(new MessageBoxWindowViewModel()
            {
                Message = string.Format("{0}", result),
            });
        }
        #endregion

        #region QueryClosingBehaviorCommand
        RelayCommand _QueryClosingBehaviorCommand;
        public RelayCommand QueryClosingBehaviorCommand
        {
            get
            {
                return _QueryClosingBehaviorCommand ?? (_QueryClosingBehaviorCommand = new RelayCommand(OnQueryClosingBehaviorCommand));
            }
        }
        void OnQueryClosingBehaviorCommand()
        {
            App.WindowsControlViewModel.ItemsSource.Add(new QueryClosingBehaviorSampleVM());
        }
        #endregion

        #region MultiSelectorAttCommand
        RelayCommand _MultiSelectorAttCommand;
        public RelayCommand MultiSelectorAttCommand
        {
            get
            {
                return _MultiSelectorAttCommand ?? (_MultiSelectorAttCommand = new RelayCommand(OnMultiSelectorAttCommand));
            }
        }
        void OnMultiSelectorAttCommand()
        {
            App.WindowsControlViewModel.ItemsSource.Add(new MultiSelectorAttSampleVM());
        }
        #endregion

        #region InjectedViewActionCommand
        RelayCommand _InjectedViewActionCommand;
        public RelayCommand InjectedViewActionCommand
        {
            get
            {
                return _InjectedViewActionCommand ?? (_InjectedViewActionCommand = new RelayCommand(OnInjectedViewActionCommand));
            }
        }
        void OnInjectedViewActionCommand()
        {
            App.WindowsControlViewModel.ItemsSource.Add(new InjectedViewActionSampleVM());
        }
        #endregion

        #region RadioButtonAttCommand
        RelayCommand _RadioButtonAttCommand;
        public RelayCommand RadioButtonAttCommand
        {
            get
            {
                return _RadioButtonAttCommand ?? (_RadioButtonAttCommand = new RelayCommand(OnRadioButtonAttCommand));
            }
        }
        void OnRadioButtonAttCommand()
        {
            App.WindowsControlViewModel.ItemsSource.Add(new RadioButtonAttSampleVM());
        }
        #endregion

        #region ChildWindow2Command
        RelayCommand _ChildWindow2Command;
        public RelayCommand ChildWindow2Command
        {
            get
            {
                return _ChildWindow2Command ?? (_ChildWindow2Command = new RelayCommand(OnChildWindow2Command));
            }
        }
        void OnChildWindow2Command()
        {
            App.WindowsControlViewModel.ItemsSource.Add(new ChildWindow2VM());
        }
        #endregion
    }

    class WindowsControlSampleChildVM : ViewModelBase, IShowDialog
    {
        public bool IsShowDialog => new Random().Next() % 2 == 0;

        public object Owner { get; set; }
    }
}
