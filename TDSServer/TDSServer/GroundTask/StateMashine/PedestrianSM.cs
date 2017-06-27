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
            refGroundAtom.currentSpeed = refActivityMovement.Speed;
			
			// YD: randomly choose a route to follow
            typRoute R = refGroundAtom.m_GameObject.travelRoutes[refGroundAtom.currentStartWaypoint, refGroundAtom.currentEndWaypoint];
            refGroundAtom.currentRoute = R;
            refGroundAtom.currentRegularActivity = refActivityMovement;
			
			// YD: subscribe to event handlers
            refGroundAtom.earthquakeStartedEventHandler += earthquakeStartedDefaultEventHandler;
            refGroundAtom.earthquakeEndedEventHandler += earthquakeEndedDefaultEventHandler;
            refGroundAtom.forcesHaveArrivedEventHandler += forcesHaveArrivedDefaultEventHandler;
            refGroundAtom.deathEventHandler += deathDefaultEventHandler;
            refGroundAtom.incapacitationEventHandler += incapacitationDefaultEventHandler;
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

            if (refGroundAtom.currentRoute == null)
            {
                return;
            }

            // YD: go to a different direction when reached destination
            if (refGroundAtom.currentLeg > refGroundAtom.currentRoute.arr_legs.Count)
            {
                // destination waypoint is now the source waypoint
                refGroundAtom.currentStartWaypoint = refGroundAtom.currentEndWaypoint;
                int randomWaypoint = Util.random.Next(refGroundAtom.m_GameObject.travelRoutes.GetLength(1) - 1);
                if (randomWaypoint >= refGroundAtom.currentStartWaypoint) randomWaypoint++;

                refGroundAtom.currentEndWaypoint = randomWaypoint;
                refGroundAtom.currentLeg = 1;
                REGULAR_MOVEMENT_STATE regularMovement = (REGULAR_MOVEMENT_STATE)refGroundAtom.currentState;
                // re-enter the state
                regularMovement.Enter(refGroundAtom);
                return;
            }

            base.Execute(refGroundAtom);
        }
    }
	
	// YD: get on sidewalk when not on it
    public class GET_ON_SIDEWALK : MOVEMENT_STATE
    {
        private clsActivityMovement refActivityMovement;
        private DPoint pointOnSidewalk;
        private Route routeFromPoint;

        public GET_ON_SIDEWALK(clsActivityMovement refActivityMovement, DPoint pointOnSidewalk, Route routeFromPoint) : base(refActivityMovement)
        {
            this.pointOnSidewalk = pointOnSidewalk;
            this.routeFromPoint = routeFromPoint;
            this.refActivityMovement = refActivityMovement;
        }
        public override void Enter(clsGroundAtom refGroundAtom)
        {
            if (routeFromPoint == null)
            {
                // only go to sidewalk
                DPoint source = new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y);
                DPoint dest = new DPoint(pointOnSidewalk.x, pointOnSidewalk.y);
                refGroundAtom.currentRoute = RouteUtils.planStraightLineRoute(source, dest, "GetOnSidewalk_" + refGroundAtom.MyName);
            }
            else
            {
                // go to sidewalk and then to a route from that sidewalk given at routeFromPoint
                List<DPoint> points = new List<DPoint>();
                points.Add(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y));
                points.Add(new DPoint(pointOnSidewalk.x, pointOnSidewalk.y));

                // add all points except first which is the point on the sidewalk
                for (int i = 0; i < routeFromPoint.Points.Count(); i++)
                {
                    points.Add(routeFromPoint.Points.ElementAt(i)); ;
                }

                refGroundAtom.currentRoute = RouteUtils.createTypRoute(points, "GetOnSidewalk_" + refGroundAtom.MyName);
            }
        }

        public override void Execute(clsGroundAtom refGroundAtom)
        {
            // go to a different direction
            if (refGroundAtom.currentLeg > refGroundAtom.currentRoute.arr_legs.Count)
            {
                if (routeFromPoint == null)
                {
                    refGroundAtom.ChangeState(new ADMINISTRATIVE_STATE());
                    return;
                }

                // destination waypoint is now the source waypoint
                refGroundAtom.currentStartWaypoint = refGroundAtom.currentEndWaypoint;
                int randomWaypoint = Util.random.Next(refGroundAtom.m_GameObject.travelRoutes.GetLength(1) - 1);
                if (randomWaypoint >= refGroundAtom.currentStartWaypoint) randomWaypoint++;

                refGroundAtom.currentEndWaypoint = randomWaypoint;
                refGroundAtom.currentLeg = 1;
                refGroundAtom.ChangeState(new REGULAR_MOVEMENT_STATE(refActivityMovement));
                refGroundAtom.reEvaluationEventHandler += reevaluationAfterEarthquakeEndedEventHandler;
                return;
            }

            base.Execute(refGroundAtom);
        }
    }
	
	// YD: Hold on to an object that doesn't move
    public class HOLD_ON_TO_OBJECT_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement = null;
        private int backToNormalDelay;
        private DateTime earthquakeEndedClockTime;
        private bool earthquakeEnded;

        public HOLD_ON_TO_OBJECT_STATE(clsActivityMovement activityMovement)
            : base(activityMovement)
        {
            refActivityMovement = activityMovement;
            backToNormalDelay = Util.random.Next(1, 60);
        }

        public override void Enter(clsGroundAtom refGroundAtom)
        {
            // move towards a grabable object and a safer place
            double randomDistance = Util.random.NextDouble() * 5;
            double randomAzimuth = Util.random.NextDouble() * 360;

            double newX, newY;
            // calculate new point
            TerrainService.MathEngine.CalcProjectedLocationNew(refGroundAtom.curr_X, refGroundAtom.curr_Y, randomAzimuth, randomDistance, out newX, out newY);

            // make it the new route
            refGroundAtom.currentRoute = RouteUtils.planStraightLineRoute(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y),
                                                                            new DPoint(newX, newY), "HoldOnToObject");
            refGroundAtom.resetMovementData();
        }

        public override void Execute(clsGroundAtom refGroundAtom)
        {
            if (refGroundAtom.m_GameObject.earthquakeEnded())
            {
                if (!earthquakeEnded)
                {
                    earthquakeEnded = true;
                    earthquakeEndedClockTime = refGroundAtom.m_GameObject.Ex_clockDate;
                }
            }

            if (refGroundAtom.currentLeg > refGroundAtom.currentRoute.arr_legs.Count)
            {
                refActivityMovement.isEnded = true;
                refActivityMovement.isActive = false;
                return;
            }
            base.Execute(refGroundAtom);
        }
    }
	
	// YD: get curious
    public class CURIOSITY_MOVEMENT_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement = null;
        clsGroundAtom triggerAtom; // the atom that triggered this atom's curiosity

        public CURIOSITY_MOVEMENT_STATE(clsActivityMovement ActivityMovement, clsGroundAtom triggerAtom)
            : base(ActivityMovement)
        {
            refActivityMovement = ActivityMovement;
            this.triggerAtom = triggerAtom;
        }

        public override void Enter(clsGroundAtom refGroundAtom)
        {
			// go towards trigger in a straight line
            typRoute route = RouteUtils.planStraightLineRoute(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y),
                                                                  new DPoint(triggerAtom.curr_X, triggerAtom.curr_Y), "curiosity");
            refGroundAtom.currentRoute = route;
            refGroundAtom.resetMovementData();
        }

        public override void Execute(clsGroundAtom refGroundAtom)
        {
            if (refGroundAtom.currentLeg > refGroundAtom.currentRoute.arr_legs.Count)
            {
                refActivityMovement.isEnded = true;
                refActivityMovement.isActive = false;
                return;
            }

            base.Execute(refGroundAtom);
        }
    }
	
	// YD: help someone
    public class HELP_OTHER_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement = null;
        clsGroundAtom triggerAtom; // the atom that triggered this atom's will to help

        public HELP_OTHER_STATE(clsActivityMovement ActivityMovement, clsGroundAtom triggerAtom)
            : base(ActivityMovement)
        {
            refActivityMovement = ActivityMovement;
            this.triggerAtom = triggerAtom;
        }

        public override void Enter(clsGroundAtom refGroundAtom)
        {
            typRoute route = RouteUtils.planStraightLineRoute(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y),
                                                                  new DPoint(triggerAtom.curr_X, triggerAtom.curr_Y), "help_other");
            refGroundAtom.currentRoute = route;
            refGroundAtom.resetMovementData();
        }

        public override void Execute(clsGroundAtom refGroundAtom)
        {
            if (refGroundAtom.currentLeg > refGroundAtom.currentRoute.arr_legs.Count)
            {
                refActivityMovement.isEnded = true;
                refActivityMovement.isActive = false;
                return;
            }

            base.Execute(refGroundAtom);
        }
    }

    public class DEAD_STATE : BasicStateFormGroundTaskOrder
    {
        public override void Enter(clsGroundAtom refGroundAtom)
        {
            base.Enter(refGroundAtom);
            refGroundAtom.clearAllEventSubscriptions();
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
            refGroundAtom.clearAllEventSubscriptions();
        }
        public override void Execute(clsGroundAtom refGroundAtom)
        {
            // For now, do nothing. You're incapacitated but hey, you're still alive! Maybe call for help?
        }
    }
	
	// Go away from a building - you don't want to get crushed by it
    public class GO_AWAY_FROM_BUILDING_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement = null;
        TerrainService.Vector targetPosition;
        clsPolygon Structure;
        int exitEdgeNumber;

        public GO_AWAY_FROM_BUILDING_STATE(clsActivityMovement ActivityMovement, TerrainService.Vector targetPosition, clsPolygon Structure, int exitEdgeNumber)
            : base(ActivityMovement)
        {
            refActivityMovement = ActivityMovement;
            this.targetPosition = targetPosition;
            this.Structure = Structure;
            this.exitEdgeNumber = exitEdgeNumber;
        }

        public override void Enter(clsGroundAtom refGroundAtom)
        {
            typRoute route = RouteUtils.planStraightLineRoute(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y),
                                                                  new DPoint(targetPosition.x, targetPosition.y), "go_away_from_building");
            refGroundAtom.currentRoute = route;
            refGroundAtom.resetMovementData();
            refGroundAtom.earthquakeEndedEventHandler += earthquakeEndedExitingEventHandler;
        }

        public override void Execute(clsGroundAtom refGroundAtom)
        {
            if (refGroundAtom.currentLeg > refGroundAtom.currentRoute.arr_legs.Count)
            {
                refActivityMovement.isEnded = true;
                refActivityMovement.isActive = false;

                if (refGroundAtom.m_GameObject.earthquakeEnded())
                {
                    actAfterEarthquakeEnded(refGroundAtom);
                }

                return;
            }

            base.Execute(refGroundAtom);
        }

        // earthquake ended and an I'm outside the building. What now?
        protected void earthquakeEndedExitingEventHandler(object sender, EventArgs e)
        {
            clsGroundAtom refGroundAtom = ((clsGroundAtomEventArgs)e).groundAtom;
            actAfterEarthquakeEnded(refGroundAtom);
        }

        protected void actAfterEarthquakeEnded(clsGroundAtom refGroundAtom)
        {
            List<clsGroundAtom> atomsNearby = refGroundAtom.m_GameObject.m_GameManager.QuadTreeGroundAtom.SearchEntities(refGroundAtom.curr_X, refGroundAtom.curr_Y, 50, isPrecise: true);
            List<clsGroundAtom> casualtiesNearby = new List<clsGroundAtom>();

            if (moveToSocialComparisonStateIfShould(refGroundAtom)) return;

            // should the atom be curious?
            foreach (clsGroundAtom atom in atomsNearby)
            {
                if (atom.healthStatus.isIncapacitated || atom.healthStatus.isDead)
                {
                    casualtiesNearby.Add(atom);
                }
            }

            // decide whether to flock towards casualties
            if (casualtiesNearby.Count() > 0)
            {
                double randomAction = Util.random.NextDouble();
                int randomCasualty = Util.random.Next(casualtiesNearby.Count());
                clsActivityMovement curiousityActivity = RouteUtils.createActivityAndStart(refGroundAtom, (int)refGroundAtom.currentSpeed, null);

                if (randomAction < 0.3)
                {
                    refGroundAtom.ChangeState(new CURIOSITY_MOVEMENT_STATE(curiousityActivity, casualtiesNearby[randomCasualty]));
                }
                else if (randomAction >= 0.3 && randomAction < 0.8)
                {
                    refGroundAtom.ChangeState(new HELP_OTHER_STATE(curiousityActivity, casualtiesNearby[randomCasualty]));
                }
                else
                {
                    goToRegularMovementAfterExitingStructure(refGroundAtom);
                }
            }
            else
            {
                goToRegularMovementAfterExitingStructure(refGroundAtom);
            }
        }

        private void goToRegularMovementAfterExitingStructure(clsGroundAtom refGroundAtom)
        {
            clsActivityMovement backToNormal = RouteUtils.createActivityAndStart(refGroundAtom, (int)refGroundAtom.currentSpeed, null);
            clsPolygonOpeningEscapePoint escapePoint = Structure.EscapePoints[exitEdgeNumber];

            PointData closestPoint = refGroundAtom.m_GameObject.lookForClosestRegularRoute(refGroundAtom);
            Route route = RouteUtils.typRouteToRoute(closestPoint.route);
            List<DPoint> trimmedRoutePoints = new List<DPoint>();
            for (int i = closestPoint.legNum; i < route.Points.Count(); i++)
                trimmedRoutePoints.Add(route.Points.ElementAt(i));
            route.Points = trimmedRoutePoints;
            refGroundAtom.currentStartWaypoint = closestPoint.routeIndex1;
            refGroundAtom.currentEndWaypoint = closestPoint.routeIndex2;
            refGroundAtom.resetMovementData();
            refGroundAtom.currentSpeed = refGroundAtom.baselineSpeed;
            refGroundAtom.ChangeState(new GET_ON_SIDEWALK(backToNormal, new DPoint(escapePoint.x, escapePoint.y), route));
        }

        private int getClosestExitPoint(clsGroundAtom refGroundAtom, clsPolygon Structure)
        {
            DPoint[] coordinates = refGroundAtom.m_GameObject.getRegularMovementCoordinates();
            DPoint minPoint = null;
            int minIndex = 0;

            for (int i = 0; i < coordinates.Count(); i++)
            {
                DPoint coordinate = coordinates[i];
                if (minPoint == null || TerrainService.MathEngine.CalcDistance(refGroundAtom.curr_X, refGroundAtom.curr_Y, coordinate.x, coordinate.y) <
                                      TerrainService.MathEngine.CalcDistance(refGroundAtom.curr_X, refGroundAtom.curr_Y, minPoint.x, minPoint.y))
                {
                    minPoint = coordinate;
                    minIndex = i;
                }
            }

            return minIndex;
        }
    }

    public class GO_TO_POLICE_BARRIER_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement;
        public GO_TO_POLICE_BARRIER_STATE(clsActivityMovement refActivityMovement)
            : base(refActivityMovement)
        {
            this.refActivityMovement = refActivityMovement;
        } 

        public override void Enter(clsGroundAtom refGroundAtom)
        {
            // plan route to closest barrier
            Barrier closestBarrier = getClosestBarrier(refGroundAtom);
            //refGroundAtom.currentRoute = RouteUtils.planStraightLineRoute(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y),
            //    new DPoint(closestBarrier.x, closestBarrier.y), "GoToBarrier");
            //refGroundAtom.currentRoute = await refGroundAtom.m_GameObject.m_GameManager.refTerrain.createRouteByShortestPathOnly(refGroundAtom.curr_X, refGroundAtom.curr_Y,
            //                             closestBarrier.x, closestBarrier.y);
            PointData closestRouteToBarrier = refGroundAtom.m_GameObject.lookForClosestRouteToBarrier(refGroundAtom, closestBarrier);
            typRoute closestRoute = closestRouteToBarrier.route;
            int routeLength = closestRoute.arr_legs.Count();
            refGroundAtom.resetMovementData();
            List<DPoint> routePoints = new List<DPoint>();
            routePoints.Add(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y));
            for (int i = closestRouteToBarrier.legNum; i < closestRoute.arr_legs.Count(); i++)
            {
                routePoints.Add(new DPoint(closestRoute.arr_legs[i].FromLongn, closestRoute.arr_legs[i].FromLatn));
            }

            // add last point
            routePoints.Add(new DPoint(closestRoute.arr_legs[routeLength - 1].ToLongn, closestRoute.arr_legs[routeLength - 1].ToLatn));
            typRoute routeFromAtom = RouteUtils.createTypRoute(routePoints, "GoToBarrier");
            refGroundAtom.currentRoute = routeFromAtom;
        }

        public override void Execute(clsGroundAtom refGroundAtom)
        {
            if (refGroundAtom.currentRoute == null) return;

            if (refGroundAtom.currentLeg > refGroundAtom.currentRoute.arr_legs.Count)
            {
                refActivityMovement.isEnded = true;
                refActivityMovement.isActive = false;
                return;
            }

            base.Execute(refGroundAtom);
        }

        private Barrier getClosestBarrier(clsGroundAtom refGroundAtom)
        {
            Barrier closest = null;
            double minDistance = Double.PositiveInfinity;

            foreach (Barrier barrier in refGroundAtom.m_GameObject.getPoliceBarrierCoordinates())
            {
                double distance = TerrainService.MathEngine.CalcDistance(refGroundAtom.curr_X, refGroundAtom.curr_Y, barrier.x, barrier.y);
                if (distance < minDistance) {
                    minDistance = distance;
                    closest = barrier;
                }
            }

            return closest;
        }
    }
}
