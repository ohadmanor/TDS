using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDSServer
{
    public class clsPolygon
    {
        public string PolygonName;
        public string PolygonGuid;
        public IEnumerable<TerrainService.shPoint> Points;
		//YD:
		// all the openings of the polygon
        public List<clsPolygonOpening> PolygonOpenings;
		
		// the waypoint graph of the polygon - corridors, rooms, openings, etc.
        public PolygonWaypointGraph waypointGraph;
		
		// a map between an opening to its corresponding escape point
        public Dictionary<int, clsPolygonOpeningEscapePoint> EscapePoints;
		// ---

        public double minX = double.MaxValue;
        public double maxX = double.MinValue;
        public double minY = double.MaxValue;
        public double maxY = double.MinValue;
    }
	// YD:
    public class clsPolygonOpening
    {
        public String PolygonGUID;
        public int PolygonEdgeNum;
        public double x;
        public double y;
        public double openingSize;
        public List<clsPolygonOpeningEscapePoint> escapePoints;
    }

    public class clsPolygonOpeningEscapePoint
    {
        public String PolygonGUID;
        public int polygonEdgeNum;
        public double x;
        public double y;
    }

    public class PolygonWaypointGraph
    {
        public List<PolygonWaypoint> waypoints;
        public List<PolygonWaypoint> rooms;
        public List<PolygonWaypoint> corridors;
        public List<PolygonWaypoint> openings;

        public PolygonWaypointGraph()
        {
            waypoints = new List<PolygonWaypoint>();
            rooms = new List<PolygonWaypoint>();
            corridors = new List<PolygonWaypoint>();
            openings = new List<PolygonWaypoint>();
        }

        public void addWaypoint(PolygonWaypoint waypoint, params PolygonWaypoint[] neighbors)
        {
            // do not allow duplicate waypoints!
            if (waypoints.Contains(waypoint)) return;

            waypoints.Add(waypoint);
            if (waypoint.waypointType == PolygonWaypoint.WaypointType.CORRIDOR) corridors.Add(waypoint);
            if (waypoint.waypointType == PolygonWaypoint.WaypointType.ROOM) rooms.Add(waypoint);
            if (waypoint.waypointType == PolygonWaypoint.WaypointType.OPENING) openings.Add(waypoint);

            if (neighbors != null)
            {
                foreach (PolygonWaypoint neighbor in neighbors)
                {
                    // this is a directed graph so we must add neighborhoods mutually, yet avoid duplicates
                    if (!waypoint.neighbors.Contains(neighbor)) waypoint.neighbors.Add(neighbor);
                    if (!neighbor.neighbors.Contains(waypoint)) neighbor.neighbors.Add(waypoint);
                }
            }
        }

        // simple use of dijkstra's algorithm to find shortest path to an exit (incomplete, if necessary at all)
        public List<PolygonWaypoint> findShortestPathToWaypoint(PolygonWaypoint srcWaypoint, PolygonWaypoint destWaypoint)
        {
            Dictionary<PolygonWaypoint, Double> waypointDistances = new Dictionary<PolygonWaypoint, double>();
            Dictionary<PolygonWaypoint, PolygonWaypoint> prevs = new Dictionary<PolygonWaypoint, PolygonWaypoint>();
            List<PolygonWaypoint> unvisited = new List<PolygonWaypoint>();
            List<PolygonWaypoint> visited = new List<PolygonWaypoint>();

            // start with all nodes unvisited
            foreach (PolygonWaypoint point in waypoints)
            {
                waypointDistances.Add(point, Double.PositiveInfinity);
            }

            // set starting waypoint with tentative distance 0 and evaluate neighboring waypoints
            unvisited.Remove(srcWaypoint);
            waypointDistances[srcWaypoint] = 0;
            visited.Add(srcWaypoint);

            // keep evaluating waypoints until exit waypoint is evaluated
            while (true)
            {
                double minTentativeDistance = Double.PositiveInfinity;
                PolygonWaypoint minNeighbor = null;

                // find the minimal tentative distance from the visited set to the neighbor
                foreach (PolygonWaypoint visitedWaypoint in visited)
                {
                    foreach (PolygonWaypoint neighbor in visitedWaypoint.neighbors)
                    {
                        if (visited.Contains(neighbor)) continue;

                        double distance = TerrainService.MathEngine.CalcDistance(visitedWaypoint.x, visitedWaypoint.y, neighbor.x, neighbor.y);
                        if (distance + waypointDistances[visitedWaypoint] < waypointDistances[neighbor])
                        {
                            waypointDistances[neighbor] = distance + waypointDistances[visitedWaypoint];
                        }

                        if (waypointDistances[neighbor] < minTentativeDistance)
                        {
                            minTentativeDistance = waypointDistances[neighbor];
                            minNeighbor = neighbor;
                        }
                    }
                }

                visited.Add(minNeighbor);
                unvisited.Remove(minNeighbor);
            }

            return null;
        }
		
		// arbitrarily find an exit path from a specific waypoint
        public List<PolygonWaypoint> findExitPath(PolygonWaypoint waypoint)
        {
            List<PolygonWaypoint> visitedWaypoints = new List<PolygonWaypoint>();
            List<PolygonWaypoint> exitPath = new List<PolygonWaypoint>();
            visitedWaypoints.Add(waypoint);

            // if destination waypoint is already an opening let it be our path out
            if (waypoint.waypointType == PolygonWaypoint.WaypointType.OPENING)
            {
                exitPath.Add(waypoint);
                return exitPath;
            }

            // if not, go ahead and find a path
            exitPath.AddRange(findExitPath(waypoint, visitedWaypoints));
            exitPath.Add(waypoint);
            exitPath.Reverse();

            return exitPath;
        }

        // find closest waypoint in the polygon to the given coordinates
        public PolygonWaypoint findClosestWaypoint(double x, double y)
        {
            PolygonWaypoint closest = waypoints.ElementAt(0);
            foreach (PolygonWaypoint w in waypoints) {
                double distToClosest = TerrainService.MathEngine.CalcDistance(x, y, closest.x, closest.y);
                double distToCurrentWaypoint = TerrainService.MathEngine.CalcDistance(x, y, w.x, w.y);
                if (distToCurrentWaypoint < distToClosest) closest = w;
            }

            return closest;
        }
		
		// helper method for the above
        private List<PolygonWaypoint> findExitPath(PolygonWaypoint waypoint, List<PolygonWaypoint> visitedWaypoints)
        {
            foreach (PolygonWaypoint neighbor in waypoint.neighbors)
            {
                if (!visitedWaypoints.Contains(neighbor))
                {
                    visitedWaypoints.Add(neighbor);

                    // found an opening - pack away the recursion and return the path
                    if (neighbor.waypointType == PolygonWaypoint.WaypointType.OPENING)
                    {
                        List<PolygonWaypoint> exitPath = new List<PolygonWaypoint>();
                        exitPath.Add(neighbor);
                        return exitPath;
                    }
                    else
                    {
                        List<PolygonWaypoint> exitPath = findExitPath(neighbor, visitedWaypoints);
                        if (exitPath != null)
                        {
                            exitPath.Add(neighbor);
                            return exitPath;
                        }
                    }
                }
            }

            return null;
        }
    }
	
	// a waypoint in a polygon's waypoint graph
    public class PolygonWaypoint
    {
        public enum WaypointType { ROOM, CORRIDOR, OPENING };

        public List<PolygonWaypoint> neighbors;
        public WaypointType waypointType;
        public int id;
        public int edgeNum;

        public PolygonWaypoint(int id, double x, double y, WaypointType type) {
            this.id = id;
            this.x = x;
            this.y = y;
            this.waypointType = type;
            neighbors = new List<PolygonWaypoint>();
            edgeNum = -1;
        }

        public double x;
        public double y;
    }
	// ---
}
