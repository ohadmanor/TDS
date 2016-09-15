using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDSServer.GroundTask.StateMashine;

namespace TDSServer.GroundTask
{
	// YD: event handler delegates
    // start and end of earthquake
    public delegate void EarthquakeStarterEventHandler(object sender, EventArgs e);
    public delegate void EarthquakeEndedEventHandler(object sender, EventArgs e);
    // arrival of forces delegate
    public delegate void ForcesHaveArrivedEventHandler(object sender, EventArgs e);
    // dying or becoming incapacitated
    public delegate void IncapacitationEventHandler(object sender, EventArgs e);
    public delegate void DeathEventHandler(object sender, EventArgs e);
	// ---
    public class clsGroundAtom : AtomBase, IQuadTree
    {
        [NonSerialized]
        public GameObject m_GameObject = null;
        public BasicStateFormGroundTaskOrder currentState = null;
        public string GUID = String.Empty;
        public DAreaRect QuadTreeBounds;
        public bool isCollision;

        public const double MAX_OFFSET = 5;
        // human radius
        public const double RADIUS = 0.5;
        public const double OFFSET_IN_COLLISION = 1;

        public double X_Distination;
        public double Y_Distination;
        public double X_Route;
        public double Y_Route;

        public double Offset_Azimuth = 0;
        public double Offset_Distance = 0;

        private int m_currentLeg;
		// Yinon Douchan: Health status and knowledge of emergency
		public Boolean knowsAboutEarthquake;
        private HealthStatus m_healthStatus;
        public HealthStatus healthStatus
        {
            get { return m_healthStatus; }
            set { m_healthStatus = value; }
        }
		// --------------------------------------------------------
        public int currentLeg
        {
            get { return m_currentLeg; }
            set { m_currentLeg = value; }
        }
        private double m_Speed;
        public double currentSpeed
        {
            get { return m_Speed; }
            set { m_Speed = value; }
        }
        private double m_currentAzimuth = 0;
        public double currentAzimuth
        {
            get { return m_currentAzimuth; }
            set { m_currentAzimuth = value; }
        }

        private typRoute m_currentRoute = null;
        public typRoute currentRoute
        {
            get { return m_currentRoute; }
            set
            {
                currentLeg = 1;

                if (m_currentRoute == null && value == null) return;
                m_currentRoute = value;
            }
        }
		
        public clsActivityMovement currentRegularActivity;
        public int currentStartWaypoint;
        public int currentEndWaypoint;
        public DPoint lastRoutePoint;

        public List<CollisionTime> Collisions = new List<CollisionTime>();
        //private Route m_Route;
        //public void SetRoute(Route route) 
        //{           
        //        m_currentLeg = 1;
        //        m_Route = route;
            
        //}
		
		// YD: event handlers
        // event handlers for start and end of earthquake
        public event EarthquakeStarterEventHandler earthquakeStartedEventHandler;
        public event EarthquakeEndedEventHandler earthquakeEndedEventHandler;
        // event handler for arrival of forces
        public event ForcesHaveArrivedEventHandler forcesHaveArrivedEventHandler;
        // incapacitation and death
        public event IncapacitationEventHandler incapacitationEventHandler;
        public event DeathEventHandler deathEventHandler;
		// ---

        public clsGroundAtom(GameObject pGameObject)
        {
			// Yinon Douchan: Health status and knowledge of emergency
            knowsAboutEarthquake = false;
            m_GameObject = pGameObject;
            m_healthStatus = new HealthStatus();
            lastRoutePoint = new DPoint();
            clearAllEventSubscriptions();
			// -------------------------------------------------------
            //VH ChangeState(new ADMINISTRATIVE_STATE());
        }
		
		// YD: fire an event for when forces have arrived
        public void forcesHaveArrived()
        {
            if (forcesHaveArrivedEventHandler != null)
            {
                forcesHaveArrivedEventHandler(this, new clsGroundAtomEventArgs(this));
            }
        }

		// YD: fire an event for when the earthquake started
        public void earthquakeStarted()
        {
            if (earthquakeStartedEventHandler != null)
            {
                earthquakeStartedEventHandler(this, new clsGroundAtomEventArgs(this));
            }
        }
		
		// YD: fire an event for when the earthquake ended
        public void earthquakeEnded()
        {
            if (earthquakeEndedEventHandler != null)
            {
                earthquakeEndedEventHandler(this, new clsGroundAtomEventArgs(this));
            }
        }

