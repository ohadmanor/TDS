using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TDSServer.GroundTask;
//VH
using TDSServer.GroundTask.StateMashine;

namespace TDSServer
{
    public class GameObject
    {
        internal GameManager m_GameManager;
        private double LastExClockTick = 0;
        private int ExClockResolution = 200;  //200 milisec  5 times per second

        public DateTime Ex_clockDate;
        public DateTime Ex_clockGroundExecute = DateTime.MinValue;
        public Task GroundExecuteStateTask = null;

		// Yinon Douchan: Code for statistics and simulation of explosion
        public DateTime Ex_clockStartDate;
        private List<CollisionData> collisionData;
        private long totalCollisions, nonFrontalCollisions, frontalCollisions;
        private bool earthquakeOccurred;
        private bool earthquakeAlreadyEnded;
        private bool forcesAlreadyArrived;
		// --------------------------------------------------------------

        // simulation stage:
		// STAGE_1 - normal movement before the earthquake
		// STAGE_2 - during the earthquake
		// STAGE_3 - immediately after the earthquake
		// STAGE_4 - forces taking over the area
        public enum SimulationStage { STAGE_1, STAGE_2, STAGE_3, STAGE_4 };

        private SimulationStage m_simulationStage;
        public SimulationStage simulationStage
        {
            get { return m_simulationStage; }
        }


        internal ConcurrentDictionary<string, AtomBase> GroundAtomObjectCollection = new ConcurrentDictionary<string, AtomBase>();
        public Dictionary<string, List<clsActivityBase>> m_GroundActivities = new Dictionary<string, List<clsActivityBase>>();
        //VH
        public clsPolygon Structure1 = null;
        public clsPolygon Structure2 = null;
        public int AtomQtyInStructure = 50;
        public int AtomQtyOnSidewalks = 100;

        // waypoints on sidewalks
        public typRoute[,] travelRoutes;
        // routes from waypoints to barriers routesToBarriers[waypoint,barrier]
        public typRoute[,] routesToBarriers;
        // map a point to its info containing what route it belongs to and where
        public Dictionary<DPoint, PointData> pointDataMap;
        public Dictionary<DPoint, PointData> routesToBarriersDataMap;
        // barriers
        public List<Barrier> barriers;

        internal GameObject(GameManager refGameManager)
        {
            m_GameManager =refGameManager;
        }
        internal void InitObjects()
        {
            Ex_clockDate = DateTime.Now;
			// Yinon Douchan: Code for statistics and simulation of explosion
            Ex_clockStartDate = Ex_clockDate;
            earthquakeOccurred = false;
            earthquakeAlreadyEnded = false;
            forcesAlreadyArrived = false;
            m_simulationStage = SimulationStage.STAGE_1;

            // clear collision report history
            if (collisionData == null) collisionData = new List<CollisionData>();
			// --------------------------------------------------------------
            GroundAtomsInit();

            //VH
            PolygonsInit();
            RandomizeAtomsInArea(Structure1, ref m_GameManager.QuadTreeStructure1GroundAtom, AtomQtyInStructure);
            RandomizeAtomsInArea(Structure2, ref m_GameManager.QuadTreeStructure2GroundAtom, AtomQtyInStructure/2);



            GroundMissionActivitiesInit();

            // YD - add atoms to sidewalk
            randomizeAtomsOnSidewalks(AtomQtyOnSidewalks);
            // ---------------------------

        }
        //VH
        internal void PolygonsInit()
        {
            try
            {
                List<clsPolygon> structures = new List<clsPolygon>();

                Structure1 = TDS.DAL.PolygonsDB.GetPolygonByName("Polygon1");
                Structure2 = TDS.DAL.PolygonsDB.GetPolygonByName("Malam");

                // initialize waypoint graphs. TODO - move to non volatile afterwards
                Structure1.waypointGraph = new PolygonWaypointGraph();
                PolygonWaypoint waypoint1 = new PolygonWaypoint(1, 34.849099516868591, 32.098940297448664, PolygonWaypoint.WaypointType.CORRIDOR);
                PolygonWaypoint waypoint2 = new PolygonWaypoint(2, 34.849078059196472, 32.098663090529335, PolygonWaypoint.WaypointType.CORRIDOR);
                PolygonWaypoint waypoint3 = new PolygonWaypoint(3, 34.849582314491272, 32.098685812439619, PolygonWaypoint.WaypointType.CORRIDOR);
                PolygonWaypoint waypoint7 = new PolygonWaypoint(7, 34.85012412071228, 32.098722167484318, PolygonWaypoint.WaypointType.CORRIDOR);
                PolygonWaypoint waypoint8 = new PolygonWaypoint(8, 34.850113391876221, 32.098940297448664, PolygonWaypoint.WaypointType.ROOM);
                PolygonWaypoint waypoint9 = new PolygonWaypoint(9, 34.85012412071228, 32.098444959902984, PolygonWaypoint.WaypointType.ROOM);
                PolygonWaypoint waypoint10 = new PolygonWaypoint(10, 34.849571585655212, 32.0989584749222, PolygonWaypoint.WaypointType.ROOM);
                PolygonWaypoint opening0 = new PolygonWaypoint(4, 34.8490619659424, 32.0996174058948, PolygonWaypoint.WaypointType.OPENING);
                PolygonWaypoint opening4 = new PolygonWaypoint(5, 34.8496359586716, 32.0980768632898, PolygonWaypoint.WaypointType.OPENING);
                PolygonWaypoint opening5 = new PolygonWaypoint(6, 34.848627448082, 32.0989584749222, PolygonWaypoint.WaypointType.OPENING);
                opening0.edgeNum = 0;
                opening4.edgeNum = 4;
                opening5.edgeNum = 5;
                Structure1.waypointGraph.addWaypoint(waypoint1, waypoint2, opening0, opening5, waypoint10);
                Structure1.waypointGraph.addWaypoint(waypoint2, waypoint1, waypoint3);
                Structure1.waypointGraph.addWaypoint(waypoint3, waypoint2, opening4);
                Structure1.waypointGraph.addWaypoint(waypoint7, waypoint3);
                Structure1.waypointGraph.addWaypoint(waypoint8, waypoint7);
                Structure1.waypointGraph.addWaypoint(waypoint9, waypoint7);
                Structure1.waypointGraph.addWaypoint(waypoint10, waypoint3, waypoint1);
                Structure1.waypointGraph.addWaypoint(opening0, waypoint1);
                Structure1.waypointGraph.addWaypoint(opening4, waypoint3);
                Structure1.waypointGraph.addWaypoint(opening5, waypoint1);

                Structure2.waypointGraph = new PolygonWaypointGraph();
                PolygonWaypoint malamOpening1 = new PolygonWaypoint(1, 34.8513351380825, 32.098514261381, PolygonWaypoint.WaypointType.OPENING);
                malamOpening1.edgeNum = 1;
                PolygonWaypoint malamWaypoint2 = new PolygonWaypoint(2, 34.851239919662476, 32.098531303338191, PolygonWaypoint.WaypointType.CORRIDOR);
                PolygonWaypoint malamWaypoint3 = new PolygonWaypoint(3, 34.851202368736267, 32.098304083596624, PolygonWaypoint.WaypointType.CORRIDOR);
                PolygonWaypoint malamWaypoint4 = new PolygonWaypoint(4, 34.850950241088867, 32.098322261196749, PolygonWaypoint.WaypointType.CORRIDOR);
                PolygonWaypoint malamWaypoint5 = new PolygonWaypoint(5, 34.851266741752625, 32.098753978136557, PolygonWaypoint.WaypointType.CORRIDOR);
                PolygonWaypoint malamWaypoint6 = new PolygonWaypoint(6, 34.850998520851135, 32.098772155647154, PolygonWaypoint.WaypointType.CORRIDOR);
                Structure2.waypointGraph.addWaypoint(malamOpening1, malamWaypoint2);
                Structure2.waypointGraph.addWaypoint(malamWaypoint2, malamOpening1, malamWaypoint3, malamWaypoint5);
                Structure2.waypointGraph.addWaypoint(malamWaypoint3, malamWaypoint2, malamWaypoint4);
                Structure2.waypointGraph.addWaypoint(malamWaypoint4, malamWaypoint3);
                Structure2.waypointGraph.addWaypoint(malamWaypoint5, malamWaypoint2, malamWaypoint6);
                Structure2.waypointGraph.addWaypoint(malamWaypoint6, malamWaypoint5);

                structures.Add(Structure1);
                structures.Add(Structure2);

                foreach (clsPolygon structure in structures)
                {
                    foreach (var pnt in structure.Points)
                    {
                        if (pnt.x > structure.maxX) structure.maxX = pnt.x;
                        if (pnt.x < structure.minX) structure.minX = pnt.x;
                        if (pnt.y > structure.maxY) structure.maxY = pnt.y;
                        if (pnt.y < structure.minY) structure.minY = pnt.y;
                    }
                }
            }
            catch (Exception ex)
            {

            }
         
          
        }

