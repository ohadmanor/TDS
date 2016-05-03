using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDSServer.GroundTask.StateMashine
{
    public class REGULAR_MOVEMENT_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement = null;

        public REGULAR_MOVEMENT_STATE(clsActivityMovement ActivityMovement)
            : base(ActivityMovement)
        {
            refActivityMovement = ActivityMovement;
        }

        public override void Enter(clsGroundAtom refGroundAtom)
        {
            base.Enter(refGroundAtom);
        }

        public override void Execute(clsGroundAtom refGroundAtom)
        {
            // stop all activities if you're dead
            if (refGroundAtom.healthStatus.isDead)
            {
                refGroundAtom.isCollision = false;
                refGroundAtom.ChangeState(new DEAD_STATE());
                return;
            }
            // or incapacitated
            if (refGroundAtom.healthStatus.isIncapacitated)
            {
                refGroundAtom.isCollision = false;
                refGroundAtom.ChangeState(new INCAPACITATED_STATE());
                return;
            }

            DPoint explosionLocation = refGroundAtom.m_GameObject.getExplosionLocation();
            double explosionRadius = refGroundAtom.m_GameObject.getExplosionRadius();
            bool nearExplosion = TerrainService.MathEngine.CalcDistance(refGroundAtom.curr_X, refGroundAtom.curr_Y, explosionLocation.x, explosionLocation.y) <= 3 * explosionRadius;

            // if emergency event has occurred restart movement with different route
            if (!refGroundAtom.knowsAboutEmergency && refGroundAtom.m_GameObject.emergencyOccurred() && nearExplosion)
            {
                refGroundAtom.knowsAboutEmergency = true;
                refActivityMovement.isEnded = true;
                refActivityMovement.isActive = false;

                clsActivityMovement panicMovement = new clsActivityMovement();
                refGroundAtom.reRouteToEscape(panicMovement);

                refGroundAtom.ChangeState(new PANIC_MOVEMENT_STATE(panicMovement, refActivityMovement));
                return;
            }
            else if (refGroundAtom.m_GameObject.emergencyOccurred())
            {
                refGroundAtom.knowsAboutEmergency = true;

                if (nearExplosion)
                {
                    refActivityMovement.isEnded = true;
                    refActivityMovement.isActive = false;

                    clsActivityMovement panicMovement = new clsActivityMovement();
                    refGroundAtom.reRouteToEscape(panicMovement);

                    refGroundAtom.ChangeState(new PANIC_MOVEMENT_STATE(panicMovement, refActivityMovement));
                    return;
                }
            }

            base.Execute(refGroundAtom);
        }
    }

    public class PANIC_MOVEMENT_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement = null;
        clsActivityMovement refOriginalMovement = null; // original activity before entering panic
        DateTime panicEntranceTime;

        public PANIC_MOVEMENT_STATE(clsActivityMovement ActivityMovement, clsActivityMovement originalActivityMovement)
            : base(ActivityMovement)
        {
            refActivityMovement = ActivityMovement;
            refOriginalMovement = originalActivityMovement;
        }

        public async override void Enter(clsGroundAtom refGroundAtom)
        {
            refGroundAtom.currentRoute = await refGroundAtom.m_GameObject.m_GameManager.refTerrain.createRouteByShortestPathOnly(refGroundAtom.X_Route,
                refGroundAtom.Y_Route,
                refActivityMovement.RouteActivity.Points.ElementAt(refActivityMovement.RouteActivity.Points.Count() - 1).x,
                refActivityMovement.RouteActivity.Points.ElementAt(refActivityMovement.RouteActivity.Points.Count() - 1).y);
            panicEntranceTime = refGroundAtom.m_GameObject.Ex_clockDate;
        }

        public override void Execute(clsGroundAtom refGroundAtom)
        {
            base.Execute(refGroundAtom);

            TimeSpan panicDuration = refGroundAtom.m_GameObject.Ex_clockDate - panicEntranceTime;
            DPoint explosionLocation = refGroundAtom.m_GameObject.getExplosionLocation();
            double explosionRadius = refGroundAtom.m_GameObject.getExplosionRadius();
            bool nearExplosion = TerrainService.MathEngine.CalcDistance(refGroundAtom.curr_X, refGroundAtom.curr_Y, explosionLocation.x, explosionLocation.y) <= 3 * explosionRadius;
            //if (!nearExplosion)
            //{
            //    // get out of panic, start long-term reaction to event. Maybe just go back to your business, maybe get curious, maybe help casualties...
            //    if (refOriginalMovement != null)
            //    {
            //        //refGroundAtom.resetMovementData();
            //        DPoint originalDest = refOriginalMovement.RouteActivity.Points.ElementAt(refOriginalMovement.RouteActivity.Points.Count() - 1);
            //        Route reRouteToOriginal = RoutePlanner.createRoute(new DPoint(refGroundAtom.X_Route, refGroundAtom.Y_Route), originalDest);
            //        clsActivityMovement backToNormal = RoutePlanner.createActivityAndStart(refGroundAtom, refOriginalMovement.Speed, reRouteToOriginal);
            //        refOriginalMovement.ReferencePoint.x = refGroundAtom.X_Route;
            //        refOriginalMovement.ReferencePoint.y = refGroundAtom.Y_Route;
            //        refGroundAtom.ChangeState(new REGULAR_MOVEMENT_STATE(refOriginalMovement));
            //    }
            //}
        }
    }

    public class CURIOSITY_MOVEMENT_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement = null;

        public CURIOSITY_MOVEMENT_STATE(clsActivityMovement ActivityMovement)
            : base(ActivityMovement)
        {
            refActivityMovement = ActivityMovement;
        }

        public override void Enter(clsGroundAtom refGroundAtom)
        {
            base.Enter(refGroundAtom);
        }

        public override void Execute(clsGroundAtom refGroundAtom)
        {
            base.Execute(refGroundAtom);
        }
    }

    public class DEAD_STATE : BasicStateFormGroundTaskOrder
    {
        public override void Enter(clsGroundAtom refGroundAtom)
        {
            base.Enter(refGroundAtom);
        }
        public override void Execute(clsGroundAtom refGroundAtom)
        {
            // For now, do nothing. You're dead.
        }
    }

    public class INCAPACITATED_STATE : BasicStateFormGroundTaskOrder
    {
        public override void Enter(clsGroundAtom refGroundAtom)
        {
            base.Enter(refGroundAtom);
        }
        public override void Execute(clsGroundAtom refGroundAtom)
        {
            // For now, do nothing. You're incapacitated but hey, you're still alive! Maybe call for help?
        }
    }
}
