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
using System.Configuration;
using TDSClient.SAGInterface;
namespace TDSClient.Forms
{
    /// <summary>
    /// Interaction logic for frmMapWMSLayers.xaml
    /// </summary>
    public partial class frmMapWMSLayers : Window
    {
        public frmMapWMSLayers()
        {
            InitializeComponent();
            this.Background = new LinearGradientBrush(Colors.AliceBlue, Colors.LightGray, 90);
            SetGrid();

            SetData();
        }


        public void SetGrid()
        {
            dataGridLayerName.Columns.Clear();
            dataGridLayerName.RowHeaderWidth = 0;
            dataGridLayerName.AutoGenerateColumns = false;

            dataGridLayerName.HeadersVisibility = DataGridHeadersVisibility.All;       
            dataGridLayerName.CanUserAddRows = false;
            dataGridLayerName.CanUserDeleteRows = false;
            dataGridLayerName.SelectionMode = DataGridSelectionMode.Single;            // Microsoft.Windows.Controls.DataGridSelectionMode.Single;

            dataGridLayerName.CanUserSortColumns = false;
            dataGridLayerName.AutoGenerateColumns = false;


            // dataGridLayerName.ColumnHeaderStyle = (Style)FindResource("HeaderStyle");

            dataGridLayerName.ColumnHeaderHeight = 40;



            DataGridCheckBoxColumn colSelected = new DataGridCheckBoxColumn();
            colSelected.Header = "Display";// KingsGameClientModel.Properties.Resources.strDisplay;   //  "Display";
            colSelected.IsReadOnly = false;
            colSelected.Binding = new System.Windows.Data.Binding("isSelected");
            dataGridLayerName.Columns.Add(colSelected);





            DataGridTextColumn colMapName = new DataGridTextColumn();
            colMapName.Header = "Map Layer";// KingsGameClientModel.Properties.Resources.strMapLayer; //"Map Layer";
            colMapName.IsReadOnly = true;
            colMapName.Binding = new System.Windows.Data.Binding("MapName");
            // colMapName.Binding = new System.Windows.Data.Binding("MapDBName");
            dataGridLayerName.Columns.Add(colMapName);




            DataGridTextColumn colminZoom = new DataGridTextColumn();
            colminZoom.Header = "Display Zoom From";// KingsGameClientModel.Properties.Resources.strDisplayZoom + "\n     " + KingsGameClientModel.Properties.Resources.strFrom; // "Display Zoom From";
            colminZoom.MaxWidth = 80;
            colminZoom.Binding = new System.Windows.Data.Binding("UserMinZoom");
            dataGridLayerName.Columns.Add(colminZoom);


            DataGridTextColumn colmaxZoom = new DataGridTextColumn();
            colmaxZoom.Header = "Display Zoom To";// KingsGameClientModel.Properties.Resources.strDisplayZoom + "\n     " + KingsGameClientModel.Properties.Resources.strTo; // "Display Zoom To";
            colmaxZoom.MaxWidth = 80;
            colmaxZoom.Binding = new System.Windows.Data.Binding("UserMaxZoom");
            dataGridLayerName.Columns.Add(colmaxZoom);



        }

        public async void SetData()
        {

            string strWMSGeoserverUrl = ConfigurationManager.AppSettings["WMSGeoserver"];
            if (string.IsNullOrEmpty(strWMSGeoserverUrl)) return;
            WMSCapabilities Capabilities = await WMSProviderBase.WMSCapabilitiesRetrieve(strWMSGeoserverUrl);


            List<WMSProviderSelectedMaps> HelperList = new List<WMSProviderSelectedMaps>();
            if (Capabilities != null)
            {
                //List<GMap.NET.MapProviders.GMapProvider> Layers = null;


             //   Layers = UserSession.ClientSideObject.GetGMapLayers();
                UserMaps Layers = await SAGSignalR.GetUserMaps(VMMainViewModel.Instance.SimulationHubProxy, VMMainViewModel.Instance.UserName);


                int n = 1000;
                foreach (CustomImageInfo Info in Capabilities.Layers)
                {
                    n++;
                    WMSProviderSelectedMaps map = new WMSProviderSelectedMaps();
                    map.MapName = Info.MapName;
                    map.isSelected = false;
                    map.SeqNumber = n;

                    map.UserMinZoom = 0;
                    map.UserMaxZoom = 24;

                    HelperList.Add(map);
                }

                if (Layers != null)
                {
                    for (int i = 0; i < Layers.maps.Length; i++)
                    {
                        UserMapPreference layer = Layers.maps[i];

                        WMSProviderSelectedMaps map = HelperList.Where(l => l.MapName == layer.MapName).SingleOrDefault();
                        if (map != null)
                        {
                            map.SeqNumber = i;
                            map.isSelected = true;

                            map.UserMinZoom = layer.MinZoom;
                            map.UserMaxZoom = (int)layer.MaxZoom;
                        }

                    }
                }


                HelperList.Sort(delegate(WMSProviderSelectedMaps Struct1, WMSProviderSelectedMaps Struct2)
                {

                    int Compare = 0;
                    if (Struct2.SeqNumber < Struct1.SeqNumber) Compare = 1;
                    else if (Struct2.SeqNumber == Struct1.SeqNumber) Compare = 0;
                    else Compare = -1;
                    return Compare;
                });

            }






            dataGridLayerName.ItemsSource = null;

            dataGridLayerName.ItemsSource = HelperList;

        }
        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            int i = dataGridLayerName.SelectedIndex;
            if (i < 1) return;