        private clsGroundAtom[] RandomizeAtomsInArea(clsPolygon Structure, ref QuadTree<clsGroundAtom> quadTree, int Qty)
        {
            double minX = double.MaxValue;
            double maxX = 0;
            double minY = double.MaxValue;
            double maxY = 0;
            IEnumerable<TerrainService.shPoint> PolPnts = Structure.Points;
            clsGroundAtom[] Atoms = new clsGroundAtom[Qty];


            System.Random Rnd = new Random();
       
            foreach(var pnt in PolPnts)
            {
                if (pnt.x > maxX) maxX = pnt.x;
                if (pnt.x < minX) minX = pnt.x;
                if (pnt.y > maxY) maxY = pnt.y;
                if (pnt.y < minY) minY = pnt.y;
            }


            DAreaRect rect = new DAreaRect(minX, minY, maxX, maxY);
            quadTree = new QuadTree<clsGroundAtom>(rect, 0, null);


            for (int i = 0; i < Qty; i++)
            {
                double vRnd = Rnd.NextDouble();
                double randX = minX + (maxX - minX) * vRnd;
                vRnd = Rnd.NextDouble();
                double randY = minY + (maxY - minY) * vRnd;

                // choose a random waypoint between rooms and corridors
                List<PolygonWaypoint> roomsAndCorridors = new List<PolygonWaypoint>();
                roomsAndCorridors.AddRange(Structure.waypointGraph.rooms);
                roomsAndCorridors.AddRange(Structure.waypointGraph.corridors);

                int randomWaypointIndex = Rnd.Next(roomsAndCorridors.Count());
                PolygonWaypoint waypoint = roomsAndCorridors[randomWaypointIndex];

                TerrainService.shPoint[] Pnts=PolPnts.ToArray();

                while (true)
                {
                    bool inPolygon =TerrainService.GeometryHelper.GeometryMath.isPointInPolygon(randX, randY,ref  Pnts);
                    if (inPolygon == true)
                    {
                        clsGroundAtom GroundAtom = new clsGroundAtom(this);
                        GroundAtom = new clsGroundAtom(this);
                        GroundAtom.GUID = Util.CretaeGuid();
                        GroundAtom.MyName = Structure.PolygonName + "_" + i;
						// YD: generate position by waypoints and not randomly
                        //GroundAtom.curr_X = randX;
                        //GroundAtom.curr_Y = randY;
                        //GroundAtom.currPosition = new TerrainService.Vector(randX, randY, 0);
                        GroundAtom.curr_X = waypoint.x;
                        GroundAtom.curr_Y = waypoint.y;
                        GroundAtom.currPosition = new TerrainService.Vector(waypoint.x, waypoint.y, 0);
                        GroundAtom.currentStructureWaypoint = waypoint;
                        GroundAtom.currentAzimuth = Util.Azimuth2Points(GroundAtom.curr_X, GroundAtom.curr_Y,
                                            GroundAtom.currentStructureWaypoint.x, GroundAtom.currentStructureWaypoint.y);

                        List<CultureData> culturalData = TDS.DAL.CulturesDB.getCultureDataByCountry("israel");
                        CultureGenderBiasData culturalGenderBiasData = TDS.DAL.CulturesDB.getCultureGenderBiasByCountry("israel");

						// set speed randomly and not fixed
                        Atoms[i] = GroundAtom;

                        GroundAtomObjectCollection.TryAdd(GroundAtom.GUID, GroundAtom);

                        // set atom data
                        CultureData atomCulture = culturalData.ElementAt(Util.random.Next(2));
                        GroundAtom.age = atomCulture.age;
                        GroundAtom.gender = atomCulture.gender;
                        GroundAtom.proxemics = new Proxemics(atomCulture.personalSpace, atomCulture.socialSpace, atomCulture.publicSpace);
                        GroundAtom.currentSpeed = atomCulture.speed;
                        GroundAtom.baselineSpeed = GroundAtom.currentSpeed;
                        GroundAtom.genderBiasFactor = culturalGenderBiasData.bias;

                        // collision avoidance in normative conditions:
                        // between two individuals who are not in a group it is done when the lower bound of the social space is passed
                        // between two individuals in a group it is done when the personal space is violated
                        GroundAtom.collisionRadius = 2*clsGroundAtom.RADIUS + atomCulture.socialSpace;

                        GroundAtom.ChangeState(new REGULAR_MOVEMENT_IN_STRUCTURE_STATE(Structure, GroundAtom.currentStructureWaypoint));
                        m_GameManager.QuadTreeStructure1GroundAtom.PositionUpdate(GroundAtom);

                        break;
                    }
                    else
                    {
                        vRnd = Rnd.NextDouble();
                        randX = minX + (maxX - minX) * vRnd;
                        vRnd = Rnd.NextDouble();
                        randY = minY + (maxY - minY) * vRnd;
                    }
                }
            }


            return Atoms;
        }

