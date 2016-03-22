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
            if (refGroundAtom.MyName.StartsWith("Ambulance"))
            {
                refGroundAtom.ChangeState(new AMBULANCE_ADMINISTRATIVE_STATE());
                return;
            }

            if (!refGroundAtom.knowsAboutEmergency && refGroundAtom.m_GameObject.emergencyOccurred())
            {
                refGroundAtom.knowsAboutEmergency = true;

                clsActivityMovement movement = new clsActivityMovement();
                refGroundAtom.reRouteToEscape(movement);

                refGroundAtom.ChangeState(new ADMINISTRATIVE_STATE());
                return;
            }

            clsGroundAtom.CheckNextMission(refGroundAtom);
        }
    }

    public class AMBULANCE_ADMINISTRATIVE_STATE : ADMINISTRATIVE_STATE
    {
        public override void Enter(clsGroundAtom refGroundAtom)
        {
            base.Enter(refGroundAtom);
        }
        public override void Execute(clsGroundAtom refGroundAtom)
        {
            if (!refGroundAtom.knowsAboutEmergency && refGroundAtom.m_GameObject.emergencyOccurred())
            {
                refGroundAtom.knowsAboutEmergency = true;

                refGroundAtom.resetMovementData();
                Route straightLine = RoutePlanner.createRoute(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y), refGroundAtom.m_GameObject.getExplosionLocation());
                clsActivityMovement arrivalMovement = RoutePlanner.createActivityAndStart(refGroundAtom, 80, straightLine);

                refGroundAtom.ChangeState(new AMBULANCE_ARRIVAL_MOVEMENT_STATE(arrivalMovement));
                return;
            }
        }
    }

    public class MOVEMENT_STATE : BasicStateFormGroundTaskOrder
    {
        clsActivityMovement refActivityMovement = null;
        private MovementSolver movementSolver;
        public MOVEMENT_STATE(clsActivityMovement ActivityMovement)
        {
            refActivityMovement = ActivityMovement;
            movementSolver = new MovementSolver(refActivityMovement);

        }

        public async override void Enter(clsGroundAtom refGroundAtom)
        {
            refGroundAtom.currentSpeed = refActivityMovement.Speed;
          //  refGroundAtom.currentRoute = new typRoute(refActivityMovement.RouteActivity);


             typRoute R= await refGroundAtom.m_GameObject.m_GameManager.refTerrain.CreateRoute(refGroundAtom.X_Route, refGroundAtom.Y_Route, refActivityMovement.ReferencePoint.x, refActivityMovement.ReferencePoint.y, refActivityMovement.RouteActivity.RouteGuid);
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


            if (refGroundAtom.currentLeg > refGroundAtom.currentRoute.arr_legs.Count)  //-1)  // refActivityMovement.RouteActivity.Points.Count()-1)
            {
                //refActivityMovement.isEnded = true;
                //refActivityMovement.isActive = false;
                //refGroundAtom.currentRoute = null;
                //refGroundAtom.ChangeState(new ADMINISTRATIVE_STATE());

                movementSolver.restartMovementOnRoute(refGroundAtom);

                return;
            }
            
            double nextRoute_X = 0;
            double nextRoute_Y = 0;
            int nextLeg = 0;

            double X_Distination = 0;
            double Y_Distination = 0;
            double AzimDepl;

            refGroundAtom.VirtualMoveOnRoute(refGroundAtom.m_GameObject.m_GameManager.GroundCycleResolution, refGroundAtom.X_Route, refGroundAtom.Y_Route, out nextRoute_X, out nextRoute_Y,out nextLeg);

            double currentAzimuth = Util.Azimuth2Points(refGroundAtom.X_Route, refGroundAtom.Y_Route, nextRoute_X, nextRoute_Y);
            refGroundAtom.currentAzimuth = currentAzimuth;

        Lab1: ;
        
            if (refGroundAtom.Offset_Azimuth != 0.0 || refGroundAtom.Offset_Distance!=0.0)
            {
                Util.calcProjectedLocation(nextRoute_X, nextRoute_Y, currentAzimuth, refGroundAtom.Offset_Azimuth, refGroundAtom.Offset_Distance, out X_Distination, out Y_Distination);
            }
            else
            {
                X_Distination = nextRoute_X;
                Y_Distination = nextRoute_Y;
            }

            movementSolver.manageCollisions(refGroundAtom, X_Distination, Y_Distination);

            bool isRight = true;
            bool isLeft = true;

           if(refGroundAtom.isCollision)
           {
               refGroundAtom.Offset_Azimuth = 90;

               // check if I cannot go right or cannot go left
               if ((refGroundAtom.Offset_Distance + clsGroundAtom.OFFSET_IN_COLLISION) > clsGroundAtom.MAX_OFFSET) isRight = false;
               if ((refGroundAtom.Offset_Distance - clsGroundAtom.OFFSET_IN_COLLISION) < -clsGroundAtom.MAX_OFFSET) isLeft = false;

               if (isRight && !isLeft)
               {
                   // can only evade right
                   refGroundAtom.Offset_Distance += clsGroundAtom.OFFSET_IN_COLLISION;
               }
               else if (isLeft && !isRight)
               {
                   // can only evade left
                   refGroundAtom.Offset_Distance -= clsGroundAtom.OFFSET_IN_COLLISION;
               }
               else if (isLeft && isRight)
               {
                   // decide whether to turn right or left based on social comparison to most similar
                   clsGroundAtom mostSimilar = SocialComparison.findMostSimilar(refGroundAtom);
                   if (mostSimilar != null)
                   {
                       SocialComparison.setOffsetTowardsMostSimilar(refGroundAtom, mostSimilar);
                   }
                   else
                   {
                       // no similar agent - pick a side randomly
                       if (Util.random.NextDouble() > 0.5)
                       {
                           refGroundAtom.Offset_Distance += clsGroundAtom.OFFSET_IN_COLLISION;
                       }
                       else
                       {
                           refGroundAtom.Offset_Distance -= clsGroundAtom.OFFSET_IN_COLLISION;
                       }
                   }
               }

               if (refGroundAtom.Offset_Distance < -clsGroundAtom.MAX_OFFSET)
               {
                   refGroundAtom.Offset_Distance = -clsGroundAtom.MAX_OFFSET;
                   refGroundAtom.Offset_Azimuth = 0;
                   return;
               }
               else if (refGroundAtom.Offset_Distance > clsGroundAtom.MAX_OFFSET)
               {
                   refGroundAtom.Offset_Distance = clsGroundAtom.MAX_OFFSET;
                   refGroundAtom.Offset_Azimuth = 0;
                   return;
               }

               if (refGroundAtom.Offset_Distance < 0)
               {
                   refGroundAtom.Offset_Azimuth = 180 - refGroundAtom.Offset_Azimuth;
               }

               //nextRoute_X = X_Distination;
               //nextRoute_Y = Y_Distination;
               //goto Lab1;

           }
           else
           {
               refGroundAtom.X_Route = nextRoute_X;
               refGroundAtom.Y_Route = nextRoute_Y;
               refGroundAtom.currentLeg = nextLeg;

               refGroundAtom.X_Distination = X_Distination;
               refGroundAtom.Y_Distination = Y_Distination;

               refGroundAtom.MoveToDestination(refGroundAtom.m_GameObject.m_GameManager.GroundCycleResolution);
           }
        }
    }

    public class REGULAR_MOVEMENT_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement = null;

        public REGULAR_MOVEMENT_STATE(clsActivityMovement ActivityMovement) : base(ActivityMovement)
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
            }
            base.Execute(refGroundAtom);
        }
    }

    public class PANIC_MOVEMENT_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement = null;
        clsActivityMovement refOriginalMovement = null; // original activity before entering panic
        DateTime panicEntranceTime;

        public PANIC_MOVEMENT_STATE(clsActivityMovement ActivityMovement, clsActivityMovement originalActivityMovement) : base(ActivityMovement)
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
            if (!nearExplosion)
            {
                // get out of panic, start long-term reaction to event. Maybe just go back to your business, maybe get curious, maybe help casualties...
                if (refOriginalMovement != null)
                {
                    //refGroundAtom.resetMovementData();
                    DPoint originalDest = refOriginalMovement.RouteActivity.Points.ElementAt(refOriginalMovement.RouteActivity.Points.Count() - 1);
                    Route reRouteToOriginal = RoutePlanner.createRoute(new DPoint(refGroundAtom.X_Route, refGroundAtom.Y_Route), originalDest);
                    clsActivityMovement backToNormal = RoutePlanner.createActivityAndStart(refGroundAtom, refOriginalMovement.Speed, reRouteToOriginal);
                    refOriginalMovement.ReferencePoint.x = refGroundAtom.X_Route;
                    refOriginalMovement.ReferencePoint.y = refGroundAtom.Y_Route;
                    refGroundAtom.ChangeState(new REGULAR_MOVEMENT_STATE(refOriginalMovement));
                }
            }
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

    public class AMBULANCE_ARRIVAL_MOVEMENT_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement = null;

        public AMBULANCE_ARRIVAL_MOVEMENT_STATE(clsActivityMovement ActivityMovement) : base(ActivityMovement)
        {
            refActivityMovement = ActivityMovement;
        }

        public override async void Enter(clsGroundAtom refGroundAtom)
        {
            refGroundAtom.currentRoute = await refGroundAtom.m_GameObject.m_GameManager.refTerrain.createRouteByShortestPathOnly(refActivityMovement.RouteActivity.Points.ElementAt(0).x,
                refActivityMovement.RouteActivity.Points.ElementAt(0).y,
                refActivityMovement.RouteActivity.Points.ElementAt(1).x,
                refActivityMovement.RouteActivity.Points.ElementAt(1).y);
            refGroundAtom.currentSpeed = refActivityMovement.Speed;
        }

        public override void Execute(clsGroundAtom refGroundAtom)
        {
            if (refGroundAtom.currentLeg > refGroundAtom.currentRoute.arr_legs.Count)  //-1)  // refActivityMovement.RouteActivity.Points.Count()-1)
            {
                // arrived to ground zero. Now search for casualties
                refActivityMovement.isEnded = true;
                refActivityMovement.isActive = false;
                refGroundAtom.currentRoute = null;

                DPoint groundZeroLocation = refGroundAtom.m_GameObject.getExplosionLocation();
                double groundZeroRadius = refGroundAtom.m_GameObject.getExplosionRadius();
                List<clsGroundAtom> atomsInGroundZero = refGroundAtom.m_GameObject.m_GameManager.QuadTreeGroundAtom.SearchEntities(groundZeroLocation.x, groundZeroLocation.y, groundZeroRadius, isPrecise: true);
                foreach (clsGroundAtom atom in atomsInGroundZero) {
                    if (atom.healthStatus.isDead || atom.healthStatus.isIncapacitated) {
                        refGroundAtom.resetMovementData();
                        Route straightLine = RoutePlanner.createRoute(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y), new DPoint(atom.curr_X, atom.curr_Y));
                        clsActivityMovement extractionMovement = RoutePlanner.createActivityAndStart(refGroundAtom, 5, straightLine);

                        refGroundAtom.ChangeState(new AMBULANCE_GO_TO_CASUALTY_STATE(extractionMovement, atom));

                        return;
                    }
                }

                return;
            }

 	         base.Execute(refGroundAtom);
        }
    }

    public class AMBULANCE_GO_TO_CASUALTY_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement = null;
        clsGroundAtom refCasualty = null;

        public AMBULANCE_GO_TO_CASUALTY_STATE(clsActivityMovement ActivityMovement, clsGroundAtom Casualty) : base(ActivityMovement)
        {
            refActivityMovement = ActivityMovement;
            refCasualty = Casualty;
        }

        public override void Enter(clsGroundAtom refGroundAtom)
        {
            refGroundAtom.currentRoute = RoutePlanner.planStraightLineRoute(refActivityMovement.RouteActivity.Points.ElementAt(0),
                                                                refActivityMovement.RouteActivity.Points.ElementAt(1),
                                                                refActivityMovement.RouteActivity.RouteName);
            refGroundAtom.currentSpeed = refActivityMovement.Speed;
        }

        public override void Execute(clsGroundAtom refGroundAtom)
        {
            // once reached casualty start evacuating
            if (refGroundAtom.currentLeg > refGroundAtom.currentRoute.arr_legs.Count)  //-1)  // refActivityMovement.RouteActivity.Points.Count()-1)
            {
                refGroundAtom.resetMovementData();
                List<DPoint> points = new List<DPoint>();

                // go from casualty location to route and then to extraction location
                points.Add(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y));
                points.Add(new DPoint(refGroundAtom.m_GameObject.getExplosionLocation().x, refGroundAtom.m_GameObject.getExplosionLocation().y));
                points.Add(new DPoint(34.8514473088014, 32.1008536878526));
                Route route = RoutePlanner.createRoute(points);
                clsActivityMovement evacuationMovement = RoutePlanner.createActivityAndStart(refGroundAtom, 80, route);
                refGroundAtom.ChangeState(new AMBULANCE_EVACUATION_MOVEMENT_STATE(evacuationMovement, refCasualty));

                return;
            }

            base.Execute(refGroundAtom);
        }
    }

    public class AMBULANCE_EVACUATION_MOVEMENT_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement = null;
        clsGroundAtom refCasualty = null;

        public AMBULANCE_EVACUATION_MOVEMENT_STATE(clsActivityMovement ActivityMovement, clsGroundAtom Casualty) : base(ActivityMovement)
        {
            refActivityMovement = ActivityMovement;
            refCasualty = Casualty;
        }

        public async override void Enter(clsGroundAtom refGroundAtom)
        {
            refGroundAtom.currentRoute = await refGroundAtom.m_GameObject.m_GameManager.refTerrain.createRouteByShortestPathOnly(refActivityMovement.RouteActivity.Points.ElementAt(1).x,
            refActivityMovement.RouteActivity.Points.ElementAt(1).y,
            refActivityMovement.RouteActivity.Points.ElementAt(2).x,
            refActivityMovement.RouteActivity.Points.ElementAt(2).y);

            typLegSector casuatlyToGroundZero = new typLegSector();
            casuatlyToGroundZero.FromLongn = refActivityMovement.RouteActivity.Points.ElementAt(0).x;
            casuatlyToGroundZero.FromLatn = refActivityMovement.RouteActivity.Points.ElementAt(0).y;
            casuatlyToGroundZero.ToLongn = refActivityMovement.RouteActivity.Points.ElementAt(1).x;
            casuatlyToGroundZero.ToLatn = refActivityMovement.RouteActivity.Points.ElementAt(1).y;
            casuatlyToGroundZero.LegDistance = (float)TerrainService.MathEngine.CalcDistance(refActivityMovement.RouteActivity.Points.ElementAt(0).x,
                                                                                      refActivityMovement.RouteActivity.Points.ElementAt(0).y,
                                                                                      refActivityMovement.RouteActivity.Points.ElementAt(1).x,
                                                                                      refActivityMovement.RouteActivity.Points.ElementAt(1).y) / 1000f;
            refGroundAtom.currentRoute.arr_legs.Insert(0, casuatlyToGroundZero);

            //refGroundAtom.currentRoute = RoutePlanner.planStraightLineRoute(refActivityMovement.RouteActivity.Points.ElementAt(0),
            //                                        refActivityMovement.RouteActivity.Points.ElementAt(2),
            //                                        refActivityMovement.RouteActivity.RouteName);
            refGroundAtom.currentSpeed = refActivityMovement.Speed;
        }

        public override void Execute(clsGroundAtom refGroundAtom)
        {
            // continue evacuating only if there are people to evacuate
            DPoint groundZeroLocation = refGroundAtom.m_GameObject.getExplosionLocation();
            double groundZeroRadius = refGroundAtom.m_GameObject.getExplosionRadius();

            if (refGroundAtom.currentLeg > refGroundAtom.currentRoute.arr_legs.Count)  //-1)  // refActivityMovement.RouteActivity.Points.Count()-1)
            {
                List<clsGroundAtom> atomsInGroundZero = refGroundAtom.m_GameObject.m_GameManager.QuadTreeGroundAtom.SearchEntities(groundZeroLocation.x, groundZeroLocation.y, groundZeroRadius, isPrecise: true);
                bool thereAreMoreCasualties = false;

                foreach (clsGroundAtom atom in atomsInGroundZero)
                {
                    if (atom.healthStatus.isDead || atom.healthStatus.isIncapacitated)
                    {
                        thereAreMoreCasualties = true;
                        break;
                    }
                }

                if (!thereAreMoreCasualties)
                {
                    // if there are no more casualties in ground zero no need to go there.
                    refGroundAtom.ChangeState(new AMBULANCE_ADMINISTRATIVE_STATE());
                    return;
                }

                refActivityMovement.isEnded = true;
                refActivityMovement.isActive = false;
                refGroundAtom.currentRoute = null;

                refGroundAtom.knowsAboutEmergency = true;

                refGroundAtom.resetMovementData();
                Route straightLine = RoutePlanner.createRoute(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y),
                                                                        refGroundAtom.m_GameObject.getExplosionLocation());
                clsActivityMovement arrivalMovement = RoutePlanner.createActivityAndStart(refGroundAtom, 80, straightLine);

                refGroundAtom.ChangeState(new AMBULANCE_ARRIVAL_MOVEMENT_STATE(arrivalMovement));

                return;
            }

            base.Execute(refGroundAtom);

            // take the evacuee with me
            refCasualty.curr_X = refGroundAtom.curr_X;
            refCasualty.curr_Y = refGroundAtom.curr_Y;
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

    class FormGroundTaskSM
    {
    }
}
