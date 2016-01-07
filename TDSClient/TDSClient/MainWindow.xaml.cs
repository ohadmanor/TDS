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
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace TDSClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        VMMainViewModel MainViewModel = VMMainViewModel.Instance;
        public MainWindow()
        {
            InitializeComponent();

            MainViewModel.MyMainMap = MainMap;
         //   MainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
            DataContext = MainViewModel;
        }


        private void sliderExClockRatio_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int  ExClockRatioSpeed = 0;

            switch ((int)e.NewValue)
            {
                case 0:
                    ExClockRatioSpeed = 1;
                    break;
                case 1:
                    ExClockRatioSpeed = 2;
                    break;

                case 2:
                    ExClockRatioSpeed = 6;
                    break;

                case 3:
                    ExClockRatioSpeed = 12;
                    break;

                case 4:
                    ExClockRatioSpeed = 30;
                    break;

                case 5:
                    ExClockRatioSpeed = 60;
                    break;

                case 6:
                    ExClockRatioSpeed = 0;
                    break;
                default:
                    ExClockRatioSpeed = 0;
                    break;
            }
            if ((int)e.NewValue != (int)e.OldValue )
            {
              VMMainViewModel.Instance.SetExClockRatioSpeed(ExClockRatioSpeed);
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await MainViewModel.Window_Loaded();
        }
    }
}
