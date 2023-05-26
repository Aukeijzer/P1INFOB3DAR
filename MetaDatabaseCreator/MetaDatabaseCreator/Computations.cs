using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaDatabaseCreator
{
    /// <summary>
    /// Class that performs computations for the metadatabase
    /// Computes certain values such as IDF, QF and Jaccard Coefficient
    /// </summary>
    public class Computations
    {

        /// <summary>
        /// Calculates IDF for categorical Data
        /// </summary>
        /// <param name="tuples"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public double IDFCategorical(double tuples, double frequency)
        {
            return Math.Log(tuples / frequency);
        }

        public double IDFNumerical(double tuples, double value, List<double> AttributeValues)
        {
            double standardDev = StandardDeviation(AttributeValues);
            double bandwidth = CalculateBandwidth(standardDev, tuples);

            double denominator = 0;
            foreach(double values in AttributeValues)
            {
                denominator += Math.Pow(Math.E, -0.5 * Math.Pow(((values - value) / bandwidth), 2));
            }

            return Math.Log(tuples / denominator);
        }

        public static double StandardDeviation(List<double> values)
        {
            double average = values.Average();
            return Math.Sqrt(values.Average(x => Math.Pow(x - average, 2)));
        }

        public double CalculateBandwidth(double StandardDeviation, double valueTotal)
        {
            return 1.06 * StandardDeviation * Math.Pow(valueTotal, -1 / 5);
        }

        /// <summary>
        /// Calculates QF
        /// If RQFMAX is 0, then return 0
        /// It means that the value has never been queried in the workload
        /// </summary>
        /// <param name="rqfValue"></param>
        /// <param name="rqfMax"></param>
        /// <returns></returns>
        public double QF(double rqfValue, double rqfMax)
        {
            if (rqfMax == 0) 
            { 
                return 0; 
            }
            return rqfValue / rqfMax;
        }

        /// <summary>
        /// Calculates Jaccard Coefficient
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public float Jaccard(List<string> value1, List<string> value2)
        {
            List<string> queryUnion = (List<string>)value1.Union(value2);
            List<string> queryIntersect = (List<string>)value1.Intersect(value2);

            return queryIntersect.Count / queryUnion.Count;
        }
    }
}
