using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDSServer.GroundTask.StateMashine;

namespace TDSServer.GroundTask
{
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


        public List<CollisionTime> Collisions = new List<CollisionTime>();
        //private Route m_Route;
        //public void SetRoute(Route route) 
        //{           
        //        m_currentLeg = 1;
        //        m_Route = route;
            
        //}

        public clsGroundAtom(GameObject pGameObject)
        {
            m_GameObject = pGameObject;
            ChangeState(new ADMINISTRATIVE_STATE());
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
                        refGroundAtom.ChangeState(new MOVEMENT_STATE(ActivityFound as clsActivityMovement));
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

        }


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
}
