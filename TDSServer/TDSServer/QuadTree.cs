using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using GlobalMapperUtil; //VictorTerrain




namespace TDSServer
{

    [Serializable]
    public class QuadTree<TValue> where TValue : IQuadTree
    {
        //const int MAX_PER_NODE=20;
        const int MAX_PER_NODE = 200;

        //const int MAX_NODE_LEVEL = 10;

        // VH 11.11.2015
        //const int MAX_NODE_LEVEL = 30;
        const int MAX_NODE_LEVEL = 15;


        private DAreaRect m_Bounds;

        private QuadTree<TValue> m_NorthEast = null;
        private QuadTree<TValue> m_NorthWest = null;
        private QuadTree<TValue> m_SouthWest = null;
        private QuadTree<TValue> m_SouthEast = null;
        //   private QuadTree<TValue> m_Parent = null;
        private int m_nLevel;
        //    private string m_sName;

        private static QuadTree<TValue> m_BaseQuad = null;


        //  VH 28.10.2015
        //  public SortedList<string, TValue> AtomPlatformEntities = new SortedList<string, TValue>();
        private Dictionary<string, TValue> AtomPlatformEntities = new Dictionary<string, TValue>();



        public QuadTree(DAreaRect bounds, int level, QuadTree<TValue> parent)
        {
            if (m_BaseQuad == null)
            {
                m_BaseQuad = this;
            }

            m_Bounds = bounds;
            m_nLevel = level;
            //  m_Parent = parent;

        }

        public QuadTree(DAreaRect bounds, int level, QuadTree<TValue> parent, double x, double y)
        {
            if (m_BaseQuad == null)
            {
                m_BaseQuad = this;
            }

            m_Bounds = bounds;
            m_nLevel = level;
            //  m_Parent = parent;


            //if (bounds.minX == bounds.maxX)
            //{
            //}


            //  m_sName = "L" + level + ":X=" + bounds.minX + "Y=" + bounds.maxY;



            bool isFound = false;



            if (Contains(x, y))
            {
                isFound = true;

            }


            if (isFound == false) return;





            double distance = TerrainService.MathEngine.CalcDistance(m_Bounds.minX, m_Bounds.minY, m_Bounds.maxX, m_Bounds.maxY);

            if (level > 29)
            {
                return;
            }

            if (distance > 2000)
            {

                double dx = bounds.maxX - bounds.minX;
                double dy = bounds.maxY - bounds.minY;






                double nHalfHeight = dy / 2;
                double nHalfWidth = dx / 2;







                DAreaRect DRectNorthWest = new DAreaRect(bounds.minX, bounds.minY + nHalfHeight,
                                                            bounds.minX + nHalfWidth, bounds.maxY);




                m_NorthWest = new QuadTree<TValue>(DRectNorthWest, level + 1, this, x, y);



                DAreaRect DRectNorthEast = new DAreaRect(bounds.minX + nHalfWidth, bounds.minY + nHalfHeight,
                                                                     bounds.maxX, bounds.maxY);






                m_NorthEast = new QuadTree<TValue>(DRectNorthEast, level + 1, this, x, y);






                DAreaRect DRectSouthWest = new DAreaRect(bounds.minX, bounds.minY,
                                                                  bounds.minX + nHalfWidth, bounds.minY + nHalfHeight);





                m_SouthWest = new QuadTree<TValue>(DRectSouthWest, level + 1, this, x, y);







                DAreaRect DRectSouthEast = new DAreaRect(bounds.minX + nHalfWidth, bounds.minY,
                                                                                 bounds.maxX, bounds.minY + nHalfHeight);

                if (DRectSouthEast.minX == DRectSouthEast.maxX)
                {
                }


                m_SouthEast = new QuadTree<TValue>(DRectSouthEast, level + 1, this, x, y);

            }
        }

        private static object sync = new object();


        public void PositionUpdate(TValue obj)
        {
            lock (sync)
            {
                RemoveObject(obj.Key, obj);
                AddObject(obj.x, obj.y, obj.Key, obj);
            }
        }




