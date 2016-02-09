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

        public const double MAX_OFFSET = 10;

        // compare yourself to others only in this radius
        public const double COMPARISON_RADIUS = 10;

        // human field of view - 120 degrees (in radians of course, to make our lives easier)
        public const double FIELD_OF_VIEW_RADIANS = 120 * Math.PI / 180;

        // human radius
        public const double RADIUS = 1;
        private const double OFFSET_IN_COLLISION = 1.5;

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
            m_personalSpace = RADIUS;
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

                        Boolean moveIsValid = true;

                        // before moving to new position check if new position is valid
                        List<clsGroundAtom> collisionAtoms = this.m_GameObject.m_GameManager.QuadTreeGroundAtom.SearchEntities(curr_X + deltaX, curr_Y + deltaY, 2*clsGroundAtom.RADIUS, isPrecise: true);
                        foreach (clsGroundAtom atom in collisionAtoms)
                        {
                            if (atom != this && Math.Abs(currentLegOffset - atom.currentLegOffset) <= clsGroundAtom.RADIUS)
                            {
                                moveIsValid = false;
                            }
                        }

                        // move to new position only if it is valid
                        if (moveIsValid)
                        {
                            isCollision = false;
                            curr_X = curr_X + deltaX;
                            curr_Y = curr_Y + deltaY;
                        }
                        else
                        {
                            isCollision = true;
                            evadeUsingSocialComparison();
                        }
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

        public double getAngleBetweenAtom(clsGroundAtom anotherAtom)
        {
            double myDirection = getDirection();
            double hisDirection = anotherAtom.getDirection();

            double diff1 = (myDirection - hisDirection);
            if (diff1 < 0) diff1 += Math.PI * 2;
            double diff2 = (hisDirection - myDirection) % (2 * Math.PI);
            if (diff2 < 0) diff2 += Math.PI * 2;

            return Math.Min(diff1, diff2);
        }

        public bool isMovingTowardsAtom(clsGroundAtom atom)
        {
            return getAngleBetweenAtom(atom) > Math.PI / 2;
        }

        private void evadeUsingSocialComparison()
        {
            bool canEvadeToTheLeftSide = true;
            bool canEvadeToTheRightSide = true;

            // reached right boundary of sidewalk
            if (currentLegOffset + OFFSET_IN_COLLISION > MAX_OFFSET) canEvadeToTheRightSide = false;
            // reached left boundary of sidewalk
            if (currentLegOffset - OFFSET_IN_COLLISION < -MAX_OFFSET) canEvadeToTheLeftSide = false;

            // if there is only one side available to evade go there with no further questions
            if (!canEvadeToTheLeftSide && canEvadeToTheRightSide)
            {
                // evade to the right side
                addOffset(OFFSET_IN_COLLISION);
                return;
            }
            else if (!canEvadeToTheRightSide && canEvadeToTheLeftSide)
            {
                // evade to the left side
                addOffset(-OFFSET_IN_COLLISION);
                return;
            }

            // if you cannot evade to any of the sides - tough luck

            // if both sides are available go to the most similar atom
            if (canEvadeToTheLeftSide && canEvadeToTheRightSide)
            {
                // if both sides are available, evade to the side where the atom which is most similar to me is
                clsGroundAtom mostSimilarAtom = SocialComparison.findMostSimilar(this);
                if (mostSimilarAtom != null && mostSimilarAtom != this)
                {
                    if (currentLegOffset == mostSimilarAtom.currentLegOffset)
                    {
                        addOffset(Util.rand.NextDouble() > 0.5 ? OFFSET_IN_COLLISION : -OFFSET_IN_COLLISION);
                    }
                    else
                    {
                        currentLegOffset = mostSimilarAtom.currentLegOffset;
                    }
                    
                }
                else
                {
                    // if there is no one to compare myself to, choose side arbitrarily
                    addOffset(Util.rand.NextDouble() > 0.5 ? OFFSET_IN_COLLISION : -OFFSET_IN_COLLISION);
                }
            }
        }

        public void addOffset(double offset)
        {
            if (offset > 0)
            {
                if (currentLegOffset + offset > MAX_OFFSET) currentLegOffset = MAX_OFFSET;
                else currentLegOffset += offset;
            }
            else
            {
                // offset <= 0
                if (currentLegOffset + offset < -MAX_OFFSET) currentLegOffset = -MAX_OFFSET;
                else currentLegOffset += offset;
            }
        }
    }
}
