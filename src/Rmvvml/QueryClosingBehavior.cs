using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Rmvvml
{
    /// <summary>
    /// Closingのキャンセルを制御するためのBehavior
    /// Windowコントロール契機のClosingをキャンセルし、ViewModel契機でのみCloseが発生するようにする
    /// </summary>
    public class QueryClosingBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Closing += AssociatedObject_Closing;
        }

        private void AssociatedObject_Closing(object sender, CancelEventArgs e)
        {
            if(QueryClosingCommand == null || !QueryClosingCommand.CanExecute(null))
            {
                return;
            }

            if(!IsClosingAccepted)
            {
                e.Cancel = true;

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // 出直してきて
                    QueryClosingCommand.Execute(null);
                }));
            }
        }

        #region IsClosingAccepted
        public bool IsClosingAccepted
        {
            get { return (bool)GetValue(IsClosingAcceptedProperty); }
            set { SetValue(IsClosingAcceptedProperty, value); }
        }

        /// <summary>
        /// ウィンドウを閉じられる場合trueにします
        /// </summary>
        public static readonly DependencyProperty IsClosingAcceptedProperty =
            DependencyProperty.Register("IsClosingAccepted", typeof(bool), typeof(QueryClosingBehavior), new PropertyMetadata(false));
        #endregion

        #region QueryClosingCommand

        public ICommand QueryClosingCommand
        {
            get { return (ICommand)GetValue(QueryClosingCommandProperty); }
            set { SetValue(QueryClosingCommandProperty, value); }
        }

        /// <summary>
        /// ウィンドウを閉じる前に確認が必要な場合、Closing時に呼び出す処理を設定します
        /// </summary>
        public static readonly DependencyProperty QueryClosingCommandProperty =
            DependencyProperty.Register("QueryClosingCommand", typeof(ICommand), typeof(QueryClosingBehavior), new PropertyMetadata(null));

        #endregion

    }
}