        public void AddObject(double x, double y, string objName, TValue obj)
        {


            //   lock (AtomPlatformEntities)  //    this)
            {
                try
                {
                    if (obj == null) return;
                    if (obj.bVisibleToClient == false)
                        return;
                    if (Contains(x, y))
                    {
                        // int nIndex = 0;

                        //nIndex = AtomPlatformEntities.IndexOfKey(objName);



                        if (m_NorthWest != null)
                        {
                            m_NorthWest.AddObject(x, y, objName, obj);
                            m_NorthEast.AddObject(x, y, objName, obj);
                            m_SouthWest.AddObject(x, y, objName, obj);
                            m_SouthEast.AddObject(x, y, objName, obj);

                            return;
                        }




                        lock (AtomPlatformEntities)  //    this)
                        {
                            bool isExist = AtomPlatformEntities.ContainsKey(objName);
                            //if (nIndex < 0)
                            if (isExist == false)
                            {


                                AtomPlatformEntities.Add(objName, obj);
                                obj.QuadTreeBounds = m_Bounds;






                                if (AtomPlatformEntities.Count > MAX_PER_NODE && m_nLevel < MAX_NODE_LEVEL)
                                {
                                    double dx = m_Bounds.maxX - m_Bounds.minX;
                                    double dy = m_Bounds.maxY - m_Bounds.minY;
                                    double nHalfHeight = dy / 2;
                                    double nHalfWidth = dx / 2;




                                    DAreaRect DRectNorthWest = new DAreaRect(m_Bounds.minX, m_Bounds.minY + nHalfHeight,
                                                         m_Bounds.minX + nHalfWidth, m_Bounds.maxY);

                                    m_NorthWest = new QuadTree<TValue>(DRectNorthWest, m_nLevel + 1, this);




                                    DAreaRect DRectNorthEast = new DAreaRect(m_Bounds.minX + nHalfWidth, m_Bounds.minY + nHalfHeight,
                                                                 m_Bounds.maxX, m_Bounds.maxY);

                                    m_NorthEast = new QuadTree<TValue>(DRectNorthEast, m_nLevel + 1, this);





                                    DAreaRect DRectSouthWest = new DAreaRect(m_Bounds.minX, m_Bounds.minY,
                                                                 m_Bounds.minX + nHalfWidth, m_Bounds.minY + nHalfHeight);

                                    m_SouthWest = new QuadTree<TValue>(DRectSouthWest, m_nLevel + 1, this);





                                    DAreaRect DRectSouthEast = new DAreaRect(m_Bounds.minX + nHalfWidth, m_Bounds.minY,
                                               m_Bounds.maxX, m_Bounds.minY + nHalfHeight);

                                    m_SouthEast = new QuadTree<TValue>(DRectSouthEast, m_nLevel + 1, this);



                                    foreach (TValue GroundEntity in AtomPlatformEntities.Values)
                                    {
                                        string PName = GroundEntity.Key; //GroundEntity.ParentName + "_" + GroundEntity.Number.ToString();

                                        if (DRectNorthWest.Contains(GroundEntity.x, GroundEntity.y))
                                        {
                                            lock (m_NorthWest.AtomPlatformEntities)
                                            {
                                                m_NorthWest.AtomPlatformEntities.Add(PName, GroundEntity);
                                            }

                                            obj.QuadTreeBounds = m_NorthWest.m_Bounds;

                                        }
                                        else if (DRectNorthEast.Contains(GroundEntity.x, GroundEntity.y))
                                        {
                                            lock (m_NorthEast.AtomPlatformEntities)
                                            {
                                                m_NorthEast.AtomPlatformEntities.Add(PName, GroundEntity);
                                            }

                                            obj.QuadTreeBounds = m_NorthEast.m_Bounds;

                                        }
                                        else if (DRectSouthWest.Contains(GroundEntity.x, GroundEntity.y))
                                        {
                                            lock (m_SouthWest.AtomPlatformEntities)
                                            {
                                                m_SouthWest.AtomPlatformEntities.Add(PName, GroundEntity);
                                            }

                                            obj.QuadTreeBounds = m_SouthWest.m_Bounds;

                                        }
                                        else if (DRectSouthEast.Contains(GroundEntity.x, GroundEntity.y))
                                        {
                                            lock (m_SouthEast.AtomPlatformEntities)
                                            {
                                                m_SouthEast.AtomPlatformEntities.Add(PName, GroundEntity);
                                            }

                                            obj.QuadTreeBounds = m_SouthEast.m_Bounds;

                                        }

                                    }

                                    AtomPlatformEntities.Clear();

                                }
                            }
                            else
                            {


                            }



                        }





                    }
                    else
                    {
                        //RemoveObject(objName, obj);
                        //if (m_Parent != null)
                        //{
                        //    m_Parent.AddObject(x, y, objName, obj);
                        //}


                    }


                }
                catch (Exception ex)
                {
                 //   Log.WriteErrorToLog(ex, "");
                }
            }
        }











