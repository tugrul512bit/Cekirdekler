//    Cekirdekler API: a C# explicit multi-device load-balancer opencl wrapper
//    Copyright(C) 2017 Hüseyin Tuğrul BÜYÜKIŞIK

//   This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.If not, see<http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cekirdekler
{
    /// <summary>
    /// helper functions
    /// </summary>
    public class Functions
    {
        /// <summary>
        /// checks if object is derived from FastArr or not
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool isTypeOfFastArr(object o)
        {
            if (o.GetType() == typeof(float[]) ||
                o.GetType() == typeof(double[]) ||
                o.GetType() == typeof(long[]) ||
                o.GetType() == typeof(int[]) ||
                o.GetType() == typeof(uint[]) ||
                o.GetType() == typeof(char[]) ||
                o.GetType() == typeof(byte[]))
                return false;
            else
                return (o.GetType().BaseType.GetGenericTypeDefinition() == typeof(FastArr<>));
        }

        static int maxIndex(int[] arr)
        {
            int result_ = 0;
            int value_ = -100000000;
            for (int i = 0; i < arr.Length; i++)
            {
                if (value_ < arr[i])
                {
                    value_ = arr[i];
                    result_ = i;
                }
            }
            return result_;
        }
        static int minIndex(int[] arr)
        {
            int result_ = 0;
            double value_ = 1800000000;
            for (int i = 0; i < arr.Length; i++)
            {
                if (value_ > arr[i])
                {
                    value_ = arr[i];
                    result_ = i;
                }
            }
            return result_;
        }
        private static int[] tmpGlobalRanges;
        private static double[] tmpThroughputs;

        /// <summary>
        /// distribute workitems to devices accordingly with their weighted throughput
        /// </summary>
        /// <param name="benchmark">timings</param>
        /// <param name="totalRange">global range of kernel</param>
        /// <param name="globalRanges">global range per device</param>
        /// <param name="step">minimum exchange rate of workitems between devices to balance the load</param>
        public static void loadBalance(double[] benchmark, int totalRange, int[] globalRanges, int step)
        {

            if (tmpGlobalRanges == null)
            {
                tmpGlobalRanges = new int[globalRanges.Length];
                tmpThroughputs = new double[globalRanges.Length];
            }
            double totalBenchmark = benchmark.Sum() + 0.01 * globalRanges.Length;
            double totalThroughput = 0;
            for (int i = 0; i < benchmark.Length; i++)
            {
                tmpThroughputs[i] = ((totalBenchmark / (benchmark[i] + 0.01))) * (globalRanges[i] + 1);
                totalThroughput += tmpThroughputs[i];
                tmpGlobalRanges[i] = 0;
            }


            for (int i = 0; i < benchmark.Length; i++)
            {
                if (globalRanges[i] != 0)
                {
                    int tmp0 = globalRanges[i];

                    int tmp1 =globalRanges[i] - (int)((globalRanges[i]- (totalRange * tmpThroughputs[i] / totalThroughput))*0.3);
                    tmpGlobalRanges[i] = tmp1;
     
                }
                else
                {
                    int tmp0 = globalRanges[i];

                    int tmp1 = (int)((((totalRange * (totalBenchmark / (benchmark[i] + 0.01)) / totalThroughput))));
                    tmpGlobalRanges[i] = tmp1;

                }

            }

            int remains = 0;
            for (int i = 0; i < benchmark.Length; i++)
            {
                int remains0 = (tmpGlobalRanges[i]) % step;
                if (remains0 < step / 2)
                    globalRanges[i] = tmpGlobalRanges[i] - remains0;
                else
                    globalRanges[i] = tmpGlobalRanges[i] + (step - remains0);
            }

            while (globalRanges.Sum() > totalRange)
            {
                globalRanges[maxIndex(globalRanges)] -= step;
            }

            while (globalRanges.Sum() < totalRange)
            {
                globalRanges[maxIndex(globalRanges)] += step;
            }
        }
    }
}
