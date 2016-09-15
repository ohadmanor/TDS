using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR;
using TDSServer.GroundTask;

namespace TDSServer
{
    public class GameManager : IDisposable
    {
        private typGamestatus m_ManagerStatus = typGamestatus.UNDEFINED_STATUS;

        internal volatile bool StatusControllerThreadShouldStop = false;
        private volatile bool m_Ex_ManagerThreadShouldStop = false;


        public int ExClockRatioSpeed = 0;  // 0 - Max Speed
        public int GroundCycleResolution = 200;    //1000;// 1000 milliseconds==1 sec
        public int CollisionMeters = 10;


        public QuadTree<clsGroundAtom> QuadTreeGroundAtom = null;

		// YD: Quad tree of Dubek's structure
        public QuadTree<clsGroundAtom> QuadTreeStructure1GroundAtom = null;
		// YD: Quad tree of Malam's structure
        public QuadTree<clsGroundAtom> QuadTreeStructure2GroundAtom = null;


        private Thread StatusControllerThread;
        private Thread Ex_ManagerThread;
        private List<structQuery2Manager> QueryChangeScenarioStatusList = new List<structQuery2Manager>();
        public GameObject m_GameObject = null;

        public clsTerrain refTerrain = null;

        public GameManager()
        {
            refTerrain = clsTerrain.Instance;
            initManager();        

        }
        private void initManager()
        {
            m_GameObject = new GameObject(this);
			// YD: preload route data from database
            m_GameObject.preloadData();
            if (m_ManagerStatus == typGamestatus.UNDEFINED_STATUS)
            {
                InitObjects();
                m_ManagerStatus = typGamestatus.EDIT_STATUS;

            }
          



            ThreadStart worker = new ThreadStart(StatusController);
            StatusControllerThread = new Thread(worker);
            StatusControllerThread.Name = "StatusController";
            StatusControllerThreadShouldStop = false;
            StatusControllerThread.Start();

            //ThreadStart worker1 = new ThreadStart(m_GameObject.Ex_Manager);
            //Ex_ManagerThread = new Thread(worker1);
            //Ex_ManagerThread.Name = "Ex_Manager";
           

        }
        public typGamestatus ManagerStatus
        {
            get { return m_ManagerStatus; }

        }
        internal bool Ex_ManagerThreadShouldStop
        {
            get { return m_Ex_ManagerThreadShouldStop; }
            set
            {
               
                m_Ex_ManagerThreadShouldStop = value;
            }
        }
        public void ChangeScenarioStatus(structQuery2Manager Query2M)  
        {
            try
            {
                lock (QueryChangeScenarioStatusList)
                {
                    int QueryQty = QueryChangeScenarioStatusList.Count;
                    if (QueryQty <= 1)
                    {
                        QueryChangeScenarioStatusList.Add(Query2M);
                    }
                    else
                    {
                        QueryChangeScenarioStatusList[QueryQty - 1] = Query2M;

                    }
                    Monitor.Pulse(QueryChangeScenarioStatusList);
                }
               
            }
            catch { }
        }
        private void StatusController()
        {
            while (StatusControllerThreadShouldStop == false)
            {
                QUERY_SCENARIOSTATUS QUERYob = QUERY_SCENARIOSTATUS.UNDEFINED_STATUS;
                lock (QueryChangeScenarioStatusList)
                {
                    if (QueryChangeScenarioStatusList.Count > 0)
                    {
                        structQuery2Manager Query2M = (structQuery2Manager)QueryChangeScenarioStatusList[0];
                        QUERYob = Query2M.QueryStatus;
                    }
                    else
                    {
                        Monitor.Wait(QueryChangeScenarioStatusList);
                        if (StatusControllerThreadShouldStop)
                        {
                            return;
                        }

                       
                        if (QueryChangeScenarioStatusList.Count > 0)
                        {
                            structQuery2Manager Query2M = (structQuery2Manager)QueryChangeScenarioStatusList[0];
                            QUERYob = Query2M.QueryStatus;
                        }
                        
                    }

                    if (QueryChangeScenarioStatusList.Count > 0)
                    {
                        QueryChangeScenarioStatusList.RemoveAt(0);
                    }

                }
                switch (QUERYob)
                {
                    case QUERY_SCENARIOSTATUS.QUERY_START_SCENARIO:
                        if (m_ManagerStatus == typGamestatus.EDIT_STATUS)
                        {

                            InitObjects();

                            m_GameObject.Ex_clockGroundExecute = DateTime.MinValue;

                            ThreadStart worker1 = new ThreadStart(m_GameObject.Ex_Manager);
                            Ex_ManagerThread = new Thread(worker1);
                            Ex_ManagerThread.Name = "Ex_Manager";
                            Ex_ManagerThreadShouldStop = false;
                            Ex_ManagerThread.Start();
                          
                            m_ManagerStatus = typGamestatus.RUN_STATUS;
                        }
                        else if (m_ManagerStatus == typGamestatus.PAUSE_STATUS)
                        {                            
                          
                            if (!Ex_ManagerThread.IsAlive)
                            {
                                ThreadStart worker1 = new ThreadStart(m_GameObject.Ex_Manager);
                                Ex_ManagerThread = new Thread(worker1);
                                Ex_ManagerThread.Name = "Ex_Manager";
                                Ex_ManagerThreadShouldStop = false;
                                Ex_ManagerThread.Start();
                            }
                            m_ManagerStatus = typGamestatus.RUN_STATUS;
                        }
                        break;
                    case QUERY_SCENARIOSTATUS.QUERY_PAUSE_SCENARIO:
                        if (m_ManagerStatus == typGamestatus.RUN_STATUS)
                        {
                            m_ManagerStatus = typGamestatus.PAUSE_STATUS;
                            //CommonEventArgs arg = new CommonEventArgs();
                            //arg.NTEventCode = enNTEvents.NT_ChangeGamestatus;
                            //arg.ManagerStatus = m_ManagerStatus;
                            //NotifyClientsCommonEvents(arg);
                        }
                        break;
                    case QUERY_SCENARIOSTATUS.QUERY_RETURN_SCENARIO:
                        {
                           
                            m_ManagerStatus = typGamestatus.PROCESS_RUN2EDIT;
                           
                         //   m_GameObject.IsCancelAllThreads = true;                          
                         //   Terrain.PathManager.Clear();


                            if (Ex_ManagerThread != null)
                            {
                                
                                if (Ex_ManagerThread.IsAlive)
                                {
                                    Ex_ManagerThreadShouldStop = true;
                                    Thread.Sleep(20);
                                  //  lock (m_GameObject.GroundExecuteStateDelegateLock)
                                    {

                                        if (m_GameObject.GroundExecuteStateTask != null && m_GameObject.GroundExecuteStateTask.IsCompleted == false)
                                        {
                                            try
                                            {
                                                m_GameObject.GroundExecuteStateTask.Wait();
                                            }

                                            catch (AggregateException exx)
                                            {
                                              //  Log.WriteErrorToLog(exx, curScenario.DbName);
                                            }
                                        }
                                       


                                        //if (m_GameObject.DoSynchronizationProcess != null && m_GameObject.DoSynchronizationProcess.IsCompleted == false)
                                        //{

                                        //    try
                                        //    {
                                        //        m_GameObject.DoSynchronizationProcess.Wait();
                                        //    }

                                        //    catch (AggregateException exx)
                                        //    {
                                        //        Log.WriteErrorToLog(exx, curScenario.DbName);
                                        //    }
                                        //}
                                    }
                                    Ex_ManagerThread.Join();
                                }
                            }



                            //Victor 6.06.2012
                           // m_GameObject.ClearAllObjects();

                          

                           // CommonEventArgs arg = new CommonEventArgs();
                            //arg.NTEventCode = enNTEvents.NT_ChangeGamestatus;
                            //arg.ManagerStatus = m_ManagerStatus;
                            //NotifyClientsCommonEvents(arg);
                          



							// Yinon Douchan: Added code for statistics, not sure it should stay
                            // write collision report before resetting game
                            m_GameObject.writeCollisionReportAndClearData();
							// -----------------------------------------------------------------

                            InitObjects();

                            m_ManagerStatus = typGamestatus.EDIT_STATUS;



                            NotifyClientsEndCycleArgs args = new NotifyClientsEndCycleArgs();
                            args = new NotifyClientsEndCycleArgs();
                            args.Transport2Client.Ex_clockDate =m_GameObject.Ex_clockDate;
                            // args.Transport2Client.ExClockRatioSpeed = m_GameManager.ExClockRatioSpeed;
                            args.Transport2Client.AtomObjectType = 2;
                            args.Transport2Client.AtomObjectCollection = m_GameObject.PrepareGroundCommonProperty();
                            args.Transport2Client.ManagerStatus = ManagerStatus;
                            NotifyClientsEndCycle(args);
                         
                        }
                        break;
                }




            }
        }

