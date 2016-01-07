using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;


using System.Windows;
using System.Windows.Media;

using System.ComponentModel;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;

using Microsoft.AspNet.SignalR.Client;
using System.Configuration;
using System.Collections.Specialized;

using TDSClient.SAGInterface;

namespace TDSClient
{
    public delegate void delegateDrawWPF(DrawingContext dc);
    public delegate void MouseLeftClickOnMapEventWPF(object sender, MapMouseEventArgsWPF e);
    public delegate void MouseRightClickOnMapEventWPF(object sender, MapMouseEventArgsWPF e);
  
    public class VMMainViewModel : INotifyPropertyChanged
    {

        private readonly static VMMainViewModel _instance = new VMMainViewModel();
        public static VMMainViewModel Instance
        {
            get
            {
                return _instance;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private  VMMainViewModel()
        {
            try
            {
                tskEndCycle = new Task(() =>
                {
                    Run();
                });
                tskEndCycle.Start();
                CreateHubConnection();
               
               strWMSGeoserverUrl = ConfigurationManager.AppSettings["WMSGeoserver"];
               strRoadRoutingWebApiAddress = ConfigurationManager.AppSettings["RoadRoutingWebApiAddress"];
               clsRoadRoutingWebApi.SetBaseAddress(strRoadRoutingWebApiAddress);

               PlayScenarioCommand = new DelegateCommand(x => PlayScenario(), Dispatcher.CurrentDispatcher);
               PauseScenarioCommand = new DelegateCommand(x => PauseScenario(), Dispatcher.CurrentDispatcher);
               StopScenarioCommand = new DelegateCommand(x => StopScenario(), Dispatcher.CurrentDispatcher);

               MapLayersCommand = new DelegateCommand(x => MapLayers_Click(), Dispatcher.CurrentDispatcher);
               PlanningRouteCommand = new DelegateCommand(x => PlanningRoute(), Dispatcher.CurrentDispatcher);


               SelectTools = new clsSelectTools(); 
            }
            catch(Exception ex)
            {

            }
        }

        #region Commands
   
        public DelegateCommand PlayScenarioCommand { get; set; }
        public DelegateCommand PauseScenarioCommand { get; set; }
        public DelegateCommand StopScenarioCommand { get; set; }

        public DelegateCommand MapLayersCommand { get; set; }

        public DelegateCommand PlanningRouteCommand { get; set; }

        public DelegateCommand TestCommand { get; set; }

        #endregion Commands

        public string UserName="TDS_User";
        private bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    OnPropertyChanged("IsBusy");
                }

            }
        }

        public IHubProxy SimulationHubProxy { get; set; }
        private HubConnection hubConnection = null;
        public DateTime Ex_clockDate { get; set; }

        private typGamestatus _CurrentGameStatus = typGamestatus.UNDEFINED_STATUS;
        public typGamestatus CurrentGameStatus
        {
            get { return _CurrentGameStatus; }
            set
            {
                if (_CurrentGameStatus != value)
                {
                    _CurrentGameStatus = value;


                    switch (_CurrentGameStatus)
                    {
                        case SAGInterface.typGamestatus.PROCESS_LOAD_STATUS:
                        case SAGInterface.typGamestatus.PROCESS_EDIT2RUN_STATUS:
                        case SAGInterface.typGamestatus.PROCESS_RUN2EDIT:
                            IsBusy = true;
                            break;
                        default:
                            IsBusy = false;

                            break;
                    }


                    OnPropertyChanged("CurrentGameStatus");
                }
            }
        }


       
        public delegateDrawWPF dlgUserDrawWPF = null;
        internal void NotifyUserDrawEvent(DrawingContext dc)
        {
            if (dlgUserDrawWPF == null) return;
            System.Delegate[] invklist = dlgUserDrawWPF.GetInvocationList();
            IEnumerator ie = invklist.GetEnumerator();
            while (ie.MoveNext())
            {
                delegateDrawWPF handler = (delegateDrawWPF)ie.Current;
                try
                {
                    handler.Invoke(dc);
                }
                catch (Exception ex)
                {
                    dlgUserDrawWPF -= handler;
                }

            }
        }
        public MouseLeftClickOnMapEventWPF dlgMouseLeftClickOnMapEvent = delegate { };
        internal void NotifyMouseLeftClickOnMapEvent(MapMouseEventArgsWPF args)
        {
            if (dlgMouseLeftClickOnMapEvent == null) return;

            System.Delegate[] invklist = dlgMouseLeftClickOnMapEvent.GetInvocationList();
            IEnumerator ie = invklist.GetEnumerator();
            while (ie.MoveNext())
            {
                MouseLeftClickOnMapEventWPF handler = (MouseLeftClickOnMapEventWPF)ie.Current;
                try
                {
                    
                    {
                        // IAsyncResult ar = handler.BeginInvoke(this,args, null, null);
                        // handler.EndInvoke(ar);

                        handler.Invoke(this, args);
                    }

                }
                catch (System.Exception e)
                //catch
                {
                    dlgMouseLeftClickOnMapEvent -= handler;
                }
            }


        }