        // YD: Generate atoms on sidewalk randomly
        public DPoint[] getRegularMovementCoordinates()
        {
            // travel waypoints
            DPoint[] travelCoordinates = new DPoint[] { new DPoint(34.848627448082, 32.0995901398799), new DPoint(34.8495876789093, 32.0996264945646),
                                                        new DPoint(34.8505747318268, 32.0996492162353),
                                                        new DPoint(34.850612282753, 32.0982768171897), new DPoint(34.8492550849915, 32.0980950409352),
                                                        new DPoint(34.8486435413361, 32.098308627997), new DPoint(34.8514652252197, 32.0996855708965),
                                                        new DPoint(34.851508140564, 32.0986403686134), new DPoint(34.8511004447937, 32.0973452100618),
                                                        new DPoint(34.8498612642288, 32.0977905648985), new DPoint(34.8485255241394, 32.0982631839831),
                                                        new DPoint(34.8479408025742, 32.0971543430385), new DPoint(34.8504567146301, 32.096090933751)};
            return travelCoordinates;
        }

        public List<Barrier> getPoliceBarrierCoordinates()
        {
            return barriers;
        }

        public PointData lookForClosestRegularRoute(clsGroundAtom refGroundAtom)
        {
            double minDistance = Double.PositiveInfinity;
            DPoint minPoint = null;

            foreach (DPoint point in pointDataMap.Keys)
            {
                double distanceFromAtom = TerrainService.MathEngine.CalcDistance(refGroundAtom.curr_X, refGroundAtom.curr_Y, point.x, point.y);
                if (distanceFromAtom < minDistance)
                {
                    minDistance = distanceFromAtom;
                    minPoint = point;
                }
            }

            return pointDataMap[minPoint];
        }

        public PointData lookForClosestRouteToBarrier(clsGroundAtom refGroundAtom, Barrier barrier)
        {
            double minDistance = Double.PositiveInfinity;
            DPoint minPoint = null;

            foreach (DPoint point in routesToBarriersDataMap.Keys)
            {
                double distanceFromAtom = TerrainService.MathEngine.CalcDistance(refGroundAtom.curr_X, refGroundAtom.curr_Y, point.x, point.y);
                int barrierIndex = routesToBarriersDataMap[point].routeIndex2;
                if (distanceFromAtom < minDistance && barriers[barrierIndex] == barrier)
                {
                    minDistance = distanceFromAtom;
                    minPoint = point;
                }
            }

            return routesToBarriersDataMap[minPoint];
        }

        public void preloadData()
        {
            // preload barriers
            barriers = TDS.DAL.BarriersDB.getAllBarriers();

            DPoint[] coordinates = getRegularMovementCoordinates();

            // get all waypoints to waypoins and waypoins to barriers
            IEnumerable<Route> waypointToWaypointRoutes = TDS.DAL.RoutesDB.getRoutesWithNameStartingWith("WaypointToWaypoint");
            IEnumerable<Route> waypointToBarrierRoutes = TDS.DAL.RoutesDB.getRoutesWithNameStartingWith("WaypointToBarrier");

            travelRoutes = new typRoute[coordinates.Count(), coordinates.Count()];
            routesToBarriers = new typRoute[coordinates.Count(), barriers.Count()];
            pointDataMap = new Dictionary<DPoint, PointData>();
            routesToBarriersDataMap = new Dictionary<DPoint, PointData>();


            // store waypoints to waypoints in arrays. Route names are in format WaypointToWaypoint_i_j where i,j are integers
            foreach (Route route in waypointToWaypointRoutes)
            {
                typRoute typRoute = RouteUtils.createTypRoute(route.Points.ToList(), route.RouteName);
                int index1 = Convert.ToInt32(route.RouteName.Split('_')[1].Split(',')[0]);
                int index2 = Convert.ToInt32(route.RouteName.Split('_')[1].Split(',')[1]);

                travelRoutes[index1, index2] = typRoute;

                for (int i = 0; i < route.Points.Count() - 1; i++)
                {
                    DPoint point = route.Points.ElementAt(i);
                    PointData data = new PointData();
                    data.legNum = i + 1;
                    data.routeIndex1 = index1;
                    data.routeIndex2 = index2;
                    data.route = typRoute;
                    pointDataMap.Add(point, data);
                }
            }

            // store waypoints to barriers in arrays. Route names are in format WaypointToBarrier_i_j where i,j are integers
            foreach (Route route in waypointToBarrierRoutes)
            {
                typRoute typRoute = RouteUtils.createTypRoute(route.Points.ToList(), route.RouteName);
                int index1 = Convert.ToInt32(route.RouteName.Split('_')[1].Split(',')[0]);
                int index2 = Convert.ToInt32(route.RouteName.Split('_')[1].Split(',')[1]);

                routesToBarriers[index1, index2] = typRoute;

                for (int i = 0; i < route.Points.Count() - 1; i++)
                {
                    DPoint point = route.Points.ElementAt(i);
                    PointData data = new PointData();
                    data.legNum = i + 1;
                    data.routeIndex1 = index1;
                    data.routeIndex2 = index2;
                    data.route = routesToBarriers[index1, index2];
                    routesToBarriersDataMap.Add(point, data);
                }
            }
        }