        private void InitObjects( )
        {
            DAreaRect rect = new DAreaRect(-180.0, -85.0, 180.0, 85.0);           
            QuadTreeGroundAtom = new QuadTree<clsGroundAtom>(rect, 0, null);

            m_GameObject.InitObjects();

            //structQuery2Manager Query2Manager = new structQuery2Manager();
            //Query2Manager.QueryStatus = QUERY_SCENARIOSTATUS.QUERY_START_SCENARIO;
            //ChangeScenarioStatus(Query2Manager);
        }

        public void SetExClockRatioSpeed(int pExClockRatioSpeed)
        {
            ExClockRatioSpeed = pExClockRatioSpeed;



            //NotifyClientsEndCycleArgs args = new NotifyClientsEndCycleArgs();
            //args.Transport2Client.Ex_clockDate = m_GameObject.Ex_clockDate;
            //args.Transport2Client.ExClockRatioSpeed = ExClockRatioSpeed;
            //args.Transport2Client.ManagerStatus = m_ManagerStatus;
            //NotifyClientsChangeExClockRate(args);

        }
        internal void NotifyClientsEndCycle(NotifyClientsEndCycleArgs args)
        {
            NotifyClientsEndCycleArgs arg = new NotifyClientsEndCycleArgs();

            arg.Transport2Client = new structTransport2Client();
            arg.Transport2Client.RefreshInfrasite = args.Transport2Client.RefreshInfrasite;
            arg.Transport2Client.AtomObjectCollection = new structTransportCommonProperty[0];
            arg.Transport2Client.Ex_clockDate = args.Transport2Client.Ex_clockDate;
         //   arg.Transport2Client.ExClockRatioSpeed = ExClockRatioSpeed;
            arg.Transport2Client.ManagerStatus = args.Transport2Client.ManagerStatus;
            arg.Transport2Client.ScenarioID = args.Transport2Client.ScenarioID;
            arg.Transport2Client.AtomObjectType = args.Transport2Client.AtomObjectType;

            arg.Transport2Client.isEndPackage = false;
            arg.Transport2Client.typPackage = 0;
      

            arg.Transport2Client.AtomObjectCollection = args.Transport2Client.AtomObjectCollection;
            arg.Transport2Client.NavalObjectCollection = args.Transport2Client.NavalObjectCollection;
            arg.Transport2Client.isEndPackage = true;


            GlobalHost.ConnectionManager.GetHubContext<SimulationHub>().Clients.Group("0").NotifyEndCycle(args);

        
            return;




        }

        public void Dispose()
        {
            try
            {
                lock (QueryChangeScenarioStatusList)
                {
                    if (StatusControllerThread != null)
                    {
                        if (StatusControllerThread.IsAlive)
                        {                         
                            StatusControllerThreadShouldStop = true;
                            Monitor.PulseAll(QueryChangeScenarioStatusList);
                            Thread.Sleep(1000);
                        }

                    }
                    QueryChangeScenarioStatusList.Clear();

                }
                if (StatusControllerThread != null)
                {
                    StatusControllerThread.Join();
                }
            }
            catch (Exception ex)
            {
              //  Log.WriteErrorToLog(ex, curScenario.DbName);
            } 
 


            if (Ex_ManagerThread != null)
            {
                 if (Ex_ManagerThread.IsAlive)
                 {
                     Ex_ManagerThreadShouldStop = true;
                     Thread.Sleep(1000);

                     Ex_ManagerThread.Join();
                 }
            }

        }
    }
}
