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

        public static async Task SaveActivity(IHubProxy HubProxy, GeneralActivityDTO ActivityDTO)
        {
            try
            {
                await HubProxy.Invoke<GeneralActivityDTO>("SaveActivity", ActivityDTO);
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
    }
}
