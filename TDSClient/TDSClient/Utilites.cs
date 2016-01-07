using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
