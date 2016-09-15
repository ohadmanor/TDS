using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Npgsql;

namespace TDSServer.TDS.DAL
{
    public class PolygonsDB
    {
        static string strPostGISConnection = string.Empty;
        static PolygonsDB()
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            strPostGISConnection = appSettings["PostGISConnection"];
        }


        public static IEnumerable<TerrainService.shPoint> getPolygonPoints(string polygon_guid)
        {
            List<TerrainService.shPoint> pntsList = new System.Collections.Generic.List<TerrainService.shPoint>();
            string sql = "select * from polygon_points where polygon_guid='" + polygon_guid + "' order by point_num";

            using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, connection))
            {
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                ds.Reset();
                da.Fill(ds);
                dt = ds.Tables[0];

                if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
                {
                    return null;
                }

                foreach (DataRow row in dt.Rows)
                {

                    TerrainService.shPoint mp = new TerrainService.shPoint();
                    mp.x = System.Convert.ToDouble(row["pointx"]);
                    mp.y = System.Convert.ToDouble(row["pointy"]);
                    pntsList.Add(mp);
                }
            }


            return pntsList;
        }

        private static clsPolygon GetPolygonBySql(string sql)
        {
            try
            {
                clsPolygon polygon = new clsPolygon();

                using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, connection))
                {
                    DataSet dsPol = new DataSet();
                    DataTable dtPol = new DataTable();
                    dsPol.Reset();
                    da.Fill(dsPol);
                    dtPol = dsPol.Tables[0];

                    if (dtPol == null || dtPol.Rows == null || dtPol.Rows.Count == 0)
                    {
                        return null;
                    }

                    foreach (DataRow rowPol in dtPol.Rows)
                    {
                        polygon.PolygonName = rowPol["polygon_name"].ToString();
                        polygon.PolygonGuid = rowPol["polygon_guid"].ToString();                  


                    }
                }

                polygon.Points = getPolygonPoints(polygon.PolygonGuid);
				//YD:
				// get the polygon's openings
                polygon.PolygonOpenings = getPolygonOpeningsByPolygon(polygon);
				
				// get the escape points for each opening
                List<clsPolygonOpeningEscapePoint> escapePoints = getPolygonEscapePoints(polygon);
                polygon.EscapePoints = new Dictionary<int, clsPolygonOpeningEscapePoint>();
                foreach (clsPolygonOpeningEscapePoint escapePoint in escapePoints)
                {
                    polygon.EscapePoints.Add(escapePoint.polygonEdgeNum, escapePoint);
                }
				// ---

                return polygon;
            }
            catch (Exception ex)
            {

            }
            return null;
        }


        public static clsPolygon GetPolygonByName(string Name)
        {
            string sql = "select * from Polygons where polygon_name='" + Name + "'";
            clsPolygon polygon = GetPolygonBySql(sql);
            return polygon;
        }
		
		// YD:
		// get polygon openings from database by sql
        private static List<clsPolygonOpening> GetPolygonOpeningsBySql(string sql)
        {
            try
            {
                List<clsPolygonOpening> polygonOpenings = new List<clsPolygonOpening>();

                using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, connection))
                {
                    DataSet dsPol = new DataSet();
                    DataTable dtPol = new DataTable();
                    dsPol.Reset();
                    da.Fill(dsPol);
                    dtPol = dsPol.Tables[0];

                    if (dtPol == null || dtPol.Rows == null || dtPol.Rows.Count == 0)
                    {
                        return null;
                    }

                    foreach (DataRow rowPol in dtPol.Rows)
                    {
                        clsPolygonOpening polygonOpening = new clsPolygonOpening();
                        polygonOpening.PolygonGUID = rowPol["polygon_guid"].ToString();
                        polygonOpening.PolygonEdgeNum = (int)rowPol["polygon_edge_num"];
                        polygonOpening.x = Convert.ToDouble(rowPol["position_x"]);
                        polygonOpening.y = Convert.ToDouble(rowPol["position_y"]);
                        polygonOpening.openingSize = Convert.ToDouble(rowPol["opening_size_meters"]);
                        polygonOpenings.Add(polygonOpening);
                    }
                }

                return polygonOpenings;
            }
            catch (Exception ex)
            {

            }
            return null;
        }
		
		// get all polygon openings from database for a specific polygon
        public static List<clsPolygonOpening> getPolygonOpeningsByPolygon(clsPolygon polygon) {
            string sql = "select * from polygon_openings where polygon_guid='" + polygon.PolygonGuid + "'";
            List<clsPolygonOpening> openings = GetPolygonOpeningsBySql(sql);
            return openings;
        }
		
		// get polygon escape points by sql
        public static List<clsPolygonOpeningEscapePoint> getPolygonEscapePointsBySql(String sql)
        {
            try
            {
                List<clsPolygonOpeningEscapePoint> escapePoints = new List<clsPolygonOpeningEscapePoint>();

                using (NpgsqlConnection connection = new NpgsqlConnection(strPostGISConnection))
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, connection))
                {
                    DataSet dsPol = new DataSet();
                    DataTable dtPol = new DataTable();
                    dsPol.Reset();
                    da.Fill(dsPol);
                    dtPol = dsPol.Tables[0];

                    if (dtPol == null || dtPol.Rows == null || dtPol.Rows.Count == 0)
                    {
                        return null;
                    }

                    foreach (DataRow rowPol in dtPol.Rows)
                    {
                        clsPolygonOpeningEscapePoint escapePoint = new clsPolygonOpeningEscapePoint();
                        escapePoint.PolygonGUID = rowPol["polygon_guid"].ToString();
                        escapePoint.polygonEdgeNum = (int)rowPol["polygon_edge_num"];
                        escapePoint.x = Convert.ToDouble(rowPol["route_x"]);
                        escapePoint.y = Convert.ToDouble(rowPol["route_y"]);
                        escapePoints.Add(escapePoint);
                    }
                }

                return escapePoints;
            }
            catch (Exception ex)
            {

            }
            return null;
        }
		
		// get all polygon escape points for a specific polygon
        public static List<clsPolygonOpeningEscapePoint> getPolygonEscapePoints(clsPolygon polygon)
        {
            String sql = "select * from polygon_openings_escape_points where polygon_guid='" + polygon.PolygonGuid + "'";
            List<clsPolygonOpeningEscapePoint> escapePoints = getPolygonEscapePointsBySql(sql);
            return escapePoints;
        }
		
		// get a polygon by guid
        public static clsPolygon getPolygonByGuid(String guid)
        {
            string sql = "select * from Polygons where polygon_guid='" + guid + "'";
            clsPolygon polygon = GetPolygonBySql(sql);
            return polygon;
        }
		
		// ---
    }
}
