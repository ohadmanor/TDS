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
		    // Yinon Douchan: Code for simulating an ambulance - should be done differently, I know
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


            if (refGroundAtom.currentLeg > refGroundAtom.currentRoute.arr_legs.Count)  //-1)  // refActivityMovement.RouteActivity.Points.Count()-1)
            {
                // Fix 01 Start
                int nl = refGroundAtom.currentRoute.arr_legs.Count;
                double xEnd = refGroundAtom.currentRoute.arr_legs[nl - 1].ToLongn;
                double yEnd = refGroundAtom.currentRoute.arr_legs[nl - 1].ToLatn;
                refGroundAtom.curr_X = xEnd;
                refGroundAtom.curr_Y = yEnd;
                // Fix 01 End

			    // Yinon Douchan: Commented out in order to make movement cyclic.
                //refActivityMovement.isEnded = true;
                //refActivityMovement.isActive = false;
                //refGroundAtom.currentRoute = null;
                //refGroundAtom.ChangeState(new ADMINISTRATIVE_STATE());
				// --------------------------------------------------------------

				// Yinon Douchan: Only for cyclic movement
                refGroundAtom.currentLeg = 1;
                refGroundAtom.X_Route = refGroundAtom.currentRoute.arr_legs[0].FromLongn;
                refGroundAtom.Y_Route = refGroundAtom.currentRoute.arr_legs[0].FromLatn;


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
				// --------------------------------------------------------------------
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

            List<clsGroundAtom> colAtoms = refGroundAtom.m_GameObject.m_GameManager.QuadTreeGroundAtom.SearchEntities(X_Distination, Y_Distination, 2 * clsGroundAtom.RADIUS, isPrecise: true);

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

    class FormGroundTaskSM
    {
    }
}
