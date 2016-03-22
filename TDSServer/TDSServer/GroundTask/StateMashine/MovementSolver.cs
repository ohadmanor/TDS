using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDSServer.GroundTask.StateMashine
{
    class MovementSolver
    {
        private clsActivityMovement refActivityMovement;

        public MovementSolver(clsActivityMovement activityMovement)
        {
            refActivityMovement = activityMovement;
        }

        public void restartMovementOnRoute(clsGroundAtom refGroundAtom)
        {
            restartMovementOnRouteFromPoints(refGroundAtom, refGroundAtom.currentRoute.arr_legs[0].FromLongn, refGroundAtom.currentRoute.arr_legs[0].FromLatn);
        }

        public void restartMovementOnRouteFromPoints(clsGroundAtom refGroundAtom, double X_Route, double Y_Route)
        {
            refGroundAtom.currentLeg = 1;
            refGroundAtom.X_Route = X_Route;
            refGroundAtom.Y_Route = Y_Route;

            if (refGroundAtom.Offset_Azimuth != 0.0 || refGroundAtom.Offset_Distance != 0.0)
            {
                double initAzimDepl;
                double initNextRoute_X = 0;
                double initNextRoute_Y = 0;
                int initNextLeg = 0;

                refGroundAtom.VirtualMoveOnRoute(refGroundAtom.m_GameObject.m_GameManager.GroundCycleResolution, refGroundAtom.X_Route, refGroundAtom.Y_Route, out initNextRoute_X, out initNextRoute_Y, out initNextLeg);
                double initCurrentAzimuth = Util.Azimuth2Points(refGroundAtom.X_Route, refGroundAtom.Y_Route, initNextRoute_X, initNextRoute_Y);

                initAzimDepl = initCurrentAzimuth + refGroundAtom.Offset_Azimuth;
                if (initAzimDepl >= 360) initAzimDepl = initAzimDepl - 360;

                TerrainService.MathEngine.CalcProjectedLocationNew(initNextRoute_X, initNextRoute_Y, initAzimDepl, refGroundAtom.Offset_Distance, out refGroundAtom.curr_X, out refGroundAtom.curr_Y);//, true);
            }
            else
            {
                refGroundAtom.curr_X = refGroundAtom.currentRoute.arr_legs[0].FromLongn;
                refGroundAtom.curr_Y = refGroundAtom.currentRoute.arr_legs[0].FromLatn;
            }
        }

        public void manageCollisions(clsGroundAtom refGroundAtom, double X_Destination, double Y_Destination)
        {
            refGroundAtom.isCollision = false;

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

            List<clsGroundAtom> colAtoms = refGroundAtom.m_GameObject.m_GameManager.QuadTreeGroundAtom.SearchEntities(X_Destination, Y_Destination, 2 * clsGroundAtom.RADIUS, isPrecise: true);

            foreach (clsGroundAtom atom in colAtoms)
            {
                List<clsActivityBase> atomActivities;
                atom.m_GameObject.m_GroundActivities.TryGetValue(atom.GUID, out atomActivities);
                Boolean otherAtomIsIdle = true;

                if (atomActivities == null) continue;

                // check if other atom is active - we only want collisions with active atoms
                foreach(clsActivityBase otherAtomActivity in atomActivities) {
                    if (!otherAtomActivity.isEnded)
                    {
                        otherAtomIsIdle = false;
                        break;
                    }
                }

                if (!otherAtomIsIdle && !atom.healthStatus.isDead && !atom.healthStatus.isIncapacitated && atom != refGroundAtom)
                {
                    TerrainService.Vector vDest = new TerrainService.Vector(X_Destination, Y_Destination, 0);
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

                        if (atom.Collisions.Exists(v => v.name == refGroundAtom.MyName)) continue;

                        double d = TerrainService.MathEngine.CalcDistance(X_Destination, Y_Destination, atom.curr_X, atom.curr_Y);
                        refGroundAtom.isCollision = true;
                        CollisionTime cTime = new CollisionTime();
                        cTime.name = atom.MyName;
                        cTime.time = refGroundAtom.m_GameObject.Ex_clockDate;
                        refGroundAtom.Collisions.Add(cTime);

                        // add a collision. Also note whether the collision is frontal or not
                        refGroundAtom.m_GameObject.addCollision(Util.getAzimuthDifferenceDegrees(refGroundAtom.currentAzimuth, atom.currentAzimuth) > 90);
                        break;
                    }

                }
            }
        }
    }
}
