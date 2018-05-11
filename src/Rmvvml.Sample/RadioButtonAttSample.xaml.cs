using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// RadioButtonAttSample.xaml の相互作用ロジック
    /// </summary>
    public partial class RadioButtonAttSample : Window
    {
        public RadioButtonAttSample()
        {
            InitializeComponent();
        }
    }


    public enum Test1Enum
    {
        ValA,
        ValB,
    }

    public enum Test2Enum
    {
        ValX,
        ValY,
        ValZ,
    }

    public class RadioButtonAttSampleVM : ViewModelBase
    {
        #region Test1
        Test1Enum? _Test1;
        public Test1Enum? Test1
        {
            get { return _Test1; }
            set { Set(nameof(Test1), ref _Test1, value); }
        }
        #endregion

        #region Test2
        Test2Enum _Test2;
        public Test2Enum Test2
        {
            get { return _Test2; }
            set { Set(nameof(Test2), ref _Test2, value); }
        }
        #endregion

        #region Test1Command
        RelayCommand _Test1Command;
        public RelayCommand Test1Command
        {
            get
            {
                return _Test1Command ?? (_Test1Command = new RelayCommand(OnTest1CommandExecuted));
            }
        }
        void OnTest1CommandExecuted()
        {
            if (Test1 == null)
            {
                Test1 = Test1Enum.ValA;
            }
            else if (Test1 == Test1Enum.ValA)
            {
                Test1 = Test1Enum.ValB;
            }
            else
            {
                Test1 = null;
            }
        }
        #endregion

        #region Test2Command
        RelayCommand _Test2Command;
        public RelayCommand Test2Command
        {
            get
            {
                return _Test2Command ?? (_Test2Command = new RelayCommand(OnTest2Command));
            }
        }
        void OnTest2Command()
        {
            switch(Test2)
            {
                case Test2Enum.ValX:
                    Test2 = Test2Enum.ValY; break;
                case Test2Enum.ValY:
                    Test2 = Test2Enum.ValZ; break;
                case Test2Enum.ValZ:
                    Test2 = Test2Enum.ValX; break;
            }
        }
        #endregion
    }

}