		// YD: fire an event for incapacitation
        public void gotIncapacitated()
        {
            if (incapacitationEventHandler != null)
            {
                incapacitationEventHandler(this, new clsGroundAtomEventArgs(this));
            }
        }

		// YD: fire an event for death
        public void gotDead()
        {
            if (deathEventHandler != null)
            {
                deathEventHandler(this, new clsGroundAtomEventArgs(this));
            }
        }

		// YD: clear all event subscriptions
        public void clearAllEventSubscriptions()
        {
            earthquakeStartedEventHandler = null;
            earthquakeEndedEventHandler = null;
            forcesHaveArrivedEventHandler = null;
            incapacitationEventHandler = null;
            deathEventHandler = null;
        }

        internal void ExecuteState()
        {            
            if (currentState != null)
                currentState.Execute(this);
        }
        internal void ChangeState(BasicStateFormGroundTaskOrder state)
        {
            // Exit this state.
            if (currentState != null)
            {
                currentState.Exit(this);

                state.PrevState = currentState;
                state.PrevState.PrevState = null;
            }
                          

            // Set the new state.
            currentState = state;
            // Call the new states Enter method.
            currentState.Enter(this);
        }
        public static void CheckNextMission(clsGroundAtom refGroundAtom)
        {
            List<clsActivityBase> Activites = null;
            refGroundAtom.m_GameObject.m_GroundActivities.TryGetValue(refGroundAtom.GUID, out Activites);
            if (Activites == null) return;
            clsActivityBase ActivityFound = null;
            foreach (clsActivityBase Activity in Activites)
            {
                if (Activity.isEnded == false )
                {
                     if (Activity.TimeFrom <= refGroundAtom.m_GameObject.Ex_clockDate && refGroundAtom.m_GameObject.Ex_clockDate < Activity.TimeTo)
                     {
                         ActivityFound = Activity;
                         break;
                     }
                }
            }
            if(ActivityFound != null)
            {
                switch(ActivityFound.ActivityType)
                {
                    case enumActivity.MovementActivity:
						// Yinon Douchan: changed to new state
                        refGroundAtom.ChangeState(new REGULAR_MOVEMENT_STATE(ActivityFound as clsActivityMovement));
						// -----------------------------------------
                        break;
                }
            }

        }
        internal void CheckCondition()
        {

        }
        public void Move(int DeltaTime) //DeltaTime milliseconds
        {
            double DeltaTimeSec = DeltaTime * 0.001;
        LabBegin:
            for (int i = m_currentLeg - 1; i < currentRoute.arr_legs.Count; i++)
            {
                double dist = currentSpeed * DeltaTimeSec / 3600;
                double distToPoint = TerrainService.MathEngine.CalcDistance(curr_X, curr_Y, currentRoute.arr_legs[i].ToLongn, currentRoute.arr_legs[i].ToLatn) / 1000;
                if (distToPoint > dist)
                {
                  //  m_currentLeg = i + 1;

                    double curDist = currentRoute.arr_legs[i].LegDistance;
                    double deltaX = 0;
                    double deltaY = 0;

                    if (curDist != 0)
                    {
                        deltaX = ((currentRoute.arr_legs[i].ToLongn - currentRoute.arr_legs[i].FromLongn) * dist) / curDist;
                        deltaY = ((currentRoute.arr_legs[i].ToLatn - currentRoute.arr_legs[i].FromLatn) * dist) / curDist;
                        curr_X = curr_X + deltaX;
                        curr_Y = curr_Y + deltaY;
                    }

                    m_GameObject.m_GameManager.QuadTreeGroundAtom.PositionUpdate(this);
                    return;                    
                }
                else
                {
                    if (m_currentLeg == currentRoute.arr_legs.Count) // ' Last leg 5
                    {
                        curr_X = currentRoute.arr_legs[i].ToLongn;
                        curr_Y = currentRoute.arr_legs[i].ToLatn;                        
                        m_currentLeg = m_currentLeg + 1;                 

                    }
                    else
                    {
                        double timeToPointSec = (distToPoint * 3600.0 / currentSpeed);
                        DeltaTimeSec = DeltaTimeSec - timeToPointSec;
                        curr_X = currentRoute.arr_legs[i].ToLongn;
                        curr_Y = currentRoute.arr_legs[i].ToLatn;
                        m_currentLeg = m_currentLeg + 1;
                        goto LabBegin;
                    }
                }
            }

            m_GameObject.m_GameManager.QuadTreeGroundAtom.PositionUpdate(this);
        }



