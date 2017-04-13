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
using Cekirdekler.ClArrays;
using Cekirdekler.Hardware;

namespace Cekirdekler
{
    /// <summary>
    /// for testing all features in different PCs 
    /// </summary>
    public class Tester
    {
        
        /// <summary>
        /// tests byte arrays
        /// </summary>
        /// <returns>1=error,0=ok</returns>
        public static int byteArrayOperations()
        {
            ClArray<byte> bytes = new ClArray<byte>(1024);
            ClArray<byte> bytes2 = new ClByteArray(1024);
            ClArray<byte> bytes3 = new byte[1024];
            byte[] bytes4 = new byte[1024];
            byte[] bytes5 = new byte[1024];
            byte[] bytes6 = new byte[1024];
            for(int i=0;i<1024;i++)
            {
                bytes[i] =(byte) i;
                bytes2[i] =(byte) i;
                bytes3[i] =(byte) i;
                bytes4[i] =(byte) i;
                bytes5[i] =(byte)(1024- i);
            }
            int acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += bytes4[i];
            int trueVal= acc;

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += bytes3[i];
            if(acc!=trueVal)
            {
                Console.WriteLine("Error in byte array for C# wrapper");
                return 1;
            }

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += bytes2[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in byte array for implicit ClArray(C++) wrapper");
                return 1;
            }

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += bytes[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in byte array for ClArray(C++) wrapper");
                return 1;
            }



            bytes.CopyFrom(bytes5, 0);
            bytes.CopyTo(bytes6,0);
            for (int i = 0; i < 1024; i++)
            {
                if(bytes5[i]!=bytes6[i])
                {
                    Console.WriteLine("Error in byte array copy");
                    return 1;
                }
                bytes6[i] = (byte)(35 * i);    
            }
            bytes.fastArr = false;
            bytes.CopyFrom(bytes6, 0);
            bytes.CopyTo(bytes5, 0);
            for (int i = 0; i < 1024; i++)
            {
                if (bytes5[i] != bytes6[i])
                {
                    Console.WriteLine("Error in byte array copy");
                    return 1;
                }
            }
            Console.WriteLine("ClArray<byte>: ok");
            bytes.dispose();
            bytes2.dispose();
            bytes3.dispose();
            return 0;
        }

        /// <summary>
        /// tests (C# char) arrays
        /// </summary>
        /// <returns>1=error,0=ok</returns>
        public static int charArrayOperations()
        {
            ClArray<char> chars = new ClArray<char>(1024);
            ClArray<char> chars2 = new ClCharArray(1024);
            ClArray<char> chars3 = new char[1024];
            char[] chars4 = new char[1024];
            char[] chars5 = new char[1024];
            char[] chars6 = new char[1024];
            for (int i = 0; i < 1024; i++)
            {
                chars[i] =  (char)i;
                chars2[i] = (char)i;
                chars3[i] = (char)i;
                chars4[i] = (char)i;
                chars5[i] = (char)(1024 - i);
            }
            int acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += chars4[i];
            int trueVal = acc;

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += chars3[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in char array for C# wrapper");
                return 1;
            }

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += chars2[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in char array for implicit ClArray(C++) wrapper");
                return 1;
            }

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += chars[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in char array for ClArray(C++) wrapper");
                return 1;
            }

            chars.CopyFrom(chars5, 0);
            chars.CopyTo(chars6, 0);
            for (int i = 0; i < 1024; i++)
            {
                if (chars5[i] != chars6[i])
                {
                    Console.WriteLine("Error in char array copy");
                    return 1;
                }
                chars6[i] = (char)(35 * i);
            }
            chars.fastArr = false;
            chars.CopyFrom(chars6, 0);
            chars.CopyTo(chars5, 0);
            for (int i = 0; i < 1024; i++)
            {
                if (chars5[i] != chars6[i])
                {
                    Console.WriteLine("Error in char array copy");
                    return 1;
                }
            }

            chars.dispose();
            chars2.dispose();
            chars3.dispose();
            Console.WriteLine("ClArray<char>: ok");
            return 0;
        }

