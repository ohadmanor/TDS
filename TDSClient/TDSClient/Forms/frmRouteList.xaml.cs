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

using System.Globalization;

using System.ComponentModel;
using TDSClient.SAGInterface;
using TDSClient.PolygonRouteManagement;

namespace TDSClient.Forms
{
    /// <summary>
    /// Interaction logic for frmRouteList.xaml
    /// </summary>
    public partial class frmRouteList : Window
    {
        public event NotifyEndDrawPolygonEvent EndDrawPolygonEvent;
        SortedList<string, Route> RouteShowList = new SortedList<string, Route>();

        private bool isSelectVisible = false;

        public frmRouteList()
        {
            InitializeComponent();
            this.Background = new LinearGradientBrush(Colors.AliceBlue, Colors.LightGray, 90);

            IsSelectVisible = false;
            SetGridRoutes();
            LoadData();
        }

        public bool IsSelectVisible
        {
            get { return isSelectVisible; }
            set{
                isSelectVisible = value;
                if (isSelectVisible)
                    btnSelect.Visibility = System.Windows.Visibility.Visible;
                else
                    btnSelect.Visibility = System.Windows.Visibility.Hidden;
               }
        }

        private void SetGridRoutes()
        {
            dtGridRoute.Style = null;
            dtGridRoute.Columns.Clear();
            dtGridRoute.MaxWidth = 400;
            dtGridRoute.HorizontalAlignment = HorizontalAlignment.Left;
            dtGridRoute.AutoGenerateColumns = false;
            dtGridRoute.SelectionUnit = DataGridSelectionUnit.FullRow;
            dtGridRoute.CanUserAddRows = false;
            dtGridRoute.CanUserDeleteRows = false;
            dtGridRoute.ColumnHeaderHeight = 20;
            dtGridRoute.HeadersVisibility = DataGridHeadersVisibility.Column;
            dtGridRoute.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            dtGridRoute.RowHeaderWidth = 0;

            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Header = Properties.Resources.strRouteName;// (string)this.Resources["strHeaderName"];// "Name";
            col1.MinWidth = 150;
            col1.CanUserSort = true;
            col1.IsReadOnly = true;
            col1.Binding = new System.Windows.Data.Binding("RouteName");
            dtGridRoute.Columns.Add(col1);

            //DataGridTextColumn col11 = new DataGridTextColumn();

            //col11.Header = KingsGameClientModel.Properties.Resources.strDistance;// "Distance";
            //col11.CanUserSort = true;
            //col11.IsReadOnly = true;
            //col11.Binding = new System.Windows.Data.Binding("Dist");
            //dtGridRoute.Columns.Add(col11);

            //DataGridCheckBoxColumn col2 = new DataGridCheckBoxColumn();
            //col2.Header = KingsGameClientModel.Properties.Resources.strDisplay;// (string)this.Resources["strHeaderDisplay"];// "Display"; 
            //col2.MinWidth = 16;
            //col2.CanUserSort = true;
            //col2.Binding = new System.Windows.Data.Binding("IsShow");
            //dtGridRoute.Columns.Add(col2);

        }
        private  async void LoadData()
        {
            List<RouteData> RouteDataList = new List<RouteData>();
            try
            {
                IEnumerable<GeneralActivityDTO> Activities = await SAGSignalR.GetAllActivites(VMMainViewModel.Instance.SimulationHubProxy);


                IEnumerable<Route> Routes = await SAGSignalR.getRoutes(VMMainViewModel.Instance.SimulationHubProxy);
                if (Routes!=null)
                {
                    foreach (Route route in Routes)
                    {
                        RouteData Rdata = new RouteData();
                        Rdata.route = route;

                        if (Activities != null)
                        {
                            foreach (GeneralActivityDTO activity in Activities)
                            {
                                if (activity.RouteActivity.RouteGuid == route.RouteGuid)
                                {
                                    Rdata.IsInUse = true;
                                    break;
                                }
                            }

                        }


                        RouteDataList.Add(Rdata);

                    }
                }




               
                dtGridRoute.ItemsSource = null;
                dtGridRoute.ItemsSource = RouteDataList;

                if(dtGridRoute.ItemsSource==null)
                {
                    dtGridRoute.ItemsSource = new List<RouteData>();
                }


            }
            catch(Exception ex)
            {

            }

        }


        private void dtGridRoute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = dtGridRoute.SelectedIndex;
            if (i < 0) return;
            RouteData currRouteData = (RouteData)dtGridRoute.Items[i];

