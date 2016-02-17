using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDSClient.SAGInterface
{
    //public class Point
    //{
    //    public double x;
    //    public double y;
    //}
    public class Route
    {
        public string RouteName;
        public string RouteGuid;
      //  public IEnumerable<DPoint> Points;
        public List<DPoint> Points;
    }
    public class AtomData
    {
        public string UnitName;
        public string UnitGuid;
        public DPoint Location;
    }
    public enum enumActivity
    {
        Undefined = 0,
        MovementActivity
    }
    public class GeneralActivityDTO
    {
        public int ActivityId;
        public int Activity_SeqNumber;
        public AtomData Atom;
        public enumActivity ActivityType;
        public TimeSpan StartActivityOffset;
        public TimeSpan DurationActivity;
        public int Speed;
        public Route RouteActivity;
        public DPoint ReferencePoint;

    }
    public enum enumPlatformId
    {
        Undefined = 0,
        GeneralHumans = 1
    }
    public class FormationTree
    {
        public string Identification;
        public string GUID;
        public string ParentGUID;
        public int CountryId;
        public enumPlatformId PlatformCategoryId;
        public string PlatformType;
        public int FormationTypeId;

        public bool isDeployed;
        public bool isActivityes;
    }
    public class DeployedFormation
    {
        public FormationTree formation;
        public double x;
        public double y;
    }
    public enum enumLineStyle
    {
        Solid = 0,
        Dash,
        DashDot,
        Dot
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


    public class UserParameters
    {
        public string User;
        public int MapHomeZoom;
        public double MapHomeCenterX;
        public double MapHomeCenterY;
    }


    public class CustomImageInfo
    {
        private string MapName_;
        public string MapName
        {
            get { return MapName_; }
            set { MapName_ = value; }
        }
        public int TypeId;
        public string Guid;
        public double MinX;
        public double MaxX;
        public double MinY;
        public double MaxY;

        public int MinZoom;
        public int MaxZoom;

        public bool isMapLayer;

        public bool isRasterLogin;
    }
    public class WMSCapabilities
    {
        public string Version;
        public Exception Error;
        public List<CustomImageInfo> Layers = new List<CustomImageInfo>();
    }

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
    public struct structQuery2Manager
    {
        public QUERY_SCENARIOSTATUS QueryStatus;
        public int QUERYScenarioID;
        public string ActionGUIDid;
        public bool SelectedActionOnly;
        public int SelectedOperFileId;
        //  public COLOR_SIDE SelectedColorSide;
        public object QueryValue;
        public int QueryId;
        public bool IsLoadRun;
    }
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

    public sealed class structTransportCommonProperty
    {
        public string AtomClass;
      
        public String AtomName
        { get; set; }
        public string TextValue;
       
        public String GUID
        { get; set; }

        public bool isVirtualAtom;       
        //  public COLOR_SIDE CountryColorSide;     
        public double X;
        public double Y;
        public ushort AzimuthDegree;

        public ushort FontKey;
        public ushort CountryId;
        public bool isCollision;

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
}