        /// <summary>
        /// tests int arrays
        /// </summary>
        /// <returns>1=error,0=ok</returns>
        public static int intArrayOperations()
        {
            ClArray<int> ints = new ClArray<int>(1024);
            ClArray<int> ints2 = new ClIntArray(1024);
            ClArray<int> ints3 = new int[1024];
            int[] ints4 = new int[1024];
            int[] ints5 = new int[1024];
            int[] ints6 = new int[1024];
            for (int i = 0; i < 1024; i++)
            {
                ints[i] =  i;
                ints2[i] = i;
                ints3[i] = i;
                ints4[i] = i;
                ints5[i] = (1024 - i);
            }
            int acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += ints4[i];
            int trueVal = acc;

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += ints3[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in int array for C# wrapper");
                return 1;
            }

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += ints2[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in int array for implicit ClArray(C++) wrapper");
                return 1;
            }

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += ints[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in int array for ClArray(C++) wrapper");
                return 1;
            }


            ints.CopyFrom(ints5, 0);
            ints.CopyTo(ints6, 0);
            for (int i = 0; i < 1024; i++)
            {
                if (ints5[i] != ints6[i])
                {
                    Console.WriteLine("Error in int array copy");
                    return 1;
                }
                ints6[i] = (char)(35 * i);
            }
            ints.fastArr = false;
            ints.CopyFrom(ints6, 0);
            ints.CopyTo(ints5, 0);
            for (int i = 0; i < 1024; i++)
            {
                if (ints5[i] != ints6[i])
                {
                    Console.WriteLine("Error in int array copy");
                    return 1;
                }
            }

            ints.dispose();
            ints2.dispose();
            ints3.dispose();

            Console.WriteLine("ClArray<int>: ok");
            return 0;
        }

        /// <summary>
        /// tests uint arrays
        /// </summary>
        /// <returns>1=error,0=ok</returns>
        public static int uintArrayOperations()
        {
            ClArray<uint> uints = new ClArray<uint>(1024);
            ClArray<uint> uints2 = new ClUIntArray(1024);
            ClArray<uint> uints3 = new uint[1024];
            uint[] uints4 = new uint[1024];
            uint[] uints5 = new uint[1024];
            uint[] uints6 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            {
                uints[i]  =(uint) i;
                uints2[i] = (uint)i;
                uints3[i] = (uint)i;
                uints4[i] = (uint)i;
                uints5[i] = (uint)(1024 - i);
            }
            int acc = 0;
            for (int i = 0; i < 1024; i++)
                acc +=(int) uints4[i];
            int trueVal = acc;

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += (int)uints3[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in uint array for C# wrapper");
                return 1;
            }

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += (int)uints2[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in uint array for implicit ClArray(C++) wrapper");
                return 1;
            }

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc +=(int) uints[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in uint array for ClArray(C++) wrapper");
                return 1;
            }

            uints.CopyFrom(uints5, 0);
            uints.CopyTo(uints6, 0);
            for (int i = 0; i < 1024; i++)
            {
                if (uints5[i] != uints6[i])
                {
                    Console.WriteLine("Error in uint array copy");
                    return 1;
                }
                uints6[i] = (char)(35 * i);
            }
            uints.fastArr = false;
            uints.CopyFrom(uints6, 0);
            uints.CopyTo(uints5, 0);
            for (int i = 0; i < 1024; i++)
            {
                if (uints5[i] != uints6[i])
                {
                    Console.WriteLine("Error in uint array copy");
                    return 1;
                }
            }


            uints.dispose();
            uints2.dispose();
            uints3.dispose();

            Console.WriteLine("ClArray<uint>: ok");
            return 0;
        }


