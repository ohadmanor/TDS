using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using TDSClient.SAGInterface;


//using SAGClient.Forms.GroundTaskControls;

namespace TDSClient
{
    public class clsSelectTools
    {
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
            double minDistance = double.MaxValue;
            structTransportCommonProperty minDistAtom = null;
            System.Windows.Controls.ContextMenu contextmenu = new System.Windows.Controls.ContextMenu();

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

            if (minDistAtom != null && minDistance < SearchRadius)
             {
                //Create PopUP menu
                 if (minDistAtom.AtomClass == "TDSServer.GroundTask.clsGroundAtom")
                 {
                   

                     System.Windows.Controls.MenuItem miName = new System.Windows.Controls.MenuItem();
                     miName.Header = Properties.Resources.strNameColon + " " + minDistAtom.AtomName;///+ " " + minDistAtom.TextValue.Replace("_", "__"); ;
                     miName.Focusable = false;
                     contextmenu.Items.Add(miName);

                     System.Windows.Controls.Separator Sp2 = new System.Windows.Controls.Separator();
                     contextmenu.Items.Add(Sp2);

                    // if (!minDistAtom.isVirtualAtom)
                    // {
                         System.Windows.Controls.MenuItem miGroundMission = new System.Windows.Controls.MenuItem();
                         miGroundMission.Header ="Activity";// "Force Mission";
                         miGroundMission.Name = "miGroundMission";
                         miGroundMission.Tag = minDistAtom;
                         miGroundMission.Click += new System.Windows.RoutedEventHandler(mnuGroundMission_Click);
                         contextmenu.Items.Add(miGroundMission);
                    // }

                         System.Windows.Controls.MenuItem miDeleteAtom = new System.Windows.Controls.MenuItem();
                         miDeleteAtom.Header = Properties.Resources.strDelete;
                         miDeleteAtom.Name = "miDeleteAtom";
                         miDeleteAtom.Tag = minDistAtom;
                         miDeleteAtom.Click += miDeleteAtom_Click;
                         contextmenu.Items.Add(miDeleteAtom);



                 }
             }
            System.Windows.Application.Current.MainWindow.ContextMenu = contextmenu;
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

     

        public  async  void mnuGroundMission_Click(object sender, RoutedEventArgs e)
        {
            structTransportCommonProperty CommonProperty = (structTransportCommonProperty)((System.Windows.Controls.MenuItem)sender).Tag;

            IEnumerable<GeneralActivityDTO> Activites = await TDSClient.SAGInterface.SAGSignalR.GetActivitesByAtomName(VMMainViewModel.Instance.SimulationHubProxy, CommonProperty.AtomName);
            if(Activites!=null && Activites.Count()>0)
            {
                GeneralActivityDTO pActivityDTO = Activites.ToArray<GeneralActivityDTO>()[0];
                TDSClient.Forms.frmRouteActivityRouting frm = new Forms.frmRouteActivityRouting(pActivityDTO);
                frm.Owner = Application.Current.MainWindow;
                frm.Show();
            }
            
            
           // GeneralActivityDTO pActivityDTO = null;

           

           
           


            //KingsGameClientModel.GameManagerProxy.AtomBase refAtomBase;
            //KingsGameClientModel.GameManagerProxy.clsGroundAtom refGroundAtom;
          
            //foreach (Window wnd in Application.Current.Windows)
            //{
            //    frmGroundMissions wind = wnd as frmGroundMissions;
            //    if (wind != null)
            //    {
            //        if (wind.UnitIdentification == CommonProperty.AtomName && wnd.IsVisible == true)
            //            return;
            //    }
            //}

            //refAtomBase = UserSession.ClientSideObject.m_GameManagerProxy.GetAtomBase(CommonProperty.AtomName);
            //refGroundAtom = refAtomBase as KingsGameClientModel.GameManagerProxy.clsGroundAtom;

            //if (refGroundAtom != null)
            //{

            //    frmGroundMissions frm = new frmGroundMissions(refGroundAtom.MyName);
            //    frm.Owner = Application.Current.MainWindow;
            //    frm.Show();
            //}



        }
    }
}
