using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDSServer.GroundTask
{
    public class SocialComparison
    {
        private const double SAME_DIRECTION_MARGIN_DEGREES = 90;
        private const double SMIN = 1;
        private const double SMAX = 5;
        private const double GENDER_BIAS_WEIGHT = 2;
        private const double DISTANCE_WEIGHT = 2;
        private const double FIELD_OF_VIEW_DEGREES = 120;

        public static double calculateSimilarity(clsGroundAtom me, clsGroundAtom other)
        {
            // first factor: distance - the closer an entity is, the more similar.
            double distanceReciproc = 1 / TerrainService.MathEngine.CalcDistance(me.curr_X, me.curr_Y, other.curr_X, other.curr_Y);

            // second factor: difference in heading.  If it is heading towards me it is similar, if it is not heading towards me, it is not.
            // important: We chose a binary threshold for this yet we think it needs to be soft
            if (Util.getAzimuthDifferenceDegrees(me.currentAzimuth, other.currentAzimuth) > SAME_DIRECTION_MARGIN_DEGREES)
            {
                return 0;
            }
			
			// sim value for gender
            double genderSimilarity = (me.gender == other.gender ? 1 : 0) * me.genderBiasFactor;

            if (distanceReciproc > 1) return 0;
            else return 2 + DISTANCE_WEIGHT * distanceReciproc + GENDER_BIAS_WEIGHT*genderSimilarity;
        }

        // get the atom with the highest Sim() value to me
        public static clsGroundAtom findMostSimilarByDistanceAndAzimuth(clsGroundAtom me)
        {
            // find most similar only in this agent's public space
            List<clsGroundAtom> comparisonAtoms = me.m_GameObject.m_GameManager.QuadTreeGroundAtom.SearchEntities(me.curr_X, me.curr_Y, me.proxemics.publicSpace, isPrecise: true);
            return findMostSimilarFromGroup(me, comparisonAtoms);
        }

        // get the atom with the highest Sim() value to me in a structure
        public static clsGroundAtom findMostSimilarInStructure(clsGroundAtom me, clsPolygon structure)
        {
            // find most similar only in this agent's public space
            List<clsGroundAtom> comparisonAtoms = me.m_GameObject.getQuadTreeByStructure(structure).SearchEntities(me.curr_X, me.curr_Y, me.proxemics.publicSpace, isPrecise: true);
            return findMostSimilarFromGroup(me, comparisonAtoms);
        }

        // get the atom with the highest Sim() value
        public static clsGroundAtom findMostSimilarFromGroup(clsGroundAtom me, List<clsGroundAtom> others)
        {
            if (others == null || others.Count == 0) return null;

            double maxSimilarityValue = Double.MinValue;
            clsGroundAtom mostSimilarAtom = null;

            foreach (clsGroundAtom other in others)
            {
                // why compare to myself? why on earth compare myself to someone who can't move or is dead?
                if (other == me || other.healthStatus.isDead || other.healthStatus.isIncapacitated) continue;

                double similarity = calculateSimilarity(me, other);
                if (similarity >= maxSimilarityValue && similarity > SMIN && similarity < SMAX)
                {
                    maxSimilarityValue = similarity;
                    mostSimilarAtom = other;
                }
            }

            return mostSimilarAtom;
        }

        public static void setOffsetTowardsMostSimilar(clsGroundAtom me, clsGroundAtom mostSimilar)
        {
            // set offset towards most similar atom
            if (mostSimilar.Offset_Distance - me.Offset_Distance > 0)
            {
                me.Offset_Distance += clsGroundAtom.OFFSET_IN_COLLISION;
            }
            else if (mostSimilar.Offset_Distance - me.Offset_Distance < 0)
            {
                me.Offset_Distance -= clsGroundAtom.OFFSET_IN_COLLISION;
            }
            else
            {
                // if most similar has the same offset like me choose a side randomly
                if (Util.random.NextDouble() > 0.5)
                {
                    me.Offset_Distance += clsGroundAtom.OFFSET_IN_COLLISION;
                }
                else
                {
                    me.Offset_Distance -= clsGroundAtom.OFFSET_IN_COLLISION;
                }
            }
        }

        public static void correctBehaviorToMostSimilar(clsGroundAtom me, clsGroundAtom mostSimilar, double baselineSpeed)
        {
            // for now correct speed and azimuth towards most similar
            double speedDifference = me.currentSpeed - mostSimilar.currentSpeed;
            double distance = TerrainService.MathEngine.CalcDistance(me.curr_X, me.curr_Y, mostSimilar.curr_X, mostSimilar.curr_Y);

            double minDistance = me.getSocialDistance(); // minimal distance based on proxemics
            double maxDistance = 5; // above 5 meters is far enough to have the same drive to minimize distance

            // gains per second of simulation
            double speedGain = 1 + 0.1*(Math.Min(5, distance) - minDistance) * (double)me.m_GameObject.m_GameManager.GroundCycleResolution/1000f;

            me.currentSpeed = speedGain * mostSimilar.currentSpeed;

            // don't go too fast in relation to the speed of who you compare to, you're not Usain Bolt
            me.currentSpeed = Math.Min(me.currentSpeed, Math.Min(2 * baselineSpeed, 2 * mostSimilar.currentSpeed));

        }
    }
}
