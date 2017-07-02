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

        int simpleByteSingleGPU()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteSingleGPU()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharSingleGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharSingleGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntSingleGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntSingleGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntSingleGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntSingleGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongSingleGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongSingleGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleSingleGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleSingleGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatSingleGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatSingleGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        // -------------------------------------------------------------
        // -------------------------------------------------------------

        int simpleByteMultiGPU()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteMultiGPU()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharMultiGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharMultiGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntMultiGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntMultiGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntMultiGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntMultiGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongMultiGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongMultiGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleMultiGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleMultiGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatMultiGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatMultiGPU()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        // ***************************************************************************
        // ****************************************************************************
        // *****************************************************************************
        // ******************************************************************************
        // *********************************************************************************
        // ***********************************************************************************
        // *********************************************************************************

        int simpleByteSingleGPUEventPipeline()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteSingleGPUEventPipeline()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharSingleGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharSingleGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntSingleGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntSingleGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntSingleGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntSingleGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongSingleGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongSingleGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleSingleGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleSingleGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatSingleGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatSingleGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        // -------------------------------------------------------------
        // -------------------------------------------------------------

        int simpleByteMultiGPUEventPipeline()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteMultiGPUEventPipeline()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharMultiGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharMultiGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntMultiGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntMultiGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntMultiGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntMultiGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongMultiGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongMultiGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleMultiGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleMultiGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatMultiGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatMultiGPUEventPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleByteSingleGPUDriverPipeline()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteSingleGPUDriverPipeline()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharSingleGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharSingleGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntSingleGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntSingleGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntSingleGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntSingleGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongSingleGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongSingleGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleSingleGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleSingleGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatSingleGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatSingleGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        // -------------------------------------------------------------
        // -------------------------------------------------------------

        int simpleByteMultiGPUDriverPipeline()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteMultiGPUDriverPipeline()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharMultiGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharMultiGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntMultiGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntMultiGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntMultiGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntMultiGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongMultiGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongMultiGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleMultiGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleMultiGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatMultiGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatMultiGPUDriverPipeline()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        //  +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        int simpleByteSingleGPU2Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteSingleGPU2Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharSingleGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharSingleGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntSingleGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntSingleGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntSingleGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntSingleGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongSingleGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongSingleGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleSingleGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleSingleGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatSingleGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatSingleGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        // -------------------------------------------------------------
        // -------------------------------------------------------------

        int simpleByteMultiGPU2Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteMultiGPU2Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharMultiGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharMultiGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntMultiGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntMultiGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntMultiGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntMultiGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongMultiGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongMultiGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleMultiGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleMultiGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatMultiGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatMultiGPU2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        // ***************************************************************************
        // ****************************************************************************
        // *****************************************************************************
        // ******************************************************************************
        // *********************************************************************************
        // ***********************************************************************************
        // *********************************************************************************

        int simpleByteSingleGPUEventPipeline2Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteSingleGPUEventPipeline2Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharSingleGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharSingleGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntSingleGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntSingleGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntSingleGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntSingleGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongSingleGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongSingleGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleSingleGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleSingleGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatSingleGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatSingleGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        // -------------------------------------------------------------
        // -------------------------------------------------------------

        int simpleByteMultiGPUEventPipeline2Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteMultiGPUEventPipeline2Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharMultiGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharMultiGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntMultiGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntMultiGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntMultiGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntMultiGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongMultiGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongMultiGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleMultiGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleMultiGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatMultiGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatMultiGPUEventPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleByteSingleGPUDriverPipeline2Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteSingleGPUDriverPipeline2Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharSingleGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharSingleGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntSingleGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntSingleGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntSingleGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntSingleGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongSingleGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongSingleGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleSingleGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleSingleGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatSingleGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatSingleGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        // -------------------------------------------------------------
        // -------------------------------------------------------------

        int simpleByteMultiGPUDriverPipeline2Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteMultiGPUDriverPipeline2Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharMultiGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharMultiGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntMultiGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntMultiGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntMultiGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntMultiGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongMultiGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongMultiGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleMultiGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleMultiGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatMultiGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatMultiGPUDriverPipeline2Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        // 3 kernels 
        // 3 kernels


        //  +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        int simpleByteSingleGPU3Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteSingleGPU3Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharSingleGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharSingleGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntSingleGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntSingleGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntSingleGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntSingleGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongSingleGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongSingleGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleSingleGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleSingleGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatSingleGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatSingleGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        // -------------------------------------------------------------
        // -------------------------------------------------------------

        int simpleByteMultiGPU3Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteMultiGPU3Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharMultiGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharMultiGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntMultiGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntMultiGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntMultiGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntMultiGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongMultiGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongMultiGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleMultiGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleMultiGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatMultiGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatMultiGPU3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        // ***************************************************************************
        // ****************************************************************************
        // *****************************************************************************
        // ******************************************************************************
        // *********************************************************************************
        // ***********************************************************************************
        // *********************************************************************************

        int simpleByteSingleGPUEventPipeline3Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteSingleGPUEventPipeline3Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharSingleGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharSingleGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntSingleGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntSingleGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntSingleGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntSingleGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongSingleGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongSingleGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleSingleGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleSingleGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatSingleGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatSingleGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        // -------------------------------------------------------------
        // -------------------------------------------------------------

        int simpleByteMultiGPUEventPipeline3Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteMultiGPUEventPipeline3Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharMultiGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharMultiGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntMultiGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntMultiGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntMultiGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntMultiGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongMultiGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongMultiGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleMultiGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleMultiGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatMultiGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatMultiGPUEventPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleByteSingleGPUDriverPipeline3Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteSingleGPUDriverPipeline3Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharSingleGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharSingleGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntSingleGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntSingleGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntSingleGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntSingleGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongSingleGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongSingleGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleSingleGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleSingleGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatSingleGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatSingleGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus()[0], @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        // -------------------------------------------------------------
        // -------------------------------------------------------------

        int simpleByteMultiGPUDriverPipeline3Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            byte[] tData = new byte[1024];
            byte[] tData2 = new byte[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastByteMultiGPUDriverPipeline3Kernels()
        {
            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClByteArray tData = new ClByteArray(1024);
            ClByteArray tData2 = new ClByteArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 55; tData2[i] = 0; }
            ClArray<byte> data = tData;
            ClArray<byte> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ byte array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int simpleCharMultiGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            char[] tData = new char[1024];
            char[] tData2 = new char[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastCharMultiGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uchar * data,__global uchar * data2)
                { 
                    data2[get_global_id(0)*2]=data[get_global_id(0)*2];  
                    data2[get_global_id(0)*2+1]=data[get_global_id(0)*2+1];  
                }");
            ClCharArray tData = new ClCharArray(1024);
            ClCharArray tData2 = new ClCharArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 'a'; tData2[i] = 'b'; }
            ClArray<char> data = tData;
            ClArray<char> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ char array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleIntMultiGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            int[] tData = new int[1024];
            int[] tData2 = new int[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastIntMultiGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global int * data,__global int * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClIntArray tData = new ClIntArray(1024);
            ClIntArray tData2 = new ClIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = -55; tData2[i] = 3; }
            ClArray<int> data = tData;
            ClArray<int> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ int array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }



        int simpleUIntMultiGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            uint[] tData = new uint[1024];
            uint[] tData2 = new uint[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastUIntMultiGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global uint * data,__global uint * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClUIntArray tData = new ClUIntArray(1024);
            ClUIntArray tData2 = new ClUIntArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024u * 1024u * 1024u * 3u; tData2[i] = 3; }
            ClArray<uint> data = tData;
            ClArray<uint> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ uint array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleLongMultiGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            long[] tData = new long[1024];
            long[] tData2 = new long[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C# long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastLongMultiGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global long * data,__global long * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClLongArray tData = new ClLongArray(1024);
            ClLongArray tData2 = new ClLongArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 1024l * 1024l * 1024l * 1024l; tData2[i] = -3; }
            ClArray<long> data = tData;
            ClArray<long> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if (tData[i] != tData2[i])
                {
                    Console.WriteLine("Error(C++ long array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleDoubleMultiGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            double[] tData = new double[1024];
            double[] tData2 = new double[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int fastDoubleMultiGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global double * data,__global double * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClDoubleArray tData = new ClDoubleArray(1024);
            ClDoubleArray tData2 = new ClDoubleArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001d; tData2[i] = -3d; }
            ClArray<double> data = tData;
            ClArray<double> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ double array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }

        int simpleFloatMultiGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            float[] tData = new float[1024];
            float[] tData2 = new float[1024];
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C# float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        int fastFloatMultiGPUDriverPipeline3Kernels()
        {

            ClNumberCruncher cr = new ClNumberCruncher(ClPlatforms.all().gpus(), @"
                __kernel void test(__global float * data,__global float * data2)
                { 
                    data2[get_global_id(0)]=data[get_global_id(0)];  
                }");
            ClFloatArray tData = new ClFloatArray(1024);
            ClFloatArray tData2 = new ClFloatArray(1024);
            for (int i = 0; i < 1024; i++)
            { tData[i] = 0.001f; tData2[i] = -3f; }
            ClArray<float> data = tData;
            ClArray<float> data2 = tData2;
            data.nextParam(data2).compute(cr, 1, "test test test", 1024, 64, 0, true, false);
            for (int i = 0; i < 1024; i++)
                if ((tData[i] > tData2[i] + 0.00001d) || (tData[i] < tData2[i] - 0.00001d))
                {
                    Console.WriteLine("Error(C++ float array): data[" + i + "]=" + tData[i] + " data2[" + i + "]=" + tData2[i] + " ");
                    GC.Collect();
                    return 1;
                }
            GC.Collect();
            return 0;
        }


        void testTypesWithFeatures()
        {
                // todo: single kernel 2 kernel 3 kernel version --> x3
                // stream version --> x2

                int passedTests = 0; int totalTests = 0;
                passedTests += (1 - simpleByteSingleGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharSingleGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntSingleGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntSingleGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongSingleGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleSingleGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatSingleGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteSingleGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharSingleGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntSingleGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntSingleGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongSingleGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleSingleGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatSingleGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);


                passedTests += (1 - simpleByteMultiGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharMultiGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntMultiGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntMultiGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongMultiGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleMultiGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatMultiGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteMultiGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharMultiGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntMultiGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntMultiGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongMultiGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleMultiGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatMultiGPU()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);




                passedTests += (1 - simpleByteSingleGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharSingleGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntSingleGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntSingleGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongSingleGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleSingleGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatSingleGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteSingleGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharSingleGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntSingleGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntSingleGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongSingleGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleSingleGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatSingleGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);


                passedTests += (1 - simpleByteMultiGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharMultiGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntMultiGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntMultiGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongMultiGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleMultiGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatMultiGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteMultiGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharMultiGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntMultiGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntMultiGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongMultiGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleMultiGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatMultiGPUEventPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);




                passedTests += (1 - simpleByteSingleGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharSingleGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntSingleGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntSingleGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongSingleGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleSingleGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatSingleGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteSingleGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharSingleGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntSingleGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntSingleGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongSingleGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleSingleGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatSingleGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);


                passedTests += (1 - simpleByteMultiGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharMultiGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntMultiGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntMultiGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongMultiGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleMultiGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatMultiGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteMultiGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharMultiGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntMultiGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntMultiGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongMultiGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleMultiGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatMultiGPUDriverPipeline()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);


                // 2 kernels

                passedTests += (1 - simpleByteSingleGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharSingleGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntSingleGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntSingleGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongSingleGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleSingleGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatSingleGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteSingleGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharSingleGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntSingleGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntSingleGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongSingleGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleSingleGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatSingleGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);


                passedTests += (1 - simpleByteMultiGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharMultiGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntMultiGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntMultiGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongMultiGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleMultiGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatMultiGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteMultiGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharMultiGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntMultiGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntMultiGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongMultiGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleMultiGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatMultiGPU2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);




                passedTests += (1 - simpleByteSingleGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharSingleGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntSingleGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntSingleGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongSingleGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleSingleGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatSingleGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteSingleGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharSingleGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntSingleGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntSingleGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongSingleGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleSingleGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatSingleGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);


                passedTests += (1 - simpleByteMultiGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharMultiGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntMultiGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntMultiGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongMultiGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleMultiGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatMultiGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteMultiGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharMultiGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntMultiGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntMultiGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongMultiGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleMultiGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatMultiGPUEventPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);




                passedTests += (1 - simpleByteSingleGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharSingleGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntSingleGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntSingleGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongSingleGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleSingleGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatSingleGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteSingleGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharSingleGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntSingleGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntSingleGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongSingleGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleSingleGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatSingleGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);


                passedTests += (1 - simpleByteMultiGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharMultiGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntMultiGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntMultiGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongMultiGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleMultiGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatMultiGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteMultiGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharMultiGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntMultiGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntMultiGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongMultiGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleMultiGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatMultiGPUDriverPipeline2Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);


                // 3 kernels

                passedTests += (1 - simpleByteSingleGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharSingleGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntSingleGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntSingleGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongSingleGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleSingleGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatSingleGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteSingleGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharSingleGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntSingleGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntSingleGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongSingleGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleSingleGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatSingleGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);


                passedTests += (1 - simpleByteMultiGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharMultiGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntMultiGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntMultiGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongMultiGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleMultiGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatMultiGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteMultiGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharMultiGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntMultiGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntMultiGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongMultiGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleMultiGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatMultiGPU3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);




                passedTests += (1 - simpleByteSingleGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharSingleGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntSingleGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntSingleGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongSingleGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleSingleGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatSingleGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteSingleGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharSingleGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntSingleGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntSingleGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongSingleGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleSingleGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatSingleGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);


                passedTests += (1 - simpleByteMultiGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharMultiGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntMultiGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntMultiGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongMultiGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleMultiGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatMultiGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteMultiGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharMultiGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntMultiGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntMultiGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongMultiGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleMultiGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatMultiGPUEventPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);




                passedTests += (1 - simpleByteSingleGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharSingleGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntSingleGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntSingleGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongSingleGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleSingleGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatSingleGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteSingleGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharSingleGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntSingleGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntSingleGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongSingleGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleSingleGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatSingleGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);


                passedTests += (1 - simpleByteMultiGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleCharMultiGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleIntMultiGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleUIntMultiGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleLongMultiGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleDoubleMultiGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - simpleFloatMultiGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastByteMultiGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastCharMultiGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastIntMultiGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastUIntMultiGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastLongMultiGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastDoubleMultiGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);
                passedTests += (1 - fastFloatMultiGPUDriverPipeline3Kernels()); totalTests++; Console.WriteLine("passed tests: " + passedTests + "/" + totalTests);

            }




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
                cruncher = new ClNumberCruncher(benchDevices, kernelString);

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
                }", new string[] { "vectorAdd" },false);

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