            RouteShowList.Clear();
            RouteShowList.Add(currRouteData.route.RouteName, currRouteData.route);
            for (int j = 0; j < dtGridRoute.Items.Count; j++)
            {
                RouteData RData = (RouteData)dtGridRoute.Items[j];
                if (RData.IsShow)
                {
                    if (!RouteShowList.ContainsKey(RData.route.RouteName))
                    {
                        RouteShowList.Add(RData.route.RouteName,RData.route);
                    }
                }
            }

            VMMainViewModel.Instance.InvalidateVisual();



        }

        private void UserDrawWPF(DrawingContext dc)
        {
            foreach (Route Route in RouteShowList.Values)
            {
                UserDrawRoutesWPF(Route, dc);
            }

        }
        private void UserDrawRoutesWPF(Route Route, DrawingContext dc)
        {
            System.Windows.Point[] m_ScreenPnts = null;
            int PixelX = 0;
            int PixelY = 0;

            if (Route == null) return;
            if (Route.Points.Count() == 0) return;



            System.Windows.Media.SolidColorBrush curBrush = new System.Windows.Media.SolidColorBrush();

            curBrush.Color = System.Windows.Media.Colors.Gold;


            System.Windows.Media.Pen pen = new System.Windows.Media.Pen(curBrush, 3);


           // m_ScreenPnts = new System.Windows.Point[Route.arr_legs.Length + 1];
            m_ScreenPnts = new System.Windows.Point[Route.Points.Count()];


            //VMMainViewModel.Instance.ConvertCoordGroundToPixel(Route.arr_legs[0].FromLongn, Route.arr_legs[0].FromLatn, ref PixelX, ref PixelY);
            //m_ScreenPnts[0].X = PixelX;
            //m_ScreenPnts[0].Y = PixelY;


            for (int i = 0; i < Route.Points.Count(); i++)
            {
                //VMMainViewModel.Instance.ConvertCoordGroundToPixel(Route.arr_legs[i].ToLongn, Route.arr_legs[i].ToLatn, ref PixelX, ref PixelY);
                //m_ScreenPnts[i + 1].X = PixelX;
                //m_ScreenPnts[i + 1].Y = PixelY;

                VMMainViewModel.Instance.ConvertCoordGroundToPixel(Route.Points[i].X, Route.Points[i].Y, ref PixelX, ref PixelY);
                m_ScreenPnts[i].X = PixelX;
                m_ScreenPnts[i].Y = PixelY;
            }

            System.Windows.Media.PathGeometry PathGmtr = new System.Windows.Media.PathGeometry();
            System.Windows.Media.PathFigure pathFigure = new System.Windows.Media.PathFigure();

            System.Windows.Media.PolyLineSegment myPolyLineSegment = new System.Windows.Media.PolyLineSegment();
            System.Windows.Media.PointCollection pc = new System.Windows.Media.PointCollection(m_ScreenPnts);
            myPolyLineSegment.Points = pc;
            pathFigure.StartPoint = m_ScreenPnts[0];
            pathFigure.Segments.Add(myPolyLineSegment);
            PathGmtr.Figures.Add(pathFigure);

            dc.DrawGeometry(null, pen, PathGmtr);

        }
        class RouteData : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public Route route;
            private bool mIsShow;

            public string RouteName
            {
                get { return route.RouteName; }
                set { }
            }

            public bool IsShow
            {
                get { return mIsShow; }
                set
                {
                    mIsShow = value;
                    NotifyPropertyChanged("IsShow");
                }
            }
            private bool m_IsInUse;
            public bool IsInUse
            {
                get { return m_IsInUse; }
                set
                {
                    m_IsInUse = value;
                    NotifyPropertyChanged("IsInUse");
                }
            }

            public double m_Dist;
            public double Dist
            {
                get { return m_Dist; }
                set
                {
                    m_Dist = value;
                    NotifyPropertyChanged("Dist");
                }
            }


