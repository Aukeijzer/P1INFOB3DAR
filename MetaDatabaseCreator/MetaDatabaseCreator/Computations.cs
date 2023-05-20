using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaDatabaseCreator
{
    /// <summary>
    /// Class that performs computations for the metadatabase
    /// Computes certain values such as IDF, QF and Jacquard Coefficient
    /// </summary>
    internal class Computations
    {

        /// <summary>
        /// Calculates IDF
        /// </summary>
        /// <param name="tuples"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public double IDF(int tuples, int frequency)
        {
            return Math.Log(tuples / frequency);
        }

        /// <summary>
        /// Calculates QF
        /// </summary>
        /// <param name="rqfValue"></param>
        /// <param name="rqfMax"></param>
        /// <returns></returns>
        public double QF(int rqfValue, int rqfMax)
        {
            return rqfValue / rqfMax;
        }

        /// <summary>
        /// Calculates Jacquard Coefficient
        /// </summary>
        /// <param name="query1"></param>
        /// <param name="query2"></param>
        /// <returns></returns>
        public float Jacquard(List<string> value1, List<string> value2)
        {
            List<string> queryUnion = (List<string>)value1.Union(value2);
            List<string> queryIntersect = (List<string>)value1.Intersect(value2);

            return queryIntersect.Count / queryUnion.Count;
        }
    }
}
