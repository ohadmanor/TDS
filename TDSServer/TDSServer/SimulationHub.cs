using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;

namespace TDSServer
{
    public class SimulationHub : Hub
    {
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

       public void SaveActivity(GeneralActivityDTO ActivityDTO)
       {
           TDS.DAL.ActivityDB.SaveActivity(ActivityDTO);
           SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
           m.gameManager.m_GameObject.RefreshActivity(ActivityDTO);

       }

       public IEnumerable<GeneralActivityDTO> GetActivitesByAtomName(string AtomName)
       {
           AtomData atom = TDS.DAL.AtomsDB.GetAtomByName(AtomName);                // GetAtomByName(string Name)
           IEnumerable<GeneralActivityDTO> Activites= TDS.DAL.ActivityDB.GetMovementActivitesByAtom(atom.UnitGuid);
           foreach (GeneralActivityDTO ActivityDTO in Activites)
           {
               ActivityDTO.Atom = atom;
           }
           return Activites;
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
        public void DeleteAtomByAtomName(string AtomName)
        {
            SessionManager m = (SessionManager)GlobalHost.DependencyResolver.GetService(typeof(SessionManager));
             m.gameManager.m_GameObject.DeleteAtomByAtomName(AtomName);
        }
    }
}