        private clsGroundAtom[] randomizeAtomsOnSidewalks(int Qty)
        {
            DPoint[] travelCoordinates = getRegularMovementCoordinates();

            clsGroundAtom[] atoms = new clsGroundAtom[Qty];

            for (int i = 0; i < atoms.Count(); i++)
            {
                atoms[i] = new clsGroundAtom(this);

                // init ID related fields
                atoms[i].MyName = "Sidewalk" + i;
                atoms[i].GUID = Util.CretaeGuid();

                // init starting point
                int startLocationIndex = Util.random.Next(travelCoordinates.Count());

                int minutes = Util.random.Next(1);
                int seconds = Util.random.Next(1, 60);

                int endLocationIndex = Util.random.Next(travelCoordinates.Count() - 1);
                // make sure start and end location indices are not the same
                if (startLocationIndex == endLocationIndex) endLocationIndex++;
                Route route = RouteUtils.typRouteToRoute(travelRoutes[startLocationIndex, endLocationIndex]);

                // set atom data
                List<CultureData> culturalData = TDS.DAL.CulturesDB.getCultureDataByCountry("israel");
                CultureGenderBiasData culturalGenderBiasData = TDS.DAL.CulturesDB.getCultureGenderBiasByCountry("israel");
                CultureData atomCulture = culturalData.ElementAt(Util.random.Next(2));
                atoms[i].age = atomCulture.age;
                atoms[i].gender = atomCulture.gender;
                atoms[i].proxemics = new Proxemics(atomCulture.personalSpace, atomCulture.socialSpace, atomCulture.publicSpace);
                atoms[i].genderBiasFactor = culturalGenderBiasData.bias;
                atoms[i].currentSpeed = atomCulture.speed;

                // collision avoidance in normative conditions:
                // between two individuals who are not in a group it is done when the lower bound of the social space is violated
                // between two individuals in a group it is done when the personal space is violated
                atoms[i].collisionRadius = 2*clsGroundAtom.RADIUS + atomCulture.socialSpace;

                // init activity
                clsActivityMovement activity = RouteUtils.createActivityAndStart(atoms[i], atomCulture.speed, route);
                atoms[i].baselineSpeed = atomCulture.speed;
                activity.TimeFrom = DateTime.Now.AddMinutes(minutes).AddSeconds(seconds);
                List<clsActivityBase> activities = new List<clsActivityBase>();
                activities.Add(activity);
                m_GroundActivities.Add(atoms[i].GUID, activities);
                atoms[i].currentStartWaypoint = startLocationIndex;
                atoms[i].currentEndWaypoint = endLocationIndex;
                atoms[i].curr_X = travelRoutes[startLocationIndex, endLocationIndex].arr_legs.ElementAt(0).FromLongn;
                atoms[i].curr_Y = travelRoutes[startLocationIndex, endLocationIndex].arr_legs.ElementAt(0).FromLatn;
                atoms[i].X_Route = atoms[i].curr_X;
                atoms[i].Y_Route = atoms[i].curr_Y;
                atoms[i].currPosition = new TerrainService.Vector(atoms[i].curr_X, atoms[i].curr_Y, 0);

                // add atom to collection
                GroundAtomObjectCollection.TryAdd(atoms[i].GUID, atoms[i]);
                // update atom position
                m_GameManager.QuadTreeGroundAtom.PositionUpdate(atoms[i]);

                // set state
                atoms[i].ChangeState(new ADMINISTRATIVE_STATE());
            }

            return atoms;
        }


