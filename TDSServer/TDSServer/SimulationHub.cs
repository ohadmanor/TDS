using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;

using TDSServer.TDS.DAL;

namespace TDSServer
{
    public class SimulationHub : Hub
    {
        public static List<string> Users = new List<string>();

        public override System.Threading.Tasks.Task OnConnected()
        {
            string clientId = GetClientId();

            lock (Users)
            {
                if (Users.IndexOf(clientId) == -1)
                {
                    Users.Add(clientId);
                }
            }


            return base.OnConnected();
        }
        public override System.Threading.Tasks.Task OnReconnected()
        {
            string clientId = GetClientId();
            lock (Users)
            {
                if (Users.IndexOf(clientId) == -1)
                {
                    Users.Add(clientId);
                }
            }

            return base.OnReconnected();
        }
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            string clientId = GetClientId();

            lock (Users)
            {
                if (Users.IndexOf(clientId) > -1)
                {
                    Users.Remove(clientId);
                }
                if (Users.Count == 0)
                {

                    structQuery2Manager Query2M = new structQuery2Manager();
                    Query2M.QueryStatus = QUERY_SCENARIOSTATUS.QUERY_RETURN_SCENARIO;

                    SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
                    m.gameManager.ChangeScenarioStatus(Query2M);

                    //TrackerApplication.TrackerManager trackManager = TrackerApplication.TrackerManager.Instance;
                    //if (trackManager != null)
                    //{
                    //    trackManager.CloseTrackerManager();
                    //}
                }
            }



            return base.OnDisconnected(stopCalled);
        }


        private string GetClientId()
        {
            string clientId = "";
            if (Context.QueryString["clientId"] != null)
            {
                // clientId passed from application 
                clientId = this.Context.QueryString["clientId"];
            }

            if (string.IsNullOrEmpty(clientId.Trim()))
            {
                clientId = Context.ConnectionId;
            }

            return clientId;
        }





        public Task JoinGroup(string groupName)
        {
            var t = Groups.Add(Context.ConnectionId, groupName);
            SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
            NotifyClientsEndCycleArgs args = new NotifyClientsEndCycleArgs();
            args = new NotifyClientsEndCycleArgs();
            args.Transport2Client.Ex_clockDate =m.gameManager.m_GameObject.Ex_clockDate;
            // args.Transport2Client.ExClockRatioSpeed = m_GameManager.ExClockRatioSpeed;
            args.Transport2Client.AtomObjectType = 2;
            args.Transport2Client.AtomObjectCollection = m.gameManager.m_GameObject.PrepareGroundCommonProperty();
            args.Transport2Client.ManagerStatus = m.gameManager.ManagerStatus;
            m.gameManager.NotifyClientsEndCycle(args);

            return t;         
        }

        public Task LeaveGroup(string groupName)
        {
            return Groups.Remove(Context.ConnectionId, groupName);
        }


       public  UserMaps GetUserMaps(string userName)
       {
           UserMaps Maps = TDS.DAL.UsersDB.GetUserMaps(userName);
           return Maps;
       }

       public void SaveUserMaps(UserMaps usrMaps)
       {
           TDS.DAL.UsersDB.SaveUserMaps(usrMaps);
       }

       public UserParameters GetUserParameters(string userName)
       {
           UserParameters Parameters = TDS.DAL.UsersDB.GetUserParameters(userName);
           return Parameters;
       }
       public void SaveUserParameters(UserParameters usrParameters)
       {
           TDS.DAL.UsersDB.SaveUserParameters(usrParameters);
       }


       public void ChangeScenarioStatus(structQuery2Manager Q2M)
       {
           try
           {
               SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
               m.gameManager.ChangeScenarioStatus(Q2M);
              
           }
           catch (Exception e)
           {
           }
       }

