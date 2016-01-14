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

using TerrainService;

using TDSClient.SAGInterface;

namespace TDSClient.Forms
{
    /// <summary>
    /// Interaction logic for frmActivityEdit.xaml
    /// </summary>
    public partial class frmActivityEdit : Window
    {
        structTransportCommonProperty AtomCommonProperty = null;
        GeneralActivityDTO refActivityDTO = null;


        public event NotifyActivityDTOEditEvent ActivityDTOEditEvent;
        Route ActivityRoute = null;
        Route ReferenceRoute = null;

        DPoint referencePoint;


        public frmActivityEdit(structTransportCommonProperty CommonProperty, GeneralActivityDTO ActivityDTO)
        {
            InitializeComponent();
            this.Background = new LinearGradientBrush(Colors.AliceBlue, Colors.LightGray, 90);
            txPlatformName.IsReadOnly = true;

            AtomCommonProperty = CommonProperty;
            refActivityDTO = ActivityDTO;

            if (AtomCommonProperty!=null)
            {
                txPlatformName.Text = AtomCommonProperty.AtomName;
            }
            
        }
        public async void MapOnClick(object sender, MapMouseEventArgsWPF e)
        {
          
            if (checkBoxMapReferencePoint.IsChecked == true && string.IsNullOrEmpty(txtRoute.Text)==false)
            {
                txtReferenceX.Tag = null;
                referencePoint = new DPoint();


                DPoint Dpnt = new DPoint();
                Dpnt.X = e.MapXLongLatWGS84;
                Dpnt.Y = e.MapYLongLatWGS84;

                Route route = await SAGSignalR.GetRouteByName(VMMainViewModel.Instance.SimulationHubProxy, txtRoute.Text);
                ActivityRoute = route;
                if(route!=null)
                {
                 

                    double minDist = double.MaxValue;
                    DPoint minDpoint = new DPoint();
                    int leg = 0;
                    int i = -1;
                    foreach(DPoint dpoint in route.Points)
                    {
                        i++;
                        double dist = Utilites.GreatCircleDistance(Dpnt.X, Dpnt.Y, dpoint.X, dpoint.Y);
                        if(dist<minDist)
                        {
                            minDist = dist;
                            minDpoint = dpoint;
                            leg = i;
                        }
                    }



                    float NearestIndex=0;
                   // shPoint[] shPoints = Utilites.Convert2shPoint(route.Points);

                    shPoint pnt = new shPoint(minDpoint.X, minDpoint.Y);


                    //shPoint pnt = TerrainService.MathEngine.ReturnNearestPointOnPolygonBorder(Dpnt.X, Dpnt.Y, shPoints, out  NearestIndex);
                    if (pnt.x != 0.0 && pnt.y != 0.0)
                    {

                        referencePoint = new DPoint(pnt.x, pnt.y);


                    }


                    double aXOut = 0;
                    double aYOut = 0;

                    aXOut = Math.Round(pnt.x, 3);
                    aYOut = Math.Round(pnt.y, 3);

                    txtReferenceX.Content = aXOut.ToString();
                    txtReferenceY.Content = aYOut.ToString();

                    txtReferenceX.Tag = pnt.x;
                    txtReferenceY.Tag = pnt.y;


                    shPath Path = await clsRoadRoutingWebApi.FindShortPath("0", AtomCommonProperty.X, AtomCommonProperty.Y, pnt.x, pnt.y, false);

                   
                    {
                        ReferenceRoute = new Route();
                        ReferenceRoute.Points=Utilites.Convert2DPoint(Path.Points);

                        VMMainViewModel.Instance.InvalidateVisual();

                    }
                }



               
                checkBoxMapReferencePoint.IsChecked = false;

            }
        }
       