        private void GroundMissionActivitiesInit()
        {
              m_GroundActivities = new Dictionary<string, List<clsActivityBase>>();
              foreach (KeyValuePair<string, AtomBase> keyVal in GroundAtomObjectCollection)
              {
                  clsGroundAtom refGroundAtom = keyVal.Value as clsGroundAtom;

                  IEnumerable<GeneralActivityDTO> ActivitesDTO = TDS.DAL.ActivityDB.GetActivitesByAtom(refGroundAtom.GUID);

                  if (ActivitesDTO == null) continue;

                  List<clsActivityBase> Activites=new List<clsActivityBase>();
                  foreach(GeneralActivityDTO dto in ActivitesDTO )
                  {
                      switch(dto.ActivityType)
                      {
                          case enumActivity.MovementActivity:

                              clsActivityMovement MovementAct = new clsActivityMovement();
                              MovementAct.ActivityType = enumActivity.MovementActivity;   
                              MovementAct.ActivityId = dto.ActivityId;
                              MovementAct.AtomGuid=refGroundAtom.GUID;
                              MovementAct.AtomName = refGroundAtom.MyName;
                              MovementAct.DurationActivity = dto.DurationActivity;
                              MovementAct.RouteActivity = dto.RouteActivity;
                              MovementAct.Speed = dto.Speed;
                              MovementAct.StartActivityOffset = dto.StartActivityOffset;


                              MovementAct.ReferencePoint = dto.ReferencePoint;

                              MovementAct.TimeFrom = Ex_clockDate.Add(MovementAct.StartActivityOffset);
                              MovementAct.TimeTo = MovementAct.TimeFrom.Add(TimeSpan.FromDays(365));          // MovementAct.TimeFrom.Add(MovementAct.DurationActivity);
                              Activites.Add(MovementAct);

                              break;
                      }

                      
                  }
                  m_GroundActivities.Add(refGroundAtom.GUID, Activites);
              }
        }
        private void GroundAtomsInit()
        {
            GroundAtomObjectCollection = new ConcurrentDictionary<string, AtomBase>();
            IEnumerable<AtomData> atoms = TDS.DAL.AtomsDB.GetAllAtoms();
            if (atoms == null) return;
            foreach(AtomData atom in atoms)
            {
                clsGroundAtom GroundAtom = new clsGroundAtom(this);
                GroundAtom.MyName = atom.UnitName;
                GroundAtom.GUID = atom.UnitGuid;

                GroundAtom.X_Route = atom.Location.x;
                GroundAtom.Y_Route = atom.Location.y;

                GroundAtom.curr_X = atom.Location.x;
                GroundAtom.curr_Y = atom.Location.y;
                GroundAtom.reScheduleEvaluation();

                GroundAtomObjectCollection.TryAdd(GroundAtom.GUID, GroundAtom);

                m_GameManager.QuadTreeGroundAtom.PositionUpdate(GroundAtom);
                //VH
                GroundAtom.ChangeState(new ADMINISTRATIVE_STATE());
            }
        }

        public bool earthquakeStarted()
        {
            return (Ex_clockDate - Ex_clockStartDate).Minutes > 2;
        }

        public bool earthquakeEnded()
        {
            return (Ex_clockDate - Ex_clockStartDate).Minutes > 4;
        }

        public bool forcesHaveArrived()
        {
            return (Ex_clockDate - Ex_clockStartDate).Minutes > 8;
        }

        public DPoint getExplosionLocation()
        {
            return new DPoint(34.8497, 32.0996);
        }

        public double getExplosionRadius()
        {
            return 10;
        }

        private void inflictDamageOnGroundAtomsInExplosion(DPoint location, double radius)
        {
            List<clsGroundAtom> atomsToDamage = m_GameManager.QuadTreeGroundAtom.SearchEntities(location.x, location.y, radius, isPrecise: true);
            double certainDeathRadius = 0.2*radius;

            foreach (clsGroundAtom atom in atomsToDamage)
            {
                // certain death when very close to event source
                double distanceToEventSource = TerrainService.MathEngine.CalcDistance(atom.curr_X, atom.curr_Y, location.x, location.y);
                if (distanceToEventSource < certainDeathRadius)
                {
                    atom.healthStatus.isDead = true;
                    continue;
                }

                // the farther the atom is from the source the less likely he is to die - use linearly decreasing death probability
                double deathProbability = 0.5*(radius - distanceToEventSource)/(radius - certainDeathRadius);
                double random = Util.random.NextDouble();
                if (random <= deathProbability) atom.healthStatus.isDead = true;

                // if he didn't die, make him injured with twice the same probability distribution
                double injuryProbability = (radius - distanceToEventSource) / (radius - certainDeathRadius);
                random = Util.random.NextDouble();
                if (random <= injuryProbability)
                {
                    atom.healthStatus.isInjured = true;
                }

                // incapacitate him with twice the same probability distribution
                double incapacitanceProbability = 0.5*(radius - distanceToEventSource) / (radius - certainDeathRadius);
                random = Util.random.NextDouble();
                if (random <= incapacitanceProbability)
                {
                    atom.healthStatus.isIncapacitated = true;
                    atom.healthStatus.isInjured = true;
                }

                if (atom.healthStatus.isIncapacitated || atom.healthStatus.isInjured)
                {
                    atom.healthStatus.injurySeverity = (radius - distanceToEventSource) / (radius - certainDeathRadius);
                }

                // make injured atoms move slower
                if (atom.healthStatus.isInjured)
                {
                    atom.currentSpeed *= (1 - atom.healthStatus.injurySeverity);
                }

                // if he didn't die or was not injured or incapacitated - he's one lucky bastard.
            }
        }

        private void inflictDamageOnGroundAtomsInEarthquake()
        {
            foreach (KeyValuePair<String, AtomBase> atomKV in GroundAtomObjectCollection) {
                clsGroundAtom groundAtom = (clsGroundAtom)atomKV.Value;
                double deathProb = Util.random.NextDouble();

                // death with a certain probability
                if (deathProb < 0.05)
                {
                    groundAtom.healthStatus.isDead = true;
                    groundAtom.gotDead();
                    continue;
                }

                double incapacitanceProb = Util.random.NextDouble();
                // incapacitance with a certain probability
                if (incapacitanceProb < 0.05)
                {
                    groundAtom.healthStatus.isIncapacitated = true;
                    groundAtom.healthStatus.isInjured = true;
                    groundAtom.gotIncapacitated();
                    continue;
                }

                double injuryProb = Util.random.NextDouble();
                if (injuryProb < 0.05)
                {
                    double severity = Util.random.NextDouble();
                    groundAtom.healthStatus.isInjured = true;
                    groundAtom.healthStatus.injurySeverity = severity;
                    groundAtom.currentSpeed *= (1 - groundAtom.healthStatus.injurySeverity);
                    continue;
                }
            }
            
        }

        public void writeCollisionReportAndClearData()
        {
            StringBuilder sb = new StringBuilder();

            // format the lines to be written
            foreach (CollisionData data in collisionData)
            {
                double timeInterval = (data.date - Ex_clockStartDate).TotalSeconds;
                sb.AppendLine(String.Format("{0},{1},{2},{3}", timeInterval, data.all, data.nonFrontal, data.frontal));
            }

            System.IO.File.WriteAllText("collisions.csv", sb.ToString());

            collisionData.Clear();
            totalCollisions = 0;
            nonFrontalCollisions = 0;
            frontalCollisions = 0;
        }

