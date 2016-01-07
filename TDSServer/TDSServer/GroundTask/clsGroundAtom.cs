using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDSServer.GroundTask.StateMashine;

namespace TDSServer.GroundTask
{
    public class clsGroundAtom : AtomBase
    {
        [NonSerialized]
        public GameObject m_GameObject = null;
        public BasicStateFormGroundTaskOrder currentState = null;
        public string GUID = String.Empty;
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
                    m_currentLeg = i + 1;

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
        }
    }
}
