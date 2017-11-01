using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDSServer.GroundTask.StateMashine
{
	// YD: Event arguments to be passed
    public class clsGroundAtomEventArgs : EventArgs
    {
        public clsGroundAtom groundAtom;

        public clsGroundAtomEventArgs(clsGroundAtom groundAtom) {
            this.groundAtom = groundAtom;
        }

    }
	
	// YD: Event arguments to be passed when in structure
    public class AtomAndStructureEventArgs : EventArgs
    {
        public clsGroundAtom groundAtom;
        public clsPolygon Structure;

        public AtomAndStructureEventArgs(clsGroundAtom groundAtom, clsPolygon Structure)
        {
            this.groundAtom = groundAtom;
            this.Structure = Structure;
        }

    }

    public class BasicStateFormGroundTaskOrder
    {
        public BasicStateFormGroundTaskOrder PrevState;
        public bool InitialActivityExecuted = false;

        public BasicStateFormGroundTaskOrder()
        {
        }
        public virtual void Enter(clsGroundAtom refGroundAtom)
        {
			// YD: register default event listeners
            refGroundAtom.earthquakeStartedEventHandler += earthquakeStartedDefaultEventHandler;
            refGroundAtom.earthquakeEndedEventHandler += earthquakeEndedDefaultEventHandler;
            refGroundAtom.forcesHaveArrivedEventHandler += forcesHaveArrivedDefaultEventHandler;
			// ---
        }
        public virtual void Execute(clsGroundAtom refGroundAtom)
        { }
        public virtual void Exit(clsGroundAtom refGroundAtom)
        { }

        public virtual void WaitAllThreadingTask(clsGroundAtom refGroundAtom)
        { }

		// YD: default event handler for when forces arrive
        protected void forcesHaveArrivedDefaultEventHandler(object sender, EventArgs e)
        {
            clsGroundAtom refGroundAtom = ((clsGroundAtomEventArgs)e).groundAtom;

            refGroundAtom.reEvaluationEventHandler += reevaluationAfterForcesHaveArrivedEventHandler;
			
			// no need to hurry, everything is under control by the police, military, etc.
            refGroundAtom.currentSpeed = refGroundAtom.baselineSpeed;

            refGroundAtom.resetMovementData();

            if (moveToSocialComparisonStateIfShould(refGroundAtom)) return;

			// default action is simply going to the nearest police barrier
            clsActivityMovement barrierMovement = RouteUtils.createActivityAndStart(refGroundAtom, (int)refGroundAtom.currentSpeed, null);
            refGroundAtom.ChangeState(new GO_TO_POLICE_BARRIER_STATE(barrierMovement));
            return;
        }
		
		// YD: default event handler for when an earthquake starts when not in structure
        protected void earthquakeStartedDefaultEventHandler(object sender, EventArgs e)
        {
			// default action is holding on to an immobile object, be it the ground, a fence, a car etc.
            clsGroundAtom refGroundAtom = ((clsGroundAtomEventArgs)e).groundAtom;
			
			// there's an earthquake. better move fast before something falls on me
            double panicSpeedMultiplier = (Util.random.NextDouble() + 1.5);
            refGroundAtom.currentSpeed = panicSpeedMultiplier * refGroundAtom.baselineSpeed;
			
            refGroundAtom.knowsAboutEarthquake = true;

            // there are no proxemics in non-normative conditions
            refGroundAtom.collisionRadius = 2*clsGroundAtom.RADIUS;

            refGroundAtom.reEvaluationEventHandler += reevaluationAfterEarthquakeStartedEventHandler;

            refGroundAtom.lastRoutePoint.x = refGroundAtom.X_Route;
            refGroundAtom.lastRoutePoint.y = refGroundAtom.Y_Route;
            if (moveToSocialComparisonStateIfShould(refGroundAtom)) return;

            clsActivityMovement activity = RouteUtils.createActivityAndStart(refGroundAtom, (int)refGroundAtom.currentSpeed, null);
            refGroundAtom.ChangeState(new HOLD_ON_TO_OBJECT_STATE(activity));
            return;
        }
		
		// YD: default event handler for when an earthquake ends when not in structure
        protected void earthquakeEndedDefaultEventHandler(object sender, EventArgs e)
        {
            clsGroundAtom refGroundAtom = ((clsGroundAtomEventArgs)e).groundAtom;

            // set reevaluation decision probabilities
            refGroundAtom.reEvaluationEventHandler += reevaluationAfterEarthquakeEndedEventHandler;

            // decide what to do either individually or by social comparison

            if (moveToSocialComparisonStateIfShould(refGroundAtom)) return;

			// go back to regular activity
            getBackToRegularMovement(refGroundAtom);
			
			// there's nothing to hurry - earthquake has ended
            refGroundAtom.currentSpeed = refGroundAtom.baselineSpeed;
        }

        protected void reevaluationAfterEarthquakeStartedEventHandler(object sender, EventArgs e)
        {
            // nothing to do here for now
        }

        protected void reevaluationAfterEarthquakeEndedEventHandler(object sender, EventArgs e)
        {
            clsGroundAtom refGroundAtom = ((clsGroundAtomEventArgs)e).groundAtom;
            List<clsGroundAtom> atomsNearby = refGroundAtom.m_GameObject.m_GameManager.QuadTreeGroundAtom.SearchEntities(refGroundAtom.curr_X, refGroundAtom.curr_Y, 20, isPrecise: true);

            // if in social comparison state and not stuck stay in social comparison - things seem to be moving
            if (refGroundAtom.currentState.GetType() == typeof(SOCIAL_COMPARISON_STATE))
            {
                SOCIAL_COMPARISON_STATE sc = (SOCIAL_COMPARISON_STATE)refGroundAtom.currentState;
                bool stuck = refGroundAtom.currentSpeed <= (float)sc.baselineSpeed / 10.0;
                if (!stuck) return;
                else
                {
                     // stuck in social comparison - first return to baseline speed
                    refGroundAtom.currentSpeed = sc.baselineSpeed;
                    getBackToRegularMovement(refGroundAtom);
                }
            }
            else
            {
                refGroundAtom.currentSpeed = refGroundAtom.baselineSpeed;

                // check for casualties nearby
                List<clsGroundAtom> casualtiesNearby = new List<clsGroundAtom>();
                foreach (clsGroundAtom atom in atomsNearby)
                {
                    if (atom.healthStatus.isIncapacitated || atom.healthStatus.isDead)
                    {
                        casualtiesNearby.Add(atom);
                    }
                }

                // check if already handled the specific trigger
                foreach (clsGroundAtom casualty in casualtiesNearby)
                {
                    bool casualtyHandled = false;

                    foreach (Trigger trigger in refGroundAtom.triggers)
                    {
                        if (trigger.type != Trigger.Type.DEAD_OR_INCAPACITATED) continue;

                        DeadOrIncapacitatedTrigger doiTrigger = (DeadOrIncapacitatedTrigger)trigger;
                        if (doiTrigger.atomName == casualty.MyName)
                        {
                            casualtyHandled = true;
                            break;
                        }
                    }

                    if (!casualtyHandled)
                    {

                        // decide whether to help casualty or not
                        double helpOrNot = Util.random.NextDouble();

                        if (helpOrNot < 0.2)
                        {
                            // decide individually to help
                            clsActivityMovement helpActivity = RouteUtils.createActivityAndStart(refGroundAtom, (int)refGroundAtom.currentSpeed, null);
                            refGroundAtom.ChangeState(new HELP_OTHER_STATE(helpActivity, casualty));
                            refGroundAtom.triggers.Add(new DeadOrIncapacitatedTrigger(casualty.MyName));
                            return;
                        }
                        else if (helpOrNot >= 0.2 && helpOrNot < 0.3)
                        {
                            // decide individually just to look
                            clsActivityMovement curiosityActivity = RouteUtils.createActivityAndStart(refGroundAtom, (int)refGroundAtom.currentSpeed, null);
                            refGroundAtom.ChangeState(new CURIOSITY_MOVEMENT_STATE(curiosityActivity, casualty));
                            refGroundAtom.triggers.Add(new DeadOrIncapacitatedTrigger(casualty.MyName));
                            return;
                        }
                        else if (helpOrNot >= 0.3 && helpOrNot < 0.5)
                        {
                            // decide by socially comparing behavior
                            if (moveToSocialComparisonStateIfCan(refGroundAtom))
                            {
                                refGroundAtom.triggers.Add(new DeadOrIncapacitatedTrigger(casualty.MyName));
                                return;
                            }
                        }
                        else
                        {
                            refGroundAtom.triggers.Add(new DeadOrIncapacitatedTrigger(casualty.MyName));
                        }
                    }
                }
            }
        }

        protected void reevaluationAfterForcesHaveArrivedEventHandler(object sender, EventArgs e)
        {
            clsGroundAtom refGroundAtom = ((clsGroundAtomEventArgs)e).groundAtom;

            refGroundAtom.currentSpeed = refGroundAtom.baselineSpeed;

            // if in social comparison state and not stuck stay in social comparison - things seems to be moving
            if (refGroundAtom.currentState.GetType() == typeof(SOCIAL_COMPARISON_STATE))
            {
                SOCIAL_COMPARISON_STATE sc = (SOCIAL_COMPARISON_STATE)refGroundAtom.currentState;
                bool stuck = refGroundAtom.currentSpeed <= (float)sc.baselineSpeed / 10.0;
                if (stuck)
                {
                    clsActivityMovement barrierMovement = RouteUtils.createActivityAndStart(refGroundAtom, (int)refGroundAtom.currentSpeed, null);
                    refGroundAtom.ChangeState(new GO_TO_POLICE_BARRIER_STATE(barrierMovement));
                }
            }

            if (refGroundAtom.currentState.GetType() != typeof(GO_TO_POLICE_BARRIER_STATE))
            {
                clsActivityMovement barrierMovement = RouteUtils.createActivityAndStart(refGroundAtom, (int)refGroundAtom.currentSpeed, null);
                refGroundAtom.ChangeState(new GO_TO_POLICE_BARRIER_STATE(barrierMovement));
                return;
            }
        }
		// default event handler for incapacitation
        protected void incapacitationDefaultEventHandler(object sender, EventArgs e)
        {
            clsGroundAtom refGroundAtom = ((clsGroundAtomEventArgs)e).groundAtom;
            refGroundAtom.ChangeState(new INCAPACITATED_STATE());
            return;
        }
		
		// default event handler for death
        protected void deathDefaultEventHandler(object sender, EventArgs e)
        {
            clsGroundAtom refGroundAtom = ((clsGroundAtomEventArgs)e).groundAtom;
            refGroundAtom.ChangeState(new DEAD_STATE());
            return;
        }
		
		// get back to regular activity
        private void getBackToRegularMovement(clsGroundAtom refGroundAtom)
        {
            clsActivityMovement backToNormal = RouteUtils.createActivityAndStart(refGroundAtom, (int)refGroundAtom.currentSpeed, null);
            PointData closestPoint = refGroundAtom.m_GameObject.lookForClosestRegularRoute(refGroundAtom);
            Route route = RouteUtils.typRouteToRoute(closestPoint.route);
            List<DPoint> trimmedRoute = new List<DPoint>();
            for (int i = closestPoint.legNum; i < route.Points.Count(); i++)
                trimmedRoute.Add(route.Points.ElementAt(i));
            route.Points = trimmedRoute;
            refGroundAtom.currentLeg = 1;
            refGroundAtom.currentStartWaypoint = closestPoint.routeIndex1;
            refGroundAtom.currentEndWaypoint = closestPoint.routeIndex2;
            refGroundAtom.resetMovementData();

            double distanceFromLastRoutePoint = TerrainService.MathEngine.CalcDistance(refGroundAtom.lastRoutePoint.x, refGroundAtom.lastRoutePoint.y,
                                                                                       refGroundAtom.curr_X, refGroundAtom.curr_Y);

            DPoint pointOnSidewalk;
            if ((refGroundAtom.lastRoutePoint.x == 0 && refGroundAtom.lastRoutePoint.y == 0) || (distanceFromLastRoutePoint > 5))
            {
				// go to the closest sidewalk
                pointOnSidewalk = new DPoint(route.Points.ElementAt(0).x, route.Points.ElementAt(0).y);
                refGroundAtom.lastRoutePoint.x = pointOnSidewalk.x;
                refGroundAtom.lastRoutePoint.y = pointOnSidewalk.y;
            }
            else
            {
				// continue from where you were before the earthquake started
                pointOnSidewalk = refGroundAtom.lastRoutePoint;
            }

            refGroundAtom.ChangeState(new GET_ON_SIDEWALK(backToNormal, pointOnSidewalk, route));
        }

        // move to social comparison state if can and should. Return true if can and should and move, else return false
        public bool moveToSocialComparisonStateIfShould(clsGroundAtom refGroundAtom)
        {
            double socialComparisonProbability = Util.random.NextDouble();

            // draw probabilistically whether to compare or not
            if (socialComparisonProbability <= refGroundAtom.getSocialComparisonProbability())
            {
                // do social comparison
                clsGroundAtom mostSimilar = SocialComparison.findMostSimilarByDistanceAndAzimuth(refGroundAtom);

                // check if there is someone similar to me
                if (mostSimilar != null)
                {
                    clsActivityMovement moveToMostSimilar = RouteUtils.createActivityAndStart(refGroundAtom, (int)refGroundAtom.currentSpeed, null);
                    refGroundAtom.resetMovementData();
                    refGroundAtom.ChangeState(new SOCIAL_COMPARISON_STATE(moveToMostSimilar, mostSimilar));
                    return true;
                }

                // if couldn't find someone similar in vicinity do not compare
            }

            return false;
        }

        // move to social comparison state if can.
        public bool moveToSocialComparisonStateIfCan(clsGroundAtom refGroundAtom)
        {
            double socialComparisonProbability = Util.random.NextDouble();

            // draw probabilistically whether to compare or not
            // do social comparison
            clsGroundAtom mostSimilar = SocialComparison.findMostSimilarByDistanceAndAzimuth(refGroundAtom);

            // check if there is someone similar to me
            if (mostSimilar != null)
            {
                clsActivityMovement moveToMostSimilar = RouteUtils.createActivityAndStart(refGroundAtom, (int)refGroundAtom.currentSpeed, null);
                refGroundAtom.resetMovementData();
                refGroundAtom.ChangeState(new SOCIAL_COMPARISON_STATE(moveToMostSimilar, mostSimilar));
                return true;
            }

            // if couldn't find someone similar in vicinity do not compare

            return false;
        }
    }
    public class ADMINISTRATIVE_STATE : BasicStateFormGroundTaskOrder
    {
        public override void Enter(clsGroundAtom refGroundAtom)
        {
            base.Enter(refGroundAtom);
        }
        public override void Execute(clsGroundAtom refGroundAtom)
        {
            //VH
            



            if (!refGroundAtom.knowsAboutEarthquake && refGroundAtom.m_GameObject.earthquakeStarted())
            {
                refGroundAtom.knowsAboutEarthquake = true;

                clsActivityMovement movement = new clsActivityMovement();
                refGroundAtom.reRouteToEscape(movement);

                refGroundAtom.ChangeState(new ADMINISTRATIVE_STATE());
                return;
            }
			// --------------------------------------------------------------------------------------
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

		  	 // Yinon Douchan: Modified next line to plan route from route_x and route_y instead of curr_x and curr_y
             typRoute R= await refGroundAtom.m_GameObject.m_GameManager.refTerrain.CreateRoute(refGroundAtom.X_Route, refGroundAtom.Y_Route, refActivityMovement.ReferencePoint.x, refActivityMovement.ReferencePoint.y, refActivityMovement.RouteActivity.RouteGuid);
		  	 // -----------------------------------------------------------------------------------
             refGroundAtom.currentRoute = R;


           // refGroundAtom.SetRoute(refActivityMovement.RouteActivity);
             base.Enter(refGroundAtom);
        }



        public override void Execute(clsGroundAtom refGroundAtom)
        {
            List<CollisionTime> CollisionsToDelete=new List<CollisionTime>();
            foreach(var v in refGroundAtom.Collisions)
            {
                if((refGroundAtom.m_GameObject.Ex_clockDate-v.time).TotalSeconds>2)
                {
                    CollisionsToDelete.Add(v);
                }
            }
            foreach (var v in CollisionsToDelete)
            {
                refGroundAtom.Collisions.Remove(v);
            }

        //   refGroundAtom.Collisions.ForEach(t=>(refGroundAtom.m_GameObject.Ex_clockDate-t.time).TotalSeconds>2)

             if(refActivityMovement.TimeTo<refGroundAtom.m_GameObject.Ex_clockDate)
             {
                 refActivityMovement.isEnded = true;
                 refActivityMovement.isActive = false;
                 refGroundAtom.ChangeState(new ADMINISTRATIVE_STATE());
                 return;
            }


            if (refGroundAtom.currentRoute == null) return;

           
            if ( refGroundAtom.currentLeg > refGroundAtom.currentRoute.arr_legs.Count)  //-1)  // refActivityMovement.RouteActivity.Points.Count()-1)
            {
                // Fix 01 Start
                int nl = refGroundAtom.currentRoute.arr_legs.Count;
                double xEnd = refGroundAtom.currentRoute.arr_legs[nl - 1].ToLongn;
                double yEnd = refGroundAtom.currentRoute.arr_legs[nl - 1].ToLatn;
                refGroundAtom.curr_X = xEnd;
                refGroundAtom.curr_Y = yEnd;
                // Fix 01 End

                refActivityMovement.isEnded = true;
                refActivityMovement.isActive = false;
                refGroundAtom.currentRoute = null;
                refGroundAtom.ChangeState(new ADMINISTRATIVE_STATE());
                
                
                
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
            bool isRight = true;
			// Yinon Douchan: Collision management
            bool isLeft = true;

            refGroundAtom.currentAzimuth = currentAzimuth;
			// -----------------------------------
            // refGroundAtom.Collisions.Clear();
        Lab1: ;
            refGroundAtom.isCollision = false;
// Fix 02  if (refGroundAtom.Offset_Azimuth != 0.0 || refGroundAtom.Offset_Distance!=0.0)
            if (refGroundAtom.Offset_Distance != 0.0)
            {
                AzimDepl = currentAzimuth + refGroundAtom.Offset_Azimuth;
                if (AzimDepl >= 360) AzimDepl = AzimDepl - 360;
                
                TerrainService.MathEngine.CalcProjectedLocationNew(nextRoute_X, nextRoute_Y, AzimDepl, refGroundAtom.Offset_Distance, out X_Distination, out Y_Distination);//, true);
            }
            else
            {
                X_Distination = nextRoute_X;
                Y_Distination = nextRoute_Y;
            }

            List<clsGroundAtom> colAtoms = refGroundAtom.m_GameObject.m_GameManager.QuadTreeGroundAtom.SearchEntities(X_Distination, Y_Distination, refGroundAtom.collisionRadius, isPrecise: true);

        //   List<clsGroundAtom> colAtoms = refGroundAtom.m_GameObject.m_GameManager.QuadTreeGroundAtom.SearchEntities(X_Distination, refGroundAtom.curr_Y, clsGroundAtom.OFFSET_IN_COLLISION, isPrecise: true);
            foreach (clsGroundAtom atom in colAtoms)
            {
                if (atom != refGroundAtom)
                {    
                    TerrainService.Vector vDest= new TerrainService.Vector(X_Distination, Y_Distination, 0);
                    TerrainService.Vector vMe = new TerrainService.Vector(refGroundAtom.curr_X, refGroundAtom.curr_Y, 0);

                    TerrainService.Vector vCollision = new TerrainService.Vector(atom.curr_X, atom.curr_Y, 0);

                    TerrainService.Vector MyDirection = vDest - vMe;
                    MyDirection.normalize();
                    TerrainService.Vector CollisionDirection = vCollision - vMe;
                    CollisionDirection.normalize();
                    double dot = MyDirection * CollisionDirection;
                    if (dot >=0.8)// 0.6)                                                  //Against  Main Direction
                    {
                       // if (atom.Collisions.Contains(refGroundAtom.MyName)) continue;

// Fix 03              if (atom.Collisions.Exists(v => v.name == refGroundAtom.MyName)) continue;
                        if (atom.Collisions.Exists(v => v.name == refGroundAtom.MyName)) continue;
//Fix 04 - New If
                        double d = TerrainService.MathEngine.CalcDistance(X_Distination, Y_Distination, atom.curr_X, atom.curr_Y);
                        refGroundAtom.isCollision = true;
                        CollisionTime cTime = new CollisionTime();
                        cTime.name = atom.MyName;
                        cTime.time = refGroundAtom.m_GameObject.Ex_clockDate;
                        refGroundAtom.Collisions.Add(cTime);

                        break;
                    }
                  
                }
           }

           if(refGroundAtom.isCollision)
           {
               refGroundAtom.Offset_Azimuth = 90;
			   // Yinon Douchan: Modified collision handling
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
                   clsGroundAtom mostSimilar = SocialComparison.findMostSimilarByDistanceAndAzimuth(refGroundAtom);
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
             //  refGroundAtom.Offset_Distance += clsGroundAtom.OFFSET_IN_COLLISION;// 2*clsGroundAtom.RADIUS;
               if (refGroundAtom.Offset_Distance < 0)
               {
                   refGroundAtom.Offset_Azimuth = 180 - refGroundAtom.Offset_Azimuth;
               }
			   // --------------------------------------------------------------------------

			   // Yinon Douchan: Commented out - Why are you assigning offsetted position to route position? This caused bugs in movement
               //nextRoute_X = X_Distination;
               //nextRoute_Y = Y_Distination;
			   // ------------------------------------------------------------------------------------


             //  AzimDepl = currentAzimuth + refGroundAtom.Offset_Azimuth;
             //  if (AzimDepl >= 360) AzimDepl = AzimDepl - 360;
             //  TerrainService.MathEngine.CalcProjectedLocationNew(X_Distination, Y_Distination, AzimDepl, refGroundAtom.Offset_Distance, out X_Distination, out Y_Distination);//, true);

			   // Yinon Douchan: Commented out - This caused the simulator to enter an endless loop
               //goto Lab1;
			   // ---------------------------------------------------------------------------------
               //AzimDepl = currentAzimuth + refGroundAtom.Offset_Azimuth;
               //if (AzimDepl >= 360) AzimDepl = AzimDepl - 360;

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



           //List<clsGroundAtom> colAtoms = refGroundAtom.m_GameObject.m_GameManager.QuadTreeGroundAtom.SearchEntities(refGroundAtom.curr_X, refGroundAtom.curr_Y, 10, isPrecise: true);
           //foreach (clsGroundAtom atom in colAtoms)
           //{
           //    if(atom!=refGroundAtom)
           //    {
           //        refGroundAtom.isCollision = true;
           //        break;
           //    }
           //}


           //if (refGroundAtom.currentLeg >  refGroundAtom.currentRoute.arr_legs.Count)  //-1)  // refActivityMovement.RouteActivity.Points.Count()-1)
           // {
           //     refActivityMovement.isEnded = true;
           //     refActivityMovement.isActive = false;
           //     refGroundAtom.currentRoute = null;
           //     refGroundAtom.ChangeState(new ADMINISTRATIVE_STATE());
           //     return;
           // }
           //else
           //{
           //    refGroundAtom.Move(refGroundAtom.m_GameObject.m_GameManager.GroundCycleResolution);
           //}
        }
    }

    // Compare behavior to the most similar atom
    public class SOCIAL_COMPARISON_STATE : MOVEMENT_STATE
    {
        clsActivityMovement refActivityMovement;
        clsGroundAtom mostSimilarAtom;
        private double m_baselineSpeed; // speed before comparison
        public double baselineSpeed
        {
            get { return m_baselineSpeed; }
            set { m_baselineSpeed = value; }
        }

        public SOCIAL_COMPARISON_STATE(clsActivityMovement refActivityMovement, clsGroundAtom mostSimilarAtom)
            : base(refActivityMovement)
        {
            this.refActivityMovement = refActivityMovement;
            this.mostSimilarAtom = mostSimilarAtom;
        }

        public override void Enter(clsGroundAtom refGroundAtom)
        {
            refGroundAtom.currentRoute = RouteUtils.planStraightLineRoute(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y),
                                                    new DPoint(mostSimilarAtom.curr_X, mostSimilarAtom.curr_Y), "RouteToMostSimilar");
            m_baselineSpeed = refGroundAtom.currentSpeed;
        }

        public override void Execute(clsGroundAtom refGroundAtom)
        {
            // update route so that the destination will be the most similar atom
            refGroundAtom.currentRoute = RouteUtils.planStraightLineRoute(new DPoint(refGroundAtom.curr_X, refGroundAtom.curr_Y),
                                                    new DPoint(mostSimilarAtom.curr_X, mostSimilarAtom.curr_Y), "RouteToMostSimilar");

            // minimize differences
            SocialComparison.correctBehaviorToMostSimilar(refGroundAtom, mostSimilarAtom, m_baselineSpeed);

            // move
            base.Execute(refGroundAtom);
        }

        public override void Exit(clsGroundAtom refGroundAtom)
        {
            refGroundAtom.currentSpeed = m_baselineSpeed;
            base.Exit(refGroundAtom);
        }
    }

    class FormGroundTaskSM
    {
    }
}