        public void addCollision(bool isFrontal)
        {
            totalCollisions++;
            if (isFrontal) frontalCollisions++;
            else nonFrontalCollisions++;
        }
		// --------------------------------------------------------------
        public bool isAtomNameExist(string AtomName)
        {
            AtomData atom = TDS.DAL.AtomsDB.GetAtomByName(AtomName);
            if (atom != null) return true;
            else  return false;
        }
        public bool isRouteNameExist(string RouteName)
        {
            Route route = TDS.DAL.RoutesDB.GetRouteByName(RouteName);
            if (route != null) return true;
            else return false;
        }

        public void DeleteAtomByAtomName(string AtomName)
        {
            AtomData atom = TDS.DAL.AtomsDB.GetAtomByName(AtomName);
            if (atom == null) return;

            AtomBase GroundAtombase = null;
            GroundAtomObjectCollection.TryGetValue(atom.UnitGuid, out GroundAtombase);
            if (GroundAtombase == null) return;
            clsGroundAtom GroundAtom = GroundAtombase as clsGroundAtom;

            TDS.DAL.ActivityDB.DeleteActivitesByAtomGuid(GroundAtom.GUID);


            //List<clsActivityBase> Activites = null;
            //m_GroundActivities.TryGetValue(GroundAtom.GUID, out Activites);
            //if(Activites!=null)
            //{
            //    foreach (clsActivityBase Activity in Activites)
            //    {
            //        TDS.DAL.RoutesDB.DeleteRouteByGuid(Activity.RouteActivity.RouteGuid);
            //    }
            //}

            m_GroundActivities.Remove(GroundAtom.GUID);
            TDS.DAL.AtomsDB.DeleteAtomByGuid(GroundAtom.GUID);
            GroundAtomObjectCollection.TryRemove(GroundAtom.GUID, out GroundAtombase);

            NotifyClientsEndCycleArgs args = new NotifyClientsEndCycleArgs();
            args = new NotifyClientsEndCycleArgs();
            args.Transport2Client.Ex_clockDate = Ex_clockDate;
            // args.Transport2Client.ExClockRatioSpeed = m_GameManager.ExClockRatioSpeed;
            args.Transport2Client.AtomObjectType = 2;
            args.Transport2Client.AtomObjectCollection = PrepareGroundCommonProperty();
            args.Transport2Client.ManagerStatus = m_GameManager.ManagerStatus;
            m_GameManager.NotifyClientsEndCycle(args);

        }

        public void DeleteAtomFromTreeByGuid(string AtomGuid)
        {
            TDS.DAL.AtomsDB.DeleteAtomFromTreeByGuid(AtomGuid);

            AtomBase GroundAtombase = null;
            GroundAtomObjectCollection.TryGetValue(AtomGuid, out GroundAtombase);
            if (GroundAtombase == null) return;

            clsGroundAtom GroundAtom = GroundAtombase as clsGroundAtom;

            m_GroundActivities.Remove(GroundAtom.GUID);
            TDS.DAL.AtomsDB.DeleteAtomByGuid(GroundAtom.GUID);
            GroundAtomObjectCollection.TryRemove(GroundAtom.GUID, out GroundAtombase);

            NotifyClientsEndCycleArgs args = new NotifyClientsEndCycleArgs();
            args = new NotifyClientsEndCycleArgs();
            args.Transport2Client.Ex_clockDate = Ex_clockDate;
            // args.Transport2Client.ExClockRatioSpeed = m_GameManager.ExClockRatioSpeed;
            args.Transport2Client.AtomObjectType = 2;
            args.Transport2Client.AtomObjectCollection = PrepareGroundCommonProperty();
            args.Transport2Client.ManagerStatus = m_GameManager.ManagerStatus;
            m_GameManager.NotifyClientsEndCycle(args);


        }

        public void DeleteActivityById(int ActivityId)
        {
            TDS.DAL.ActivityDB.DeleteActivityById(ActivityId);

            foreach (KeyValuePair<string, List<clsActivityBase>> keyVal in m_GroundActivities)
            {
                List<clsActivityBase> Activities = keyVal.Value as List<clsActivityBase>;
                List<clsActivityBase> ListToDelete = new List<clsActivityBase>();
                foreach (clsActivityBase act in Activities)
                {
                    if (act.ActivityId == ActivityId)
                    {
                        ListToDelete.Add(act);
                    }
                }

                foreach (clsActivityBase act in ListToDelete)
                {
                    Activities.Remove(act);
                }
            }

        }

        public void DeleteRouteByGuid(string RouteGuid)
        {
            TDS.DAL.RoutesDB.DeleteRouteByGuid(RouteGuid);

            foreach (KeyValuePair<string, List<clsActivityBase>> keyVal in m_GroundActivities)
            {
                List<clsActivityBase> Activities = keyVal.Value as List<clsActivityBase>;
                List<clsActivityBase> ListToDelete = new List<clsActivityBase>();
                foreach (clsActivityBase act in Activities)
                {
                     if(act.RouteActivity.RouteGuid==RouteGuid)
                     {
                         ListToDelete.Add(act);
                     }
                }

                foreach (clsActivityBase act in ListToDelete)
                {
                    Activities.Remove(act);
                }
            }
        }

        public IEnumerable<FormationTree> GetAllAtomsFromTree()
        {
            IEnumerable<FormationTree> formations = TDS.DAL.AtomsDB.GetAllAtomsFromTree();

            if (formations!=null)
            {
                foreach (FormationTree formation in formations)
                {
                    formation.isDeployed = GroundAtomObjectCollection.ContainsKey(formation.GUID);

                    IEnumerable<GeneralActivityDTO> Actityties = TDS.DAL.ActivityDB.GetActivitesByAtom(formation.GUID);
                    if (Actityties != null && Actityties.Count() > 0)
                    {
                        formation.isActivityes = true;
                    }
                }

            }
          

            return formations;
        }

