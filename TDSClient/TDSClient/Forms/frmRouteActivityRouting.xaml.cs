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

using System.ComponentModel;

using TDSClient.SAGInterface;
using TerrainService;

namespace TDSClient.Forms
{
    /// <summary>
    /// Interaction logic for frmRouteActivityRouting.xaml
    /// </summary>
    public partial class frmRouteActivityRouting : Window
    {
        public event NotifyEndDrawPolygonEvent EndDrawPolygonEvent;

        GeneralActivityDTO refActivityDTO = null;


        DPoint pntSource = new DPoint();
        int nodeSourceId = 0;

        DPoint pntTarget = new DPoint();
        int nodeTargetId = 0;

        enOSMhighwayFilter highwayFilter = enOSMhighwayFilter.Undefined;
        List<DPoint> m_PolygonPnts = new List<DPoint>();
        List<DPoint> m_InternalPnts = new List<DPoint>();


        public frmRouteActivityRouting(GeneralActivityDTO pActivityDTO)
        {
            InitializeComponent();
            this.Background = new LinearGradientBrush(Colors.AliceBlue, Colors.LightGray, 90);

            refActivityDTO = pActivityDTO;

           


            SetGridRoutes();

            dtGridRoute.ItemsSource = new List<OsmRouteData>();
        }
        class OsmRouteData : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            // public typRoute Route;
            // private bool mIsShow;

            private string mRouteName;

            public string RouteName
            {
                get { return mRouteName; }
                set
                {
                    NotifyPropertyChanged("RouteName");
                }
            }

            private bool mIsMapClick;
            public bool IsMapClick
            {
                get { return mIsMapClick; }
                set
                {
                    mIsMapClick = value;
                    NotifyPropertyChanged("IsMapClick");
                }
            }

            private double x;
            private double y;

            public double X
            {
                get { return x; }
                set
                {
                    x = value;
                    NotifyPropertyChanged("Coordinates");
                }
            }
            public double Y
            {
                get { return y; }
                set
                {
                    y = value;
                    NotifyPropertyChanged("Coordinates");

                }
            }

            private int nodeid;
            public int NodeId
            {
                get { return nodeid; }
                set
                {
                    nodeid = value;
                    NotifyPropertyChanged("NodeId");

                }
            }



            public string Coordinates
            {
                get
                {
                    return Math.Round(x, 3).ToString() + " " + Math.Round(y, 3).ToString();
                }
            }

            //private bool m_IsInUse;
            //public bool IsInUse
            //{
            //    get { return m_IsInUse; }
            //    set
            //    {
            //        m_IsInUse = value;
            //        NotifyPropertyChanged("IsInUse");
            //    }
            //}



            public void NotifyPropertyChanged(String info)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                }
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




            DataGridCheckBoxColumn col0 = new DataGridCheckBoxColumn();

            col0.Header = TDSClient.Properties.Resources.strMarkOnTheMap;  // (string)this.Resources["strHeaderDisplay"];// "Display"; 
            col0.MinWidth = 16;
            col0.CanUserSort = true;
            col0.Binding = new System.Windows.Data.Binding("IsMapClick");
            dtGridRoute.Columns.Add(col0);



            DataGridTextColumn col2 = new DataGridTextColumn();

            col2.Header = TDSClient.Properties.Resources.strCoordinate;// (string)this.Resources["strHeaderName"];// "Name";

