using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using TDSClient.SAGInterface;
using TerrainService;


//using SAGClient.Forms.GroundTaskControls;

namespace TDSClient
{
    public class clsSelectTools
    {
        System.Windows.Controls.ContextMenu SelectMenu;
        System.Windows.Controls.ContextMenu contextmenu;

        delegate void Del();

      //  private readonly static clsSelectTools _instance = new clsSelectTools();
       //public static clsSelectTools Instance
       // {
       //     get
       //     {
       //         return _instance;
       //     }
       // }
        public clsSelectTools()
        {
          
          //  VMMainViewModel.dlgMouseRightClickOnMapEvent += MapOnRightClickWPF;  

          //  VMMainViewModel.dlgMouseRightClickOnMapEvent += MapOnRightClickWPF;  
          // VMMainViewModel.Instance.dlgMouseRightClickOnMapEvent += MapOnRightClickWPF;
        }

     
        public void Initialization(  )
        {
            VMMainViewModel.Instance.dlgMouseRightClickOnMapEvent += MapOnRightClickWPF;       
       
        }

       

        void MapOnRightClickWPF(object sender, MapMouseEventArgsWPF e)
        {
            double SearchRadius = 2000;
            double SearchNeiborRadiuse = 200;

            double minDistance = double.MaxValue;
            structTransportCommonProperty minDistAtom = null;
            List<structTransportCommonProperty> NeighborList = new List<structTransportCommonProperty>();



            double resolution = 0;
            double zoom = VMMainViewModel.Instance.MyMainMap.Zoom;
            //* Math.Cos(TerrainService.MathEngine.DegreesToRadians(e.MapYLongLatWGS84))
            resolution = (156543) / Math.Pow(2, zoom);

            SearchNeiborRadiuse = resolution * 10;



            System.Windows.Application.Current.MainWindow.ContextMenu = null;

         //  System.Windows.Controls.ContextMenu contextmenu = new System.Windows.Controls.ContextMenu();

          //  if (VMMainViewModel.Instance.currSelectionType == enSelection.Ground)
            {
                minDistance = double.MaxValue;
                if (VMMainViewModel.Instance.colGroundAtoms != null)
                {
                    foreach (structTransportCommonProperty Tr in VMMainViewModel.Instance.colGroundAtoms.Values)
                    {


                       double Distance = Utilites.GreatCircleDistance(Tr.X, Tr.Y, e.MapXLongLatWGS84, e.MapYLongLatWGS84);

                       if (Distance < minDistance && Distance > 0)
                        {
                            minDistance = Distance;
                            minDistAtom = Tr;
                        }
                    }


                }

            }

            SelectMenu = new System.Windows.Controls.ContextMenu();

            SelectMenu.Style = null;

            DPoint coordinates = new DPoint(e.MapXLongLatWGS84, e.MapYLongLatWGS84);

            // add menu item for adding barriers
            System.Windows.Controls.MenuItem mBarrier = new System.Windows.Controls.MenuItem();
            mBarrier.Header = "Add Barrier";
            mBarrier.Tag = coordinates;
            mBarrier.Click += miAddBarier_Click;
            SelectMenu.Items.Add(mBarrier);

            System.Windows.Controls.MenuItem mCoordinates = new System.Windows.Controls.MenuItem();
            mCoordinates.Header = e.MapXLongLatWGS84.ToString() + " " + e.MapYLongLatWGS84.ToString();
            mCoordinates.Tag = coordinates;
            mCoordinates.Click += miCoordinates_Click;
            SelectMenu.Items.Add(mCoordinates);
            System.Windows.Application.Current.MainWindow.ContextMenu = SelectMenu;

            if (minDistAtom != null && minDistance < SearchRadius)
             {
                //Create PopUP menu
                 if (minDistAtom.AtomClass == "TDSServer.GroundTask.clsGroundAtom")
                 {

                     foreach (structTransportCommonProperty Tr in VMMainViewModel.Instance.colGroundAtoms.Values)
                     {
                         double  Distance = TerrainService.MathEngine.CalcDistanceForPerformance(Tr.X, Tr.Y, minDistAtom.X, minDistAtom.Y);
                         if (Distance <= SearchNeiborRadiuse)
                         {
                             NeighborList.Add(Tr);

                         }
                     }

                     if (NeighborList.Count==1)
                     {
                         ShowGroundSelectAtomMenu(null, minDistAtom);
                     }
                     else
                     {
                       // System.Windows.Controls.ContextMenu SelectMenu = new System.Windows.Controls.ContextMenu();
                         SelectMenu = new System.Windows.Controls.ContextMenu();

                        SelectMenu.Style = null;
                       
                         for (int i = 0; i < NeighborList.Count; i++)
                         {
                            // System.Windows.Controls.MenuItem mSelectGroundAtom = new System.Windows.Controls.MenuItem();
                            // mSelectGroundAtom.Style = null;

                             System.Windows.Controls.MenuItem mSelectGroundAtom = new System.Windows.Controls.MenuItem();
                             mSelectGroundAtom.Header =  NeighborList[i].AtomName;
                             mSelectGroundAtom.Tag = NeighborList[i];
                             mSelectGroundAtom.Focusable = false;
                             mSelectGroundAtom.Click += miName_Click;
                             SelectMenu.Items.Add(mSelectGroundAtom);
                         }

                         System.Windows.Application.Current.MainWindow.ContextMenu = SelectMenu;
                         return;
                     }





                 }
             }
           
        }

