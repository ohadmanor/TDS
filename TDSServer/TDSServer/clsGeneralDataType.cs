using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainService;
namespace TDSServer
{
   

    public enum QUERY_SCENARIOSTATUS
    {
        UNDEFINED_STATUS = 0,
        QUERY_START_SCENARIO = 1,
        QUERY_PAUSE_SCENARIO = 2,
        QUERY_CONT_SCENARIO = 3,
        QUERY_RETURN_SCENARIO = 4,
        QUERY_LOAD_SCENARIO = 5,
        QUERY_UNLOAD_SCENARIO = 6,
        QUERY_RETURN_SPECIFIC_TIME = 7
    };
    public enum typGamestatus
    {
        UNDEFINED_STATUS = 0,
        UNLOADED_STATUS = 1,
        PROCESS_LOAD_STATUS = 2,
        EDIT_STATUS = 3,
        PROCESS_EDIT2RUN_STATUS = 4,
        RUN_STATUS = 5,
        PAUSE_STATUS = 6,
        PROCESS_RUN2EDIT = 7,
        PROCESS_RETURN_TO_SPECIFIC_TIME = 8
    };

    public struct structQuery2Manager
    {       
        public QUERY_SCENARIOSTATUS QueryStatus;   
      
       
    }
    public abstract class AtomBase
    {       
        public string MyName;       
        public string Key;      
        public string Country;
        public int CountryId;   
        public double curr_X;     
        public double curr_Y;

    }


    public sealed class structTransportCommonProperty
    {      
        public string AtomClass;      
        public string AtomName;      
        public string TextValue;    
        public bool isVirtualAtom;     
      //  public COLOR_SIDE CountryColorSide;     
        public double X;     
        public double Y;     
        public ushort AzimuthDegree;
      
        public ushort FontKey;       
        public ushort CountryId;


    }
    public struct structTransport2Client
    {      
        public int typPackage;         //0 -First Package ,1-Next Package      
        public bool isEndPackage;      
        public int ExClockRatioSpeed;       
        public DateTime Ex_clockDate;      
        public typGamestatus ManagerStatus;     
        public int ScenarioID;       
        public int AtomObjectType;  //0-All,1-InfraSites,2-Ground,3-Air    
        public structTransportCommonProperty[] AtomObjectCollection;       
        public structTransportCommonProperty[] NavalObjectCollection;      
        public bool RefreshInfrasite;
    }
    public sealed class NotifyClientsEndCycleArgs : System.EventArgs
    {       
        public structTransport2Client Transport2Client;

    }

    public class UserMapPreference
    {
        public string MapName;
        public int MinZoom;
        public int MaxZoom;
    }

    public class UserMaps
    {
        public string User;
        public UserMapPreference[] maps;
    }

    public sealed class typLegSector
    {        
        public double FromLatn;       
        public double FromLongn;        
        public double ToLatn;       
        public double ToLongn;    
        public int HoldTime;
        private float m_LegDistance;       
        public float LegDistance
        {
            get
            {
                return m_LegDistance;
            }
            set { m_LegDistance = value; }
        }
      
        public float Speed;                 //double
        public float azimuthDegree;         //double
      
        public string AltitudeLevel;
        public float Altitude;             //double   

        public typLegSector()
        {
        }

    }
    public sealed class typRoute
    {
        public void ClearObject()
        {
            if (arr_legs != null)
                arr_legs.Clear();
        }

        public string RouteName;     
        public List<typLegSector> arr_legs;

        public double RouteDistance()
        {
            double ans = 0;
            if (arr_legs != null)
            {
                for (int i = 0; i < arr_legs.Count; i++)
                {
                    ans += arr_legs[i].LegDistance;
                }
            }
            return ans;
        }

        
        public typRoute()
        {
            arr_legs = new List<typLegSector>();

        }
        public typRoute(Route pathpoints)
        {
            arr_legs = new List<typLegSector>();

            
            DPoint[] points = pathpoints.Points.ToArray<DPoint>();

            for (int i = 0; i < points.Length-1; i++)
            {
                typLegSector leg = new typLegSector();
                leg.FromLongn = points[i].x;
                leg.FromLatn = points[i].y;
                leg.ToLongn = points[i+1].x;
                leg.ToLatn = points[i+1].y;
                leg.LegDistance = (float)TerrainService.MathEngine.CalcDistance(leg.FromLongn, leg.FromLatn, leg.ToLongn, leg.ToLatn) / 1000f;
                if (leg.LegDistance == 0.0)
                {
                    continue;
                }
                arr_legs.Add(leg);
            }
        } 
    }
    

    public class DPoint
    {
        public double x;
        public double y;
    }
    public class Route
    {
        public string RouteName;
        public string RouteGuid;
        public IEnumerable<DPoint> Points;
    }
    public class AtomData
    {
        public string UnitName;
        public string UnitGuid;
        public DPoint Location;
    }
    public enum enumActivity
    {       
        Undefined=0,
        MovementActivity
    }
    public class GeneralActivityDTO
    {
        public int ActivityId;
        public AtomData Atom;
        public enumActivity ActivityType;
        public TimeSpan StartActivityOffset;
        public TimeSpan DurationActivity;
        public int Speed;
        public Route RouteActivity;

    }

    public class clsActivityBase
    {
        public int ActivityId;      
        public string AtomName;
        public string AtomGuid;
        public enumActivity ActivityType;
        public DateTime TimeFrom;       
        public DateTime TimeTo;
        public TimeSpan StartActivityOffset;
        public TimeSpan DurationActivity;
        public int Speed;
        public Route RouteActivity;

        public bool isActive = false;
        public bool isEnded = false;       
        public bool isAborted = false;
        public bool isStarted = false;
    }
    public sealed class clsActivityMovement : clsActivityBase
    {

    }
    
    public class clsGeneralDataType
    {       
    }
}
