using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Threading;
using System.Windows.Threading;


using System.Windows.Controls;

using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;


using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

using System.Windows;

using GMap.NET.WindowsPresentation;
using System.Windows.Media;

using System.Globalization;

using TDSClient.SAGInterface;
using TerrainService;
//using SAGClient.ViewModels;
//using SAGClient.SAGInterface;

namespace TDSClient
{
    public class GMapEx : GMapControl
    {
        public event NotifyAtomDeployedEvent AtomDeployedEvent;

        delegate void Del();
        readonly Typeface tf = new Typeface("GenericSansSerif");
        readonly System.Windows.FlowDirection fd = new System.Windows.FlowDirection();
        GMapMarker marker = null;
        public UserDrawLayer UserLayer = null;

        public VMMainViewModel MainViewModel = null;


        public GMapEx()
        {
            this.AllowDrop = true;
            GMap.NET.PointLatLng Position = new GMap.NET.PointLatLng(0, 0);
            marker = new GMapMarker(Position);
            UserLayer = new UserDrawLayer();
            marker.Shape = UserLayer;
            this.Markers.Add(marker);
        }


        public void ConvertCoordGroundToPixel(double GroundX,double GroundY,ref int PixelX,ref int PixelY)
        {
            GMap.NET.PointLatLng Position = new GMap.NET.PointLatLng();
            Position.Lat = GroundY;
            Position.Lng = GroundX;
            GMap.NET.GPoint p = FromLatLngToLocal(Position);
            PixelX = (int)p.X;
            PixelY = (int)p.Y;
        }
        public void CenterOnGroundPointZoom(double aX, double aY)
        {
            this.Position = new GMap.NET.PointLatLng(aY, aX);
          //  this.Zoom = this.currZoom + 1;
            this.Zoom = this.Zoom + 1;
            int XDown = (int)0;
            int YDown = (int)0;
            GMap.NET.PointLatLng curPosition = FromLocalToLatLng(XDown, YDown);
            marker.Position = curPosition;
            // InvalidateVisualGeneralAtoms();
            return;
        }

        protected  async override void OnDrop( System.Windows.DragEventArgs e)
        {
            double currMapX = 0;
            double currMapY = 0;           

            System.Windows.DataObject d = (System.Windows.DataObject)e.Data;

            string[] dataFormats = d.GetFormats();
            string dataText = d.GetText();

            Point position = e.GetPosition(this);
            GMap.NET.PointLatLng curPosition = FromLocalToLatLng((int)position.X, (int)position.Y);
            currMapX = curPosition.Lng;
            currMapY = curPosition.Lat;

            for (int i = 0; i < dataFormats.Length; i++)
            {
                string dragFormat = dataFormats[i];

                if (dragFormat.Contains("FormationTree") && dataText == "Actor")
                {                 
                   
                    object dragObject = d.GetData(dragFormat);

                    FormationTree formation = dragObject as FormationTree;
                    if (formation == null) continue;

                    enOSMhighwayFilter highwayFilter = enOSMhighwayFilter.Undefined;
                    SetHighwayFilter(highwayFilter);
                    shPointId PointId = await clsRoadRoutingWebApi.GetNearestPointIdOnRoad("0", highwayFilter, currMapX, currMapY);
                    if (PointId!=null)
                    {
                       shPoint pnt = PointId.point;
                       DeployedFormation deployFormation = new DeployedFormation();
                       deployFormation.x = pnt.x;
                       deployFormation.y = pnt.y;
                       deployFormation.formation = formation;

                        AtomData atom=   await TDSClient.SAGInterface.SAGSignalR.DeployFormationFromTree(VMMainViewModel.Instance.SimulationHubProxy, deployFormation);
                        if(atom!=null)
                        {
                            AtomDeployedEventArgs args = new AtomDeployedEventArgs();
                            args.atom = atom;
                            if(AtomDeployedEvent!=null)
                            {
                                AtomDeployedEvent(this, args);                                
                            }
                        }
                    }

                    return;
                }
            }

      


        }