        public AtomData DeployFormationFromTree(DeployedFormation deployFormation)
        {

            if (GroundAtomObjectCollection.ContainsKey(deployFormation.formation.GUID)) return null;



            AtomData atom = new AtomData();
            atom.Location = new DPoint(deployFormation.x, deployFormation.y);
            atom.UnitGuid = deployFormation.formation.GUID;
            atom.UnitName = deployFormation.formation.Identification;

            TDS.DAL.AtomsDB.AddAtom(atom);




            clsGroundAtom GroundAtom = new clsGroundAtom(this);
            GroundAtom.MyName = atom.UnitName;
            GroundAtom.GUID = atom.UnitGuid;
            GroundAtom.curr_X = atom.Location.x;
            GroundAtom.curr_Y = atom.Location.y;
            GroundAtomObjectCollection.TryAdd(GroundAtom.GUID, GroundAtom);
            m_GameManager.QuadTreeGroundAtom.PositionUpdate(GroundAtom);




            NotifyClientsEndCycleArgs args = new NotifyClientsEndCycleArgs();
            args = new NotifyClientsEndCycleArgs();
            args.Transport2Client.Ex_clockDate = Ex_clockDate;
            // args.Transport2Client.ExClockRatioSpeed = m_GameManager.ExClockRatioSpeed;
            args.Transport2Client.AtomObjectType = 2;
            args.Transport2Client.AtomObjectCollection = PrepareGroundCommonProperty();
            args.Transport2Client.ManagerStatus = m_GameManager.ManagerStatus;
            m_GameManager.NotifyClientsEndCycle(args);

            return atom;
        }

        public void MoveGroundObject(DeployedFormation deployFormation)
        {
            TDS.DAL.AtomsDB.UpdateAtomPositionByGuid(deployFormation.formation.GUID, deployFormation.x, deployFormation.y);
            AtomBase GroundAtom = null;
            GroundAtomObjectCollection.TryGetValue(deployFormation.formation.GUID, out GroundAtom);
            GroundAtom.curr_X = deployFormation.x;
            GroundAtom.curr_Y = deployFormation.y;


            NotifyClientsEndCycleArgs args = new NotifyClientsEndCycleArgs();
            args = new NotifyClientsEndCycleArgs();
            args.Transport2Client.Ex_clockDate = Ex_clockDate;
            // args.Transport2Client.ExClockRatioSpeed = m_GameManager.ExClockRatioSpeed;
            args.Transport2Client.AtomObjectType = 2;
            args.Transport2Client.AtomObjectCollection = PrepareGroundCommonProperty();
            args.Transport2Client.ManagerStatus = m_GameManager.ManagerStatus;
            m_GameManager.NotifyClientsEndCycle(args);


        }

        public void   RefreshActivity(GeneralActivityDTO ActivityDTO)
        {
            AtomBase GroundAtombase = null;
            GroundAtomObjectCollection.TryGetValue(ActivityDTO.Atom.UnitGuid, out GroundAtombase);
            if (GroundAtombase == null)
            {
                clsGroundAtom GroundAtom = new clsGroundAtom(this);
                GroundAtom.MyName = ActivityDTO.Atom.UnitName;
                GroundAtom.GUID = ActivityDTO.Atom.UnitGuid;
                GroundAtom.curr_X = ActivityDTO.Atom.Location.x;
                GroundAtom.curr_Y = ActivityDTO.Atom.Location.y;
                GroundAtomObjectCollection.TryAdd(GroundAtom.GUID, GroundAtom);

                NotifyClientsEndCycleArgs args = new NotifyClientsEndCycleArgs();
                args = new NotifyClientsEndCycleArgs();
                args.Transport2Client.Ex_clockDate = Ex_clockDate;
                // args.Transport2Client.ExClockRatioSpeed = m_GameManager.ExClockRatioSpeed;
                args.Transport2Client.AtomObjectType = 2;
                args.Transport2Client.AtomObjectCollection = PrepareGroundCommonProperty();
                args.Transport2Client.ManagerStatus =   m_GameManager.ManagerStatus;
                m_GameManager.NotifyClientsEndCycle(args);
            }
        }