            col2.CanUserSort = false;
            col2.IsReadOnly = true;
            col2.Binding = new System.Windows.Data.Binding("Coordinates");
            dtGridRoute.Columns.Add(col2);


        }


        private  async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            startActivity.Value = TimeSpan.Parse("00:00:01");
            durationActivity.Value = TimeSpan.Parse("20:00:00");
            speedUpDown.Value = 5;

            if (refActivityDTO != null)
            {
                SetHighwayFilter();

                txPlatformName.Text = refActivityDTO.Atom.UnitName;
                txPlatformName.IsEnabled = false;



                pntSource = refActivityDTO.Atom.Location;
                txtSourceX.Text = Math.Round(pntSource.X, 4).ToString();
                txtSourceY.Text = Math.Round(pntSource.Y, 4).ToString();

                 shPointId PointId = await    clsRoadRoutingWebApi.GetNearestPointIdOnRoad("0", highwayFilter, pntSource.X, pntSource.Y);

                 nodeSourceId = PointId.nodeId;
                

                if (refActivityDTO.RouteActivity.Points != null)
                {
                    m_PolygonPnts = refActivityDTO.RouteActivity.Points.ToList<DPoint>();
                    txtRouteName.Text = refActivityDTO.RouteActivity.RouteName;
                    txtRouteName.IsEnabled = false;


                    pntTarget = refActivityDTO.RouteActivity.Points.Last();
                    txtTargetX.Text = Math.Round(pntTarget.X, 4).ToString();
                    txtTargetY.Text = Math.Round(pntTarget.Y, 4).ToString();

                    shPointId PointIdTarget = await clsRoadRoutingWebApi.GetNearestRoadNodeWithCondition("0", highwayFilter, pntTarget.X, pntTarget.Y, nodeSourceId, true);
                    nodeTargetId = PointIdTarget.nodeId;
                }

                speedUpDown.Value = refActivityDTO.Speed;
                startActivity.Value = refActivityDTO.StartActivityOffset;
                durationActivity.Value = refActivityDTO.DurationActivity;

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
        public void SetHighwayFilter()
        {
            highwayFilter = enOSMhighwayFilter.Undefined;
            if ((bool)chkCarMostImportant.IsChecked)
            {
                highwayFilter = highwayFilter | enOSMhighwayFilter.CarMostImportant;


            }

            if (highwayFilter.HasFlag(enOSMhighwayFilter.Undefined) == true)
            {

            }

            // uint b = (uint)highwayFilter;


            if ((bool)chkCarMediumImportant.IsChecked)
            {
                highwayFilter = highwayFilter | enOSMhighwayFilter.CarMediumImportant;
            }
            if ((bool)chkCarLowImportant.IsChecked)
            {
                highwayFilter = highwayFilter | enOSMhighwayFilter.CarLowImportant;
            }
            if ((bool)chkConstruction.IsChecked)
            {
                highwayFilter = highwayFilter | enOSMhighwayFilter.Construction;
            }

        }

        public async void MapOnClick(object sender, MapMouseEventArgsWPF e)
        {
            try
            {
                SetHighwayFilter();

                DPoint Dpnt = new DPoint();
                Dpnt.X = e.MapXLongLatWGS84;
                Dpnt.Y = e.MapYLongLatWGS84;

                if (checkBoxMapSource.IsChecked == true)
                {

                    checkBoxMapSource.IsChecked = false;

                    shPoint pnt = new shPoint();
                    int nodeid = 0;

                    shPointId PointId = await clsRoadRoutingWebApi.GetNearestPointIdOnRoad("0", highwayFilter, Dpnt.X, Dpnt.Y);
                    pnt = PointId.point;
                    nodeid = PointId.nodeId;

                    if (pnt.x != 0 || pnt.y != 0)
                    {
                        pntSource.X = pnt.x;
                        pntSource.Y = pnt.y;
                        nodeSourceId = nodeid;
                        txtSourceX.Text = Math.Round(pntSource.X, 4).ToString();
                        txtSourceY.Text = Math.Round(pntSource.Y, 4).ToString();

                    }

                    VMMainViewModel.Instance.InvalidateVisual();

                   
                }
                else if (checkBoxMapTarget.IsChecked == true)
                {

                    checkBoxMapTarget.IsChecked = false;

                    shPoint pnt = new shPoint();
                    int nodeid = 0;

                    shPointId PointId = await clsRoadRoutingWebApi.GetNearestRoadNodeWithCondition("0", highwayFilter, Dpnt.X, Dpnt.Y, nodeSourceId, true);
                    pnt = PointId.point;
                    nodeid = PointId.nodeId;


                    //UserSession.ClientSideObject.m_GameManagerProxy.GetNearestPointOnRoadWithCondition(out pnt, out nodeid,highwayFilter, Dpnt.X, Dpnt.Y, nodeSourceId, true);


                    if (pnt.x != 0 || pnt.y != 0)
                    {
                        pntTarget.X = pnt.x;
                        pntTarget.Y = pnt.y;

                        txtTargetX.Text = Math.Round(pntTarget.X, 4).ToString();
                        txtTargetY.Text = Math.Round(pntTarget.Y, 4).ToString();
                        nodeTargetId = nodeid;
                    }
                    else
                    {

                    }


                    VMMainViewModel.Instance.InvalidateVisual();
                }
                else
                {
                    for (int i = 0; i < dtGridRoute.Items.Count; i++)
                    {
                        OsmRouteData DetailData = ((List<OsmRouteData>)dtGridRoute.ItemsSource)[i];
                        if (DetailData.IsMapClick)
                        {
                            DetailData.IsMapClick = false;

                            shPoint pnt = new shPoint();
                            //  string err = UserSession.ClientSideObject.m_GameManagerProxy.GetNearestPointOnRoad(out pnt, Dpnt.X, Dpnt.Y);
                            int nSourceId = 0;
                            if (i == 0)
                            {
                                nSourceId = nodeSourceId;
                            }
                            else
                            {
                                OsmRouteData prevDetailData = ((List<OsmRouteData>)dtGridRoute.ItemsSource)[i - 1];
                                nSourceId = prevDetailData.NodeId;
                            }

                            int nodeid = 0;


                            shPointId PointId = await clsRoadRoutingWebApi.GetNearestRoadNodeWithCondition("0", highwayFilter, Dpnt.X, Dpnt.Y, nSourceId, true);
                            pnt = PointId.point;
                            nodeid = PointId.nodeId;


                            //  UserSession.ClientSideObject.m_GameManagerProxy.GetNearestPointOnRoadWithCondition(out pnt, out nodeid, highwayFilter, Dpnt.X, Dpnt.Y, nSourceId, true);


                            if (pnt.x != 0 || pnt.y != 0)
                            {
                                DetailData.X = pnt.x;
                                DetailData.Y = pnt.y;
                                DetailData.NodeId = nodeid;

                            }
                            else
                            {

                            }


                            VMMainViewModel.Instance.InvalidateVisual();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void UserDrawWPF(DrawingContext dc)
        {
            System.Windows.Media.Color m_LineColor = System.Windows.Media.Colors.Gold;
            System.Windows.Media.SolidColorBrush m_LineColorBrush = System.Windows.Media.Brushes.Gold;
            int m_LineWidth = 4;
            try
            {
                int PixelX = 0;
                int PixelY = 0;
                if (pntSource.X != 0.0 && pntSource.Y != 0.0)
                {
                    VMMainViewModel.Instance.ConvertCoordGroundToPixel(pntSource.X, pntSource.Y, ref PixelX, ref PixelY);
                    ImageSource ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Images/" + "flag_blue.png"));
                    Utilites.UserDrawRasterWPFScreenCoordinate(dc, ImageSource, PixelX, PixelY, 22, 22);
                }
                if (pntTarget.X != 0.0 && pntTarget.Y != 0.0)
                {
                    VMMainViewModel.Instance.ConvertCoordGroundToPixel(pntTarget.X, pntTarget.Y, ref PixelX, ref PixelY);
                    ImageSource ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Images/" + "flag_red.png"));
                    Utilites.UserDrawRasterWPFScreenCoordinate(dc, ImageSource, PixelX, PixelY, 22, 22);
                }






                if (m_PolygonPnts == null) return;
                //  if (m_PolygonPnts.Count == 0) return;

                //  m_LineColor = System.Windows.Media.Colors.Gold;

                //   m_LineColorBrush.Color = System.Windows.Media.Colors.Gold;



                System.Windows.Media.Pen pen = new System.Windows.Media.Pen(m_LineColorBrush, m_LineWidth);
                pen.Thickness = m_LineWidth;
                pen.LineJoin = PenLineJoin.Round;



                System.Windows.Point[] m_ScreenPnts = new System.Windows.Point[m_PolygonPnts.Count];
                for (int i = 0; i < m_PolygonPnts.Count; i++)
                {

                    //   int PixelX = 0;
                    //   int PixelY = 0;
                    VMMainViewModel.Instance.ConvertCoordGroundToPixel(m_PolygonPnts[i].X, m_PolygonPnts[i].Y, ref PixelX, ref PixelY);
                    m_ScreenPnts[i].X = PixelX;
                    m_ScreenPnts[i].Y = PixelY;
                }



                if (m_ScreenPnts.Length > 1)
                {




                    System.Windows.Media.PathGeometry PathGmtr = new System.Windows.Media.PathGeometry();
                    System.Windows.Media.PathFigure pathFigure = new System.Windows.Media.PathFigure();

                    System.Windows.Media.PolyLineSegment myPolyLineSegment = new System.Windows.Media.PolyLineSegment();
                    System.Windows.Media.PointCollection pc = new System.Windows.Media.PointCollection(m_ScreenPnts);
                    myPolyLineSegment.Points = pc;
                    pathFigure.StartPoint = m_ScreenPnts[0];
                    pathFigure.Segments.Add(myPolyLineSegment);
                    PathGmtr.Figures.Add(pathFigure);




                    pathFigure.IsClosed = false;

                    dc.DrawGeometry(null, pen, PathGmtr);

                }



                if (dtGridRoute != null)
                {
                    FormattedText frm = new FormattedText("o",
                                                             System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                                                             System.Windows.FlowDirection.RightToLeft,
                                                             new System.Windows.Media.Typeface("Arial"),
                                                             18, System.Windows.Media.Brushes.Red);

                    for (int i = 0; i < dtGridRoute.Items.Count; i++)
                    {
                        if (dtGridRoute.SelectedIndex == i)
                        {
                            frm.SetFontSize(24);
                        }
                        else
                        {
                            frm.SetFontSize(18);
                        }


                        OsmRouteData DetailData = ((List<OsmRouteData>)dtGridRoute.ItemsSource)[i];
                        VMMainViewModel.Instance.ConvertCoordGroundToPixel(DetailData.X, DetailData.Y, ref PixelX, ref PixelY);


                        frm.SetFontWeight(System.Windows.FontWeights.Bold);
                        frm.TextAlignment = TextAlignment.Center;
                        dc.DrawText(frm, new System.Windows.Point(PixelX - frm.Width / 2, PixelY - frm.Height / 2));


                        FormattedText frmNum = new FormattedText((i + 1).ToString(),
                                              System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                                              System.Windows.FlowDirection.LeftToRight,
                                              new System.Windows.Media.Typeface("Arial"),
                                              14, System.Windows.Media.Brushes.Red);

                        frmNum.SetFontWeight(System.Windows.FontWeights.Bold);

                        dc.DrawText(frmNum, new System.Windows.Point(PixelX, PixelY));

                    }

                }




            }
            catch (Exception ex)
            {
            }
        }

        private async void cmdRoute_Click(object sender, RoutedEventArgs e)
        {
            checkBoxMapSource.IsChecked = false;
            checkBoxMapTarget.IsChecked = false;

            SetHighwayFilter();

            List<int> nodeList = new List<int>();

            if (nodeSourceId != 0)
            {
                nodeList.Add(nodeSourceId);
            }
            for (int i = 0; i < dtGridRoute.Items.Count; i++)
            {
                OsmRouteData DetailData = ((List<OsmRouteData>)dtGridRoute.ItemsSource)[i];
                if (DetailData.NodeId != 0)
                {
                    nodeList.Add(DetailData.NodeId);
                }
            }
            if (nodeTargetId != 0)
            {
                nodeList.Add(nodeTargetId);
            }





            //            typRoute RoadRoute = UserSession.ClientSideObject.m_GameManagerProxy.GetRoadsRoute(pntSource.X, pntSource.Y, pntTarget.X, pntTarget.Y);
            enOSMhighwayFilter[] arrHighwayFilter = new enOSMhighwayFilter[1];
            arrHighwayFilter[0] = highwayFilter;


            shPath Path = await clsRoadRoutingWebApi.FindShortPathWithArrayNodes("0", nodeList.ToArray(), arrHighwayFilter);
            m_PolygonPnts.Clear();
            if (Path != null && Path.Points != null)
            {
                for (int i = 0; i < Path.Points.Count; i++)
                {
                    DPoint Dpnt = new DPoint();
                    Dpnt.X = Path.Points[i].x;
                    Dpnt.Y = Path.Points[i].y;
                    m_PolygonPnts.Add(Dpnt);
                }
            }



            VMMainViewModel.Instance.InvalidateVisual();
           
        }



        private void cmdClearAutoRoute_Click(object sender, RoutedEventArgs e)
        {

        }

        private void checkBoxMapSource_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxMapTarget.IsChecked = false;
        }

        private void checkBoxMapTarget_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxMapSource.IsChecked = false;
        }


        private void cmdNew_Click(object sender, RoutedEventArgs e)
        {
            OsmRouteData Rdata = new OsmRouteData();
            ((List<OsmRouteData>)(dtGridRoute.ItemsSource)).Add(Rdata);
            dtGridRoute.Items.Refresh();
            dtGridRoute.SelectedItem = Rdata;
            //    dtGridRoute.SelectedIndex = dtGridRoute.Items.Count;
            dtGridRoute.CurrentItem = Rdata;

            DataGridWPFUtility.DataGridGotoLast(dtGridRoute);

        }

        private void cmdDelete_Click(object sender, RoutedEventArgs e)
        {
            int i = dtGridRoute.SelectedIndex;
            if (i < 0) return;
            OsmRouteData currRouteData = (OsmRouteData)dtGridRoute.Items[i];
            ((List<OsmRouteData>)(dtGridRoute.ItemsSource)).Remove(currRouteData);
            dtGridRoute.Items.Refresh();
            dtGridRoute.Items.MoveCurrentToNext();

            VMMainViewModel.Instance.InvalidateVisual();
           
        }


        private void dtGridRoute_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.DisplayIndex == 0)
            {

                CheckBox chBox = e.EditingElement as CheckBox;
                if ((bool)chBox.IsChecked)
                {

                    checkBoxMapSource.IsChecked = false;
                    checkBoxMapTarget.IsChecked = false;
                    //   buttonClear.Visibility = System.Windows.Visibility.Visible;
                    int selind = dtGridRoute.SelectedIndex;

                    for (int i = 0; i < dtGridRoute.Items.Count; i++)
                    {
                        OsmRouteData DetailData = ((List<OsmRouteData>)dtGridRoute.ItemsSource)[i];
                        if (i != selind)
                        {
                            DetailData.IsMapClick = false;
                        }
                    }
                }
                dtGridRoute.CommitEdit(DataGridEditingUnit.Cell, true);
            }

        }


        private void dtGridRoute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VMMainViewModel.Instance.InvalidateVisual();
            
        }


        public async Task<bool> CheckOk()
        {
            if (txtRouteName.Text == String.Empty)
            {               
                System.Windows.MessageBox.Show(TDSClient.Properties.Resources.strRouteNameisEmpty, String.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (txPlatformName.Text == String.Empty)
            {
                System.Windows.MessageBox.Show(TDSClient.Properties.Resources.strPlatformNameisEmpty, String.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if(refActivityDTO ==null) //new 
            {
               bool isExist= await SAGSignalR.isAtomNameExist(VMMainViewModel.Instance.SimulationHubProxy, txPlatformName.Text);
               if (isExist)
               {
                   System.Windows.MessageBox.Show(TDSClient.Properties.Resources.strPlatformWithTheSamenameAlreadyExists, String.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);
                   return false;
               }

               isExist = await SAGSignalR.isRouteNameExist(VMMainViewModel.Instance.SimulationHubProxy, txtRouteName.Text);
               if (isExist)
               {
                   System.Windows.MessageBox.Show(TDSClient.Properties.Resources.strRouteNameAlreadyExist, String.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);
                   return false;
               }

             
            }
          

            //txtRouteName.Text = UserSession.TreatApostrophe(txtRouteName.Text);


            if (m_PolygonPnts == null || m_PolygonPnts.Count < 2)
            {                
                System.Windows.MessageBox.Show(TDSClient.Properties.Resources.strRoutePoinsmustbeadded, String.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }



            ////  if (isNew)
            //{
            //    typRoute[] Routes = UserSession.ClientSideObject.m_GameManagerProxy.GetRoutes();
            //    if (Routes != null)
            //    {
            //        for (int i = 0; i < Routes.Length; i++)
            //        {
            //            if (Routes[i].RouteName == txtRouteName.Text)
            //            {

            //                ComponentsUtility.KGMsgBox.ShowCustomMsgOk(UserSession.GetParentWindow(this),
            //                 KingsGameClientModel.Properties.Resources.strRouteNameAlreadyExist);


            //                //  System.Windows.MessageBox.Show("Route Name is Exist", String.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);
            //                return false;
            //            }
            //        }
            //    }

            //}

            return true;
        }

        private async void cmdExit_Click(object sender, RoutedEventArgs e)
        {

            bool isCheckOk = await CheckOk();

            if (isCheckOk == false) return;

            DrawPolygonEventArgs arg = new DrawPolygonEventArgs();
            arg.isCancel = false;
            arg.isNew = true;

            arg.PolygonName = txtRouteName.Text.Trim();

            arg.PolygonPnts = m_PolygonPnts.ToArray<DPoint>();
            //  isOK = true;
            if (EndDrawPolygonEvent != null)
            {
                EndDrawPolygonEvent(this, arg);
            }


            GeneralActivityDTO ActivityDTO = new GeneralActivityDTO();
            ActivityDTO.ActivityType = enumActivity.MovementActivity;

            AtomData atomdata = new AtomData();
            atomdata.UnitName = txPlatformName.Text;
            atomdata.Location = m_PolygonPnts[0];

            ActivityDTO.StartActivityOffset = (TimeSpan)startActivity.Value;
            ActivityDTO.DurationActivity = (TimeSpan)durationActivity.Value;
            ActivityDTO.Speed = (int)speedUpDown.Value;

            Route route = new Route();
            route.RouteName = txtRouteName.Text;
            route.Points = m_PolygonPnts;
            ActivityDTO.RouteActivity = route;

            if (refActivityDTO!=null)
            {
                atomdata.UnitGuid = refActivityDTO.Atom.UnitGuid;
                ActivityDTO.ActivityId = refActivityDTO.ActivityId;
                if (ActivityDTO.RouteActivity!=null)
                {
                    ActivityDTO.RouteActivity.RouteGuid = refActivityDTO.RouteActivity.RouteGuid;
                }
             

                

            }



            ActivityDTO.Atom = atomdata;

           


            
           


            await SAGSignalR.SaveActivity(VMMainViewModel.Instance.SimulationHubProxy, ActivityDTO);


            this.Close();
        }

        private void cmdInsert_Click(object sender, RoutedEventArgs e)
        {
            int j = dtGridRoute.SelectedIndex;
            if (j < 0) return;

            OsmRouteData Rdata = new OsmRouteData();

            ((List<OsmRouteData>)(dtGridRoute.ItemsSource)).Insert(j, Rdata);

            dtGridRoute.Items.Refresh();
            dtGridRoute.SelectedItem = Rdata;
            dtGridRoute.CurrentItem = Rdata;

            DataGridWPFUtility.DataGridGotoByIndex(dtGridRoute, j);

        }



       
    }
}