        private void UserDrawWPF(DrawingContext dc)
        {
            int PixelX = 0;
            int PixelY = 0;
            if (ActivityRoute!=null)
            {
                Utilites.UserDrawRoutesWPF(ActivityRoute, dc, System.Windows.Media.Colors.Blue);

                VMMainViewModel.Instance.ConvertCoordGroundToPixel(ActivityRoute.Points[ActivityRoute.Points.Count - 1].X, ActivityRoute.Points[ActivityRoute.Points.Count - 1].Y, ref PixelX, ref PixelY);
                ImageSource ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Images/" + "flag_red.png"));
                Utilites.UserDrawRasterWPFScreenCoordinate(dc, ImageSource, PixelX, PixelY, 22, 22);
            }

            if (referencePoint.X != 0.0 && referencePoint.Y != 0.0)
            {
               
                VMMainViewModel.Instance.ConvertCoordGroundToPixel(referencePoint.X, referencePoint.Y, ref PixelX, ref PixelY);
                ImageSource ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Images/" + "flag_blue.png"));
                Utilites.UserDrawRasterWPFScreenCoordinate(dc, ImageSource, PixelX, PixelY, 22, 22);
            }

            //if (ReferenceRoute != null)
            //{
            //    Utilites.UserDrawRoutesWPF(ReferenceRoute, dc, System.Windows.Media.Colors.Red);
            //}


        }
        private void cmdRoute_Click(object sender, RoutedEventArgs e)
        {


            frmRouteList frm = new frmRouteList();
            frm.IsSelectVisible = true;
            frm.Owner = this;


            //frm.WindowStartupLocation = WindowStartupLocation.Manual;



            //frm.Left = 20; //ParentObject.Left;
            //frm.Top = Application.Current.MainWindow.ActualHeight - frm.Height - 50;
            //Point pt1 = new Point();
            //Point pt = cmdRoute.PointToScreen(pt1);


            frm.EndDrawPolygonEvent += new NotifyEndDrawPolygonEvent(frmRouteList_EndDrawPolygonEvent);

            frm.Show();

            //string PolygonOwner = String.Empty;
          

            //if (optPrivateRoute.IsChecked == true) PolygonOwner = SelectedFormation.Identification;


            //frmRouteList frm = new frmRouteList(PolygonOwner);

            //frm.CountryId = SelectedFormation.CountryId; // OperOrder.CountryId;

            //frm.CountryColorSide = SelectedFormation.CountryColorSide; // OperOrder.ColorSide;

            //frm.RouteType = enumRouteType.Ground_Route;

            //frm.RouteNameStartShow = txtRoute.Text;

            //frm.Owner = this;

            //frm.WindowStartupLocation = WindowStartupLocation.Manual;



            //frm.Left = 20; //ParentObject.Left;
            //frm.Top = Application.Current.MainWindow.ActualHeight - frm.Height - 50;
            //Point pt1 = new Point();
            //Point pt = cmdRoute.PointToScreen(pt1);


            //frm.EndDrawPolygonEvent += new NotifyEndDrawPolygonEvent(frmRouteList_EndDrawPolygonEvent);
            //frm.Show();

        }
        public async void frmRouteList_EndDrawPolygonEvent(object sender, DrawPolygonEventArgs args)
        {
            if (txtRoute.Text != args.PolygonName)
            {
                txtReferenceX.Tag = null;
                txtReferenceX.Content = string.Empty;
                txtReferenceY.Content = string.Empty;
                referencePoint.X = 0;
                referencePoint.Y = 0;

            }


            txtRoute.Text = args.PolygonName;
            Route route = await SAGSignalR.GetRouteByName(VMMainViewModel.Instance.SimulationHubProxy, txtRoute.Text);
            ActivityRoute = route;

            VMMainViewModel.Instance.InvalidateVisual();

        }


