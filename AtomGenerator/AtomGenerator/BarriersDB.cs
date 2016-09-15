using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace DBUtils
{
    class BarriersDB
    {
        private NpgsqlConnection connection;

        public BarriersDB(NpgsqlConnection connection)
        {
            this.connection = connection;
        }

        public void addBarrier(Barrier barrier)
        {
            String query = "INSERT INTO barriers(guid, x, y, angle)"
             + " VALUES (:guid, :x, :y, :angle)";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.Parameters.Add(new NpgsqlParameter("guid", barrier.guid));
            command.Parameters.Add(new NpgsqlParameter("x", barrier.x));
            command.Parameters.Add(new NpgsqlParameter("y", barrier.y));
            command.Parameters.Add(new NpgsqlParameter("angle", barrier.angle));
            command.ExecuteNonQuery();
        }

        public List<Barrier> readAllBarriers()
        {
            NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM barriers", connection);
            NpgsqlDataReader reader = command.ExecuteReader();

            List<Barrier> barriers = new List<Barrier>();

            // read the routes themselves
            while (reader.Read())
            {
                String guid = (reader[0] == DBNull.Value) ? null : (String)reader[0];
                double x = (double)reader[1];
                double y = (double)reader[2];
                double angle = (double)reader[3];
                Barrier barrier = new Barrier(guid, x, y, angle);

                barriers.Add(barrier);
            }

            reader.Close();

            return barriers;
        }

        public void addBarriers(Barrier[] barriers)
        {
            foreach (Barrier barrier in barriers)
            {
                addBarrier(barrier);
            }
        }
    }
}
