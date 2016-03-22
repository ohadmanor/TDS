using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TDSServer.GroundTask;

namespace TDSServer
{
    public class GameObject
    {
        internal GameManager m_GameManager;
        private double LastExClockTick = 0;
        private int ExClockResolution = 200;  //200 milisec  5 times per second

        public DateTime Ex_clockDate;
        public DateTime Ex_clockStartDate;
        public DateTime Ex_clockGroundExecute = DateTime.MinValue;
        public Task GroundExecuteStateTask = null;

        private List<CollisionData> collisionData;
        private long totalCollisions, nonFrontalCollisions, frontalCollisions;
        private bool explosionOccurred;


        internal ConcurrentDictionary<string, AtomBase> GroundAtomObjectCollection = new ConcurrentDictionary<string, AtomBase>();
        public Dictionary<string, List<clsActivityBase>> m_GroundActivities = new Dictionary<string, List<clsActivityBase>>();


        internal GameObject(GameManager refGameManager)
        {
            m_GameManager =refGameManager;
        }
        internal void InitObjects()
        {
            Ex_clockDate = DateTime.Now;
            Ex_clockStartDate = Ex_clockDate;
            explosionOccurred = false;

            // clear collision report history
            if (collisionData == null) collisionData = new List<CollisionData>();

            GroundAtomsInit();
            GroundMissionActivitiesInit();

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



                GroundAtomObjectCollection.TryAdd(GroundAtom.GUID, GroundAtom);

                m_GameManager.QuadTreeGroundAtom.PositionUpdate(GroundAtom);
            }
        }

        public bool emergencyOccurred()
        {
            return (Ex_clockDate - Ex_clockStartDate).Minutes > 2;
        }

        public DPoint getExplosionLocation()
        {
            return new DPoint(34.8497, 32.0996);
        }

        public double getExplosionRadius()
        {
            return 10;
        }

        private void inflictDamageOnGroundAtoms(DPoint location, double radius)
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

                  // update statistics every second
                  if ((Ex_clockStartDate - Ex_clockDate).TotalMilliseconds % 10000 == 0)
                  {
                      collisionData.Add(new CollisionData(Ex_clockDate, totalCollisions, nonFrontalCollisions, frontalCollisions));
                  }

                  if (!explosionOccurred && emergencyOccurred())
                  {
                      explosionOccurred = true;
                      inflictDamageOnGroundAtoms(getExplosionLocation(), getExplosionRadius());
                  }



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
                CommonPropertyObject.isDead = refGroundAtom.healthStatus.isDead;
                CommonPropertyObject.isIncapacitated = refGroundAtom.healthStatus.isIncapacitated;
                CommonPropertyObject.isInjured = refGroundAtom.healthStatus.isInjured;

                TransportCommonProperty.Add(CommonPropertyObject);
               
            }
            return TransportCommonProperty.ToArray<structTransportCommonProperty>();
        }
    }
}
