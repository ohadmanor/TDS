using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainService;

namespace DBUtils
{
    class Activity
    {
        public int activityId;
        public String atomGuid;
        public int activitySeqNumber;
        public int activityType;
        public String startActivityOffset;
        public String durationActivity;
        public int speed;
        public String routeGuid;
        public double refX;
        public double refY;

        public Activity(int activityId, String atomGuid, int activitySeqNumber, int activityType, String startActivityOffset, String durationActivity,
                        int speed, String routeGuid, double refX, double refY)
        {
            this.activityId = activityId;
            this.atomGuid = atomGuid;
            this.activitySeqNumber = activitySeqNumber;
            this.activityType = activityType;
            this.startActivityOffset = startActivityOffset;
            this.durationActivity = durationActivity;
            this.speed = speed;
            this.routeGuid = routeGuid;
            this.refX = refX;
            this.refY = refY;
        }

    }

    class AtomObject
    {
        public AtomObject(String name, int countryId, double pointX, double pointY)
        {
            guid = Util.CreateGuid();
            this.name = name;
            this.countryId = countryId;
            this.pointX = pointX;
            this.pointY = pointY;
        }

        public String guid;
        public String name;
        public int countryId;
        public double pointX;
        public double pointY;
    }

    class Route
    {
        public String guid;
        public String name;
        public int countryId;
        public int routeTypeId;
        public String owner;
        public List<DPoint> routePoints;
    }

    class DPoint
    {
        public double x;
        public double y;

        public DPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    class PolygonOpening
    {
        public String openingGuid;
        public String polygonGuid;
        public int polygonEdgeNum;
        public double x;
        public double y;
        public double openingSize;

        public PolygonOpening(String polygonGuid, int polygonEdgeNum, double x, double y, double openingSize)
        {
            this.openingGuid = Util.CreateGuid();
            this.polygonGuid = polygonGuid;
            this.polygonEdgeNum = polygonEdgeNum;
            this.x = x;
            this.y = y;
            this.openingSize = openingSize;
        }
    }

    class PolygonPoint
    {
        public String polygonGuid;
        public int pointNum;
        public double x;
        public double y;

        public PolygonPoint(String polygonGuid, int pointNum, double x, double y)
        {
            this.polygonGuid = polygonGuid;
            this.pointNum = pointNum;
            this.x = x;
            this.y = y;
        }
    }

    class Polygon
    {
        public String guid;
        public String name;

        public Polygon(String guid, String name)
        {
            this.guid = guid;
            this.name = name;
        }
    }

    class Barrier
    {
        public String guid;
        public double x;
        public double y;
        public double angle;

        public Barrier(String guid, double x, double y, double angle)
        {
            this.guid = guid;
            this.x = x;
            this.y = y;
            this.angle = angle;
        }
    }

    class CultureData
    {
        public String guid;

        // age, gender and country
        public int age;
        public String gender;
        public String country;

        // to what formation is this culture data relevant to? individual? groups?
        public String formationType;

        // if it is in a group, how many males and females are in that group?
        public int formationMales;
        public int formationFemales;

        // proxemics
        public double personalSpace;
        public double socialSpace;
        public double publicSpace;

        // collision avoidance side - probability to move left in case of a collision
        public double avoidanceSideLeftProb;

        // walking speed
        public double speed;

        public CultureData(String guid, int age, String gender, String country, String formationType, int formationMales, int formationFemales,
                            double personalSpace, double socialSpace, double publicSpace, double avoidanceSideLeftProb, double speed) {
            this.guid = guid;
            this.age = age;
            this.gender = gender;
            this.country = country;
            this.formationType = formationType;
            this.formationMales = formationMales;
            this.formationFemales = formationFemales;
            this.personalSpace = personalSpace;
            this.socialSpace = socialSpace;
            this.publicSpace = publicSpace;
            this.avoidanceSideLeftProb = avoidanceSideLeftProb;
            this.speed = speed;
        }
    }

    // some cultures have gender bias. This may cause agents to depend more on agents of the same gender
    class CultureGenderBiasData
    {
        public String guid;
        public String country;

        // the bias factor - the higher it is, the more the agent tends to compare itself to agents of the same gender
        public double bias;

        public CultureGenderBiasData(String guid, String country, double bias)
        {
            this.guid = guid;
            this.country = country;
            this.bias = bias;
        }
    }

    public class shPath
    {
        public IList<shPoint> Points = new List<shPoint>();
    }
}
