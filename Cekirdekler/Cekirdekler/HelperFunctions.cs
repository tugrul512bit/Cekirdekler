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
using Cekirdekler;
using Cekirdekler.ClArrays; 
namespace ClObject
{
    /// <summary>
    /// helper functions
    /// </summary>
    internal class Functions
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
        /// shift each layer 1 step older, put new values to newest, oldest layer is lost
        /// </summary>
        public static void performanceHistoryShiftOld(double[][] history, double[] newPerformance)
        {
            for (int i = 0; i < history.Length - 1; i++)
            {
                for (int j = 0; j < history[i].Length; j++)
                {
                    history[i][j] = history[i + 1][j];
                }
            }
            for (int j = 0; j < history[history.Length - 1].Length; j++)
            {
                history[history.Length - 1][j] = newPerformance[j];
            }
        }

        /// <summary>
        /// simple smoothing for load balance
        /// </summary>
        /// <param name="hist"></param>
        /// <returns></returns>
        public static double[] performanceHistoryAverage(double[][] hist)
        {
            double[] result = new double[hist[0].Length];
            for(int i=0;i<hist.Length;i++)
            {
                for (int j = 0; j < hist[i].Length; j++)
                {
                    result[j] += hist[i][j];
                }

            }
            double div = 1.0f / ((double)hist.Length);
            for (int i = 0; i < result.Length; i++)
            {
                result[i] *= div;
            }
            return result;
        }

        /// <summary>
        /// quicker convergence
        /// </summary>
        /// <param name="hist"></param>
        /// <returns></returns>
        public static double[] performanceHistoryPID(double[][] hist)
        {
            double[] result = null;
            return result;
        }

        /// <summary>
        /// finding change rate, knowing whats next before it happens
        /// </summary>
        /// <param name="hist"></param>
        /// <returns></returns>
        public static double[] performanceHistoryDerivative5pStencil(double [][] hist)
        {
            double[] result = null;
            return result;
        }


        /// <summary>
        /// distribute workitems to devices accordingly with their weighted throughput
        /// </summary>
        /// <param name="benchmark">timings</param>
        /// <param name="smooth">smoothing on off</param>
        /// <param name="throughputHistory">smoothing data against OS interrupts and other effects</param>
        /// <param name="totalRange">global range of kernel</param>
        /// <param name="globalRanges">global range per device</param>
        /// <param name="step">minimum exchange rate of workitems between devices to balance the load</param>
        public static void loadBalance(double[] benchmark,bool smooth,double[][]throughputHistory, int totalRange, int[] globalRanges, int step)
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


            // evade divide by zero
            if (totalThroughput <= 0.0000001)
                totalThroughput = 0.01;



            // normalize shares to [0,1]
            double[] tmpNormalizedThroughputs = new double[benchmark.Length];

            if (smooth)
            {
                for (int i = 0; i < benchmark.Length; i++)
                {
                    tmpNormalizedThroughputs[i] = tmpThroughputs[i] / totalThroughput;
                }

                // push from newest, oldest pops
                performanceHistoryShiftOld(throughputHistory, tmpNormalizedThroughputs);

                // if "average" option is chosen
                tmpNormalizedThroughputs = performanceHistoryAverage(throughputHistory);
            }

            for (int i = 0; i < benchmark.Length; i++)
            {
                // if load balancer smoothing is on
                double normalizedThrougput = (smooth&& throughputHistory[0][0]>0.00001) ? tmpNormalizedThroughputs[i] : (tmpThroughputs[i] / totalThroughput);

                if (globalRanges[i] != 0)
                {
                    int tmp0 = globalRanges[i];

                    
                    int tmp1 =globalRanges[i] - (int)((globalRanges[i]- (totalRange * normalizedThrougput ))*0.3);
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