        public bool Contains(double x, double y)
        {
            bool result = false;
            if ((m_Bounds.minX <= x) && (m_Bounds.maxX >= x) && (m_Bounds.minY <= y) && (m_Bounds.maxY >= y)) result = true;
            return result;
        }


        public static bool Intersect(DAreaRect Rect1, DAreaRect Rect2)
        {
            bool result = false;


            if (Rect2.maxX >= Rect1.minX && Rect2.minX <= Rect1.maxX)
            {
                if (Rect2.maxY >= Rect1.minY && Rect2.minY <= Rect1.maxY)
                {
                    result = true;
                }

            }


            return result;
        }

        public static bool InRect(double x, double y, DAreaRect rect)
        {
            bool result = false;
            if ((rect.minX <= x) && (rect.maxX >= x) && (rect.minY <= y) && (rect.maxY >= y)) result = true;
            return result;
        }

        public static bool InRect(DAreaRect rectIn, DAreaRect rectOut)
        {
            bool result = false;
            if (rectIn.minX >= rectOut.minX && rectIn.maxX <= rectOut.maxX && rectIn.minY >= rectOut.minY && rectIn.maxY <= rectOut.maxY) result = true;
            return result;
        }

        public static DAreaRect GetAreaRect(double x, double y, double radius)
        {

            double minX = 0;
            double minY = 0;

            double maxX = 0;
            double maxY = 0;


            double Xm = 0;
            double Ym = 0;



            double Xtest=0;
            double Ytest=0;

           
           // TerrainService.MathEngine.CalcProjectedLocation(x, y, 270, radius, out minX, out Ym, true);
          // TerrainService.MathEngine.CalcProjectedLocation(x, y, 270, radius, out Xtest, out Ytest, true);


            TerrainService.MathEngine.CalcProjectedLocationNew(x, y, 270, radius, out minX, out Ym);

            //VH 22.07.2015

            double Dx = (x - minX);

            maxX = x + Dx;
            //TerrainService.MathEngine.CalcProjectedLocation(x, y, 90, radius, out maxX, out Ym, true);



            maxY = y + Dx;
            // TerrainService.MathEngine.CalcProjectedLocation(x, y, 0, radius, out Xm, out maxY, true);


            minY = y - Dx;
            // TerrainService.MathEngine.CalcProjectedLocation(x, y, 180, radius, out Xm, out minY, true);




            DAreaRect resRect = new DAreaRect(minX, minY, maxX, maxY);


            return resRect;
        }











        public List<TValue> SearchEntities(double x, double y, double Radius, bool isPrecise = true)
        {
            List<TValue> Objects = new List<TValue>(4000);
            DAreaRect rect = QuadTree<TValue>.GetAreaRect(x, y, Radius);

            SearchEntities(rect, x, y, Radius, ref Objects, isPrecise: isPrecise);
            return Objects;
        }

        public List<TValue> SearchEntities(DAreaRect AreaRectangle)
        {
            List<TValue> Objects = new List<TValue>(4000);

            //DAreaRect rect = QuadTree<TValue>.GetAreaRect(x, y, Radius);

            SearchEntities(AreaRectangle, 0, 0, 0, ref Objects);

            return Objects;
        }


