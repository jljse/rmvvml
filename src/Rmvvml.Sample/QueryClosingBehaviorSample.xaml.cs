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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rmvvml.Sample
{
    /// <summary>
    /// QueryClosingBehaviorSample.xaml の相互作用ロジック
    /// </summary>
    public partial class QueryClosingBehaviorSample : Window
    {
        public QueryClosingBehaviorSample()
        {
            InitializeComponent();
        }
    }

    public class QueryClosingBehaviorSampleVM : ViewModelBase
    {
        #region IsClosingAccepted
        bool _IsClosingAccepted;
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
        void OnQueryClosingCommand()
        {
            var result = App.WindowsControlViewModel.ShowMessage(new MessageBoxWindowViewModel
            {
                Message = "Will you close this window?",
                Button = MessageBoxButton.OKCancel,
            });
            if (result == MessageBoxResult.OK)
            {
                IsClosingAccepted = true;
                App.WindowsControlViewModel.ItemsSource.Remove(this);
            }
        }
        #endregion

        #region CloseCommand
        RelayCommand _CloseCommand;
        public RelayCommand CloseCommand
        {
            get
            {
                return _CloseCommand ?? (_CloseCommand = new RelayCommand(OnCloseCommand));
            }
        }
        void OnCloseCommand()
        {
            // ダサいけど、IsClosingAcceptedがtrueなら～の分岐がView側にあるので仕方ない
            if (IsClosingAccepted)
            {
                ForceCloseCommand.Execute(null);
            }
            else
            {
                QueryClosingCommand.Execute(null);
            }
        }
        #endregion

        #region ForceCloseCommand
        RelayCommand _ForceCloseCommand;
        public RelayCommand ForceCloseCommand
        {
            get
            {
                return _ForceCloseCommand ?? (_ForceCloseCommand = new RelayCommand(OnForceCloseCommand));
            }
        }
        void OnForceCloseCommand()
        {
            IsClosingAccepted = true;
            App.WindowsControlViewModel.ItemsSource.Remove(this);
        }
        #endregion
    }
}