        public event MouseRightClickOnMapEventWPF dlgMouseRightClickOnMapEvent = delegate { };
        internal void NotifyMouseRightClickOnMapEvent(MapMouseEventArgsWPF args)
        {
            if (dlgMouseRightClickOnMapEvent == null) return;

            dlgMouseRightClickOnMapEvent(this, args);
            

        }


        public clsSelectTools SelectTools = null;

        public Dictionary<string, structTransportCommonProperty> colGroundAtoms = new Dictionary<string, structTransportCommonProperty>(2000);
        private List<NotifyClientsEndCycleArgs> _queueEndCycleArgs = new List<NotifyClientsEndCycleArgs>();
        public Task tskEndCycle = null;

        public string strRoadRoutingWebApiAddress;
        public string strWMSGeoserverUrl;
        private GMapEx p_objMap = null;
        public GMapEx MyMainMap
        {
            get
            {
                return p_objMap;
            }
            set
            {
                p_objMap = value;                
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => DisplayMap()));               
            }
        }
        public async Task<Exception> InitMapsAsync()
        {
            try
            {
                UserMaps usermaps = null;
                this.strWMSGeoserverUrl = ConfigurationManager.AppSettings["WMSGeoserver"];
               // string strWMSGeoserverUrl = ConfigurationManager.AppSettings["WMSGeoserver"];
                if (string.IsNullOrEmpty(strWMSGeoserverUrl)) return null;
                WMSCapabilities Capabilities = await WMSProviderBase.WMSCapabilitiesRetrieve(strWMSGeoserverUrl);
                if (Capabilities != null && Capabilities.Error == null)
                {
                    List<WMSProviderBase> overlays = new List<WMSProviderBase>();
                    WMSProviderBase MapProvider = null;
                    if (VMMainViewModel.Instance.SimulationHubProxy != null)
                    {
                        usermaps = await SAGSignalR.GetUserMaps(VMMainViewModel.Instance.SimulationHubProxy, UserName);
                        if(usermaps!=null)
                        {
                            foreach (UserMapPreference info in usermaps.maps)
                             {
                                 if (overlays.Exists(t => t.Name == info.MapName)) continue;
                                 bool isExist = Capabilities.Layers.Exists(t => t.MapName == info.MapName);
                                 if (isExist)
                                 {
                                     WMSProviderBase provider = new WMSProviderBase(info.MapName);
                                     provider.Init(strWMSGeoserverUrl, info.MapName, "png");

                                     provider.MinZoom = info.MinZoom;
                                     provider.MaxZoom = info.MaxZoom;

                                     overlays.Add(provider);
                                     if (MapProvider == null)
                                     {
                                         MapProvider = provider;
                                     }
                                 }
                             }
                        }
                        if (MapProvider != null)
                        {
                            MapProvider.overlays = overlays.ToArray();
                            p_objMap.MapProvider = MapProvider;

                            p_objMap.Zoom = 10;
                            p_objMap.ShowTileGridLines = false;
                            p_objMap.InvalidateVisual(true);


                        }
                    //    p_objMap.enGMapProvider = enGMapProviders.WMSCustomProvider;                     

                    }
                }

                return Capabilities.Error;

            }
            catch (Exception ex)
            {
                return ex;
            }

            return null;
        }
        private async Task SetupMap()
        {
            Exception Result = await InitMapsAsync();
        }



        public void ChangeWMSProviderLayout(List<WMSProviderSelectedMaps> NewWMSProviderMaps)
        {
            List<WMSProviderBase> overlays = new List<WMSProviderBase>();

            WMSProviderBase MapProvider = null;
            //if (string.IsNullOrEmpty(UserSession.strWMSDefaultLayer) == false)
            //{


            //    bool isExist = Capabilities.Layers.Exists(t => t.MapName == UserSession.strWMSDefaultLayer);
            //    if (isExist)
            //    {
            //        // provider.Init(UserSession.strWMSGeoserverUrl, "WorldMaps:World 250m", "png");
            //        //  MapProvider = new WMSProviderBase();
            //        MapProvider = new WMSProviderBase(UserSession.strWMSDefaultLayer);
            //        MapProvider.Init(UserSession.strWMSGeoserverUrl, UserSession.strWMSDefaultLayer, "png");

            //        //VH 26.08.2014
            //        // MapProvider.MaxZoom = 24;
            //        // MapProvider.MinZoom = 0;


            //        MapProvider.initialized = true;


            //        overlays.Add(MapProvider);
            //    }
            //}

            if (NewWMSProviderMaps != null)
            {
                foreach (WMSProviderSelectedMaps SelectedMap in NewWMSProviderMaps)
                {
                    if (overlays.Exists(t => t.Name == SelectedMap.MapName)) continue;
                    if (SelectedMap.isSelected)
                    {
                       
                        WMSProviderBase provider = new WMSProviderBase(SelectedMap.MapName);
                        provider.Init(strWMSGeoserverUrl, SelectedMap.MapName, "png");


                        provider.MinZoom = SelectedMap.UserMinZoom;
                        provider.MaxZoom = SelectedMap.UserMaxZoom;

                        overlays.Add(provider);
                        if (MapProvider == null)
                        {
                            MapProvider = provider;
                        }
                    }
                }
            }

            if (MapProvider != null)
            {
                MapProvider.overlays = overlays.ToArray();
                p_objMap.MapProvider = MapProvider;
            }








        }

        private void DisplayMap()
        {
            try
            {
                //  MyMainMap.Init();
                MyMainMap.Manager.Mode = AccessMode.ServerOnly;
                MyMainMap.MaxZoom = 20;


                //  MyMapOpacity = MyMapOpacity;
                MyMainMap.Position = new PointLatLng(32.6846404044146, 35.3269501057924);
              
             //   MyMainMap.MapProvider = GMapProviders.BingHybridMap;

                MyMainMap.Zoom = 6;
                MyMainMap.OpacityMask = Brushes.White;

                MyMainMap.MainViewModel = this;
            }
            catch (Exception ex)
            {
                // MyLog.ReportException(ex);
            }
        }
        public void InvalidateVisual()
        {
            MyMainMap.InvalidateVisualUserDrawLayer();
        }
        public void ConvertCoordGroundToPixel(double GroundX, double GroundY, ref int PixelX, ref int PixelY)
        {      
                GMap.NET.PointLatLng Position = new GMap.NET.PointLatLng();
                Position.Lat = GroundY;
                Position.Lng = GroundX;
                GMap.NET.GPoint p = MyMainMap.FromLatLngToLocal(Position);
                PixelX = (int)p.X;
                PixelY = (int)p.Y;           

        }

        public void ConvertCoordPixelToGround(int PixelX, int PixelY, ref double GroundX, ref double GroundY)
        {
            GMap.NET.PointLatLng curPosition = MyMainMap.FromLocalToLatLng(PixelX, PixelY);
                GroundX = curPosition.Lng;
        }







        public async void CreateHubConnection()
        {
            string SignalHostUrl = string.Empty;
            string SimulationHost = ConfigurationManager.AppSettings["SimulationHost"];

            if (string.IsNullOrEmpty(SimulationHost))
            {
                return;
            }

            SignalHostUrl = SimulationHost + "signalr";          


            hubConnection = new Microsoft.AspNet.SignalR.Client.HubConnection(SignalHostUrl);
            hubConnection.Closed += hubConnection_Closed;


            SimulationHubProxy = hubConnection.CreateHubProxy("SimulationHub");


       

            SimulationHubProxy.On<NotifyClientsEndCycleArgs>("NotifyEndCycle", arg =>
                        {
                            Thread.CurrentThread.IsBackground = true;
                            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                            SignalEndCycleEvent(arg);
                        }

                  );
            try
            {

                await hubConnection.Start();
                try
                {
                    await VMMainViewModel.Instance.SimulationHubProxy.Invoke("JoinGroup", "0");               

                }

                catch (Exception e1)
                {
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to TDS server", "TDS");
            }
        }

        void hubConnection_Closed()
        {
            //  throw new NotImplementedException();
        }

        private void Run()
        {
            NotifyClientsEndCycleArgs current = null;

            while (true)
            {
                lock (_queueEndCycleArgs)
                {
                    if (_queueEndCycleArgs.Count > 0)
                    {
                        current = _queueEndCycleArgs[0];

                    }
                    else
                    {
                        Monitor.Wait(_queueEndCycleArgs);
                        if (_queueEndCycleArgs.Count > 0)
                        {
                            current = _queueEndCycleArgs[0];
                        }
                    }
                    if (_queueEndCycleArgs.Count > 0)
                    {
                        _queueEndCycleArgs.RemoveAt(0);
                    }
                }
                if (current != null)
                {

                    try
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        NotifyEndCycleEvent(current), DispatcherPriority.Background);
                    }
                    catch (Exception ex)
                    {

                    }

                    current = null;
                }
            }
        }
        private void SignalEndCycleEvent(NotifyClientsEndCycleArgs args)
        {
            lock (_queueEndCycleArgs)
            {
                if (_queueEndCycleArgs.Count > 60)
                {
                    _queueEndCycleArgs.RemoveRange(0, 50);
                }
                _queueEndCycleArgs.Add(args);
                Monitor.Pulse(_queueEndCycleArgs);
            }
        }

        private void NotifyEndCycleEvent(NotifyClientsEndCycleArgs args)
        {
            //***************************************************

            Ex_clockDate = args.Transport2Client.Ex_clockDate;
            CurrentGameStatus = args.Transport2Client.ManagerStatus;
            lock (colGroundAtoms)
            {
                if (args.Transport2Client.AtomObjectType == 0 || args.Transport2Client.AtomObjectType == 2) //Ground
                {
                    colGroundAtoms.Clear();
                    foreach (structTransportCommonProperty TR in args.Transport2Client.AtomObjectCollection)
                    {
                        if (TR.AtomClass == "TDSServer.GroundTask.clsGroundAtom")
                        {
                            if (colGroundAtoms != null)
                            {
                                if (colGroundAtoms.ContainsKey(TR.AtomName) == false)
                                    colGroundAtoms.Add(TR.AtomName, TR);
                            }
                        }
                        
                    }
                }
                else if (args.Transport2Client.AtomObjectType == 3)
                {
                    //colAirAtoms.Clear();
                    //foreach (structTransportCommonProperty TR in args.Transport2Client.AtomObjectCollection)
                    //{
                    //    if (TR.AtomClass == "GameService.AirTask.clsAirAtom")
                    //    {
                    //        if (colAirAtoms != null)
                    //        {
                    //            if (colAirAtoms.ContainsKey(TR.AtomName) == false)
                    //                colAirAtoms.Add(TR.AtomName, TR);
                    //        }
                    //    }
                    //}
                }
                else if (args.Transport2Client.AtomObjectType == 4)
                {
                    //colInfraStructureAtoms.Clear();
                    //foreach (structTransportCommonProperty TR in args.Transport2Client.AtomObjectCollection)
                    //{
                    //    {

                    //        if (colInfraStructureAtoms.ContainsKey(TR.AtomName) == false)
                    //            colInfraStructureAtoms.Add(TR.AtomName, TR);

                    //    }
                    //}
                }

                if (MyMainMap!=null)
                {
                    MyMainMap.InvalidateVisualUserDrawLayer();
                }
               

            }



        }

        internal void DrawGroundAtomWPF(structTransportCommonProperty refGroundAtom, System.Windows.Media.DrawingContext dc)
        {
            int PixelX = 0;
            int PixelY = 0;
            char ch = new char();
            string ParentName = string.Empty;
            System.Windows.Media.SolidColorBrush BackgroundBrush = new SolidColorBrush();
            System.Windows.Media.SolidColorBrush curBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);


            try
            {
                BackgroundBrush = Brushes.Red;
                curBrush.Color = System.Windows.Media.Colors.Red;
                



                MyMainMap.ConvertCoordGroundToPixel(refGroundAtom.X, refGroundAtom.Y, ref PixelX, ref PixelY);

                ch = (char)1000;         //(char)refGroundAtom.FontKey;


                System.Windows.Media.FormattedText frm2 = new System.Windows.Media.FormattedText(new string(ch, 1),
                                                    System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                                                    System.Windows.FlowDirection.LeftToRight,
                                                    new System.Windows.Media.Typeface("Simulation Font Environmental"),
                                                    42, curBrush);

                frm2.TextAlignment = System.Windows.TextAlignment.Center;
                dc.DrawText(frm2, new System.Windows.Point(PixelX, PixelY - frm2.Height / 2));





            }
            catch (Exception ex)
            {
            }


        }



        private async void PlayScenario()
        {
            structQuery2Manager Q2M = new structQuery2Manager();
            Q2M.QUERYScenarioID = 0;   
            Q2M.QueryStatus = QUERY_SCENARIOSTATUS.QUERY_START_SCENARIO;
            Q2M.ActionGUIDid = "";
            Q2M.SelectedActionOnly = false;

            try
            {
                await VMMainViewModel.Instance.SimulationHubProxy.Invoke("ChangeScenarioStatus", Q2M);

            }
            catch (Exception ex)
            {

            }
        }
        private async void PauseScenario()
        {         
            structQuery2Manager Q2M = new structQuery2Manager();
            Q2M.QUERYScenarioID = 0;   
            Q2M.QueryStatus = QUERY_SCENARIOSTATUS.QUERY_PAUSE_SCENARIO;
            Q2M.ActionGUIDid = "";
            Q2M.SelectedActionOnly = false;

            try
            {
                await VMMainViewModel.Instance.SimulationHubProxy.Invoke("ChangeScenarioStatus", Q2M);

            }
            catch (Exception ex)
            {

            }
        }
        private async void StopScenario()
        {
            

            structQuery2Manager Q2M = new structQuery2Manager();
            Q2M.QUERYScenarioID = 0;    
            Q2M.QueryStatus = QUERY_SCENARIOSTATUS.QUERY_RETURN_SCENARIO;
            Q2M.ActionGUIDid = "";
            Q2M.SelectedActionOnly = false;

            try
            {
                await VMMainViewModel.Instance.SimulationHubProxy.Invoke("ChangeScenarioStatus", Q2M);

            }
            catch (Exception ex)
            {

            }
        }
        private void MapLayers_Click()
        {            

            try
            {
                TDSClient.Forms.frmMapWMSLayers frm = new Forms.frmMapWMSLayers();
             //   Forms.ResultMessages.frmResultMessages frm = new Forms.ResultMessages.frmResultMessages();
                frm.Owner = Application.Current.MainWindow;
                frm.Show();
            }
            catch (Exception ex)
            {

            }
        }


        private void PlanningRoute()
        {
            TDSClient.Forms.frmRouteActivityRouting frm = new Forms.frmRouteActivityRouting(null);           
            frm.Owner = Application.Current.MainWindow;
            frm.Show();
        }
        public async void SetExClockRatioSpeed(int ExClockRatioSpeed)
        {
            await SAGSignalR.SetExClockRatioSpeed(VMMainViewModel.Instance.SimulationHubProxy, ExClockRatioSpeed);
          
        }
        public async Task Window_Loaded()
        {
            await SetupMap();
             VMMainViewModel.Instance.SelectTools.Initialization();
        }
    }
}