        /// <summary>
        /// tests float arrays
        /// </summary>
        /// <returns>1=error,0=ok</returns>
        public static int floatArrayOperations()
        {
            ClArray<float> floats = new ClArray<float>(1024);
            ClArray<float> floats2 = new ClFloatArray(1024);
            ClArray<float> floats3 = new float[1024];
            float[] floats4 = new float[1024];
            float[] floats5 = new float[1024];
            float[] floats6 = new float[1024];
            for (int i = 0; i < 1024; i++)
            {
                floats[i] = i;
                floats2[i] = i;
                floats3[i] = i;
                floats4[i] = i;
                floats5[i] = 1024-i;
            }
            int acc = 0;
            for (int i = 0; i < 1024; i++)
                acc +=(int) floats4[i];
            int trueVal = acc;

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc +=(int) floats3[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in float array for C# wrapper");
                return 1;
            }

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += (int)floats2[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in float array for implicit ClArray(C++) wrapper");
                return 1;
            }

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += (int)floats[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in float array for ClArray(C++) wrapper");
                return 1;
            }



            floats.CopyFrom(floats5, 0);
            floats.CopyTo(  floats6, 0);
            for (int i = 0; i < 1024; i++)
            {
                if (floats5[i] != floats6[i])
                {
                    Console.WriteLine("Error in float array copy");
                    return 1;
                }
                floats6[i] = (char)(35 * i);
            }
            floats.fastArr = false;
            floats.CopyFrom(floats6, 0);
            floats.CopyTo(floats5, 0);
            for (int i = 0; i < 1024; i++)
            {
                if (floats5[i] != floats6[i])
                {
                    Console.WriteLine("Error in float array copy");
                    return 1;
                }
            }

            floats.dispose();
            floats2.dispose();
            floats3.dispose();

            Console.WriteLine("ClArray<float>: ok");
            return 0;
        }

        /// <summary>
        /// tests double arrays
        /// </summary>
        /// <returns>1=error,0=ok</returns>
        public static int doubleArrayOperations()
        {
            ClArray<double> doubles = new ClArray<double>(1024);
            ClArray<double> doubles2 = new ClDoubleArray(1024);
            ClArray<double> doubles3 = new double[1024];
            double[] doubles4 = new double[1024];
            double[] doubles5 = new double[1024];
            double[] doubles6 = new double[1024];
            for (int i = 0; i < 1024; i++)
            {
                doubles[i] = i;
                doubles2[i] = i;
                doubles3[i] = i;
                doubles4[i] = i;
                doubles5[i] = 1024-i;
            }
            int acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += (int)doubles4[i];
            int trueVal = acc;

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += (int)doubles3[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in double array for C# wrapper");
                return 1;
            }

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += (int)doubles2[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in double array for implicit ClArray(C++) wrapper");
                return 1;
            }

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += (int)doubles[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in double array for ClArray(C++) wrapper");
                return 1;
            }

            
            doubles.CopyFrom(doubles5, 0);
            doubles.CopyTo(doubles6, 0);
            for (int i = 0; i < 1024; i++)
            {
                if (doubles5[i] != doubles6[i])
                {
                    Console.WriteLine("Error in double array copy");
                    return 1;
                }
                doubles6[i] = (char)(35 * i);
            }
            doubles.fastArr = false;
            doubles.CopyFrom(doubles6, 0);
            doubles.CopyTo(doubles5, 0);
            for (int i = 0; i < 1024; i++)
            {
                if (doubles5[i] != doubles6[i])
                {
                    Console.WriteLine("Error in double array copy");
                    return 1;
                }
            }
            doubles.dispose();
            doubles2.dispose();
            doubles3.dispose();

            Console.WriteLine("ClArray<double>: ok");
            return 0;
        }

