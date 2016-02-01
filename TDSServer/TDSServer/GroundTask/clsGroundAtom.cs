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

        private const double OFFSET_IN_COLLISION = 0.3F;
        public const double MAX_OFFSET = 10;

        // compare yourself to others only in this radius
        public const double COMPARISON_RADIUS = 10;

        // human field of view - 120 degrees (in radians of course, to make our lives easier)
        public const double FIELD_OF_VIEW_RADIANS = 120 * Math.PI / 180;

        private int m_currentLeg;
        private int m_age;
        private double m_avoidanceSideProbability;
        private double m_personalSpace;
        private String m_gender;
        private int m_groupId;

        public int currentLeg
        {
            get { return m_currentLeg; }
            set { m_currentLeg = value; }
        }
        public double personalSpace
        {
            get { return m_personalSpace; }
            set { m_personalSpace = value; }
        }

        // offset from route - poisitive means I'm moving to the right side, negative means I'm moving to the left side
        private double m_currentLegOffset;
        public double currentLegOffset
        {
            get { return m_currentLegOffset; }
            set { m_currentLegOffset = value; }
        }

        private double m_speedGain;
        public double speedGain
        {
            get { return m_speedGain; }
            set { m_speedGain = value; }
        }

        private double m_speedCosine;
        public double speedCosine
        {
            get { return m_speedCosine; }
            set { m_speedCosine = value; }
        }
 
        private double m_Speed;
        public double currentSpeed
        {
            get { return m_Speed; }
            set { m_Speed = value; }
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
            m_personalSpace = 0.5;
            m_speedGain = 1;
            m_speedCosine = 1;

            // position atom in a random place between -MAX_OFFSET and MAX_OFFSET
            m_currentLegOffset = 2 * MAX_OFFSET * Util.rand.NextDouble() - MAX_OFFSET;
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
                double dist = currentSpeed * speedGain * speedCosine * DeltaTimeSec / 3600;
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

        public void setOffsetInCollision()
        {
            // get atom's avoidance side probability - TODO get from database
            double avoidanceSideProbability = 0.5;

            Random rand = new Random();

            if (rand.NextDouble() > avoidanceSideProbability)
            {
                // take the other side if this side is blocked
                if (currentLegOffset + OFFSET_IN_COLLISION <= MAX_OFFSET)
                    currentLegOffset += OFFSET_IN_COLLISION;
                else
                    currentLegOffset += OFFSET_IN_COLLISION;
            }
            else
            {
                // take the other side if this side is blocked
                if (currentLegOffset - OFFSET_IN_COLLISION >= -MAX_OFFSET)
                    currentLegOffset -= OFFSET_IN_COLLISION;
                else
                    currentLegOffset += OFFSET_IN_COLLISION;
            }
        }

        public double getDirection()
        {
            int leg;

            if (currentRoute == null) return 0;

            if (currentLeg >= currentRoute.arr_legs.Count)
            {
                leg = currentRoute.arr_legs.Count - 1;
            }
            else
            {
                leg = currentLeg;
            }

            double fromX = currentRoute.arr_legs[leg].FromLongn;
            double toX = currentRoute.arr_legs[leg].ToLongn;
            double fromY = currentRoute.arr_legs[leg].FromLatn;
            double toY = currentRoute.arr_legs[leg].ToLatn;

            return Util.calcAngle(fromX, fromY, toX, toY);
        }

        public Boolean inFrontOfAtom(clsGroundAtom anotherAtom)
        {
            // calculate the angle between this atom and another atom
            double relativeDirection = Util.calcAngle(curr_X, curr_Y, anotherAtom.curr_X, anotherAtom.curr_Y);
            return relativeDirection < clsGroundAtom.FIELD_OF_VIEW_RADIANS / 2
                                         || relativeDirection > 2 * Math.PI - clsGroundAtom.FIELD_OF_VIEW_RADIANS / 2;
        }
    }
}
