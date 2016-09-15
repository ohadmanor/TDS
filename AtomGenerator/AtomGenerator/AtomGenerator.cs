using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace DBUtils
{
    class AtomGenerator
    {
        private NpgsqlConnection connection;

        public AtomGenerator(NpgsqlConnection connection)
        {
            this.connection = connection;
        }

        public void deleteAllAtomsAndActivities()
        {
            // delete all atoms
            String query = "TRUNCATE table atomobjects";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.ExecuteNonQuery();

            // delete all activities
            query = "TRUNCATE table activites";
            command.CommandText = query;
            command.ExecuteNonQuery();

            // delete all tree objects
            query = "TRUNCATE table treeobject";
            command.CommandText = query;
            command.ExecuteNonQuery();
        }

        public void createAtom(AtomObject atom)
        {
            // save the object to the database
            String query = "INSERT INTO atomobjects(atom_guid, atom_name, countryid, pointx, pointy) VALUES (:guid, :name, :countryId, :x, :y)";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.Parameters.Add(new NpgsqlParameter("guid", atom.guid));
            command.Parameters.Add(new NpgsqlParameter("name", atom.name));
            command.Parameters.Add(new NpgsqlParameter("countryId", atom.countryId));
            command.Parameters.Add(new NpgsqlParameter("x", atom.pointX));
            command.Parameters.Add(new NpgsqlParameter("y", atom.pointY));
            command.ExecuteNonQuery();
        }

        public void createActivityToAtom(Activity activity, AtomObject atom)
        {
            // TODO - read from sequence

            String query = "INSERT INTO activites(activityid, atom_guid, activity_seqnumber, activitytype,"
                + " startactivityoffset, durationactivity, speed, route_guid, referencepointx, referencepointy)"
                + " VALUES (:id, :atomGuid, :activitySeq, :activityType, :startOffset, :duration, :speed, :routeGuid, :refX, :refY)";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.Parameters.Add(new NpgsqlParameter("id", activity.activityId));
            command.Parameters.Add(new NpgsqlParameter("atomGuid", activity.atomGuid));
            command.Parameters.Add(new NpgsqlParameter("activitySeq", activity.activitySeqNumber));
            command.Parameters.Add(new NpgsqlParameter("activityType", activity.activityType));
            command.Parameters.Add(new NpgsqlParameter("startOffset", activity.startActivityOffset));
            command.Parameters.Add(new NpgsqlParameter("duration", activity.durationActivity));
            command.Parameters.Add(new NpgsqlParameter("speed", activity.speed));
            command.Parameters.Add(new NpgsqlParameter("routeGuid", activity.routeGuid));
            command.Parameters.Add(new NpgsqlParameter("refX", activity.refX));
            command.Parameters.Add(new NpgsqlParameter("refY", activity.refY));
            command.ExecuteNonQuery();

            // TODO - increment seuquence
        }

        public void addAtomToTreeObject(AtomObject atom)
        {
            String query = "INSERT INTO treeobject(identification, guid, parentguid, countryid, platformcategoryid, platformtype, formationtypeid)"
                         + " VALUES (:identification, :guid, :parentguid, :countryid, :platformcategoryid, :platformtype, :formationtypeid)";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.Parameters.Add(new NpgsqlParameter("identification", atom.name));
            command.Parameters.Add(new NpgsqlParameter("guid", atom.guid));
            command.Parameters.Add(new NpgsqlParameter("parentguid", ""));
            command.Parameters.Add(new NpgsqlParameter("countryid", atom.countryId));
            command.Parameters.Add(new NpgsqlParameter("platformcategoryid", 1));
            command.Parameters.Add(new NpgsqlParameter("platformtype", ""));
            command.Parameters.Add(new NpgsqlParameter("formationtypeid", 1));
            command.ExecuteNonQuery();
        }
    }
}
