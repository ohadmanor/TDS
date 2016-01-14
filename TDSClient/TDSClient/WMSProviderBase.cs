using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using System.Threading.Tasks;


using System.Net.Http;
using System.Net.Http.Headers;

using System.Globalization;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.Projections;

using System.Security.Cryptography;

using TDSClient.SAGInterface;
//using ProjNet.Converters;
//using ProjNet.CoordinateSystems;
//using ProjNet.CoordinateSystems.Transformations;

namespace TDSClient
{
    public class WMSProviderBase : GMapProvider
    {
        const string UrlFormat = "{0}?service=WMS&version=1.3.1&request=GetMap&layers={1}&styles=&bbox={2},{3},{4},{5}&width={6}&height={7}&srs={8}&format={9}&transparent=true";
        //  static readonly string UrlFormat = "http://kingsspasial:8080/geoserver/wms?&VERSION=1.3.1&REQUEST=GetMap&SERVICE=WMS&LAYERS=World 250m&styles=&bbox={0},{1},{2},{3}&width={4}&height={5}&srs=CRS:84&format=image/png";

        public bool initialized;
        string name = "Undefined WMS Provider";
        string url;
        string layer;
      //  string srs = "EPSG:4326"; // = WGS84
        string srs = "EPSG:900913"; // = WGS84
        string tileFormat = "png";
        //EPSG:900913
     //   ICoordinateTransformation coordTrans;
        List<GMapProvider> overlayList = new List<GMapProvider>();

      //  readonly Guid id = new Guid("CAF1D2FB-FA91-0576-A1BB-FB43128EBCFF");
        Guid id = Guid.NewGuid();

        //public static readonly WMSProviderBase Instance;

        static WMSProviderBase()
        {
          //  Instance = new WMSProviderBase();
        }

        public  WMSProviderBase(string _id)
        {
            using (var HashProvider = new SHA1CryptoServiceProvider())
            {                
                byte[] ab= Encoding.ASCII.GetBytes(_id);

                DbId = Math.Abs(BitConverter.ToInt32(HashProvider.ComputeHash(ab), 0));
            }
        }
     


        public void Init(string url, string layer, string tileformat)
        {
            this.url = url;
            this.layer = layer;

            name = layer;

            if (string.IsNullOrEmpty(tileformat) == false) this.tileFormat = tileformat;

            //this.overlayList = new List<GMapProvider>(1);
            //if (overlays != null) this.overlayList.AddRange(overlays);
            //this.overlayList.Add(this);

            this.initialized = true;


        }


        public void Init2(string name, string url, string layer, string zoom, string bounds, string wkt, string tileformat, List<GMapProvider> overlays)
        {
            this.name = name;
            this.url = url;
            this.layer = layer;
            if (string.IsNullOrEmpty(tileformat) == false) this.tileFormat = tileformat;

            // Set zoom levels
            int minzoom, maxzoom;
            bool zoomOk = ParseZoom(zoom, out minzoom, out maxzoom);
            if (zoomOk == true && minzoom >= 0) MinZoom = minzoom;
            if (zoomOk == true && maxzoom > 0) MaxZoom = maxzoom;

            // Set area (bounds)
            float latnorth, longwest, latsouth, longeast;
            bool boundsOk = ParseBounds(bounds, out latnorth, out longwest, out latsouth, out longeast);
            if (boundsOk == true) Area = new RectLatLng(latnorth, longwest, longeast - longwest, latnorth - latsouth);

            // Set wkt + srs

            //VH
            //if (string.IsNullOrEmpty(wkt) == false)
            //{
            //    CoordinateSystemFactory csFactory = new CoordinateSystemFactory();
            //    ICoordinateSystem coordsystem = csFactory.CreateFromWkt(wkt);
            //    this.srs = coordsystem.Authority + ":" + coordsystem.AuthorityCode.ToString(NumberFormatInfo.InvariantInfo);

            //    CoordinateTransformationFactory transFactory = new CoordinateTransformationFactory();
            //    this.coordTrans = transFactory.CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, coordsystem);
            //}




            // Overlays
            this.overlayList = new List<GMapProvider>(1);
            if (overlays != null) this.overlayList.AddRange(overlays);
            this.overlayList.Add(this);

            this.initialized = true;
        }