        public void SearchEntities(DAreaRect AreaRect, double x, double y, double Radius, ref List<TValue> Objects, bool isPrecise = true)
        {

            if (QuadTree<TValue>.Intersect(m_Bounds, AreaRect))
            {
                if (m_NorthWest != null)
                {
                    m_NorthWest.SearchEntities(AreaRect, x, y, Radius, ref Objects, isPrecise: isPrecise);
                    m_NorthEast.SearchEntities(AreaRect, x, y, Radius, ref Objects, isPrecise: isPrecise);
                    m_SouthWest.SearchEntities(AreaRect, x, y, Radius, ref Objects, isPrecise: isPrecise);
                    m_SouthEast.SearchEntities(AreaRect, x, y, Radius, ref Objects, isPrecise: isPrecise);
                }
                else
                {
                    if (QuadTree<TValue>.InRect(m_Bounds, AreaRect))
                    {
                        lock (AtomPlatformEntities)
                        {

                            if (AtomPlatformEntities.Count > 0)
                            {
                                Objects.AddRange(AtomPlatformEntities.Values);
                            }



                            //   for (int i = 0; i < AtomPlatformEntities.Count; i++)
                            //   foreach (TValue atom in AtomPlatformEntities.Values)



                            //foreach (string key in AtomPlatformEntities.Keys)
                            //{
                            //    TValue atom = AtomPlatformEntities[key];
                            //   // TValue atom = AtomPlatformEntities.Values.ElementAt(i);
                            //   // TValue atom = AtomPlatformEntities.Values[i];  

                            //    Objects.Add(atom);
                            //}
                        }

                    }
                    else
                    {

                        if (isPrecise == true && Radius != 0 && (x != 0 || y != 0))
                        {
                            lock (AtomPlatformEntities)
                            {
                                // for (int i = 0; i < AtomPlatformEntities.Count; i++)
                                // foreach (TValue atom in AtomPlatformEntities.Values)
                                foreach (string key in AtomPlatformEntities.Keys)
                                {
                                    TValue atom = AtomPlatformEntities[key];
                                    // TValue atom = AtomPlatformEntities.Values.ElementAt(i);
                                    //                                    TValue atom = AtomPlatformEntities.Values[i];
                                    // if (atom.Platform.PlatformCategoryId == enumPlatformId.SOLDIER) continue;
                                    double dist = TerrainService.MathEngine.CalcDistance(x, y, atom.x, atom.y);
                                    if (dist <= Radius)
                                    {
                                        Objects.Add(atom);
                                    }
                                }
                            }


                        }
                        else
                        {
                            lock (AtomPlatformEntities)
                            {
                                //   for (int i = 0; i < AtomPlatformEntities.Count; i++)
                                //                                foreach (TValue atom in AtomPlatformEntities.Values)
                                foreach (string key in AtomPlatformEntities.Keys)
                                {
                                    TValue atom = AtomPlatformEntities[key];
                                    //  TValue atom = AtomPlatformEntities.Values.ElementAt(i);
                                    //                                    TValue atom = AtomPlatformEntities.Values[i];
                                    if (QuadTree<TValue>.InRect(atom.x, atom.y, AreaRect))
                                    {
                                        Objects.Add(atom);
                                    }
                                }
                            }
                        }

                    }



                }
            }



        }


        public void ClearObject()
        {
            lock (AtomPlatformEntities)//    this)
            {
                ClearObjectHLP();
            }
        }

        private void ClearObjectHLP()
        {

            if (AtomPlatformEntities != null && AtomPlatformEntities.Count > 0)
            {

                AtomPlatformEntities.Clear();
            }

            if (m_NorthEast != null)
            {
                m_NorthEast.ClearObjectHLP();
            }
            if (m_NorthWest != null)
            {
                m_NorthWest.ClearObjectHLP();
            }
            if (m_SouthWest != null)
            {
                m_SouthWest.ClearObjectHLP();
            }
            if (m_SouthEast != null)
            {
                m_SouthEast.ClearObjectHLP();
            }

        }
        public void RemoveObject(string objName, object obj)
        {

            // lock (AtomPlatformEntities)//    this)
            {
                try
                {
                    // int nIndex = 0;
                    // nIndex = AtomPlatformEntities.IndexOfKey(objName);

                    lock (AtomPlatformEntities)//    this)
                    {

                        bool isExist = AtomPlatformEntities.ContainsKey(objName);
                        //                    if (nIndex >= 0)
                        if (isExist == true)
                        {
                            AtomPlatformEntities.Remove(objName);
                        }

                    }


                    if (m_NorthEast != null)
                    {
                        m_NorthEast.RemoveObject(objName, obj);
                    }
                    if (m_NorthWest != null)
                    {
                        m_NorthWest.RemoveObject(objName, obj);
                    }
                    if (m_SouthWest != null)
                    {
                        m_SouthWest.RemoveObject(objName, obj);
                    }
                    if (m_SouthEast != null)
                    {
                        m_SouthEast.RemoveObject(objName, obj);
                    }



                }
                catch (Exception ex)
                {
                  //  Log.WriteErrorToLog(ex, "");
                }
            }
        }

    }

    public interface IQuadTree
    {
        string Key
        {
            get;
        }
        double x
        {
            get;
        }
        double y
        {
            get;
        }
        DAreaRect QuadTreeBounds
        {
            set;
        }
        bool bVisibleToClient
        {
            get;
        }
    }
}

