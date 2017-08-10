using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Rmvvml
{
    /// <summary>
    /// DataContextを使って標準のMessageBoxを表示する
    /// 最初はBindingすることでVMに依存しないように実装しようとしたけどなぜかバインディングが動かなかったので諦めた
    /// 標準のは画面の真ん中に出ちゃうからいまいちなんだよなー
    /// </summary>
    public class MessageBoxWindowHandler : FrameworkElement, IVolatileWindowHandler
    {
        /// <summary>
        /// メッセージボックスの親
        /// </summary>
        public Window Owner { get; set; }

        public void ShowDialog()
        {
            var vm = DataContext as MessageBoxWindowViewModel;
            if (vm == null) return;

            if (Owner != null)
            {
                vm.Result = MessageBox.Show(
                    Owner, vm.Message, vm.Caption, vm.Button, vm.Image, vm.Result, MessageBoxOptions.None);
            }
            else
            {
                vm.Result = MessageBox.Show(
                    vm.Message, vm.Caption, vm.Button, vm.Image, vm.Result, MessageBoxOptions.None);
            }
        }
    }

    /// <summary>
    /// メッセージボックスを表示する際に使用するViewModel
    /// これは標準MessageBox以外に対しても大抵使えると思われる
    /// </summary>
    public class MessageBoxWindowViewModel : ViewModelBase, IShowDialog
    {
        public bool IsShowDialog => true;
        public object Owner { get; set; }

        #region Message
        string _Message = string.Empty;
        public string Message
        {
            get { return _Message; }
            set { Set(nameof(Message), ref _Message, value); }
        }
        #endregion

        #region Caption
        string _Caption = string.Empty;
        public string Caption
        {
            get { return _Caption; }
            set { Set(nameof(Caption), ref _Caption, value); }
        }
        #endregion

        #region Button
        MessageBoxButton _Button = MessageBoxButton.OK;
        public MessageBoxButton Button
        {
            get { return _Button; }
            set { Set(nameof(Button), ref _Button, value); }
        }
        #endregion

        #region Image
        MessageBoxImage _Image = MessageBoxImage.None;
        public MessageBoxImage Image
        {
            get { return _Image; }
            set { Set(nameof(Image), ref _Image, value); }
        }
        #endregion

        #region Result
        MessageBoxResult _Result = MessageBoxResult.None;
        public MessageBoxResult Result
        {
            get { return _Result; }
            set { Set(nameof(Result), ref _Result, value); }
        }
        #endregion
    }


}
