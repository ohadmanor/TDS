using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using TDSClient.SAGInterface;

using TerrainService;

namespace TDSClient
{
    public class Utilites
    {
        public static void UserDrawRasterWPFScreenCoordinate(System.Windows.Media.DrawingContext dc, System.Windows.Media.ImageSource ImageSource,
                                                              int PixelX, int PixelY, int Width, int Height, double angle = 0.0, System.Windows.Media.ImageBrush pImageBrush = null)
        {
            System.Windows.Size sz = new System.Windows.Size(Width, Height);
            System.Windows.Point pnt = new System.Windows.Point(PixelX - sz.Width / 2, PixelY - sz.Height / 2);
            System.Windows.Rect rectBack = new System.Windows.Rect(pnt, sz);
            //if (angle == 0.0)
            //{
            //    dc.DrawImage(ImageSource, rectBack);
            //}
            //else
            //{
            System.Windows.Media.RotateTransform rotRect = new System.Windows.Media.RotateTransform(angle, PixelX, PixelY);

            System.Windows.Media.RectangleGeometry RectGeo = new System.Windows.Media.RectangleGeometry(rectBack);
            RectGeo.Transform = rotRect;
            System.Windows.Media.ImageBrush imbrush = null;
            if (pImageBrush != null)
            {
                imbrush = pImageBrush;
            }
            else if (ImageSource != null)
            {
                imbrush = new System.Windows.Media.ImageBrush(ImageSource);
            }
            // imbrush.Viewport = imbrush.Viewport = new Rect(0, 0, 16, 16);
            //dc.DrawImage(_ImageSource, rectBack);

            if (imbrush != null)
            {
                imbrush.Transform = rotRect;
                dc.DrawGeometry(imbrush, null, RectGeo);
            }


            // }
        }




        public static string TreatApostrophe(string str)
        {
            //   return str.Replace("'", "''");
            return str.Replace("'", "");

        }
        public static string TreatDoubleApostrophe(string str)
        {
            //   return str.Replace("'", "''");
            return str.Replace("\"", "");

        }


        public static double DegreesToRadians(double degrees)
        {
            return Math.PI * degrees / 180.0;
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }
        public static double GreatCircleDistance(double x1, double y1, double x2, double y2)//LatLon p1, LatLon p2)
        {
            double R = 6371000;



            double lat1 = DegreesToRadians(y1);
            double lon1 = DegreesToRadians(x1);
            double lat2 = DegreesToRadians(y2);
            double lon2 = DegreesToRadians(x2);

            if (lat1 == lat2 && lon1 == lon2)
                return 0.0;

            // "Haversine formula," taken from http://en.wikipedia.org/wiki/Great-circle_distance#Formul.C3.A6
            double a = Math.Sin((lat2 - lat1) / 2.0);
            double b = Math.Sin((lon2 - lon1) / 2.0);
            double c = a * a + +Math.Cos(lat1) * Math.Cos(lat2) * b * b;
            double distanceRadians = 2.0 * Math.Asin(Math.Sqrt(c));

            double d = R * distanceRadians;

            return d;
        }



        public static shPoint[] Convert2shPoint(List<DPoint> Points)
        {
            List<shPoint> shPoint = new List<shPoint>();
            foreach (DPoint point in Points)
            {
                shPoint.Add(new shPoint(point.X, point.Y));
            }
            return shPoint.ToArray<shPoint>(); ;
        }


        public static List<DPoint> Convert2DPoint(IList<shPoint> Points)
        {
            List<DPoint> DPoints = new List<DPoint>();
            foreach (shPoint point in Points)
            {
                DPoints.Add(new DPoint(point.x, point.y));
            }
            return DPoints;
        }

