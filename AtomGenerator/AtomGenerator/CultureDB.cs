using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace DBUtils
{
    class CultureDB
    {
        private NpgsqlConnection connection;

        public CultureDB(NpgsqlConnection connection)
        {
            this.connection = connection;
        }

        public void addCultureData(CultureData data)
        {
            String query = "INSERT INTO cultural_data(guid, age, gender, country, formation_type, formation_males,"
                                + "formation_females, personal_space, social_space, public_space, avoidance_side_left_probability, speed)"
             + " VALUES (:guid, :age, :gender, :country, :formation_type, :formation_males, :formation_females, :personal_space, :social_space, "
             + ":public_space, :avoidance_side_left_probability, :speed)";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.Parameters.Add(new NpgsqlParameter("guid", data.guid));
            command.Parameters.Add(new NpgsqlParameter("age", data.age));
            command.Parameters.Add(new NpgsqlParameter("gender", data.gender));
            command.Parameters.Add(new NpgsqlParameter("country", data.country));
            command.Parameters.Add(new NpgsqlParameter("formation_type", data.formationType));
            command.Parameters.Add(new NpgsqlParameter("formation_males", data.formationMales));
            command.Parameters.Add(new NpgsqlParameter("formation_females", data.formationFemales));
            command.Parameters.Add(new NpgsqlParameter("personal_space", data.personalSpace));
            command.Parameters.Add(new NpgsqlParameter("social_space", data.socialSpace));
            command.Parameters.Add(new NpgsqlParameter("public_space", data.publicSpace));
            command.Parameters.Add(new NpgsqlParameter("avoidance_side_left_probability", data.avoidanceSideLeftProb));
            command.Parameters.Add(new NpgsqlParameter("speed", data.speed));
            command.ExecuteNonQuery();


        }

        public void addCultureGenderBiasData(CultureGenderBiasData data)
        {
            String query = "INSERT INTO cultural_gender_bias(guid, country, bias)"
                          + " VALUES (:guid, :country, :bias)";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.Parameters.Add(new NpgsqlParameter("guid", Guid.NewGuid()));
            command.Parameters.Add(new NpgsqlParameter("country", data.country));
            command.Parameters.Add(new NpgsqlParameter("bias", data.bias));
            command.ExecuteNonQuery();
        }

        public void removeAllCultureData()
        {
            NpgsqlCommand deleteCulturalDataCommand = new NpgsqlCommand("TRUNCATE TABLE cultural_data", connection);
            deleteCulturalDataCommand.ExecuteNonQuery();

        }

        public void removeAllCultureGenderBias()
        {
            NpgsqlCommand deleteCulturalBiasCommand = new NpgsqlCommand("TRUNCATE TABLE cultural_gender_bias", connection);
            deleteCulturalBiasCommand.ExecuteNonQuery();
        }
    }
}
