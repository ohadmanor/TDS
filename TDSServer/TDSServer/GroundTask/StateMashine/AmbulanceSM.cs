using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDSServer.GroundTask.StateMashine
{
    public class AMBULANCE_ADMINISTRATIVE_STATE : ADMINISTRATIVE_STATE
    {
        public override void Enter(clsGroundAtom refGroundAtom)
        {
            base.Enter(refGroundAtom);
        }
        public override void Execute(clsGroundAtom refGroundAtom)
        {
            if (!refGroundAtom.knowsAboutEarthquake && refGroundAtom.m_GameObject.earthquakeStarted())
            {
                refGroundAtom.knowsAboutEarthquake = true;

                refGroundAtom.resetMovementData();
                Route straightLine = RouteUtils.createRoute(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y), refGroundAtom.m_GameObject.getExplosionLocation());
                clsActivityMovement arrivalMovement = RouteUtils.createActivityAndStart(refGroundAtom, 80, straightLine);

                refGroundAtom.ChangeState(new AMBULANCE_ARRIVAL_MOVEMENT_STATE(arrivalMovement));
                return;
            }
        }
    }

    public class AMBULANCE_ARRIVAL_MOVEMENT_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement = null;

        public AMBULANCE_ARRIVAL_MOVEMENT_STATE(clsActivityMovement ActivityMovement)
            : base(ActivityMovement)
        {
            refActivityMovement = ActivityMovement;
        }

        public override async void Enter(clsGroundAtom refGroundAtom)
        {
            //refGroundAtom.currentRoute = RoutePlanner.planStraightLineRoute(refActivityMovement.RouteActivity.Points.ElementAt(0),
            //                                        refActivityMovement.RouteActivity.Points.ElementAt(1),
            //                                        refActivityMovement.RouteActivity.RouteName);
            refGroundAtom.currentRoute = await refGroundAtom.m_GameObject.m_GameManager.refTerrain.createRouteByShortestPathOnly(refActivityMovement.RouteActivity.Points.ElementAt(0).x,
                refActivityMovement.RouteActivity.Points.ElementAt(0).y,
                refActivityMovement.RouteActivity.Points.ElementAt(1).x,
                refActivityMovement.RouteActivity.Points.ElementAt(1).y);
            refGroundAtom.currentSpeed = refActivityMovement.Speed;
        }

        public override void Execute(clsGroundAtom refGroundAtom)
        {
            if (refGroundAtom.currentRoute == null) return;

            if (refGroundAtom.currentLeg > refGroundAtom.currentRoute.arr_legs.Count)  //-1)  // refActivityMovement.RouteActivity.Points.Count()-1)
            {
                // arrived to ground zero. Now search for casualties
                refActivityMovement.isEnded = true;
                refActivityMovement.isActive = false;
                refGroundAtom.currentRoute = null;

                DPoint groundZeroLocation = refGroundAtom.m_GameObject.getExplosionLocation();
                double groundZeroRadius = refGroundAtom.m_GameObject.getExplosionRadius();
                List<clsGroundAtom> atomsInGroundZero = refGroundAtom.m_GameObject.m_GameManager.QuadTreeGroundAtom.SearchEntities(groundZeroLocation.x, groundZeroLocation.y, groundZeroRadius, isPrecise: true);
                foreach (clsGroundAtom atom in atomsInGroundZero)
                {
                    if (atom.healthStatus.isDead || atom.healthStatus.isIncapacitated)
                    {
                        refGroundAtom.resetMovementData();
                        Route straightLine = RouteUtils.createRoute(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y), new DPoint(atom.curr_X, atom.curr_Y));
                        clsActivityMovement extractionMovement = RouteUtils.createActivityAndStart(refGroundAtom, 5, straightLine);

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

        public AMBULANCE_GO_TO_CASUALTY_STATE(clsActivityMovement ActivityMovement, clsGroundAtom Casualty)
            : base(ActivityMovement)
        {
            refActivityMovement = ActivityMovement;
            refCasualty = Casualty;
        }

        public override void Enter(clsGroundAtom refGroundAtom)
        {
            refGroundAtom.currentRoute = RouteUtils.planStraightLineRoute(refActivityMovement.RouteActivity.Points.ElementAt(0),
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
                Route route = RouteUtils.createRoute(points);
                clsActivityMovement evacuationMovement = RouteUtils.createActivityAndStart(refGroundAtom, 80, route);
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

        public AMBULANCE_EVACUATION_MOVEMENT_STATE(clsActivityMovement ActivityMovement, clsGroundAtom Casualty)
            : base(ActivityMovement)
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

                refGroundAtom.knowsAboutEarthquake = true;

                DPoint ambulanceStartPoint = new DPoint(34.8514473088014, 32.1008536878526);
                refGroundAtom.curr_X = ambulanceStartPoint.x;
                refGroundAtom.curr_Y = ambulanceStartPoint.y;
                Route straightLine = RouteUtils.createRoute(new DPoint(ambulanceStartPoint.x, ambulanceStartPoint.y),
                                                                        refGroundAtom.m_GameObject.getExplosionLocation());
                clsActivityMovement arrivalMovement = RouteUtils.createActivityAndStart(refGroundAtom, 80, straightLine);

                refGroundAtom.ChangeState(new AMBULANCE_ARRIVAL_MOVEMENT_STATE(arrivalMovement));

                return;
            }

            base.Execute(refGroundAtom);

            // take the evacuee with me
            refCasualty.curr_X = refGroundAtom.curr_X;
            refCasualty.curr_Y = refGroundAtom.curr_Y;
        }
    }
}