        public static void UserDrawRoutesWPF(Route Route, DrawingContext dc,System.Windows.Media.Color color)
        {
            System.Windows.Point[] m_ScreenPnts = null;
            int PixelX = 0;
            int PixelY = 0;

            if (Route == null) return;
            if (Route.Points.Count() == 0) return;



            System.Windows.Media.SolidColorBrush curBrush = new System.Windows.Media.SolidColorBrush();


            curBrush.Color = color;// System.Windows.Media.Colors.Gold;


            System.Windows.Media.Pen pen = new System.Windows.Media.Pen(curBrush, 3);


            // m_ScreenPnts = new System.Windows.Point[Route.arr_legs.Length + 1];
            m_ScreenPnts = new System.Windows.Point[Route.Points.Count()];


            //VMMainViewModel.Instance.ConvertCoordGroundToPixel(Route.arr_legs[0].FromLongn, Route.arr_legs[0].FromLatn, ref PixelX, ref PixelY);
            //m_ScreenPnts[0].X = PixelX;
            //m_ScreenPnts[0].Y = PixelY;


            for (int i = 0; i < Route.Points.Count(); i++)
            {
                //VMMainViewModel.Instance.ConvertCoordGroundToPixel(Route.arr_legs[i].ToLongn, Route.arr_legs[i].ToLatn, ref PixelX, ref PixelY);
                //m_ScreenPnts[i + 1].X = PixelX;
                //m_ScreenPnts[i + 1].Y = PixelY;

                VMMainViewModel.Instance.ConvertCoordGroundToPixel(Route.Points[i].X, Route.Points[i].Y, ref PixelX, ref PixelY);
                m_ScreenPnts[i].X = PixelX;
                m_ScreenPnts[i].Y = PixelY;
            }

            System.Windows.Media.PathGeometry PathGmtr = new System.Windows.Media.PathGeometry();
            System.Windows.Media.PathFigure pathFigure = new System.Windows.Media.PathFigure();

            System.Windows.Media.PolyLineSegment myPolyLineSegment = new System.Windows.Media.PolyLineSegment();
            System.Windows.Media.PointCollection pc = new System.Windows.Media.PointCollection(m_ScreenPnts);
            myPolyLineSegment.Points = pc;
            pathFigure.StartPoint = m_ScreenPnts[0];
            pathFigure.Segments.Add(myPolyLineSegment);
            PathGmtr.Figures.Add(pathFigure);

            dc.DrawGeometry(null, pen, PathGmtr);

        }


        public static ImageSource ImageSourceAtom(structTransportCommonProperty refTarget)
        {

            string imageKey = "";

            ImageSource imageSource = null;

            System.Windows.Size sz = new System.Windows.Size(24, 24);
            System.Windows.Size szFor = new System.Windows.Size(15, 15);

            System.Windows.Size szP = new System.Windows.Size(20, 20);

            System.Windows.Point pnt = new System.Windows.Point(0, 0);
            System.Windows.Point pntP = new System.Windows.Point(5, 5);
            System.Windows.Point pntFor = new System.Windows.Point(2, 2);


            System.Windows.Rect rectBack = new System.Windows.Rect(pnt, sz);
            System.Windows.Rect rectP = new System.Windows.Rect(pntP, szP);

            System.Windows.Rect rectFor = new System.Windows.Rect(pntFor, szFor);

            ImageSource ImageSourceDrawHand = new BitmapImage(new Uri("pack://application:,,,/Images/PanDrag.ico"));


            

          //  if (refTarget.AtomClass == "GameService.clsGroundAtom" || refTarget.AtomClass == "GameService.clsTarget" || refTarget.AtomClass == "GameService.NavalTask.clsNavalAtom")
            {
                char ch = new char();
                System.Windows.Media.SolidColorBrush curBrush = new System.Windows.Media.SolidColorBrush();


               
                curBrush.Color = System.Windows.Media.Colors.Red;
               



               // ch = (char)refTarget.FontKey;
                ch = (char)150;
                //switch (refTarget.CountryColorSide)
                //{
                //    case COLOR_SIDE.BLUE:
                //        curBrush.Color = System.Windows.Media.Colors.Blue;
                //        break;
                //    case COLOR_SIDE.RED:
                //        curBrush.Color = System.Windows.Media.Colors.Red;
                //        break;
                //    case COLOR_SIDE.GREEN:
                //        curBrush.Color = System.Windows.Media.Colors.Green;
                //        break;
                //    case COLOR_SIDE.WHITE:
                //        curBrush.Color = System.Windows.Media.Colors.Gray;
                //        break;
                //}

                //System.Windows.Media.FormattedText frm = new System.Windows.Media.FormattedText(new string(ch, 1),
                //                                     System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                //                                     System.Windows.FlowDirection.LeftToRight,
                //                                     new System.Windows.Media.Typeface("Simulation Font Environmental"),
                //                                     14, curBrush);

                System.Windows.Media.FormattedText frm = new System.Windows.Media.FormattedText(new string(ch, 1),
                                               System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                                               System.Windows.FlowDirection.LeftToRight,
                                               new System.Windows.Media.Typeface("Wingdings 2"),
                                               42, curBrush);


                frm.TextAlignment = System.Windows.TextAlignment.Center;
                // frm.SetFontWeight(System.Windows.FontWeights.Bold);

                RenderTargetBitmap bmp = new RenderTargetBitmap(36, 36, 120, 96, PixelFormats.Pbgra32);

                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();

                drawingContext.DrawText(frm, new System.Windows.Point(15, 5));
                drawingContext.DrawImage(ImageSourceDrawHand, rectP);

                drawingContext.Close();

                bmp.Render(drawingVisual);
                imageSource = bmp;


            }

            return imageSource;
        }

    }
}
