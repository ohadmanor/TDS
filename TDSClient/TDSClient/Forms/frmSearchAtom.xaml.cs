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
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using TDSClient.SAGInterface;

namespace TDSClient.Forms
{
    /// <summary>
    /// Interaction logic for frmSearchAtom.xaml
    /// </summary>
    public partial class frmSearchAtom : Window
    {
        int PixelX = 0;
        int PixelY = 0;
        double fx = 0;
        double fy = 0;
        bool bFound;

        public frmSearchAtom(string guid)
        {
            InitializeComponent();
            this.Background = new LinearGradientBrush(Colors.AliceBlue, Colors.LightGray, 90);
            buildAtomComboBox();

            comboBoxAtom.SelectedValue = guid;
        }

        private void buildAtomComboBox()
        {
            List<structTransportCommonProperty> AtomsList = new List<structTransportCommonProperty>();

            foreach (structTransportCommonProperty Tr in VMMainViewModel.Instance.colGroundAtoms.Values) 
            {
                AtomsList.Add(Tr);
            }
            comboBoxAtom.ItemsSource = null;
            comboBoxAtom.ItemsSource = AtomsList;
            comboBoxAtom.DisplayMemberPath = "AtomName";
            comboBoxAtom.SelectedValuePath = "GUID";

            comboBoxAtom.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("AtomName", System.ComponentModel.ListSortDirection.Ascending));    
        }

        private void Find()
        {
            bFound = false;
          // string guid= comboBoxAtom.SelectedValue as string;

           string name = comboBoxAtom.Text as string;

        //   if (guid == null) return;
           structTransportCommonProperty Tr=null;
           VMMainViewModel.Instance.colGroundAtoms.TryGetValue(name, out Tr);
           if (Tr != null)
           {
               VMMainViewModel.Instance.ConvertCoordGroundToPixel(Tr.X, Tr.Y, ref PixelX, ref PixelY);
               bFound = true;
               fx = Tr.X;
               fy = Tr.Y;
               VMMainViewModel.Instance.InvalidateVisual();
           }


        }
        private void UserDrawWPF(DrawingContext dc)
        {
            if (bFound == true)
            {
                VMMainViewModel.Instance.ConvertCoordGroundToPixel(fx, fy, ref PixelX, ref PixelY);
                RectAnimationUsingKeyFrames n = new RectAnimationUsingKeyFrames();
                n.Duration = TimeSpan.FromMilliseconds(700);
                n.RepeatBehavior = RepeatBehavior.Forever;
                n.AutoReverse = true;
                n.KeyFrames.Add(
                new LinearRectKeyFrame(
                    new Rect(new Point(PixelX - 40, PixelY - 40), new Size(80, 80)), // Target value (KeyValue)
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(350))) // KeyTime
                );

                n.KeyFrames.Add(
                new LinearRectKeyFrame(
                    new Rect(new Point(PixelX - 25, PixelY - 25), new Size(50, 50)), // Target value (KeyValue)
                    KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(700))) // KeyTime
                );

                Rect rect = new Rect(new Point(PixelX - 25, PixelY - 25), new Size(50, 50));
                AnimationClock myClock = n.CreateClock();


                BitmapImage b = new BitmapImage();
                b.BeginInit();
                b.UriSource = new Uri("pack://application:,,,/images/Sight.png");
                b.EndInit();



                dc.DrawImage(b, rect, myClock);

              

            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VMMainViewModel.Instance.dlgUserDrawWPF += UserDrawWPF;           
            VMMainViewModel.Instance.InvalidateVisual();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            VMMainViewModel.Instance.dlgUserDrawWPF -= UserDrawWPF;
            VMMainViewModel.Instance.InvalidateVisual();
        }

        private void buttonFind_Click(object sender, RoutedEventArgs e)
        {
            Find();
        }

        private void btnZoom_Click(object sender, RoutedEventArgs e)
        {
            string name = comboBoxAtom.Text as string;

            //   if (guid == null) return;
            structTransportCommonProperty Tr = null;
            VMMainViewModel.Instance.colGroundAtoms.TryGetValue(name, out Tr);
            if (Tr != null)
            {
                VMMainViewModel.Instance.ConvertCoordGroundToPixel(Tr.X, Tr.Y, ref PixelX, ref PixelY);
                bFound = true;
                fx = Tr.X;
                fy = Tr.Y;
               
                VMMainViewModel.Instance.MyMainMap.CenterOnGroundPointZoom(fx, fy);



               // VMMainViewModel.Instance.InvalidateVisual();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