        async void miAddBarier_Click(object sender, RoutedEventArgs e)
        {
            // add a police barrier in the closest route
            DPoint coordinates = (DPoint)((System.Windows.Controls.MenuItem)sender).Tag;
            enOSMhighwayFilter highwayFilter = enOSMhighwayFilter.Undefined;
            shPointId PointId = await clsRoadRoutingWebApi.GetNearestPointIdOnRoad("0", highwayFilter, coordinates.X, coordinates.Y);
        }

        void miCoordinates_Click(object sender, RoutedEventArgs e)
        {
            // copy coordinates to clipboard
            DPoint coordinates = (DPoint)((System.Windows.Controls.MenuItem)sender).Tag;
            Clipboard.SetText(coordinates.X + ", " + coordinates.Y);
        }

        void miName_Click(object sender, RoutedEventArgs e)
        {
           structTransportCommonProperty CommonProperty = (structTransportCommonProperty)((System.Windows.Controls.MenuItem)sender).Tag;
           ShowGroundSelectAtomMenu(sender, CommonProperty);
        }


        private void ShowGroundSelectAtomMenu(object sender,structTransportCommonProperty minDistAtom)
        {
            System.Windows.Application.Current.MainWindow.ContextMenu = null;

         //   System.Windows.Controls.ContextMenu contextmenu = new System.Windows.Controls.ContextMenu();
             contextmenu = new System.Windows.Controls.ContextMenu();


            System.Windows.Controls.MenuItem miName = new System.Windows.Controls.MenuItem();
            miName.Header = Properties.Resources.strNameColon + " " + minDistAtom.AtomName;///+ " " + minDistAtom.TextValue.Replace("_", "__"); ;
            miName.Focusable = false;
            contextmenu.Items.Add(miName);

            System.Windows.Controls.Separator Sp2 = new System.Windows.Controls.Separator();
            contextmenu.Items.Add(Sp2);


            System.Windows.Controls.MenuItem miGroundMission = new System.Windows.Controls.MenuItem();
            miGroundMission.Header = "Activity";// "Force Mission";
            miGroundMission.Name = "miGroundMission";
            miGroundMission.Tag = minDistAtom;
            miGroundMission.Click += new System.Windows.RoutedEventHandler(mnuGroundMission_Click);
            contextmenu.Items.Add(miGroundMission);



            System.Windows.Controls.MenuItem miMove = new System.Windows.Controls.MenuItem();
            miMove.Header = Properties.Resources.strMove; // "Move";
            miMove.Tag = minDistAtom;
            miMove.Name = "mnuMove";

            miMove.Click += miMove_Click;     // new System.Windows.RoutedEventHandler(mnuMove_Click);
            contextmenu.Items.Add(miMove);

            if(VMMainViewModel.Instance.CurrentGameStatus!= typGamestatus.EDIT_STATUS)
            {
                miMove.IsEnabled = false;
            }


            System.Windows.Controls.MenuItem miDeleteAtom = new System.Windows.Controls.MenuItem();
            miDeleteAtom.Header = Properties.Resources.strUndeploy;// Properties.Resources.strDelete;
            miDeleteAtom.Name = "miDeleteAtom";
            miDeleteAtom.Tag = minDistAtom;
            miDeleteAtom.Click += miDeleteAtom_Click;
            contextmenu.Items.Add(miDeleteAtom);

           // System.Windows.Application.Current.MainWindow.ContextMenu = new System.Windows.Controls.ContextMenu();
            System.Windows.Application.Current.MainWindow.ContextMenu = contextmenu;

            contextmenu.Visibility = Visibility.Visible;
            System.Windows.Application.Current.MainWindow.ContextMenu.Visibility = Visibility.Visible;
            System.Windows.Application.Current.MainWindow.ContextMenu.IsOpen = true;
          //  System.Windows.Application.Current.MainWindow.ContextMenu.Width = 1000;
        }

        void miMove_Click(object sender, RoutedEventArgs e)
        {
            VMMainViewModel.Instance .MoveObjectCommonProperty= (structTransportCommonProperty)((System.Windows.Controls.MenuItem)sender).Tag;
            VMMainViewModel.Instance.CurrMapTool = enMapTool.MoveObjectTool;
        }


        async  void  miDeleteAtom_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show(TDSClient.Properties.Resources.strAreyousureyouwanttodeleteit, "TDS", MessageBoxButton.YesNo);
            if(res== MessageBoxResult.Yes)
            {
                structTransportCommonProperty CommonProperty = (structTransportCommonProperty)((System.Windows.Controls.MenuItem)sender).Tag;
                await TDSClient.SAGInterface.SAGSignalR.DeleteAtomByAtomName(VMMainViewModel.Instance.SimulationHubProxy, CommonProperty.AtomName);
            }
        }

     

        public   void mnuGroundMission_Click(object sender, RoutedEventArgs e)
        {
            structTransportCommonProperty CommonProperty = (structTransportCommonProperty)((System.Windows.Controls.MenuItem)sender).Tag;


            TDSClient.Forms.frmActivities frm = new Forms.frmActivities(CommonProperty);
            frm.Owner = Application.Current.MainWindow;
            frm.Show();

            //IEnumerable<GeneralActivityDTO> Activites = await TDSClient.SAGInterface.SAGSignalR.GetActivitesByAtomName(VMMainViewModel.Instance.SimulationHubProxy, CommonProperty.AtomName);

            //if(Activites!=null && Activites.Count()>0)
            //{
            //    GeneralActivityDTO pActivityDTO = Activites.ToArray<GeneralActivityDTO>()[0];
            //    TDSClient.Forms.frmRouteActivityRouting frm = new Forms.frmRouteActivityRouting(pActivityDTO);
            //    frm.Owner = Application.Current.MainWindow;
            //    frm.Show();
            //}       
                    


        }
    }
}