        public static bool ParseZoom(string zoom, out int min, out int max)
        {
            // Format: min[empty]|max[empty] 
            min = -1;
            max = -1;
            if (string.IsNullOrEmpty(zoom) == true) return true;

            bool success = false;
            string[] p = zoom.Split('|');
            if (p.Length == 2)
            {
                if (p[0].Length > 0) int.TryParse(p[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out min);
                if (p[1].Length > 0) int.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out max);
                if (min >= 0 || max >= 0) success = true;
            }
            return success;
        }


        public static bool ParseBounds(string bounds, out float latnorth, out float longwest, out float latsouth, out float longeast)
        {
            // Format: LatNorth;LongWest|LatSouth;LongEast 
            latnorth = 0f;
            longwest = 0f;
            latsouth = 0f;
            longeast = 0f;
            if (string.IsNullOrEmpty(bounds) == true) return true;

            bool success = false;
            string[] p = bounds.Replace(',', '.').Split('|');
            if (p.Length == 2)
            {
                string[] p0 = p[0].Split(';');
                string[] p1 = p[1].Split(';');
                if (p0.Length == 2 && p1.Length == 2)
                {
                    bool parseOk = float.TryParse(p0[0], NumberStyles.Float, CultureInfo.InvariantCulture, out latnorth);
                    if (parseOk == true) parseOk = float.TryParse(p0[1], NumberStyles.Float, CultureInfo.InvariantCulture, out longwest);
                    if (parseOk == true) parseOk = float.TryParse(p1[0], NumberStyles.Float, CultureInfo.InvariantCulture, out latsouth);
                    if (parseOk == true) parseOk = float.TryParse(p1[1], NumberStyles.Float, CultureInfo.InvariantCulture, out longeast);
                    if (parseOk == true)
                    {
                        if (latnorth > latsouth && longeast > longwest) success = true;
                    }
                }
            }
            return success;
        }


        #region GMapProvider Members

        public override Guid Id
        {

            get { return id; }
           // get { return Guid.Empty; }
        }

        public override string Name
        {
            get { return this.name; }
        }


       public GMapProvider[] overlays;
        public override GMapProvider[] Overlays
        {
           // get { return this.overlayList.ToArray(); }
            get
            {
                if (overlays == null)
                {
                    overlays = new WMSProviderBase[] { this };
                }
                return overlays;
            }
            //set
            //{
            //    overlays = value;
            //}
        }

        public override PureProjection Projection
        {
            get { return MercatorProjection.Instance; }
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            if (this.initialized == false) return null;

            string tileUrl = MakeTileImageUrl(pos, zoom, LanguageStr);
            return GetTileImageUsingHttp(tileUrl);
        }

        #endregion



        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            if (this.initialized == false) return string.Empty;

            GPoint px1 = Projection.FromTileXYToPixel(pos);
            GPoint px2 = px1;

            px1.Offset(0, Projection.TileSize.Height);
            PointLatLng p1 = Projection.FromPixelToLatLng(px1, zoom);

            px2.Offset(Projection.TileSize.Width, 0);
            PointLatLng p2 = Projection.FromPixelToLatLng(px2, zoom);

            //if (this.coordTrans != null)
            //{
            //    double[] convp1 = this.coordTrans.MathTransform.Transform(new double[] { p1.Lat, p1.Lng });
            //    p1.Lat = Math.Round(convp1[0], 6);
            //    p1.Lng = Math.Round(convp1[1], 6);
            //    double[] convp2 = this.coordTrans.MathTransform.Transform(new double[] { p2.Lat, p2.Lng });
            //    p2.Lat = Math.Round(convp2[0], 6);
            //    p2.Lng = Math.Round(convp2[1], 6);
            //}

            //string ret = string.Format(CultureInfo.InvariantCulture, UrlFormat, this.url, this.layer,
            //    p1.Lng.ToString(NumberFormatInfo.InvariantInfo), p1.Lat.ToString(NumberFormatInfo.InvariantInfo),
            //    p2.Lng.ToString(NumberFormatInfo.InvariantInfo), p2.Lat.ToString(NumberFormatInfo.InvariantInfo),
            //    Projection.TileSize.Width.ToString(NumberFormatInfo.InvariantInfo), Projection.TileSize.Height.ToString(NumberFormatInfo.InvariantInfo),
            //    this.srs, "image/" + this.tileFormat);