        public void VirtualMoveOnRoute(int DeltaTime, double X, double Y, out double route_X, out double route_Y,out int leg) //DeltaTime milliseconds
        {
            
            route_X = X;
            route_Y = Y;
            int currLeg = m_currentLeg;
            leg=currLeg;

            double DeltaTimeSec = DeltaTime * 0.001;
        LabBegin:
            for (int i = currLeg - 1; i < currentRoute.arr_legs.Count; i++)
            {
                double dist = currentSpeed * DeltaTimeSec / 3600;
                double distToPoint = TerrainService.MathEngine.CalcDistance(X, Y, currentRoute.arr_legs[i].ToLongn, currentRoute.arr_legs[i].ToLatn) / 1000;
                if (distToPoint > dist)
                {
                    //  m_currentLeg = i + 1;

                    double curDist = currentRoute.arr_legs[i].LegDistance;
                    double deltaX = 0;
                    double deltaY = 0;

                    if (curDist != 0)
                    {
                        deltaX = ((currentRoute.arr_legs[i].ToLongn - currentRoute.arr_legs[i].FromLongn) * dist) / curDist;
                        deltaY = ((currentRoute.arr_legs[i].ToLatn - currentRoute.arr_legs[i].FromLatn) * dist) / curDist;
                        route_X = X + deltaX;
                        route_Y = Y + deltaY;
                    }
                    leg = currLeg;
                    return;
                }
                else
                {
                    if (currLeg == currentRoute.arr_legs.Count) // ' Last leg 5
                    {
                        route_X = currentRoute.arr_legs[i].ToLongn;
                        route_Y = currentRoute.arr_legs[i].ToLatn;
                        currLeg = currLeg + 1;

                    }
                    else
                    {
                        double timeToPointSec = (distToPoint * 3600.0 / currentSpeed);
                        DeltaTimeSec = DeltaTimeSec - timeToPointSec;
                        route_X = currentRoute.arr_legs[i].ToLongn;
                        route_Y = currentRoute.arr_legs[i].ToLatn;

                        //VH
                        X = route_X;
                        Y = route_Y;

                        currLeg = currLeg + 1;
                        goto LabBegin;
                    }
                }
            }

            leg = currLeg;
        }

        public void MoveToDestination(int DeltaTime)
        {
           if (X_Distination == curr_X && Y_Distination == curr_Y) return ;
           double DeltaTimeSec = DeltaTime * 0.001; //'Convert to second
           double dist = (currentSpeed * DeltaTimeSec / 3600) * 1000;
           double curDist = TerrainService.MathEngine.CalcDistance(curr_X, curr_Y, X_Distination, Y_Distination);
           if ( dist >= curDist)
           {
               curr_X = X_Distination;
               curr_Y = Y_Distination;
           }
           else
           {
               double deltaX = ((X_Distination - curr_X) * dist) / curDist;
               double deltaY = ((Y_Distination - curr_Y) * dist) / curDist;
               curr_X = curr_X + deltaX;
               curr_Y = curr_Y + deltaY;
           }

           m_GameObject.m_GameManager.QuadTreeGroundAtom.PositionUpdate(this);
        }