       public void SetExClockRatioSpeed(int pExClockRatioSpeed)
       {
           try
           {
               SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
               m.gameManager.SetExClockRatioSpeed(pExClockRatioSpeed);

           }
           catch (Exception e)
           {
           }
       }

       public GeneralActivityDTO SaveActivity(GeneralActivityDTO ActivityDTO)
       {
           GeneralActivityDTO activity= TDS.DAL.ActivityDB.SaveActivity(ActivityDTO);
           SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
           m.gameManager.m_GameObject.RefreshActivity(ActivityDTO);
           return activity;

       }


       public void DeleteActivityById(int ActivityId)
       {
           SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
           m.gameManager.m_GameObject.DeleteActivityById(ActivityId);
       }


       public   IEnumerable<GeneralActivityDTO> GetAllActivites()
       {
           IEnumerable<GeneralActivityDTO> Activites = TDS.DAL.ActivityDB.GetAllActivites();



           return Activites;
       }

       public IEnumerable<GeneralActivityDTO> GetActivitesByAtomName(string AtomName)
       {
           IEnumerable<GeneralActivityDTO> Activites = null;
           AtomData atom = TDS.DAL.AtomsDB.GetAtomByName(AtomName);                // GetAtomByName(string Name)

           if(atom!=null)
           {
               Activites = TDS.DAL.ActivityDB.GetActivitesByAtom(atom.UnitGuid);

               if (Activites!=null)
               {
                   foreach (GeneralActivityDTO ActivityDTO in Activites)
                   {
                       ActivityDTO.Atom = atom;
                   }
               }

              

           }
          


           return Activites;
       }
       public void DeleteAtomByAtomName(string AtomName)
       {
           SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
           m.gameManager.m_GameObject.DeleteAtomByAtomName(AtomName);
       }


       public void MoveGroundObject(DeployedFormation deployFormation)
       {
           SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
           m.gameManager.m_GameObject.MoveGroundObject(deployFormation);
       }


       public FormationTree SaveTreeObject(FormationTree atomDTO)
       {
           FormationTree result = TDS.DAL.AtomsDB.SaveTreeObject(atomDTO);
           return result;
       }


       public bool isAtomNameExist(string AtomName)
       {
           SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
           return m.gameManager.m_GameObject.isAtomNameExist(AtomName);
       }
       public bool isRouteNameExist(string RouteName)
       {
           SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
           return m.gameManager.m_GameObject.isRouteNameExist(RouteName);
       }

       public void DeleteAtomFromTreeByGuid(string AtomGuid)
       {
           SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
           m.gameManager.m_GameObject.DeleteAtomFromTreeByGuid(AtomGuid);
       }

       public bool isAtomNameFromTreeExist(string AtomName)
       {
           FormationTree atom = TDS.DAL.AtomsDB.GetAtomObjectFromTreeByName(AtomName);
           if (atom != null) return true;
           else return false;
       }


       public IEnumerable<FormationTree> GetAllAtomsFromTree()
       {
           SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
           IEnumerable<FormationTree> formations = m.gameManager.m_GameObject.GetAllAtomsFromTree();

           return formations;
       }

       public AtomData DeployFormationFromTree(DeployedFormation deployFormation)
       {
           SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
           AtomData atom= m.gameManager.m_GameObject.DeployFormationFromTree(deployFormation);
           return atom;
       }




       public IEnumerable<Route> getRoutes()
       {
           IEnumerable<Route> Routes = TDS.DAL.RoutesDB.getRoutes();
           return Routes;
       }

       public  Route GetRouteByName(string Name)
       {
           Route route = TDS.DAL.RoutesDB.GetRouteByName(Name);
           return route;
       }

       public void SaveRoute(Route route)
       {
             CreateUserEnum  result =  TDS.DAL.RoutesDB.SaveRoute(route);            
       }

       public void DeleteRouteByGuid(string RouteGuid)
       {
           SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
           m.gameManager.m_GameObject.DeleteRouteByGuid(RouteGuid);
       }

    }
}