          //  var ret = string.Format(CultureInfo.InvariantCulture, UrlFormat, p1.Lng, p1.Lat, p2.Lng, p2.Lat, Projection.TileSize.Width, Projection.TileSize.Height);

            var ret = string.Format(CultureInfo.InvariantCulture, UrlFormat, this.url, this.layer, p1.Lng, p1.Lat, p2.Lng, p2.Lat, Projection.TileSize.Width, Projection.TileSize.Height, "CRS:84", "image/png");
           // ret = string.Format(CultureInfo.InvariantCulture, UrlFormat, this.url, this.layer, p1.Lng, p1.Lat, p2.Lng, p2.Lat, Projection.TileSize.Width, Projection.TileSize.Height, "EPSG:3857", "image/png");

            return ret;
        }


        public async static Task<WMSCapabilities> WMSCapabilitiesRetrieve(string WMSGeoserverUrl)
        {
           WMSCapabilities Capabilities = new WMSCapabilities();
           Capabilities.Layers = new List<CustomImageInfo>();

           if (string.IsNullOrEmpty(WMSGeoserverUrl))
           {
                return null;
           }
           string result = string.Empty;
           string BaseUrlFormat = "{0}://{1}:{2}/";


           using( var client = new HttpClient())
           {
               client.Timeout = new TimeSpan(0, 0, 15);

               HttpResponseMessage response = null;
               try
               {
                   Uri u = new Uri(WMSGeoserverUrl);
                   string BaseUrl = string.Format(CultureInfo.InvariantCulture, BaseUrlFormat, u.Scheme, u.Host, u.Port);
                   string ReqUrl = u.PathAndQuery + "?request=getCapabilities";

                   client.BaseAddress = new Uri(BaseUrl);
                   client.DefaultRequestHeaders.Accept.Clear();
                   client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                   response = await client.GetAsync(ReqUrl);
                   if (response.IsSuccessStatusCode == false) return null;

                   using (HttpContent content = response.Content)
                   {
                       result = await content.ReadAsStringAsync();
                   }                

                   XmlDocument doc = new XmlDocument();
                   doc.LoadXml(result);
                   XmlNodeList CapabilitiesNodes = doc.GetElementsByTagName("WMS_Capabilities");
                   XmlNode CapabilitiesNode = CapabilitiesNodes[0];
                   Capabilities.Version = CapabilitiesNode.Attributes["version"].Value;

                   XmlNodeList Layers = doc.GetElementsByTagName("Layer");
                   foreach (XmlNode layer in Layers)
                   {

                       foreach (XmlNode ch in layer.ChildNodes)
                       {
                           if (ch.Name == "Layer")
                           {
                               CustomImageInfo ImageInfo = new CustomImageInfo();
                               foreach (XmlNode l in ch.ChildNodes)
                               {
                                   if (l.Name == "Name")
                                   {
                                       ImageInfo.MapName = l.InnerText;
                                   }
                                   if (l.Name == "EX_GeographicBoundingBox")
                                   {
                                       foreach (XmlNode b in l.ChildNodes)
                                       {
                                           if (b.Name == "westBoundLongitude")
                                           {
                                               double.TryParse(b.InnerText, out ImageInfo.MinX);
                                           }
                                           else if (b.Name == "eastBoundLongitude")
                                           {
                                               double.TryParse(b.InnerText, out ImageInfo.MaxX);
                                           }
                                           else if (b.Name == "southBoundLatitude")
                                           {
                                               double.TryParse(b.InnerText, out ImageInfo.MinY);
                                           }
                                           else if (b.Name == "northBoundLatitude")
                                           {
                                               double.TryParse(b.InnerText, out ImageInfo.MaxY);
                                           }


                                       }

                                   }
                               }
                               Capabilities.Layers.Add(ImageInfo);

                           }
                       }
                   }


                   return Capabilities;
               }

               catch (Exception ex)
               {
                   Capabilities.Error = ex;
                   return Capabilities;
               }   
            
  
           }

           




        }

        

      
    }
}