		// Yinon Douchan: Code for re routing to escape point for explosions
        public void reRouteToEscape(clsActivityMovement panicMovement)
        {
            // form the activity
            panicMovement.ActivityId = 300;
            panicMovement.ActivityType = enumActivity.MovementActivity;
            panicMovement.AtomGuid = GUID;
            panicMovement.AtomName = MyName;
            panicMovement.TimeFrom = m_GameObject.Ex_clockDate;
            panicMovement.StartActivityOffset = TimeSpan.Zero;
            panicMovement.TimeTo = panicMovement.TimeFrom.Add(TimeSpan.FromDays(365));
            panicMovement.DurationActivity = TimeSpan.FromSeconds(1);
            panicMovement.Speed = (int)(currentSpeed*1.5);

            // plan the route
            String escapeRouteName = "Escape3";
            String reversedEscapeRouteName = "Escape3_reversed";
            Route escapeRoute = TDS.DAL.RoutesDB.GetRouteByName(escapeRouteName);
            Route reversedEscapeRoute = TDS.DAL.RoutesDB.GetRouteByName(reversedEscapeRouteName);

            // find closest route point
            double minDistance = Double.MaxValue;
            int minPointIndex = 0;
            for (int i = 0; i < escapeRoute.Points.Count() - 1; i++)
            {
                DPoint from = escapeRoute.Points.ElementAt(i);
                DPoint to = escapeRoute.Points.ElementAt(i + 1);

                double fromAzim = Util.Azimuth2Points(X_Route, Y_Route, from.x, from.y);
                double toAzim = Util.Azimuth2Points(X_Route, Y_Route, to.x, to.y);

                if ((from.x == X_Route && from.y == Y_Route)
                    || (to.x == X_Route && to.y == Y_Route)
                    || (Util.getAzimuthDifferenceDegrees(fromAzim, toAzim) > 90))
                {
                    minPointIndex = i;
                    break;
                }
            }

            DPoint closestFrom = escapeRoute.Points.ElementAt(minPointIndex);
            DPoint closestTo = escapeRoute.Points.ElementAt(minPointIndex + 1);

            // calculate escape route azimuth close to me in order to determine which route to select
            double escapeRouteAzimuth = Util.Azimuth2Points(closestFrom.x, closestFrom.y, closestTo.x, closestTo.y);
            DPoint explosionLocation = m_GameObject.getExplosionLocation();
            double explosionAzimuth = Util.Azimuth2Points(curr_X, curr_Y, explosionLocation.x, explosionLocation.y);

            if (Util.getAzimuthDifferenceDegrees(explosionAzimuth, escapeRouteAzimuth) < 90)
            {
                // if escape route leads to explosion, take the reversed route
                panicMovement.RouteActivity = TDS.DAL.RoutesDB.GetRouteByName(reversedEscapeRouteName);
            }
            else
            {
                // if not - take the escape route
                panicMovement.RouteActivity = TDS.DAL.RoutesDB.GetRouteByName(escapeRouteName);
            }

            DPoint destPoint = panicMovement.RouteActivity.Points.ElementAt(panicMovement.RouteActivity.Points.Count() - 1);

            panicMovement.isActive = true;
            panicMovement.isStarted = true;
            panicMovement.isAborted = false;
            panicMovement.isEnded = false;

            panicMovement.ReferencePoint = new DPoint(destPoint.x, destPoint.y);

            // check if need to change sign of offset
            if (panicMovement.RouteActivity.Points.Count() > 1)
            {
                DPoint p0 = panicMovement.RouteActivity.Points.ElementAt(0);
                DPoint p1 = panicMovement.RouteActivity.Points.ElementAt(1);

                double nextAzimuth = Util.Azimuth2Points(p0.x, p0.y, p1.x, p1.y);

                // if rerouting to opposite direction change sign of offset
                if (Util.getAzimuthDifferenceDegrees(currentAzimuth, nextAzimuth) > 90)
                {
                    Offset_Distance *= -1;
                }
            }


            // add activity to atom
            List<clsActivityBase> atomActivities;
            m_GameObject.m_GroundActivities.TryGetValue(GUID, out atomActivities);
            //VH
            if (atomActivities!=null)
            {
                atomActivities.Add(panicMovement);
            }
          
        }

        public void resetMovementData()
        {
            X_Route = curr_X;
            Y_Route = curr_Y;
            Offset_Azimuth = 0;
            Offset_Distance = 0;
        }
		// --------------------------------------------------------------------------------

        public new string Key
        {
            get { return GUID; }
        }

        public double x
        {
            get { return curr_X; }
        }

        public double y
        {
            get { return curr_Y; }
        }

        DAreaRect IQuadTree.QuadTreeBounds
        {
            set { QuadTreeBounds = value; }
        }

        public bool bVisibleToClient
        {
            get { return true; }
        }
    }

	// Yinon Douchan: An atom's sealth status
    public class HealthStatus
    {
        public double injurySeverity;
        public bool isIncapacitated;
        public bool isInjured;
        public bool isDead;

        public HealthStatus()
        {
            injurySeverity = 0;
            isIncapacitated = false;
            isInjured = false;
            isDead = false;
        }

        public bool isHealthy()
        {
            return !isDead && !isIncapacitated && !isInjured;
        }
    }
	// ------------------------------------------------------------------
}
