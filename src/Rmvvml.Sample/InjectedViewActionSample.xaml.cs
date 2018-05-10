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
    /// AttachedPropertyActionSample.xaml の相互作用ロジック
    /// </summary>
    public partial class InjectedViewActionSample : Window
    {
        public InjectedViewActionSample()
        {
            InitializeComponent();
        }
    }

    public class InjectedViewActionSampleAtt
    {
        #region MinimizeAction

        public static InjectedViewAction GetMinimizeAction(DependencyObject obj)
        {
            return (InjectedViewAction)obj.GetValue(MinimizeActionProperty);
        }

        public static void SetMinimizeAction(DependencyObject obj, InjectedViewAction value)
        {
            obj.SetValue(MinimizeActionProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinimizeAction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimizeActionProperty =
            DependencyProperty.RegisterAttached("MinimizeAction", typeof(InjectedViewAction), typeof(InjectedViewActionSampleAtt), new PropertyMetadata(null, OnMinimizeActionChanged));

        private static void OnMinimizeActionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InjectedViewActionHandler<Window, object>.Inject(MinimizeActionProperty, d, e, (obj, param) =>
            {
                obj.WindowState = WindowState.Minimized;
            });
        }

        #endregion
    }

    public class InjectedViewActionSampleVM : ViewModelBase
    {
        #region MinimizeAction
        InjectedViewAction _MinimizeAction = new InjectedViewAction();
        public InjectedViewAction MinimizeAction
        {
            get { return _MinimizeAction; }
        }
        #endregion

        #region MinimizeCommand
        RelayCommand _MinimizeCommand;
        public RelayCommand MinimizeCommand
        {
            get
            {
                return _MinimizeCommand ?? (_MinimizeCommand = new RelayCommand(OnMinimizeCommand));
            }
        }
        void OnMinimizeCommand()
        {
            MinimizeAction.Invoke();
        }
        #endregion

        #region MaximizeAction
        InjectedViewAction _MaximizeAction = new InjectedViewAction();
        public InjectedViewAction MaximizeAction
        {
            get { return _MaximizeAction; }
        }
        #endregion

        #region MaximizeCommand
        RelayCommand _MaximizeCommand;
        public RelayCommand MaximizeCommand
        {
            get
            {
                return _MaximizeCommand ?? (_MaximizeCommand = new RelayCommand(OnMaximizeCommand));
            }
        }
        void OnMaximizeCommand()
        {
            MaximizeAction.Invoke();
        }
        #endregion
    }
}