        internal void Ex_Manager()
        {
             LastExClockTick = Environment.TickCount;
             while(m_GameManager.Ex_ManagerThreadShouldStop==false)
             {
                  if (m_GameManager.ManagerStatus == typGamestatus.PAUSE_STATUS)
                  {
                      if (GroundExecuteStateTask != null && GroundExecuteStateTask.IsCompleted == false)
                      {
                          
                          try
                          {
                              GroundExecuteStateTask.Wait();
                              GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

                          }
                          catch (AggregateException exx)
                          {
                             // Log.WriteErrorToLog(exx, m_GameManager.curScenario.DbName);
                          }
                      }


                      Thread.Sleep(1000);
                      continue;
                  }
                  else if ((m_GameManager.ManagerStatus == typGamestatus.EDIT_STATUS) || (m_GameManager.ManagerStatus == typGamestatus.PROCESS_RUN2EDIT))
                  {
                      // Thread.CurrentThread.Abort();
                      break;
                  }
              //    m_GameManager.ExClockRatioSpeed = 6;
                  if (m_GameManager.ExClockRatioSpeed != 0)
                  {
                      double currExClockTick = Environment.TickCount;
                      double TickCount = currExClockTick - LastExClockTick;
                    //  int sleepTime = 1000 / m_GameManager.ExClockRatioSpeed - (int)TickCount;
                      int sleepTime = ExClockResolution / m_GameManager.ExClockRatioSpeed - (int)TickCount;
                      if (sleepTime > 0 && sleepTime < Int32.MaxValue)
                      {
                          Thread.Sleep(sleepTime);
                      }
                  }

                


                  

                  //int sleepTime = 1000 / m_GameManager.ExClockRatioSpeed - (int)TickCount;
                  //if (sleepTime > 0 && sleepTime < Int32.MaxValue)
                  //{
                  //    Thread.Sleep(sleepTime);
                  //}
                //  ExClockResolution = 1000;
                  Ex_clockDate = Ex_clockDate.AddMilliseconds(ExClockResolution);

				  // Yinon Douchan: Code for statistics and simulation of explosion
                  // update statistics every second
                  if ((Ex_clockStartDate - Ex_clockDate).TotalMilliseconds % 10000 == 0)
                  {
                      collisionData.Add(new CollisionData(Ex_clockDate, totalCollisions, nonFrontalCollisions, frontalCollisions));
                  }

				  // ---------------------------------------------------------------


                  LastExClockTick = Environment.TickCount;

                  TimeSpan TSGroundExecute = Ex_clockDate.Subtract(Ex_clockGroundExecute);
                  if (TSGroundExecute.TotalMilliseconds >= m_GameManager.GroundCycleResolution)
                  {
                      Ex_clockGroundExecute = Ex_clockDate;

                      if (GroundExecuteStateTask != null)
                      {
                          try
                          {
                              GroundExecuteStateTask.Wait();
                              GroundExecuteStateTask.Dispose();
                              GroundExecuteStateTask = null;
                          }
                          catch (AggregateException Exx)
                          {
                              GroundExecuteStateTask.Dispose();
                              GroundExecuteStateTask = null;
                             // GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                             // Log.WriteErrorToLog(Exx, m_GameManager.curScenario.DbName);
                          }
                      }


                      GroundExecuteStateTask = new Task(() =>
                      {
                          manageEvents();

                          foreach (KeyValuePair<string, AtomBase> keyVal in GroundAtomObjectCollection)
                          {
                              clsGroundAtom refGroundAtom = keyVal.Value as clsGroundAtom;
                              refGroundAtom.ExecuteState();
                              refGroundAtom.CheckCondition();
                          }

                          NotifyClientsEndCycleArgs args = new NotifyClientsEndCycleArgs();
                          args = new NotifyClientsEndCycleArgs();
                          args.Transport2Client.Ex_clockDate = Ex_clockDate;
                          // args.Transport2Client.ExClockRatioSpeed = m_GameManager.ExClockRatioSpeed;
                          args.Transport2Client.AtomObjectType = 2;
                          args.Transport2Client.AtomObjectCollection = PrepareGroundCommonProperty();
                          args.Transport2Client.ManagerStatus = m_GameManager.ManagerStatus;
                          m_GameManager.NotifyClientsEndCycle(args);

                      }
                     );
                      GroundExecuteStateTask.Start();



                  }

                  Thread.Sleep(5);  // (10);   

                 

             }
        }

        private void manageEvents()
        {
            if (!earthquakeOccurred && earthquakeStarted())
            {
                earthquakeOccurred = true;
                m_simulationStage = SimulationStage.STAGE_2;
                //inflictDamageOnGroundAtomsInExplosion(getExplosionLocation(), getExplosionRadius());
                inflictDamageOnGroundAtomsInEarthquake();

                // notify all atoms on earthquake
                foreach (KeyValuePair<String, AtomBase> atomKV in GroundAtomObjectCollection)
                {
                    clsGroundAtom groundAtom = (clsGroundAtom)atomKV.Value;
                    groundAtom.earthquakeStarted();
                }
            }

            if (!forcesAlreadyArrived && forcesHaveArrived())
            {
                forcesAlreadyArrived = true;
                m_simulationStage = SimulationStage.STAGE_4;
                foreach (KeyValuePair<String, AtomBase> atomKV in GroundAtomObjectCollection)
                {
                    clsGroundAtom groundAtom = (clsGroundAtom)atomKV.Value;
                    groundAtom.forcesHaveArrived();
                }
            }

            if (!earthquakeAlreadyEnded && earthquakeEnded())
            {
                earthquakeAlreadyEnded = true;
                m_simulationStage = SimulationStage.STAGE_3;
                foreach (KeyValuePair<String, AtomBase> atomKV in GroundAtomObjectCollection)
                {
                    clsGroundAtom groundAtom = (clsGroundAtom)atomKV.Value;
                    groundAtom.earthquakeEnded();
                }
            }
        }
        internal structTransportCommonProperty[] PrepareGroundCommonProperty()
        {
            List<structTransportCommonProperty> TransportCommonProperty = new List<structTransportCommonProperty>(GroundAtomObjectCollection.Count);
            foreach (KeyValuePair<string, AtomBase> keyVal in GroundAtomObjectCollection)
            {
                clsGroundAtom refGroundAtom = keyVal.Value as clsGroundAtom;

                structTransportCommonProperty CommonPropertyObject = new structTransportCommonProperty();
                CommonPropertyObject.AtomClass = refGroundAtom.GetType().ToString();
                CommonPropertyObject.AtomName = refGroundAtom.MyName;
                CommonPropertyObject.GUID = refGroundAtom.GUID;
                CommonPropertyObject.X = refGroundAtom.curr_X;
                CommonPropertyObject.Y = refGroundAtom.curr_Y;

                CommonPropertyObject.isCollision = refGroundAtom.isCollision;
				// Yinon Douchan: Code for simulation of casualties
                CommonPropertyObject.isDead = refGroundAtom.healthStatus.isDead;
                CommonPropertyObject.isIncapacitated = refGroundAtom.healthStatus.isIncapacitated;
                CommonPropertyObject.isInjured = refGroundAtom.healthStatus.isInjured;
				// -------------------------------------------------

                TransportCommonProperty.Add(CommonPropertyObject);
               
            }
            return TransportCommonProperty.ToArray<structTransportCommonProperty>();
        }

        public QuadTree<clsGroundAtom> getQuadTreeByStructure(clsPolygon structure)
        {
            if (structure == Structure1) return m_GameManager.QuadTreeStructure1GroundAtom;
            else if (structure == Structure2) return m_GameManager.QuadTreeStructure2GroundAtom;
            else return null;
        }
    }
}