        public  bool CheckOk()
        {
            if (txtRoute.Text == String.Empty)
            {
                System.Windows.MessageBox.Show(TDSClient.Properties.Resources.strRouteNameisEmpty, String.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (txtReferenceX.Tag == null)
            {
                System.Windows.MessageBox.Show(TDSClient.Properties.Resources.strReferencePointMustbeFilled, String.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
         {

           //  bool isCheckOk = await CheckOk();
             bool isCheckOk = CheckOk();
             if (isCheckOk == false) return;

             GeneralActivityDTO ActivityDTO = new GeneralActivityDTO();

             ActivityDTO.Atom = new AtomData();
             ActivityDTO.Atom.UnitName = AtomCommonProperty.AtomName;

             ActivityDTO.ActivityType = enumActivity.MovementActivity;
             ActivityDTO.StartActivityOffset = (TimeSpan)startActivity.Value;      
             ActivityDTO.Speed = (int)speedUpDown.Value;


             double X = 0;
             double Y = 0;
             if (txtReferenceX.Tag != null)
             {
                 double.TryParse(txtReferenceX.Tag.ToString(), out X);
                 double.TryParse(txtReferenceY.Tag.ToString(), out Y);

                 ActivityDTO.ReferencePoint = new DPoint(X,Y);
             }

            


           //  Route route = new Route();
           //  route.RouteName = txtRoute.Text;

             ActivityDTO.RouteActivity = ActivityRoute;// route;

             ActivityDTOEditEventArgs args = new ActivityDTOEditEventArgs();
             args.isNew = false;
             if (refActivityDTO == null)
             {
                args.isNew = true;
             }            
             else
             {
                 ActivityDTO.ActivityId = refActivityDTO.ActivityId;
                 ActivityDTO.Activity_SeqNumber = refActivityDTO.Activity_SeqNumber;
             }
             args.ActivityDTO = ActivityDTO;



             if(ActivityDTOEditEvent!=null)
             {
                 ActivityDTOEditEvent(this, args);
             }

             Close();

         }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            startActivity.Value = TimeSpan.Parse("00:00:01");           
            speedUpDown.Value = 5;

           

            if (refActivityDTO != null)
            {
                speedUpDown.Value = refActivityDTO.Speed;
                txtRoute.Text = refActivityDTO.RouteActivity.RouteName;
                ActivityRoute = await SAGSignalR.GetRouteByName(VMMainViewModel.Instance.SimulationHubProxy, txtRoute.Text);

               

                double aXOut = 0;
                double aYOut = 0;

                aXOut = Math.Round(refActivityDTO.ReferencePoint.X, 3);
                aYOut = Math.Round(refActivityDTO.ReferencePoint.Y, 3);

                txtReferenceX.Content = aXOut.ToString();
                txtReferenceY.Content = aYOut.ToString();

                txtReferenceX.Tag = refActivityDTO.ReferencePoint.X;
                txtReferenceY.Tag = refActivityDTO.ReferencePoint.Y;

                startActivity.Value = refActivityDTO.StartActivityOffset;

                referencePoint = new DPoint(refActivityDTO.ReferencePoint.X, refActivityDTO.ReferencePoint.Y);




                //shPath Path = await clsRoadRoutingWebApi.FindShortPath("0", AtomCommonProperty.X, AtomCommonProperty.Y, referencePoint.X, referencePoint.Y, false);
                //if (Path != null)
                //{

                //      ReferenceRoute = new Route();
                //      ReferenceRoute.Points = Utilites.Convert2DPoint(Path.Points);
                //}
             

                VMMainViewModel.Instance.InvalidateVisual();
            }




            VMMainViewModel.Instance.dlgUserDrawWPF += UserDrawWPF;
            VMMainViewModel.Instance.dlgMouseLeftClickOnMapEvent += MapOnClick;
            VMMainViewModel.Instance.InvalidateVisual();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

             VMMainViewModel.Instance.dlgUserDrawWPF -= UserDrawWPF;
             VMMainViewModel.Instance.dlgMouseLeftClickOnMapEvent -= MapOnClick;
             VMMainViewModel.Instance.InvalidateVisual();
        }
    }
}
