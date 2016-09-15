using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDSServer.GroundTask.StateMashine
{
    public class MOVEMENT_IN_STRUCTURE_STATE : BasicStateFormGroundTaskOrder
    {
        public clsPolygon Structure = null;
        public TerrainService.Vector targetPosition;
        protected const double WAYPOINT_TOLERANCE = 15;
		
		// YD: event handler for when earthquake started and in structure
        protected void earthquakeStartedInStructureEventHandler(object sender, EventArgs e)
        {
			// exit the structure
            clsGroundAtom refGroundAtom = ((clsGroundAtomEventArgs)e).groundAtom;
            refGroundAtom.ChangeState(new EXIT_STRUCTURE_STATE(Structure));
        }

        public MOVEMENT_IN_STRUCTURE_STATE(clsPolygon _Structure)
        {
            Structure = _Structure;
            
        }
        public override void Enter(clsGroundAtom refGroundAtom)
        {
			// YD: subscribe to event handlers
            refGroundAtom.earthquakeStartedEventHandler += earthquakeStartedInStructureEventHandler;
            refGroundAtom.earthquakeEndedEventHandler += earthquakeEndedDefaultEventHandler;
            refGroundAtom.forcesHaveArrivedEventHandler += forcesHaveArrivedDefaultEventHandler;
            refGroundAtom.incapacitationEventHandler += incapacitationDefaultEventHandler;
            refGroundAtom.deathEventHandler += deathDefaultEventHandler;
            targetPosition = CalculateNextRandomPosition(5000, refGroundAtom.curr_X, refGroundAtom.curr_Y);
        }
        public override void Execute(clsGroundAtom refGroundAtom)
        {
            MoveInStructure(refGroundAtom,refGroundAtom.m_GameObject.m_GameManager.GroundCycleResolution);
        }
        private void MoveInStructure(clsGroundAtom refGroundAtom,int DeltaTime)
        {
            double DeltaTimeSec = DeltaTime * 0.001;
            double dist = refGroundAtom.currentSpeed * DeltaTimeSec / 3600;

            double oldX = refGroundAtom.curr_X;
            double oldY = refGroundAtom.curr_Y;

            // YD - fixed bug where atoms in building travel way to fast than they should be.
            // Details: In the third line below addition of distance in meters to position in coordinates!
            //TerrainService.Vector vt = targetPosition - refGroundAtom.currPosition;
            //vt.normalize();
            //TerrainService.Vector NewPosition = refGroundAtom.currPosition + (vt * dist); 
            ////.........

            TerrainService.Vector vt = targetPosition - refGroundAtom.currPosition;
            double targetDist = TerrainService.MathEngine.CalcDistance(refGroundAtom.currPosition.x, refGroundAtom.currPosition.y, targetPosition.x, targetPosition.y);
            double targetAzimuth = Util.Azimuth2Points(refGroundAtom.currPosition.x, refGroundAtom.currPosition.y, targetPosition.x, targetPosition.y);
            TerrainService.Vector NewPosition = new TerrainService.Vector();
            TerrainService.MathEngine.CalcProjectedLocationNew(refGroundAtom.currPosition.x, refGroundAtom.currPosition.y, targetAzimuth, 1000 * dist, out NewPosition.x, out NewPosition.y);
            // ----------------------------------------------------------------------------------------

            // check if in collision
            clsGroundAtom collidingAtom = null;
            manageCollisions(refGroundAtom, NewPosition, out collidingAtom);

            TerrainService.GeometryHelper.Edge CrossedEdge = new TerrainService.GeometryHelper.Edge();
            TerrainService.shPoint[] Pnts = Structure.Points.ToArray();
            // step aside in case of collision
            if (!refGroundAtom.isCollision)
            {
                // draw avoidance side randomly to when collision will occur
                int random = Util.random.Next(2) * 2 - 1;
                refGroundAtom.Offset_Azimuth = 90 * random;
            }
            if (refGroundAtom.isCollision)
            {
				// choose side to step to according to social comparison theory
                clsGroundAtom mostSimilar = SocialComparison.findMostSimilarInStructure(refGroundAtom, Structure);

                if (mostSimilar != null)
                {
                    TerrainService.Vector headingVector = new TerrainService.Vector();
                    headingVector.x = targetPosition.x - refGroundAtom.curr_X;
                    headingVector.y = targetPosition.y - refGroundAtom.curr_Y;
                    TerrainService.Vector vectorToMostSimilar = new TerrainService.Vector();
                    vectorToMostSimilar.x = mostSimilar.curr_X - refGroundAtom.curr_X;
                    vectorToMostSimilar.y = mostSimilar.curr_Y - refGroundAtom.curr_Y;
                    bool mostSimilarIsToTheLeft = (headingVector ^ vectorToMostSimilar).norm() > 0;
                    if (mostSimilarIsToTheLeft) refGroundAtom.Offset_Azimuth = 90;
                    else refGroundAtom.Offset_Azimuth = -90;
                }

                List<CollisionTime> collisions = refGroundAtom.Collisions;
                double azimuthToCollidingAtom = Util.Azimuth2Points(refGroundAtom.currPosition.x, refGroundAtom.currPosition.y, collidingAtom.currPosition.x, collidingAtom.currPosition.y);
                TerrainService.MathEngine.CalcProjectedLocationNew(refGroundAtom.currPosition.x, refGroundAtom.currPosition.y, azimuthToCollidingAtom + refGroundAtom.Offset_Azimuth, 1000 * dist, out NewPosition.x, out NewPosition.y);
            }

            bool isCross = TerrainService.GeometryHelper.GeometryMath.isPolygonBorderCross(refGroundAtom.curr_X, refGroundAtom.curr_Y, NewPosition.x, NewPosition.y, ref Pnts, ref CrossedEdge);
            if (isCross)
            {
               CrossedEdge.rot90();
               TerrainService.Vector orig = new TerrainService.Vector(CrossedEdge.org.x, CrossedEdge.org.y,0);
               TerrainService.Vector dest = new TerrainService.Vector(CrossedEdge.dest.x, CrossedEdge.dest.y, 0);
               TerrainService.Vector n = dest - orig;
               
                n.normalize();

            

               TerrainService.Vector NewD = vt - 2 * (vt * n) * n;
               NewD.normalize();
            
               TerrainService.Vector NewDirect = NewD * 5000;

         
		 		// YD: calculate new target position according to next waypoint and not randomly
                //targetPosition = NewDirect;
               targetPosition = CalculateNextWaypointPosition(refGroundAtom);
			   // ---

                // YD: Bug fix for moving too fast in buildings
                //vt = targetPosition - refGroundAtom.currPosition;
                //vt.normalize();
                //NewPosition = refGroundAtom.currPosition + (vt * dist);

                vt = targetPosition - refGroundAtom.currPosition;
                targetDist = TerrainService.MathEngine.CalcDistance(refGroundAtom.currPosition.x, refGroundAtom.currPosition.y, targetPosition.x, targetPosition.y) / 1000;
                targetAzimuth = Util.Azimuth2Points(refGroundAtom.currPosition.x, refGroundAtom.currPosition.y, targetPosition.x, targetPosition.y);
                TerrainService.MathEngine.CalcProjectedLocationNew(refGroundAtom.currPosition.x, refGroundAtom.currPosition.y, targetAzimuth, 1000 * dist, out NewPosition.x, out NewPosition.y);
                // ---------------------------------------------------------------------------------

                //refGroundAtom.currPosition = NewPosition;
                //refGroundAtom.curr_X = refGroundAtom.currPosition.x;
                //refGroundAtom.curr_Y = refGroundAtom.currPosition.y;

               // return;
            }
          

            refGroundAtom.currPosition = NewPosition;
			// YD: get current azimuth
            refGroundAtom.currentAzimuth = Util.Azimuth2Points(refGroundAtom.curr_X, refGroundAtom.curr_Y, NewPosition.x, NewPosition.y);
            refGroundAtom.curr_X = refGroundAtom.currPosition.x;
            refGroundAtom.curr_Y = refGroundAtom.currPosition.y;



            bool isIn = TerrainService.GeometryHelper.GeometryMath.isPointInPolygon(refGroundAtom.curr_X, refGroundAtom.curr_Y, ref Pnts);
            if (isIn == false)
            {
                System.Random Rnd = new Random();
                while (true)
                {
                    double vRnd = Rnd.NextDouble();
                    double randX = Structure.minX + (Structure.maxX - Structure.minX) * vRnd;
                    vRnd = Rnd.NextDouble();
                    double randY = Structure.minY + (Structure.maxY - Structure.minY) * vRnd;
                    bool inPolygon = TerrainService.GeometryHelper.GeometryMath.isPointInPolygon(randX, randY, ref  Pnts);
                    if (inPolygon == true)
                    {
                        refGroundAtom.curr_X = randX;
                        refGroundAtom.curr_Y = randY;
                        refGroundAtom.currPosition = new TerrainService.Vector(randX, randY, 0);
						// YD: calculate new target position according to next waypoint and not randomly
                        //targetPosition = CalculateNextRandomPosition(5000, refGroundAtom.curr_X, refGroundAtom.curr_Y);
                        targetPosition = CalculateNextWaypointPosition(refGroundAtom);
						// ---
                        break;
                    }
                }
            }

			// YD: since there are more than once structure, update possition according to the right structure's quadtree
            refGroundAtom.m_GameObject.getQuadTreeByStructure(Structure).PositionUpdate(refGroundAtom);
            
        }
        internal TerrainService.Vector CalculateNextRandomPosition(double MaxRange, double Xcenter, double Ycenter)
        {
           
            double dist = 0;
            double MapX = 0;
            double MapY = 0;
         
            double azim = 360 *  Util.random.NextDouble();
            dist = MaxRange; //MaxRange * Util.random.NextDouble();            
            TerrainService.MathEngine.CalcProjectedLocationNew(Xcenter, Ycenter, azim, dist, out MapX, out MapY);
            TerrainService.Vector Position = new TerrainService.Vector(MapX, MapY, 0);            
            return Position;
        }
		
		// YD: get next waypoint to go to in structure
        internal TerrainService.Vector CalculateNextWaypointPosition(clsGroundAtom refGroundAtom)
        {
			// next point is drawn randomly
            int numOfWaypointNeighbors = refGroundAtom.currentStructureWaypoint.neighbors.Count();
            PolygonWaypoint nextWaypoint = refGroundAtom.currentStructureWaypoint.neighbors[Util.random.Next(numOfWaypointNeighbors)];
            targetPosition = new TerrainService.Vector();
            targetPosition.x = nextWaypoint.x;
            targetPosition.y = nextWaypoint.y;
            refGroundAtom.currentStructureWaypoint = nextWaypoint;
            refGroundAtom.currentAzimuth = Util.Azimuth2Points(refGroundAtom.curr_X, refGroundAtom.curr_Y,
                                                               nextWaypoint.x, nextWaypoint.y);
            return targetPosition;
        }
		
		// YD: manage collisions like on sidewalk
        private void manageCollisions(clsGroundAtom refGroundAtom, TerrainService.Vector newPosition, out clsGroundAtom collidingAtom)
        {
            collidingAtom = null;
            List<CollisionTime> CollisionsToDelete = new List<CollisionTime>();
            foreach (var v in refGroundAtom.Collisions)
            {
                if ((refGroundAtom.m_GameObject.Ex_clockDate - v.time).TotalSeconds > 2)
                {
                    CollisionsToDelete.Add(v);
                }
            }
            foreach (var v in CollisionsToDelete)
            {
                refGroundAtom.Collisions.Remove(v);
            }

            refGroundAtom.isCollision = false;

            List<clsGroundAtom> colAtoms = refGroundAtom.m_GameObject.getQuadTreeByStructure(Structure).SearchEntities(newPosition.x, newPosition.y, 2 * clsGroundAtom.RADIUS, isPrecise: true);

            foreach (clsGroundAtom atom in colAtoms)
            {
                if (atom != refGroundAtom)
                {
                    TerrainService.Vector vDest = new TerrainService.Vector(newPosition.x, newPosition.y, 0);
                    TerrainService.Vector vMe = new TerrainService.Vector(refGroundAtom.curr_X, refGroundAtom.curr_Y, 0);

                    TerrainService.Vector vCollision = new TerrainService.Vector(atom.curr_X, atom.curr_Y, 0);

                    TerrainService.Vector MyDirection = vDest - vMe;
                    MyDirection.normalize();
                    TerrainService.Vector CollisionDirection = vCollision - vMe;
                    CollisionDirection.normalize();
                    double dot = MyDirection * CollisionDirection;
                    if (dot >= 0.8)// 0.6)                                                  //Against  Main Direction
                    {
                        // if (atom.Collisions.Contains(refGroundAtom.MyName)) continue;

                        // Fix 03              if (atom.Collisions.Exists(v => v.name == refGroundAtom.MyName)) continue;
                        if (atom.Collisions.Exists(v => v.name == refGroundAtom.MyName)) continue;
                        //Fix 04 - New If
                        double d = TerrainService.MathEngine.CalcDistance(newPosition.x, newPosition.y, atom.curr_X, atom.curr_Y);
                        refGroundAtom.isCollision = true;
                        CollisionTime cTime = new CollisionTime();
                        cTime.name = atom.MyName;
                        cTime.time = refGroundAtom.m_GameObject.Ex_clockDate;
                        refGroundAtom.Collisions.Add(cTime);
                        collidingAtom = atom;
                        break;
                    }

                }
            }
        }
		
		// check if atom reached target position
        public bool reachedTargetPosition(clsGroundAtom refGroundAtom)
        {
            double DeltaTimeSec = refGroundAtom.m_GameObject.m_GameManager.GroundCycleResolution * 0.001;
            double dist = (refGroundAtom.currentSpeed * DeltaTimeSec / 3600) * 1000;
            double distanceFromTarget = TerrainService.MathEngine.CalcDistance(refGroundAtom.curr_X, refGroundAtom.curr_Y, targetPosition.x, targetPosition.y);
            double azimuthToTargetPosition = Util.Azimuth2Points(refGroundAtom.curr_X, refGroundAtom.curr_Y, targetPosition.x, targetPosition.y);
            double headingDifference = Util.getAzimuthDifferenceDegrees(refGroundAtom.currentAzimuth, azimuthToTargetPosition);
            return (distanceFromTarget < WAYPOINT_TOLERANCE * dist) && (Math.Abs(headingDifference) < 90);
        }
    }
    class MoveInStructureSM
    {
    }
	
	// YD: regular movement in structure
    class REGULAR_MOVEMENT_IN_STRUCTURE_STATE : MOVEMENT_IN_STRUCTURE_STATE
    {
        private PolygonWaypoint nextWaypoint;

        public REGULAR_MOVEMENT_IN_STRUCTURE_STATE(clsPolygon _Structure, PolygonWaypoint nextWaypoint)
            : base(_Structure)
        {
            this.nextWaypoint = nextWaypoint;
        }

        public override void Enter(clsGroundAtom refGroundAtom)
        {
			// subscribe event handlers
            refGroundAtom.earthquakeStartedEventHandler += earthquakeStartedInStructureEventHandler;
            refGroundAtom.earthquakeEndedEventHandler += earthquakeEndedDefaultEventHandler;
            refGroundAtom.forcesHaveArrivedEventHandler += forcesHaveArrivedDefaultEventHandler;
            refGroundAtom.incapacitationEventHandler += incapacitationDefaultEventHandler;
            refGroundAtom.deathEventHandler += deathDefaultEventHandler;

            if (nextWaypoint != null)
            {
                targetPosition = new TerrainService.Vector();
                targetPosition.x = nextWaypoint.x;
                targetPosition.y = nextWaypoint.y;
            }
            else
            {
                targetPosition = CalculateNextWaypointPosition(refGroundAtom);  
            }
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

			// get next waypoint when the current waypoint is reached
            if (reachedTargetPosition(refGroundAtom))
            {
                targetPosition = CalculateNextWaypointPosition(refGroundAtom);
                return;
            }
			
			// perform step
            base.Execute(refGroundAtom);
        }
    }
	
	// YD: staying in structure
    class STAY_IN_STRUCTURE_STATE : MOVEMENT_IN_STRUCTURE_STATE
    {
        public STAY_IN_STRUCTURE_STATE(clsPolygon _Structure)
            : base(_Structure)
        {
        }

        public override void Enter(clsGroundAtom refGroundAtom)
        {
            targetPosition = new TerrainService.Vector();
            double randomDistance = Util.random.NextDouble() * 10;
            double randomAzimuth = Util.random.NextDouble() * 360;
            TerrainService.MathEngine.CalcProjectedLocationNew(refGroundAtom.currPosition.x, refGroundAtom.currPosition.y, randomAzimuth, randomDistance, out targetPosition.x, out targetPosition.y);
            TerrainService.GeometryHelper.Edge CrossedEdge = new TerrainService.GeometryHelper.Edge();
            TerrainService.shPoint[] Pnts = Structure.Points.ToArray();
            bool isCross = TerrainService.GeometryHelper.GeometryMath.isPolygonBorderCross(refGroundAtom.curr_X, refGroundAtom.curr_Y, targetPosition.x, targetPosition.y, ref Pnts, ref CrossedEdge);

            if (isCross)
            {
                targetPosition.x = refGroundAtom.curr_X;
                targetPosition.y = refGroundAtom.curr_Y;
            }

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

            base.Execute(refGroundAtom);
        }
    }
	
	// YD: exit structure
    class EXIT_STRUCTURE_STATE : MOVEMENT_IN_STRUCTURE_STATE
    {

        private int exitEdgeNumber;
        private int waypointIndex;
        private bool earthquakeEnded;
        private List<PolygonWaypoint> exitWaypoints;

        public EXIT_STRUCTURE_STATE(clsPolygon _Structure) : base(_Structure)
        {
        }

        public override void Enter(clsGroundAtom refGroundAtom)
        {
            refGroundAtom.clearAllEventSubscriptions();
            refGroundAtom.forcesHaveArrivedEventHandler += forcesHaveArrivedDefaultEventHandler;
            exitWaypoints = Structure.waypointGraph.findExitPath(refGroundAtom.currentStructureWaypoint);
            PolygonWaypoint exitWaypoint = exitWaypoints[exitWaypoints.Count() - 1];
            exitEdgeNumber = exitWaypoint.edgeNum;
            targetPosition = new TerrainService.Vector();
            targetPosition.x = exitWaypoints[waypointIndex].x;
            targetPosition.y = exitWaypoints[waypointIndex].y;
            waypointIndex++;
        }
        public override void Execute(clsGroundAtom refGroundAtom)
        {
            exitBuilding(refGroundAtom);

            base.Execute(refGroundAtom);

            // check if current waypoint has been reached
            double distanceFromWaypoint = TerrainService.MathEngine.CalcDistance(refGroundAtom.curr_X, refGroundAtom.curr_Y,
                                                    refGroundAtom.currentStructureWaypoint.x, refGroundAtom.currentStructureWaypoint.y);
            if (reachedTargetPosition(refGroundAtom) && waypointIndex < exitWaypoints.Count()) {
                refGroundAtom.currentStructureWaypoint = exitWaypoints[waypointIndex];
                targetPosition.x = exitWaypoints[waypointIndex].x;
                targetPosition.y = exitWaypoints[waypointIndex].y;
                refGroundAtom.currentAzimuth = Util.Azimuth2Points(refGroundAtom.curr_X, refGroundAtom.curr_Y,
                                                                   targetPosition.x, targetPosition.y);
                waypointIndex++;
            }
        }

        protected void earthquakeEndedExitingEventHandler(object sender, EventArgs e)
        {
            earthquakeEnded = true;
        }

        private void exitBuilding(clsGroundAtom refGroundAtom)
        {
            double distanceToExit = TerrainService.MathEngine.CalcDistance(refGroundAtom.curr_X, refGroundAtom.curr_Y, targetPosition.x, targetPosition.y);
            double DeltaTimeSec = refGroundAtom.m_GameObject.m_GameManager.GroundCycleResolution * 0.001;
            double targetAzimuth = Util.Azimuth2Points(refGroundAtom.currPosition.x, refGroundAtom.currPosition.y, targetPosition.x, targetPosition.y);
            double stepDistanceInMeters = 1000 * (refGroundAtom.currentSpeed * DeltaTimeSec / 3600);

            TerrainService.Vector NewPosition = new TerrainService.Vector();
            TerrainService.MathEngine.CalcProjectedLocationNew(refGroundAtom.currPosition.x, refGroundAtom.currPosition.y, targetAzimuth, stepDistanceInMeters, out NewPosition.x, out NewPosition.y);
            TerrainService.GeometryHelper.Edge CrossedEdge = new TerrainService.GeometryHelper.Edge();
            TerrainService.shPoint[] Pnts = Structure.Points.ToArray();
            bool isCross = TerrainService.GeometryHelper.GeometryMath.isPolygonBorderCross(refGroundAtom.curr_X, refGroundAtom.curr_Y, NewPosition.x, NewPosition.y, ref Pnts, ref CrossedEdge);

            // reached exit - go away from building, you don't want it to fall on you
            if (isCross)
            {
                // calculate displacement vector
                TerrainService.Vector currentDirection = NewPosition - new TerrainService.Vector(refGroundAtom.curr_X, refGroundAtom.curr_Y, 0);

                // calculate edge vector
                TerrainService.Vector edgePointVector0 = new TerrainService.Vector(CrossedEdge.org.x, CrossedEdge.org.y, 0);
                TerrainService.Vector edgePointVector1 = new TerrainService.Vector(CrossedEdge.dest.x, CrossedEdge.dest.y, 0);
                TerrainService.Vector edgeVector = edgePointVector1 - edgePointVector0;

                // now calculate the perpendicular to the edge
                TerrainService.Vector projectionOnEdge = edgeVector * (currentDirection * edgeVector) / (edgeVector * edgeVector);
                TerrainService.Vector perpendicular = currentDirection - projectionOnEdge;

                // and normalize it
                perpendicular.normalize();

                // get the azimuth perpendicular to the edge
                double perpendicularAzimuth = Util.Azimuth2Points(0, 0, perpendicular.x, perpendicular.y) + Util.random.NextDouble() * 120 - 30;

                // distance to go away from building
                double distanceMeters = 15 * Util.random.NextDouble() + 10;

                TerrainService.Vector targetLocation = new TerrainService.Vector();
                TerrainService.MathEngine.CalcProjectedLocationNew(refGroundAtom.curr_X, refGroundAtom.curr_Y, perpendicularAzimuth, distanceMeters, out targetLocation.x, out targetLocation.y);

                clsActivityMovement goAwayActivity = RouteUtils.createActivityAndStart(refGroundAtom, (int)refGroundAtom.currentSpeed, null);
                refGroundAtom.ChangeState(new GO_AWAY_FROM_BUILDING_STATE(goAwayActivity, targetLocation, Structure, exitEdgeNumber));
                return;
            }
        }
    }
}