        public void SetHighwayFilter(enOSMhighwayFilter highwayFilter)
        {
            highwayFilter = highwayFilter | enOSMhighwayFilter.CarMostImportant;
            highwayFilter = highwayFilter | enOSMhighwayFilter.CarMediumImportant;
            highwayFilter = highwayFilter | enOSMhighwayFilter.CarLowImportant;
            highwayFilter = highwayFilter | enOSMhighwayFilter.Construction;

        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

           int XDown = (int)0;
           int YDown = (int)0;// (int)ActualHeight - 100;
           GMap.NET.PointLatLng curPosition = FromLocalToLatLng(XDown, YDown);
           marker.Position = curPosition;

            InvalidateVisualStatusBar();
            InvalidateVisualUserDrawLayer();
        }

      //  protected override void 

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == "Zoom")
            {
               // PrevZoom = System.Convert.ToDouble(e.OldValue);
                InvalidateVisualUserDrawLayer();
            }
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            int XDown = (int)0;
            int YDown = (int)0; // sizeInfo.NewSize.Height - 100;
            GMap.NET.PointLatLng curPosition = FromLocalToLatLng(XDown, YDown);
            marker.Position = curPosition;

            UserLayer.Width = sizeInfo.NewSize.Width;
            UserLayer.Height = sizeInfo.NewSize.Height;
           // InvalidateVisualStatusBar();
        }



        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);


            Point pnt = e.GetPosition(this);
            int XDown = (int)pnt.X;
            int YDown = (int)pnt.Y;
            GMap.NET.PointLatLng Position = FromLocalToLatLng(XDown, YDown);


            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                MapMouseEventArgsWPF me = new MapMouseEventArgsWPF();


                me.MapXLongLatWGS84 = Position.Lng;
                me.MapYLongLatWGS84 = Position.Lat;
                me.X = pnt.X;
                me.Y = pnt.Y;
                try
                {
                    VMMainViewModel.Instance.NotifyMouseLeftClickOnMapEvent(me);
                }
                catch (Exception ex)
                {
                }


            }
            else if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                MapMouseEventArgsWPF me = new MapMouseEventArgsWPF();
                me.MapXLongLatWGS84 = Position.Lng;
                me.MapYLongLatWGS84 = Position.Lat;
                me.X = pnt.X;
                me.Y = pnt.Y;
                try
                {
                   VMMainViewModel.Instance.NotifyMouseRightClickOnMapEvent(me);
                }
                catch (Exception ex)
                {
                }

            }

        }




        protected override void  OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Point pnt = e.GetPosition(this);
            int XDown = (int)pnt.X;
            int YDown = (int)pnt.Y;

            XDown = (int)0;
            YDown = (int)0;// (int)ActualHeight - 100;
            GMap.NET.PointLatLng curPosition = FromLocalToLatLng(XDown, YDown);
            marker.Position = curPosition;
            if (IsDragging)
            {
                InvalidateVisualUserDrawLayer();
            }
            InvalidateVisualStatusBar();
        }
        protected override void  OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsDragging)
            {
              //  IsDraggingStarted = false;
                InvalidateVisualUserDrawLayer();

                Cursor = null;
                Mouse.Capture(null);
            }
           


            base.OnMouseUp(e);
        }
        public void InvalidateVisualStatusBar()
        {

            Del d;
            //d = delegate()
            //{
                 try
                 {

                     if (MainViewModel == null) return;

                        using (DrawingContext dc = UserLayer.ObjectDrawingVisualStatusBar.RenderOpen())
                        {
                            string strEx_clockDate = MainViewModel.Ex_clockDate.ToString("dd/MM/yyyy HH:mm:ss");

                            FormattedText FTEx_clockDate = new FormattedText(strEx_clockDate, CultureInfo.CurrentUICulture, fd, tf, 20, Brushes.Yellow);
                            dc.DrawText(FTEx_clockDate, new Point(this.ActualWidth - FTEx_clockDate.Width - 20, this.ActualHeight - FTEx_clockDate.Height));

                            /*
                            if (FTCoordinate != null)
                                dc.DrawText(FTCoordinate, new Point(this.ActualWidth - FTCoordinate.Width - 300, this.ActualHeight - FTCoordinate.Height));
                            if (FTTerrainHeight != null)
                                dc.DrawText(FTTerrainHeight, new Point(this.ActualWidth - FTTerrainHeight.Width - 700, this.ActualHeight - FTTerrainHeight.Height));


                            if (FTDistanceLabel != null)
                                dc.DrawText(FTDistanceLabel, new Point(this.ActualWidth - FTDistanceLabel.Width - 900, this.ActualHeight - FTDistanceLabel.Height));

                            FormattedText FTZoom = new FormattedText("Zoom: " + Zoom, CultureInfo.CurrentUICulture, fd, tf, 20, Brushes.Yellow);

                            dc.DrawText(FTZoom, new Point(0.0, this.ActualHeight - FTZoom.Height));
                            */
                        }
                  }
                catch(Exception ex)
                 {

                 }

            //};



            //Dispatcher.BeginInvoke(d, null);



        }


        public void InvalidateVisualUserDrawLayer()
        {
            Del d;
            //d = delegate()
            //{
               

                InvalidateVisualGeneralAtoms();
                InvalidateVisualAirAtoms();
                    

                InvalidateVisualStatusBar();



          //  };
          ////  Dispatcher.BeginInvoke(d, null);
          //  Dispatcher.Invoke(d, null);
        }
        public void InvalidateVisualAirAtoms()
        {
            //if (MainViewModel == null) return;
            //lock (MainViewModel.colAirAtoms)
            //{
            //    using (DrawingContext dc = UserLayer.ObjectDrawingVisualAir.RenderOpen())
            //    {
            //        if (MainViewModel != null && MainViewModel.colAirAtoms != null)
            //        {
            //            foreach (structTransportCommonProperty Tr in MainViewModel.colAirAtoms.Values)
            //            {
            //                MainViewModel.DrawAirAtomWPF(Tr, dc);
            //            }

            //        }
            //    }
            //}
                      
           
        }
        public void InvalidateVisualGeneralAtoms()
        {
            if (MainViewModel == null) return;
            lock (MainViewModel.colGroundAtoms)
            {
                using (DrawingContext dc = UserLayer.ObjectDrawingVisualGeneralAtoms.RenderOpen())
                {
                    VMMainViewModel.Instance.NotifyUserDrawEvent(dc);
                    if (MainViewModel.colGroundAtoms != null)
                    {
                        foreach (structTransportCommonProperty Tr in MainViewModel.colGroundAtoms.Values)
                        {
                            MainViewModel.DrawGroundAtomWPF(Tr, dc);
                        }

                    }
                  //  VMMainViewModel.Instance.NotifyUserDrawEvent(dc);


                    //if (MainViewModel.colInfraStructureAtoms != null)
                    //{
                    //    foreach (structTransportCommonProperty Tr in MainViewModel.colInfraStructureAtoms.Values)
                    //    {
                    //        MainViewModel.DrawTargetAtomWPF(Tr, dc);
                    //    }

                    //}
                }
            }


        }
    }
    public class UserDrawLayer : Canvas
    {
        public DrawingVisual ObjectDrawingVisual = new DrawingVisual();

        public DrawingVisual ObjectDrawingVisualGeneralAtoms = new DrawingVisual();


        public DrawingVisual ObjectDrawingVisualAir = new DrawingVisual();
        public DrawingVisual ObjectDrawingVisualStatusBar = new DrawingVisual();

        public DrawingVisual ObjectDrawingVisualBullets = new DrawingVisual();

        public UserDrawLayer()
        {
            AddVisualChild(ObjectDrawingVisual);
            AddLogicalChild(ObjectDrawingVisual);


            AddVisualChild(ObjectDrawingVisualGeneralAtoms);
            AddLogicalChild(ObjectDrawingVisualGeneralAtoms);


            AddVisualChild(ObjectDrawingVisualAir);
            AddLogicalChild(ObjectDrawingVisualAir);

            AddVisualChild(ObjectDrawingVisualStatusBar);
            AddLogicalChild(ObjectDrawingVisualStatusBar);

            AddVisualChild(ObjectDrawingVisualBullets);
            AddLogicalChild(ObjectDrawingVisualBullets);




        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

          //  VMMainViewModel.Instance.NotifyUserDrawEvent(dc);

        }
        protected override int VisualChildrenCount
        {
            get
            {
                return 5;
            }
        }
        protected override Visual GetVisualChild(int index)
        {
            if (index == 0)
            {
                return ObjectDrawingVisual;
            }
            else if (index == 1)
            {
                return ObjectDrawingVisualGeneralAtoms;
            }
            else if (index == 2)
            {
                return ObjectDrawingVisualAir;
            }
            else if (index == 3)
            {
                return ObjectDrawingVisualStatusBar;
            }
            else if (index == 4)
            {
                return ObjectDrawingVisualBullets;
            }


            return null;
        }

    }




}
