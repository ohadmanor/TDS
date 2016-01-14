using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDSServer.GroundTask.StateMashine
{
    public class BasicStateFormGroundTaskOrder
    {

        public BasicStateFormGroundTaskOrder PrevState;
        public bool InitialActivityExecuted = false;
        public BasicStateFormGroundTaskOrder()
        { }
        public virtual void Enter(clsGroundAtom refGroundAtom)
        { }
        public virtual void Execute(clsGroundAtom refGroundAtom)
        { }
        public virtual void Exit(clsGroundAtom refGroundAtom)
        { }

        public virtual void WaitAllThreadingTask(clsGroundAtom refGroundAtom)
        { }
    }
    public class ADMINISTRATIVE_STATE : BasicStateFormGroundTaskOrder
    {
        public override void Enter(clsGroundAtom refGroundAtom)
        {
            base.Enter(refGroundAtom);
        }
        public override void Execute(clsGroundAtom refGroundAtom)
        {
            clsGroundAtom.CheckNextMission(refGroundAtom);
        }
    }
    public class MOVEMENT_STATE : BasicStateFormGroundTaskOrder
    {
        clsActivityMovement refActivityMovement = null;
        public MOVEMENT_STATE(clsActivityMovement ActivityMovement)
        {
            refActivityMovement = ActivityMovement;
        }
        public async override void Enter(clsGroundAtom refGroundAtom)
        {
            refGroundAtom.currentSpeed = refActivityMovement.Speed;
          //  refGroundAtom.currentRoute = new typRoute(refActivityMovement.RouteActivity);


             typRoute R= await refGroundAtom.m_GameObject.m_GameManager.refTerrain.CreateRoute(refGroundAtom.curr_X, refGroundAtom.curr_Y, refActivityMovement.ReferencePoint.x, refActivityMovement.ReferencePoint.y, refActivityMovement.RouteActivity.RouteGuid);
             refGroundAtom.currentRoute = R;


           // refGroundAtom.SetRoute(refActivityMovement.RouteActivity);
        }



        public override void Execute(clsGroundAtom refGroundAtom)
        {
           if(refActivityMovement.TimeTo<refGroundAtom.m_GameObject.Ex_clockDate)
           {
               refActivityMovement.isEnded = true;
               refActivityMovement.isActive = false;
               refGroundAtom.ChangeState(new ADMINISTRATIVE_STATE());
               return;
           }


           if (refGroundAtom.currentRoute == null) return;


           refGroundAtom.isCollision = false;
           List<clsGroundAtom> colAtoms = refGroundAtom.m_GameObject.m_GameManager.QuadTreeGroundAtom.SearchEntities(refGroundAtom.curr_X, refGroundAtom.curr_Y, 10, isPrecise: true);
           foreach (clsGroundAtom atom in colAtoms)
           {
               if(atom!=refGroundAtom)
               {
                   refGroundAtom.isCollision = true;
                   break;
               }
           }


           if (refGroundAtom.currentLeg >  refGroundAtom.currentRoute.arr_legs.Count)  //-1)  // refActivityMovement.RouteActivity.Points.Count()-1)
            {
                refActivityMovement.isEnded = true;
                refActivityMovement.isActive = false;
                refGroundAtom.currentRoute = null;
                refGroundAtom.ChangeState(new ADMINISTRATIVE_STATE());
                return;
            }
           else
           {
               refGroundAtom.Move(refGroundAtom.m_GameObject.m_GameManager.GroundCycleResolution);
           }
        }
    }

    class FormGroundTaskSM
    {
    }
}
