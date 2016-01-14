using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using System.IO;
using System.IO.Compression;
using System.Data;

namespace TDSClient.SAGInterface
{
    public static class SAGSignalR
    {
        public static async Task<UserMaps> GetUserMaps(IHubProxy HubProxy, string userName)
        {
            try
            {
                UserMaps maps = await HubProxy.Invoke<UserMaps>("GetUserMaps", userName);
                return maps;
            }
            catch (Exception ex)
            {
            }

            return null;
        }
        //IEnumerable<GeneralActivityDTO>

        public static async Task<IEnumerable<GeneralActivityDTO>> GetActivitesByAtomName(IHubProxy HubProxy, string AtomName)
        {
            try
            {
                IEnumerable<GeneralActivityDTO> Activites = await HubProxy.Invoke<IEnumerable<GeneralActivityDTO>>("GetActivitesByAtomName", AtomName);
                return Activites;
            }
            catch (Exception ex)
            {
            }

            return null;
        }

        public static async Task<IEnumerable<GeneralActivityDTO>> GetAllActivites(IHubProxy HubProxy)
        {
            try
            {
                IEnumerable<GeneralActivityDTO> activities = await HubProxy.Invoke<IEnumerable<GeneralActivityDTO>>("GetAllActivites");
                return activities;
            }
            catch (Exception ex)
            {
            }
            return null;
        }


        public static async Task SaveUserMaps(IHubProxy HubProxy, UserMaps usrMaps)
        {
            try
            {
                await HubProxy.Invoke<UserMaps>("SaveUserMaps", usrMaps);
            }
            catch (Exception ex)
            {
            }

        }

        public static async Task<UserParameters> GetUserParameters(IHubProxy HubProxy, string userName)
        {
            try
            {
                UserParameters Parameters = await HubProxy.Invoke<UserParameters>("GetUserParameters", userName);
                return Parameters;
            }
            catch (Exception ex)
            {
            }

            return null;
        }

        public static async Task SaveUserParameters(IHubProxy HubProxy, UserParameters usrParameters)
        {
            try
            {
                await HubProxy.Invoke<UserMaps>("SaveUserParameters", usrParameters);
            }
            catch (Exception ex)
            {
            }

        }



        public static async Task<GeneralActivityDTO> SaveActivity(IHubProxy HubProxy, GeneralActivityDTO ActivityDTO)
        {
            try
            {
                 GeneralActivityDTO activity= await HubProxy.Invoke<GeneralActivityDTO>("SaveActivity", ActivityDTO);
                 return activity;
            }
            catch (Exception ex)
            {
            }
            return null;
        }



        public static async Task DeleteActivityById(IHubProxy HubProxy, int ActivityId)
        {
            try
            {
                await HubProxy.Invoke<GeneralActivityDTO>("DeleteActivityById", ActivityId);
            }
            catch (Exception ex)
            {
            }

        }

      


        public static async Task SetExClockRatioSpeed(IHubProxy HubProxy, int pExClockRatioSpeed)
        {
            try
            {
                await HubProxy.Invoke<GeneralActivityDTO>("SetExClockRatioSpeed", pExClockRatioSpeed);
            }
            catch (Exception ex)
            {
            }

        }


        public static async Task<bool> isAtomNameExist(IHubProxy HubProxy, string AtomName)
        {
            try
            {
                bool exist = await HubProxy.Invoke<bool>("isAtomNameExist", AtomName);
                return exist;
            }
            catch (Exception ex)
            {
            }

            return true;
        }

        public static async Task<bool> isAtomNameFromTreeExist(IHubProxy HubProxy, string AtomName)
        {
            try
            {
                bool exist = await HubProxy.Invoke<bool>("isAtomNameFromTreeExist", AtomName);
                return exist;
            }
            catch (Exception ex)
            {
            }

            return true;
        }




        public static async Task DeleteAtomByAtomName(IHubProxy HubProxy, string AtomName)
        {
            try
            {
                await HubProxy.Invoke<GeneralActivityDTO>("DeleteAtomByAtomName", AtomName);
            }
            catch (Exception ex)
            {
            }

        }
        public static async Task MoveGroundObject(IHubProxy HubProxy, DeployedFormation deployFormation)
        {
            try
            {
                await HubProxy.Invoke<DeployedFormation>("MoveGroundObject", deployFormation);
            }
            catch (Exception ex)
            {
            }

        }
        //MoveGroundObject

        public static async Task<bool> isRouteNameExist(IHubProxy HubProxy, string RouteName)
        {
            try
            {
                bool exist = await HubProxy.Invoke<bool>("isRouteNameExist", RouteName);
                return exist;
            }
            catch (Exception ex)
            {
            }

            return true;
        }

        public static async Task<IEnumerable<Route>>   getRoutes(IHubProxy HubProxy)
        {
            try
            {
                IEnumerable<Route> Routes = await HubProxy.Invoke<IEnumerable<Route>>("getRoutes");
                return Routes;
            }
            catch (Exception ex)
            {
            }

            return null;
        }

        public static async Task<Route> GetRouteByName(IHubProxy HubProxy, string Name)
        {
            try
            {
                Route route = await HubProxy.Invoke<Route>("GetRouteByName", Name);
                return route;
            }
            catch (Exception ex)
            {
            }

            return null;
        }

        public static async Task SaveRoute(IHubProxy HubProxy, Route route)
        {
            try
            {
                await HubProxy.Invoke<Route>("SaveRoute", route);
            }
            catch (Exception ex)
            {
            }

        }




        public static async Task DeleteRouteByGuid(IHubProxy HubProxy, string RouteGuid)
        {
            try
            {
                await HubProxy.Invoke<Route>("DeleteRouteByGuid", RouteGuid);
            }
            catch (Exception ex)
            {
            }

        }



        public static async Task<FormationTree> SaveTreeObject(IHubProxy HubProxy, FormationTree atomDTO)
        {
            try
            {
                FormationTree result = await HubProxy.Invoke<FormationTree>("SaveTreeObject", atomDTO);
                return result;
            }
            catch (Exception ex)
            {
            }
            return null;
        }


        public static async Task<IEnumerable<FormationTree>> GetAllAtomsFromTree(IHubProxy HubProxy)
        {
            try
            {
                IEnumerable<FormationTree> atoms = await HubProxy.Invoke<IEnumerable<FormationTree>>("GetAllAtomsFromTree");
                return atoms;
            }
            catch (Exception ex)
            {
            }

            return null;
        }

        public static async Task DeleteAtomFromTreeByGuid(IHubProxy HubProxy, string AtomGuid)
        {
            try
            {
                await HubProxy.Invoke<GeneralActivityDTO>("DeleteAtomFromTreeByGuid", AtomGuid);
            }
            catch (Exception ex)
            {
            }

        }

        public static async Task<AtomData> DeployFormationFromTree(IHubProxy HubProxy, DeployedFormation formation)
        {
            try
            {
                AtomData atom = await HubProxy.Invoke<AtomData>("DeployFormationFromTree", formation);

                return atom;
            }
            catch (Exception ex)
            {
            }
            return null;
        }

    }
}
