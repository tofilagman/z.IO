using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace z.IO
{
    /// <summary>
    /// LJ 20150803
    /// </summary>
    public static class Geometry
    {

        /// <summary>
        /// Get Greatest Common Factor of two specified arguments
        /// </summary>
        /// <param name="num"></param>
        /// <param name="num2"></param>
        /// <returns>
        /// Ratio
        /// </returns>
        public static long GCF(long num, long num2)
        {
            List<long> kn = new List<long>();
            List<long> ln = new List<long>();

            for (long i = 1; i <= num; i++) if (num % i == 0) kn.Add(i);
            for (long i = 1; i <= num2; i++) if (num2 % i == 0) ln.Add(i);
            List<long> mn = new List<long>();

            foreach (long i in kn)
            {
                foreach (long o in ln) if (i == o) mn.Add(i);
            }

            return mn.Max();
        }

        public static long Ratio(long num, long gcf)
        {
            return (num > gcf) ? num / gcf : gcf / num;
        }

        public static long Factorial(long nominator, long denominator, long factnum)
        {
            return (factnum / nominator) * denominator;
        }

        public static float BackScale(float Actual, float Scale, float VirtualValue)
        {
            return (Actual / Scale) * VirtualValue;
        }

        public static Tuple<int, int> BackScale(float Actual, float Scale, int X, int Y)
        {
            return new Tuple<int, int>(Convert.ToInt32(BackScale(Actual, Scale, X)),
                Convert.ToInt32(BackScale(Actual, Scale, Y)));
        }
    }
}
