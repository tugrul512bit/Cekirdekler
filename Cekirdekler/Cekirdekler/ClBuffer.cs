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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Cekirdekler;
using Cekirdekler.ClArrays;
namespace ClObject
{
    /// <summary>
    /// wrapper for cl::buffer and read-write-map-unmap functions
    /// </summary>
    internal class ClBuffer
    {
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr createBuffer(IntPtr hContext, int numberOfElements, 
            int clDeviceType,int isCSharpArray,
            IntPtr arrayPointer, bool GDDR_BUFFER);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBuffer(IntPtr hCommandQueue, IntPtr hBuffer, float[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBuffer(IntPtr hCommandQueue, IntPtr hBuffer,IntPtr hAArr);

        

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferEvent(IntPtr hCommandQueue, IntPtr hBuffer, float[] obj, IntPtr hEventArr, IntPtr hEvt);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferEvent(IntPtr hCommandQueue, IntPtr hBuffer, IntPtr hAArr, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBuffer(IntPtr hCommandQueue, IntPtr hBuffer, float[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBuffer(IntPtr hCommandQueue, IntPtr hBuffer, IntPtr hAArr);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferEvent(IntPtr hCommandQueue, IntPtr hBuffer, float[] obj, IntPtr hEventArr, IntPtr hEvt);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferEvent(IntPtr hCommandQueue, IntPtr hBuffer, IntPtr hAArr, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferRanged(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, float[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferRanged(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, IntPtr hAArr);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferRangedEvent(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, float[] obj, IntPtr hEventArr, IntPtr hEvt);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferRangedEvent(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, IntPtr hAArr, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferRanged(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, float[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferRanged(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, IntPtr hAArr);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferRangedEvent(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, float[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferRangedEvent(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, IntPtr hAArr, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBuffer(IntPtr hCommandQueue, IntPtr hBuffer, int[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferEvent(IntPtr hCommandQueue, IntPtr hBuffer, int[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBuffer(IntPtr hCommandQueue, IntPtr hBuffer, int[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferEvent(IntPtr hCommandQueue, IntPtr hBuffer, int[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferRanged(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, int[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferRangedEvent(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, int[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferRanged(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, int[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferRangedEvent(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, int[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBuffer(IntPtr hCommandQueue, IntPtr hBuffer, uint[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferEvent(IntPtr hCommandQueue, IntPtr hBuffer, uint[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBuffer(IntPtr hCommandQueue, IntPtr hBuffer, uint[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferEvent(IntPtr hCommandQueue, IntPtr hBuffer, uint[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferRanged(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, uint[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferRangedEvent(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, uint[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferRanged(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, uint[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferRangedEvent(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, uint[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBuffer(IntPtr hCommandQueue, IntPtr hBuffer, long[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferEvent(IntPtr hCommandQueue, IntPtr hBuffer, long[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBuffer(IntPtr hCommandQueue, IntPtr hBuffer, long[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferEvent(IntPtr hCommandQueue, IntPtr hBuffer, long[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferRanged(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, long[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferRangedEvent(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, long[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferRanged(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, long[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferRangedEvent(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, long[] obj, IntPtr hEventArr, IntPtr hEvt);



        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBuffer(IntPtr hCommandQueue, IntPtr hBuffer, double[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferEvent(IntPtr hCommandQueue, IntPtr hBuffer, double[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBuffer(IntPtr hCommandQueue, IntPtr hBuffer, double[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferEvent(IntPtr hCommandQueue, IntPtr hBuffer, double[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferRanged(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, double[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferRangedEvent(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, double[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferRanged(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, double[] obj);

        // yapılacak: bunun diğerleri de yapılacak
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferRangedEvent(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, double[] obj,IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBuffer(IntPtr hCommandQueue, IntPtr hBuffer, byte[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferEvent(IntPtr hCommandQueue, IntPtr hBuffer, byte[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBuffer(IntPtr hCommandQueue, IntPtr hBuffer, byte[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferEvent(IntPtr hCommandQueue, IntPtr hBuffer, byte[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferRanged(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, byte[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromBufferRangedEvent(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, byte[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferRanged(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, byte[] obj);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToBufferRangedEvent(IntPtr hCommandQueue, IntPtr hBuffer, int reference, int range, byte[] obj, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void deleteBuffer(IntPtr hBuffer);


        /// <summary>
        /// opencl buffer type, duplicated for CSpaceArrays for "internal" optimization
        /// </summary>
        public enum SizeOf : int
        {
            cl_float = 0,
            cl_double = 1,
            cl_int = 2,
            cl_long = 3,
            cl_half = 4,
            cl_char = 5,
            cl_uint=6
        };

        private IntPtr hBuffer;
        private IntPtr hContext;
        private int numberOfElements;
        private SizeOf clType;
        private bool isDeleted = false;

        /// <summary>
        /// create opencl buffer 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="numberOfElements_"></param>
        /// <param name="clType_"></param>
        /// <param name="GDDR_BUFFER"></param>
        /// <param name="isCSharpArray_"></param>
        /// <param name="arrPointer"></param>
        public ClBuffer(ClContext context, int numberOfElements_, SizeOf clType_, bool GDDR_BUFFER, int isCSharpArray_=1, IntPtr arrPointer=new IntPtr())
        {
            hContext = context.h();
            numberOfElements = numberOfElements_;
            clType = clType_;
            hBuffer = createBuffer(hContext, numberOfElements, (int)clType, isCSharpArray_, arrPointer,GDDR_BUFFER);
        }

        /// <summary>
        /// handle to buffer object 
        /// </summary>
        /// <returns></returns>
        public IntPtr h()
        {
            return hBuffer;
        }

        /// <summary>
        /// write to buffer from array
        /// </summary>
        /// <param name="cq">command queue to enqueue this write command</param>
        /// <param name="arr">array to read (to write on buffer)</param>
        public void write(ClCommandQueue cq, object arr)
        {
            if (arr.GetType() == typeof(float[]))
                writeToBuffer(cq.h(), hBuffer, (float[])arr);
            else if (arr.GetType() == typeof(int[]))
                writeToBuffer(cq.h(), hBuffer, (int[])arr);
            else if (arr.GetType() == typeof(uint[]))
                writeToBuffer(cq.h(), hBuffer, (uint[])arr);
            else if (arr.GetType() == typeof(double[]))
                writeToBuffer(cq.h(), hBuffer, (double[])arr);
            else if (arr.GetType() == typeof(long[]))
                writeToBuffer(cq.h(), hBuffer, (long[])arr);
            else if (arr.GetType() == typeof(byte[]))
                writeToBuffer(cq.h(), hBuffer, (byte[])arr);
            else if(Functions.isTypeOfFastArr(arr))
                writeToBuffer(cq.h(), hBuffer,((IMemoryHandle)arr).ha());
        }

        /// <summary>
        /// write to buffer from array but with constraints
        /// </summary>
        /// <param name="cq"></param>
        /// <param name="reference"></param>
        /// <param name="range"></param>
        /// <param name="arr"></param>
        public void writeRanged(ClCommandQueue cq, int reference, int range, object arr)
        {
            if (arr.GetType() == typeof(float[]))
                writeToBufferRanged(cq.h(), hBuffer, reference, range, (float[])arr);
            else if (arr.GetType() == typeof(int[]))
                writeToBufferRanged(cq.h(), hBuffer, reference, range, (int[])arr);
            else if (arr.GetType() == typeof(uint[]))
                writeToBufferRanged(cq.h(), hBuffer, reference, range, (uint[])arr);
            else if (arr.GetType() == typeof(double[]))
                writeToBufferRanged(cq.h(), hBuffer, reference, range, (double[])arr);
            else if (arr.GetType() == typeof(long[]))
                writeToBufferRanged(cq.h(), hBuffer, reference, range, (long[])arr);
            else if (arr.GetType() == typeof(byte[]))
                writeToBufferRanged(cq.h(), hBuffer, reference, range, (byte[])arr);
            else if (Functions.isTypeOfFastArr(arr))
                writeToBufferRanged(cq.h(), hBuffer, reference, range, ((IMemoryHandle)arr).ha());

        }

        /// <summary>
        /// write to buffer from array but with constraints and events
        /// </summary>
        /// <param name="cq"></param>
        /// <param name="reference"></param>
        /// <param name="range"></param>
        /// <param name="arr"></param>
        /// <param name="eArr"></param>
        /// <param name="e"></param>
        public void writeRangedEvent(ClCommandQueue cq, int reference, int range, object arr, ClEventArray eArr = null, ClEvent e = null)
        {
            if (arr.GetType() == typeof(float[]))
                writeToBufferRangedEvent(cq.h(), hBuffer, reference, range, (float[])arr,eArr.h(),e.h());
            else if (arr.GetType() == typeof(int[]))
                writeToBufferRangedEvent(cq.h(), hBuffer, reference, range, (int[])arr, eArr.h(), e.h());
            else if (arr.GetType() == typeof(uint[]))
                writeToBufferRangedEvent(cq.h(), hBuffer, reference, range, (uint[])arr, eArr.h(), e.h());
            else if (arr.GetType() == typeof(double[]))
                writeToBufferRangedEvent(cq.h(), hBuffer, reference, range, (double[])arr, eArr.h(), e.h());
            else if (arr.GetType() == typeof(long[]))
                writeToBufferRangedEvent(cq.h(), hBuffer, reference, range, (long[])arr, eArr.h(), e.h());
            else if (arr.GetType() == typeof(byte[]))
                writeToBufferRangedEvent(cq.h(), hBuffer, reference, range, (byte[])arr, eArr.h(), e.h());
            else if (Functions.isTypeOfFastArr(arr))
                writeToBufferRangedEvent(cq.h(), hBuffer, reference, range, ((IMemoryHandle)arr).ha(),eArr.h(),e.h());

        }

        /// <summary>
        /// read from buffer and write to array
        /// </summary>
        /// <param name="cq"></param>
        /// <param name="reference"></param>
        /// <param name="range"></param>
        /// <param name="arr"></param>
        public void read(ClCommandQueue cq, int reference, int range, object arr)
        {
            if (arr.GetType() == typeof(float[]))
                readFromBufferRanged(cq.h(), hBuffer, reference, range, (float[])arr);
            else if (arr.GetType() == typeof(int[]))
                readFromBufferRanged(cq.h(), hBuffer, reference, range, (int[])arr);
            else if (arr.GetType() == typeof(uint[]))
                readFromBufferRanged(cq.h(), hBuffer, reference, range, (uint[])arr);
            else if (arr.GetType() == typeof(double[]))
                readFromBufferRanged(cq.h(), hBuffer, reference, range, (double[])arr);
            else if (arr.GetType() == typeof(long[]))
                readFromBufferRanged(cq.h(), hBuffer, reference, range, (long[])arr);
            else if (arr.GetType() == typeof(byte[]))
                readFromBufferRanged(cq.h(), hBuffer, reference, range, (byte[])arr);
            else if (Functions.isTypeOfFastArr(arr))
                readFromBufferRanged(cq.h(), hBuffer, reference, range, ((IMemoryHandle)arr).ha());
        }

        /// <summary>
        /// read from buffer and write to array but with events
        /// </summary>
        /// <param name="cq"></param>
        /// <param name="reference"></param>
        /// <param name="range"></param>
        /// <param name="arr"></param>
        /// <param name="eArr"></param>
        /// <param name="e"></param>
        public void readEvent(ClCommandQueue cq, int reference, int range, object arr, ClEventArray eArr, ClEvent e)
        {
            if (arr.GetType() == typeof(float[]))
                readFromBufferRangedEvent(cq.h(), hBuffer, reference, range, (float[])arr,eArr.h(),e.h());
            else if (arr.GetType() == typeof(int[]))
                readFromBufferRangedEvent(cq.h(), hBuffer, reference, range, (int[])arr, eArr.h(), e.h());
            else if (arr.GetType() == typeof(uint[]))
                readFromBufferRangedEvent(cq.h(), hBuffer, reference, range, (uint[])arr, eArr.h(), e.h());
            else if (arr.GetType() == typeof(double[]))
                readFromBufferRangedEvent(cq.h(), hBuffer, reference, range, (double[])arr, eArr.h(), e.h());
            else if (arr.GetType() == typeof(long[]))
                readFromBufferRangedEvent(cq.h(), hBuffer, reference, range, (long[])arr, eArr.h(), e.h());
            else if (arr.GetType() == typeof(byte[]))
                readFromBufferRangedEvent(cq.h(), hBuffer, reference, range, (byte[])arr, eArr.h(), e.h());
            else if (Functions.isTypeOfFastArr(arr))
                readFromBufferRangedEvent(cq.h(), hBuffer, reference, range, ((IMemoryHandle)arr).ha(),eArr.h(),e.h());
            
        }

        /// <summary>
        /// release C++ resources
        /// </summary>
        public void dispose()
        {

            if (!isDeleted)
                deleteBuffer(hBuffer);
            isDeleted = true;
        }

    }
}