            public void NotifyPropertyChanged(String info)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                }
            }
        }

        private void cmdNew_Click(object sender, RoutedEventArgs e)
        {
            //Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            //bool isOk = await clsRoadRoutingWebApi.PingRoutingWeb();
            //if (isOk == false)
            //{
            //    try
            //    {
            //        string err = KingsGameClientModel.Properties.Resources.strSpatialServicenotActive;
            //        ComponentsUtility.KGMsgBox.ShowCustomMsgOk(UserSession.GetParentWindow(this), err);
            //    }
            //    catch { }

            //    Mouse.OverrideCursor = null;
            //    return;

            //}
            //Mouse.OverrideCursor = null;

            frmRouteEditOSMRouting frm = null;
            foreach (Window win in Application.Current.Windows)
            {
                if (win is frmRouteEditOSMRouting)
                {
                    win.Activate();
                    return;
                }
            }
            frm = new frmRouteEditOSMRouting();
            frm.EndDrawPolygonEvent += new NotifyEndDrawPolygonEvent(RouteEdit_EndDrawPolygonEvent);
            frm.Owner = this;    //UserSession.GetParentWindow(this);// Application.Current.MainWindow;
            frm.Show();

        }

        public async void RouteEdit_EndDrawPolygonEvent(object sender, DrawPolygonEventArgs args)
        {
            dtGridRoute.Focus();


            if (args.isCancel)
            {
                int i = dtGridRoute.SelectedIndex;
                if (i > 0)
                {

                    RouteData currRouteData = (RouteData)dtGridRoute.Items[i];
                    if (!RouteShowList.ContainsKey(currRouteData.RouteName))
                    {
                        RouteShowList.Add(currRouteData.RouteName, currRouteData.route);
                    }
                }
            }
            else
            {
                Route route = null;// 
                if (!args.isNew)
                {
                      

                    if (dtGridRoute.ItemsSource == null) return;

                    List<RouteData> listData = (List<RouteData>)(dtGridRoute.ItemsSource);

                    RouteData currRouteData = null;
                    for (int j = 0; j < listData.Count; j++)
                    {
                        if (listData[j].RouteName == args.PolygonName)
                        {
                            currRouteData = listData[j];
                            break;
                        }
                    }

                    if (currRouteData == null) return;

                 //Victor   currRouteData.route.arr_legs = LegSector;
                    route = currRouteData.route;
                    if (!RouteShowList.ContainsKey(currRouteData.RouteName))
                    {
                        RouteShowList.Add(currRouteData.RouteName, currRouteData.route);
                    }


                }
                else
                {
                    route = new Route();                   

                   // LegSector = CreateLegSector(args.PolygonPnts);
                    route.Points =new List<DPoint>(args.PolygonPnts);
                    route.RouteName = args.PolygonName;

                    RouteData Rdata = new RouteData();
                    Rdata.route = route;




                    ((List<RouteData>)(dtGridRoute.ItemsSource)).Add(Rdata);
                    dtGridRoute.Items.Refresh();
                    dtGridRoute.SelectedItem = Rdata;
                    dtGridRoute.CurrentItem = Rdata;
                    DataGridWPFUtility.DataGridGotoLast(dtGridRoute);
                }

                await SAGSignalR.SaveRoute(VMMainViewModel.Instance.SimulationHubProxy, route);
            }
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            VMMainViewModel.Instance.dlgUserDrawWPF -= UserDrawWPF;
            VMMainViewModel.Instance.InvalidateVisual();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VMMainViewModel.Instance.dlgUserDrawWPF += UserDrawWPF;
        }

        private void cmdExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {

            int i = dtGridRoute.SelectedIndex;
            if (i < 0) return;


            if (EndDrawPolygonEvent != null)
            {
                RouteData currRouteData = (RouteData)dtGridRoute.Items[i];

                DrawPolygonEventArgs arg = new DrawPolygonEventArgs();
                arg.isCancel = false;
                arg.PolygonName = currRouteData.RouteName;
                EndDrawPolygonEvent(this, arg);
            }
            this.Close();
        }

        private  async void cmdDelete_Click(object sender, RoutedEventArgs e)
        {
            int i = dtGridRoute.SelectedIndex;
            if (i < 0) return;
            MessageBoxResult res = MessageBox.Show(Properties.Resources.strAreyousureyouwanttodeleteit, String.Empty, MessageBoxButton.YesNo);
            if (res == MessageBoxResult.No) return;

            RouteData currRouteData = (RouteData)dtGridRoute.Items[i];

            if (currRouteData.IsInUse)
            {
                MessageBoxResult res2 = MessageBox.Show("All Activities will be deleted!", String.Empty, MessageBoxButton.YesNo);
                if (res2 == MessageBoxResult.No) return;
            }




            await TDSClient.SAGInterface.SAGSignalR.DeleteRouteByGuid(VMMainViewModel.Instance.SimulationHubProxy, currRouteData.route.RouteGuid);

            ((List<RouteData>)(dtGridRoute.ItemsSource)).Remove(currRouteData);
            dtGridRoute.Items.Refresh();
            dtGridRoute.Items.MoveCurrentToNext();

        }
    }

    [ValueConversion(typeof(object), typeof(Boolean))]
    public class BoolValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
         object parameter, CultureInfo culture)
        {
            bool ans = (bool)System.Convert.ChangeType(value, typeof(Boolean));
            return ans;
        }

        public object ConvertBack(object value, Type targetType,
         object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack not supported");
        }
    }
}