        /// <summary>
        /// tests long arrays
        /// </summary>
        /// <returns>1=error,0=ok</returns>
        public static int longArrayOperations()
        {
            ClArray<long> longs = new ClArray<long>(1024);
            ClArray<long> longs2 = new ClLongArray(1024);
            ClArray<long> longs3 = new long[1024];
            long[] longs4 = new long[1024];
            long[] longs5 = new long[1024];
            long[] longs6 = new long[1024];
            for (int i = 0; i < 1024; i++)
            {
                longs[i] = i;
                longs2[i] = i;
                longs3[i] = i;
                longs4[i] = i;
                longs5[i] = 1024-i;
            }
            int acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += (int)longs4[i];
            int trueVal = acc;

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += (int)longs3[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in long array for C# wrapper");
                return 1;
            }

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += (int)longs2[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in long array for implicit ClArray(C++) wrapper");
                return 1;
            }

            acc = 0;
            for (int i = 0; i < 1024; i++)
                acc += (int)longs[i];
            if (acc != trueVal)
            {
                Console.WriteLine("Error in long array for ClArray(C++) wrapper");
                return 1;
            }


            longs.CopyFrom(longs5, 0);
            longs.CopyTo(longs6, 0);
            for (int i = 0; i < 1024; i++)
            {
                if (longs5[i] != longs6[i])
                {
                    Console.WriteLine("Error in long array copy");
                    return 1;
                }
                longs6[i] = (char)(35 * i);
            }
            longs.fastArr = false;
            longs.CopyFrom(longs6, 0);
            longs.CopyTo(longs5, 0);
            for (int i = 0; i < 1024; i++)
            {
                if (longs5[i] != longs6[i])
                {
                    Console.WriteLine("Error in long array copy");
                    return 1;
                }
            }
            longs.dispose();
            longs2.dispose();
            longs3.dispose();

            Console.WriteLine("ClArray<long>: ok");
            return 0;
        }

        /// <summary>
        /// tests host side buffer operations
        /// </summary>
        public static void buffers()
        {
            int err = 0;
            err+=byteArrayOperations();
            err+=charArrayOperations();
            err+=intArrayOperations();
            err+=uintArrayOperations();
            err+=floatArrayOperations();
            err+=doubleArrayOperations();
            err+=longArrayOperations();
            if(err==0)
                Console.WriteLine("buffers on host: ok");
        }

        /// <summary>
        /// compares an nbody all-pairs(bruteforce) implementation for a C# vs opencl (2D-forces within (+/-)0.01f)
        /// </summary>
        /// <param name="n">optional number of particles</param>
        /// <param name="benchDevices">optional benchmark devices</param>
        /// <param name="streamEnabled">use devices as streaming processors or with dedicated memories</param>
        /// <param name="consoleLog">logs intermediate benchmark information to console</param>
        /// <returns></returns>
        public static int nBody(int n = 8 * 1024, ClDevices benchDevices = null, bool streamEnabled = false,bool consoleLog=true)
        {
            int err = 0;
            float[] x = new float[n];
            float[] y = new float[n];

            Random r_ = new Random();
            for (int i = 0; i < n; i++)
            {
                x[i] = (float)(r_.NextDouble()*30.0f - 15.0f);
                y[i] = (float)(r_.NextDouble()*30.0f - 15.0f);
            }


            float[] fx = new float[n];
            float[] fxCl = new float[n];
            float[] fy = new float[n];
            float[] fyCl = new float[n];

            // host nbody start
            for(int i=0;i<n;i++)
            {
                float fx0 = 0;
                float fy0 = 0;
                for (int j = 0; j < n; j++)
                {
                    float dx = x[i] - x[j];
                    float dy = y[i] - y[j];
                    float r = (float)Math.Sqrt(dx*dx+dy*dy+0.0001f);
                    fx0 += (dx * (1.0f / (r * r * r)));
                    fy0 += (dx * (1.0f / (r * r * r)));
                }
                fx[i] = fx0;
                fy[i] = fy0;
            }
            // host nbody end

            // opencl nbody start
            // Cekirdekler API usage type - 1 
            ClArray<float> xCl = x;
            ClArray<float> yCl = y;
            ClArray<float> fxCl_ = fxCl;
            ClArray<float> fyCl_ = fyCl;
            ClNumberCruncher cruncher = null;
            string kernelString = @"
                __kernel void nBody(__global float * x,__global float * y,__global float * fx,__global float * fy)
                {
                    int i=get_global_id(0);    
                    float fx0 = 0;
                    float fy0 = 0;
                    for (int j = 0; j < " + n + @"; j++)
                    {
                        float dx = x[i] - x[j];
                        float dy = y[i] - y[j];
                        float r = (float)sqrt(dx*dx+dy*dy+0.0001f);
                        fx0 += (dx * (1.0f / (r * r * r)));
                        fy0 += (dx * (1.0f / (r * r * r)));
                    }
                    fx[i] = fx0;
                    fy[i] = fy0;
                }
                ";
            if(benchDevices==null)
                cruncher = new ClNumberCruncher(AcceleratorType.CPU | AcceleratorType.GPU,kernelString , -1, -1, streamEnabled);
            else
                cruncher = new ClNumberCruncher(benchDevices, kernelString, streamEnabled);

            if (cruncher.numberCruncher.errorCode()!=0)
            {
                Console.WriteLine(cruncher.numberCruncher.errorMessage());
                Console.WriteLine("nbody error.");
                cruncher.dispose();
                return 1;
            }

            cruncher.performanceFeed = consoleLog;

            for (int i = 0; i < 150; i++)
            {
                if (i % 15 == 0)
                    Console.WriteLine("Test progress:%"+(100.0f*((float)i)/150.0f));

                xCl.nextParam(yCl, fxCl_, fyCl_).compute(cruncher, 1, "nBody", n, 64);
            }
            // opencl nbody end

            for(int i=0;i<n;i++)
            {
                if(consoleLog)
                if((i%1000)==0)
                {
                    Console.WriteLine("Nbody force: fx["+i+"]=" + fx[i] + " fxCl["+i+"]=" + fxCl[i]);
                    Console.WriteLine("             fy["+i+"]=" + fy[i] + " fyCl["+i+"]=" + fyCl[i]);
                }

                if ((fx[i]>fxCl[i]+0.01f) || (fx[i] < fxCl[i]- 0.01f) || (fy[i] > fyCl[i] + 0.01f) || (fy[i] < fyCl[i] - 0.01f))
                {

                    Console.WriteLine("Nbody error. fx["+i+"]=" + fx[i]+" fxCl["+i+"]="+fxCl[i]);
                    Console.WriteLine("             fy["+i+"]=" + fy[i]+" fyCl["+i+"]="+fyCl[i]);
                    cruncher.dispose();
                    xCl.dispose();
                    yCl.dispose();
                    fxCl_.dispose();
                    fyCl_.dispose();
                    return 1;
                }
            }
            if(consoleLog)
            Console.WriteLine("releasing nbody resources");
            cruncher.dispose();
            xCl.dispose();
            yCl.dispose();
            fxCl_.dispose();
            fyCl_.dispose();
            Console.WriteLine("nbody: ok");
            return err;
        }