            WMSProviderSelectedMaps temp = ((List<WMSProviderSelectedMaps>)dataGridLayerName.ItemsSource)[i - 1];
            ((List<WMSProviderSelectedMaps>)dataGridLayerName.ItemsSource)[i - 1] = ((List<WMSProviderSelectedMaps>)dataGridLayerName.ItemsSource)[i];
            ((List<WMSProviderSelectedMaps>)dataGridLayerName.ItemsSource)[i] = temp;


            dataGridLayerName.Items.Refresh();

        }
        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            int i = dataGridLayerName.SelectedIndex;
            if (i < 0) return;
            if (i == dataGridLayerName.Items.Count - 1) return;

            WMSProviderSelectedMaps temp = ((List<WMSProviderSelectedMaps>)dataGridLayerName.ItemsSource)[i + 1];
            ((List<WMSProviderSelectedMaps>)dataGridLayerName.ItemsSource)[i + 1] = ((List<WMSProviderSelectedMaps>)dataGridLayerName.ItemsSource)[i];

            ((List<WMSProviderSelectedMaps>)dataGridLayerName.ItemsSource)[i] = temp;
            dataGridLayerName.Items.Refresh();
        }

        private async void cmdSaveUser_Click(object sender, RoutedEventArgs e)
        {
            ApplyNewLayout();

           List<WMSProviderSelectedMaps> Maps = (List<WMSProviderSelectedMaps>)dataGridLayerName.ItemsSource;
           UserMaps usermaps = new UserMaps();
           List<UserMapPreference> MapPreferenceList = new List<UserMapPreference>();
           foreach (WMSProviderSelectedMaps SelectedMap in Maps)
           {
               if (SelectedMap.isSelected)
               {           
                   UserMapPreference userMapInfo = new UserMapPreference();
                   userMapInfo.MapName = SelectedMap.MapName;
                   userMapInfo.MaxZoom = SelectedMap.UserMaxZoom;
                   userMapInfo.MinZoom = SelectedMap.UserMinZoom;
                   MapPreferenceList.Add(userMapInfo);       
               }
            }
            usermaps.maps = MapPreferenceList.ToArray<UserMapPreference>();
            usermaps.User = VMMainViewModel.Instance.UserName;

         //   usermaps = await SAGSignalR.GetUserMaps(VMMainViewModel.Instance.SimulationHubProxy, UserName);

            try
            {
                if (VMMainViewModel.Instance.SimulationHubProxy != null)
                {
                    await SAGSignalR.SaveUserMaps(VMMainViewModel.Instance.SimulationHubProxy, usermaps);


                }
            }
            catch (Exception ex)
            { } 


            //MapInfo.UserMapList = usermapDictionary.Values.ToArray<UserMapLayerInfo>();
            //MapList.Add(MapInfo);

            //if (UserSession.ClientSideObject.m_GameManagerProxy != null)
            //{
            //    UserSession.ClientSideObject.m_GameManagerProxy.SetUserMapInfoCustomProvider(MapList.ToArray<UserMapInfo>());
            //}


        }



        private void ApplyNewLayout()
        {

            List<WMSProviderSelectedMaps> Maps = new List<WMSProviderSelectedMaps>();

            Maps = (List<WMSProviderSelectedMaps>)dataGridLayerName.ItemsSource;

            VMMainViewModel.Instance.ChangeWMSProviderLayout(Maps);       

        }


        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            ApplyNewLayout();

            Close();
        }

        private void trackTransparant_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //if (isTrackTransparantInitiated == false) return;
            //UserSession.ClientSideObject.ChangeTransparency(e.NewValue);
            //UserSession.ClientSideObject.InvalidateVisual();
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
          //  UserSession.ClientSideObject.Capabilities = await WMSProviderBase.WMSCapabilitiesRetrieve(UserSession.strWMSGeoserverUrl);

            string strWMSGeoserverUrl = ConfigurationManager.AppSettings["WMSGeoserver"];
            if (string.IsNullOrEmpty(strWMSGeoserverUrl)) return ;
            WMSCapabilities Capabilities = await WMSProviderBase.WMSCapabilitiesRetrieve(strWMSGeoserverUrl);


            //if (UserSession.ClientSideObject.Capabilities.Error != null)
            //{
            //    string Msg = "WMS Server:" + "\n";
            //    Msg = Msg + UserSession.ClientSideObject.Capabilities.Error;

            //    ComponentsUtility.KGMsgBox.ShowCustomMsgOk(System.Windows.Application.Current.MainWindow,
            //                      Msg, 5000, true);
            //}
            //else
            //{
            //    SetData();
            //}

            SetData();

        }
    }
}
