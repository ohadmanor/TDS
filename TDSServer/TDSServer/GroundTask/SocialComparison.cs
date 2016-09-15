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
        private const double COMPARISON_RADIUS = 10;
        private const double FIELD_OF_VIEW_DEGREES = 120;

        public static double calculateSimilarity(clsGroundAtom me, clsGroundAtom other)
        {
            double distanceReciproc = 1 / TerrainService.MathEngine.CalcDistance(me.curr_X, me.curr_Y, other.curr_X, other.curr_Y);

            if (Util.getAzimuthDifferenceDegrees(me.currentAzimuth, other.currentAzimuth) > SAME_DIRECTION_MARGIN_DEGREES)
            {
                return 0;
            }
            if (distanceReciproc > 1) return 0;
            else return 2 + 2 * distanceReciproc;
        }

        // get the atom with the highest Sim() value to me
        public static clsGroundAtom findMostSimilar(clsGroundAtom me)
        {

            List<clsGroundAtom> comparisonAtoms = me.m_GameObject.m_GameManager.QuadTreeGroundAtom.SearchEntities(me.curr_X, me.curr_Y, COMPARISON_RADIUS, isPrecise: true);
            return findMostSimilarFromGroup(me, comparisonAtoms);
        }

        // get the atom with the highest Sim() value to me in a structure
        public static clsGroundAtom findMostSimilarInStructure(clsGroundAtom me, clsPolygon structure)
        {
            List<clsGroundAtom> comparisonAtoms = me.m_GameObject.getQuadTreeByStructure(structure).SearchEntities(me.curr_X, me.curr_Y, COMPARISON_RADIUS, isPrecise: true);
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
                if (other == me) continue;

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
    }
}