        /// <summary>
        /// pipelined + streamed data performance test.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int stream_C_equals_A_plus_B_1M_elements(int n=1024*1024)
        {
            int err = 0;
            ClFloatArray arrayA = new ClFloatArray(1024 * 1024);
            float[] arrayB = new float[1024 * 1024];
            ClArray<float> arrayC = new ClArray<float>(1024 * 1024);
            for(int i=0;i<1024*1024;i++)
            {
                arrayA[i] = 3;
                arrayB[i] = i;
                arrayC[i] = 105;
            }
            // Cekirdekler API usage type - 2
            Cekirdekler.Cores gpuAndCpu = new Cores("cpu gpu", @"
                __kernel void vectorAdd(__global float*a,__global float*b,__global float*c)
                {
                    int i=get_global_id(0);
                    c[i]=a[i]+b[i];
                }", new string[] { "vectorAdd" });

            for (int i = 0; i < 10; i++)
            {
                gpuAndCpu.compute(new string[] { "vectorAdd" }, 0, "", new object[] { arrayA, arrayB, arrayC },
                    new string[] { "partial read", "partial read", "write" }, new int[] { 1, 1, 1 }, n, 1, 0, true,
                    8, Cekirdekler.Cores.PIPELINE_DRIVER, 256);
                gpuAndCpu.performanceReport(1);
            }
            if((arrayC[700]>(arrayA[700]+arrayB[700]-0.5f))&& (arrayC[700] < (arrayA[700] + arrayB[700] + 0.5f)))
                Console.WriteLine("Streaming c[i]=a[i]+b[i]: ok");
            else
            {
                Console.WriteLine("Error in streaming c[i]=a[i]+b[i]: i=700: C="+arrayC[700]+",  A+B="+(arrayA[700]+arrayB[700]));
            }
            arrayC.dispose();
            arrayA.dispose();
            gpuAndCpu.dispose();
            return err;
        }
    }
}
