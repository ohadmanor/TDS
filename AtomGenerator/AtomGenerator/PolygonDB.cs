using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http;
using System.Net.Http.Headers;
using Npgsql;

namespace DBUtils
{
    class PolygonDB
    {
        private NpgsqlConnection connection;

        public PolygonDB(NpgsqlConnection connection)
        {
            this.connection = connection;
        }

        public void addPolygonOpeningToPolygon(PolygonOpening opening)
        {
            String query = "INSERT INTO polygon_openings(opening_guid, polygon_guid, polygon_edge_num, position_x, position_y, opening_size_meters)"
             + " VALUES (:opening_guid, :polygon_guid, :polygon_edge_num, :position_x, :position_y, :opening_size_meters)";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.Parameters.Add(new NpgsqlParameter("opening_guid", opening.openingGuid));
            command.Parameters.Add(new NpgsqlParameter("polygon_guid", opening.polygonGuid));
            command.Parameters.Add(new NpgsqlParameter("polygon_edge_num", opening.polygonEdgeNum));
            command.Parameters.Add(new NpgsqlParameter("position_x", opening.x));
            command.Parameters.Add(new NpgsqlParameter("position_y", opening.y));
            command.Parameters.Add(new NpgsqlParameter("opening_size_meters", opening.openingSize));
            command.ExecuteNonQuery();
        }

        public List<PolygonPoint> getPolygonPointsByPolygonGUID(String polygonGuid)
        {
            NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM polygon_points WHERE polygon_guid=:guid", connection);
            command.Parameters.Add(new NpgsqlParameter("guid", polygonGuid));

            NpgsqlDataReader reader = command.ExecuteReader();
            List<PolygonPoint> points = new List<PolygonPoint>();

            while (reader.Read())
            {
                String routeGuid = (String)reader[0];
                int pointNum = (int)reader[1];
                double x = (double)reader[2];
                double y = (double)reader[3];
                points.Add(new PolygonPoint(routeGuid, pointNum, x, y));
            }

            reader.Close();
            return points;
        }

        public Polygon getPolygonByName(String name)
        {
            NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM polygons WHERE polygon_name=:name", connection);
            command.Parameters.Add(new NpgsqlParameter("name", name));

            NpgsqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
            {
                reader.Close();
                return null;
            }

            Polygon polygon = new Polygon((String)reader[0], (String)reader[1]);
            reader.Close();
            return polygon;
        }

        public void addPolygonToDB(Polygon polygon) {
            String query = "INSERT INTO polygons(polygon_guid, polygon_name)"
                        + " VALUES (:polygon_guid, :polygon_name)";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.Parameters.Add(new NpgsqlParameter("polygon_guid", polygon.guid));
            command.Parameters.Add(new NpgsqlParameter("polygon_name", polygon.name));
            command.ExecuteNonQuery();
        }

        public void addPolygonPoints(Polygon polygon, List<PolygonPoint> points) {
            foreach (PolygonPoint point in points)
            {
                String query = "INSERT INTO polygon_points(polygon_guid, point_num, pointx, pointy)"
            + " VALUES (:polygon_guid, :point_num, :pointx, :pointy)";
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.Parameters.Add(new NpgsqlParameter("polygon_guid", polygon.guid));
                command.Parameters.Add(new NpgsqlParameter("point_num", point.pointNum));
                command.Parameters.Add(new NpgsqlParameter("pointx", point.x));
                command.Parameters.Add(new NpgsqlParameter("pointy", point.y));
                command.ExecuteNonQuery();
            }
        }

        public void addPolygonEscapePoint(String polygonName, int edgeNum, DPoint escapePoint)
        {
            Polygon polygon = getPolygonByName(polygonName);
            String query = "INSERT INTO polygon_openings_escape_points(polygon_guid, polygon_edge_num, route_x, route_y)"
                         + " VALUES (:polygon_guid, :polygon_edge_num, :route_x, :route_y)";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.Parameters.Add(new NpgsqlParameter("polygon_guid", polygon.guid));
            command.Parameters.Add(new NpgsqlParameter("polygon_edge_num", edgeNum));
            command.Parameters.Add(new NpgsqlParameter("route_x", escapePoint.x));
            command.Parameters.Add(new NpgsqlParameter("route_y", escapePoint.y));
            command.ExecuteNonQuery();
        }
    }
}
