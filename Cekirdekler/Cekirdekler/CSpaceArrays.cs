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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Cekirdekler
{
    /// <summary>
    /// C++ "C" space array functions for fast GPGPU
    /// </summary>
    public class CSpaceArrays
    {
        /// <summary>
        /// 32-bit precision floats in C# 
        /// </summary>
        public const int ARR_FLOAT = 0;

        /// <summary>
        /// 64-bit precision floats in C#
        /// </summary>
        public const int ARR_DOUBLE = 1;

        /// <summary>
        /// 64-bit integer in C#
        /// </summary>
        public const int ARR_LONG = 2;

        /// <summary>
        /// 32-bit integer in C#
        /// </summary>
        public const int ARR_INT = 3;

        /// <summary>
        /// 32-bit unsigned integer in C#
        /// </summary>
        public const int ARR_UINT = 4;

        /// <summary>
        /// 8bit integer
        /// </summary>
        public const int ARR_BYTE = 5;

        /// <summary>
        /// 2-byte or 16-bit container for integers in C#
        /// </summary>
        public const int ARR_CHAR = 6; // cl_half 16bit

        /// <summary>
        /// table of number of bytes per type of element
        /// </summary>
        protected static int[] sizeOf_ = new int[20];

        /// <summary>
        /// loading type sizes from "C" space opencl
        /// </summary>
        static CSpaceArrays()
        {
            sizeOf(sizeOf_);
        }

        /// <summary>
        /// query array element size from table
        /// </summary>
        /// <param name="type">example: ARR_BYTE = 5</param>
        /// <returns>for ARR_BYTE returns 1</returns>
        public static int sizeOf(int type)
        {
            return sizeOf_[type];
        }
        

        /// <summary>
        /// just to get cl_float cl_int sizes from "C" space
        /// </summary>
        /// <param name="definitionArr"></param>
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void sizeOf(int [] definitionArr);


        /// <summary>
        /// create "C" space arrays
        /// </summary>
        /// <param name="numberOfElements"></param>
        /// <param name="alignment"></param>
        /// <param name="typeOfArray"></param>
        /// <returns>returns pointer to augmented array</returns>
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr createArray(int numberOfElements, int alignment, int typeOfArray);


        /// <summary>
        /// get the first N-aligned element address from "C" space array
        /// </summary>
        /// <param name="hArr">handle for "C" space array</param>
        /// <returns></returns>
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr alignedArrHead(IntPtr hArr);

        /// <summary>
        /// delete "C" space array
        /// </summary>
        /// <param name="hArr">handle for augmented array in "C" space</param>
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void deleteArray(IntPtr hArr);

        // ---------------------------------------------------------------------

        /// <summary>
        /// IntPtr to IntPtr copy in bytes count
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="count">byte number</param>
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
    }

    /// <summary>
    /// C++ dizileri ile C# veya C++ dizileri arasında kopyalama işlemleri için
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMemoryOperations<T>:IMemoryHandle
    {
        /// <summary>
        /// C# array to C++ array copy
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        void CopyFrom(T[] array, int arrayIndex);

        /// <summary>
        /// C++ array to C++ array copy
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        void CopyFrom_(FastArr<T> array, int arrayIndex);

        /// <summary>
        /// C++ array to C++ array copy
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        void CopyTo_(FastArr<T> array, int arrayIndex);
    }

    /// <summary>
    /// C++ - C# array communication-compatibility interface
    /// </summary>
    public interface IMemoryHandle
    {
        /// <summary>
        /// first (4096 default)aligned element address of array
        /// </summary>
        /// <returns></returns>
        IntPtr ha();

        /// <summary>
        /// C++ array's number of elements (for compatibility with C# arrays)
        /// </summary>
        int Length { get; }

        /// <summary>
        /// C++ array element size
        /// </summary>
        int sizeOf { get; set; }

        /// <summary>
        /// cl_float, cl_int definitions for compatibility with "C" space opencl
        /// </summary>
        ClBuffer.SizeOf sizeOfEnum { get; set; }

        /// <summary>
        /// C++ dizisini siler
        /// </summary>
        void dispose();
        
    }



    /// <summary>
    /// <para>C++ array in "C" space for fast GPGPU buffer access/map/read/write</para>
    /// <para>switchabe from C#'s float,double,int,long</para>
    /// <para>and byte,uint,char</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FastArr<T>  : IList<T>,IMemoryOperations<T>
    {
        /// <summary>
        /// C++ array start(non-aligned)
        /// </summary>
        protected IntPtr hArr; 

        /// <summary>
        /// C++ array first (4096 default)aligned element address
        /// </summary>
        protected readonly IntPtr hAArr;

        
        /// <summary>
        /// number of elements in array
        /// </summary>
        protected int n_;

        /// <summary>
        /// <para>C++ array type compatible to C# side as</para>
        /// <para>CSpaceArrays.Arr_FLOAT</para>
        /// <para>CSpaceArrays.Arr_INT</para>
        /// <para>CSpaceArrays.Arr_BYTE</para>
        /// </summary>
        public int arrType { get; set; }

        /// <summary> 
        /// size of each array element
        /// </summary>
        public int sizeOf { get; set; }

        /// <summary>
        /// ClBuffer connection compatibility variable
        /// </summary>
        public ClBuffer.SizeOf sizeOfEnum { get; set; }


        internal FastArr(int n, int alignment = 4096)
        {
            n_ = n;
            if (typeof(T) == typeof(float))
            {
                arrType = CSpaceArrays.ARR_FLOAT; 
                sizeOfEnum = ClBuffer.SizeOf.cl_float;
            }
            else if (typeof(T) == typeof(int))
            {
                arrType = CSpaceArrays.ARR_INT;
                sizeOfEnum = ClBuffer.SizeOf.cl_int;
            }
            else if (typeof(T) == typeof(double))
            {
                arrType = CSpaceArrays.ARR_DOUBLE;
                sizeOfEnum = ClBuffer.SizeOf.cl_double;
            }
            else if (typeof(T) == typeof(long))
            {
                arrType = CSpaceArrays.ARR_LONG;
                sizeOfEnum = ClBuffer.SizeOf.cl_long;
            }
            else if (typeof(T) == typeof(char))
            {
                arrType = CSpaceArrays.ARR_CHAR;
                sizeOfEnum = ClBuffer.SizeOf.cl_half;
            }
            else if (typeof(T) == typeof(uint))
            {
                arrType = CSpaceArrays.ARR_UINT;
                sizeOfEnum = ClBuffer.SizeOf.cl_int;
            }
            else if (typeof(T) == typeof(byte))
            {
                arrType = CSpaceArrays.ARR_BYTE;
                sizeOfEnum = ClBuffer.SizeOf.cl_char;
            }
            sizeOf = CSpaceArrays.sizeOf(arrType);
            hArr = CSpaceArrays.createArray(n, alignment,arrType);
            hAArr = CSpaceArrays.alignedArrHead(hArr);

        }

        /// <summary>
        /// delete C++ "C" space array
        /// </summary>
        ~FastArr()
        {
            dispose();
        }

        /// <summary>
        /// C++ array's number of elements (just like C# array) 
        /// </summary>
        public int Length
        {
            get
            {
                return n_;
            }
        }
        
        /// <summary>
        /// overriden by derived classes and not be used by client codes for now
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        virtual public T this[int i]
        {
            get
            {
                unsafe
                {
                    return default(T);
                }

            }
            set
            {
                unsafe
                {
                    T t = value;
                }
            }
        }
        
        /// <summary>
        /// first properly aligned element of C++ array
        /// </summary>
        /// <returns></returns>
        public IntPtr ha()
        {
            return hAArr;
        }

        /// <summary>
        /// deletes C++ array, can be called multiple times
        /// </summary>
        public void dispose()
        {
            if (hArr != IntPtr.Zero)
            {
                CSpaceArrays.deleteArray(hArr);
                hArr = IntPtr.Zero;
            }
        }

        /// <summary>
        /// return a copy of C++ array as a C# array
        /// </summary>
        /// <returns></returns>
        virtual public T[] ToArray()
        {
            T[] f = new T[Length];
            GCHandle gc = GCHandle.Alloc(f, GCHandleType.Pinned);
            CSpaceArrays.CopyMemory(gc.AddrOfPinnedObject(), hAArr,(uint) (Length*sizeOf));
            return f;
        }

        /// <summary>
        /// for compatibility with IList{T} 
        /// </summary>
        public int Count
        {
            get
            {
                return n_;
            }
        }

        /// <summary>
        /// not implemented
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// not implemented
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// not implemented
        /// </summary>
        public void Clear()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// not implemented
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// overridden by derived classes
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// not implemented
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// not implemented
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// not implemented
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// not implemented
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// not implemented
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// not implemented
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// overriden by derived classes
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public virtual void CopyFrom(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// overriden by derived classes
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public virtual void CopyFrom_(FastArr<T> array, int arrayIndex)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// overriden by derived classes
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public virtual void CopyTo_(FastArr<T> array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// C++ byte array
    /// </summary>
    public unsafe class ClByteArray : FastArr<byte>
    {
        private byte* pByte;

        /// <summary>
        /// allocates a byte array in "C" space for faster opencl computations
        /// </summary>
        /// <param name="n"></param>
        /// <param name="alignment"></param>
        public ClByteArray(int n, int alignment = 4096) : base(n, alignment)
        {
            pByte = (byte*)hAArr.ToPointer();
        }

        /// <summary>
        /// access just like a C# array
        /// beware! check or be sure for out-of-bounds
        /// beware! don't use this after dispose()
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override byte this[int i]
        {
            get
            {
                    return *(pByte + i);
            }
            set
            {
                     *(pByte + i) = value;
            }
        }

        /// <summary>
        /// get a copy of fast array as a C# array
        /// </summary>
        /// <returns></returns>
        public override byte[] ToArray()
        {
            int dL = Length;
            byte[] f = new byte[dL];
            unsafe
            {
                fixed (byte* p = f)
                {
                    byte* p2 = (byte*)hAArr.ToPointer();
                    for (int i = 0; i < dL; i++)
                    {
                        f[i] = *(p2 + i);
                    }
                }

                return f;
            }
        }


        /// <summary>
        /// copy C++ array to C# array
        /// both must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyTo(byte[] array, int arrayIndex)
        {
            int dL = Length;
            if(dL==array.Length)
            {
                Marshal.Copy(hAArr, array, arrayIndex, dL - arrayIndex);
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy C# array to C++ array
        /// both must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyFrom(byte[] array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                Marshal.Copy(array, arrayIndex, hAArr, dL - arrayIndex);
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy C++ array to C++ array
        /// both must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyTo_(FastArr<byte> array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                CSpaceArrays.CopyMemory(new IntPtr(((IMemoryOperations<byte>)array).ha().ToInt64() + (long)arrayIndex),
                                   new IntPtr(ha().ToInt64() + (long)arrayIndex),
                                   (uint)((dL - arrayIndex)*sizeOf));
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy C++ array to C++ array
        /// both must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyFrom_(FastArr<byte> array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                CSpaceArrays.CopyMemory(new IntPtr(ha().ToInt64() + (long)arrayIndex),
                                   new IntPtr(((IMemoryOperations<byte>)array).ha().ToInt64() + (long)arrayIndex),
                                   (uint)((dL - arrayIndex) * sizeOf));
                return;
            }
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// C++ float array
    /// </summary>
    public unsafe class ClFloatArray : FastArr<float>
    {
        private float* pFloat;

        /// <summary>
        /// C++ float array
        /// </summary>
        /// <param name="n">number of elements</param>
        /// <param name="alignment">byte alignment value</param>
        public ClFloatArray(int n, int alignment = 4096) : base(n, alignment)
        {

            pFloat = (float*)hAArr.ToPointer();
        }

        /// <summary>
        /// array indexing similar to C# arrays
        /// beware, don't cross bounds
        /// beware, don't use after dispose()
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override float this[int i]
        {
            get
            {
                return *(pFloat + i);
            }
            set
            {
                *(pFloat + i) = value;
            }
        }

        /// <summary>
        /// get a copy of C++ array as a C# array 
        /// </summary>
        /// <returns></returns>
        public override float[] ToArray()
        {
            int dL = Length;
            float[] f = new float[dL];
            unsafe
            {
                fixed (float* p = f)
                {
                    float* p2 = (float*)hAArr.ToPointer();
                    for (int i = 0; i < dL; i++)
                    {
                        f[i] = *(p2 + i);
                    }
                }

                return f;
            }
        }

        /// <summary>
        /// copy C++ float array to C# float array
        /// </summary>
        /// <param name="array">C# float array</param>
        /// <param name="arrayIndex">copy start index</param>
        public override void CopyTo(float[] array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                Marshal.Copy(hAArr, array, arrayIndex, dL - arrayIndex);
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy C# float array to C++ float array
        /// </summary>
        /// <param name="array">C# array</param>
        /// <param name="arrayIndex">copy start index</param>
        public override void CopyFrom(float[] array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                Marshal.Copy(array, arrayIndex, hAArr, dL - arrayIndex);
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// C++ array to C++ array copy, both must be same length
        /// </summary>
        /// <param name="array">C++ float array</param>
        /// <param name="arrayIndex">copy start index</param>
        public override void CopyTo_(FastArr<float> array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                CSpaceArrays.CopyMemory(new IntPtr(((IMemoryOperations<float>)array).ha().ToInt64() + (long)arrayIndex),
                                   new IntPtr(ha().ToInt64() + (long)arrayIndex),
                                   (uint)((dL - arrayIndex) * sizeOf));
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// C++ to C++ array copy, both must be same length
        /// </summary>
        /// <param name="array">C++ float array</param>
        /// <param name="arrayIndex">copy start index</param>
        public override void CopyFrom_(FastArr<float> array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                CSpaceArrays.CopyMemory(new IntPtr(ha().ToInt64() + (long)arrayIndex),
                                   new IntPtr(((IMemoryOperations<float>)array).ha().ToInt64() + (long)arrayIndex),
                                   (uint)((dL - arrayIndex) * sizeOf));
                return;
            }
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// C++ int array
    /// </summary>
    public unsafe class ClIntArray : FastArr<int>
    {
        private int* pInt;

        /// <summary>
        /// C++ int array
        /// </summary>
        /// <param name="n"></param>
        /// <param name="alignment"></param>
        public ClIntArray(int n, int alignment = 4096) : base(n, alignment)
        {
            pInt = (int*)hAArr.ToPointer();
        }

        /// <summary>
        /// access like a C# array, beware: don't cross boundaries and don't use after dispose()
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override int this[int i]
        {
            get
            {
                return *(pInt + i);
            }
            set
            {
                *(pInt + i) = value;
            }
        }

        /// <summary>
        /// get a copy of C++ int array as C# array
        /// </summary>
        /// <returns></returns>
        public override int[] ToArray()
        {
            int dL = Length;
            int[] f = new int[dL];
            unsafe
            {
                fixed (int* p = f)
                {
                    int* p2 = (int*)hAArr.ToPointer();
                    for (int i = 0; i < dL; i++)
                    {
                        f[i] = *(p2 + i);
                    }
                }

                return f;
            }
        }


        /// <summary>
        /// copy from C++ int array to C# int array, must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyTo(int[] array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                Marshal.Copy(hAArr, array, arrayIndex, dL - arrayIndex);
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy from C# int array to C++ int array, must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyFrom(int[] array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                Marshal.Copy(array, arrayIndex, hAArr, dL - arrayIndex);
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy from C++ int array to C++ int array, must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyTo_(FastArr<int> array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                CSpaceArrays.CopyMemory(new IntPtr(((IMemoryOperations<int>)array).ha().ToInt64() + (long)arrayIndex),
                                   new IntPtr(ha().ToInt64() + (long)arrayIndex),
                                   (uint)((dL - arrayIndex) * sizeOf));
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy from C++ int array to C++ int array, must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyFrom_(FastArr<int> array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                CSpaceArrays.CopyMemory(new IntPtr(ha().ToInt64() + (long)arrayIndex),
                                   new IntPtr(((IMemoryOperations<int>)array).ha().ToInt64() + (long)arrayIndex),
                                   (uint)((dL - arrayIndex) * sizeOf));
                return;
            }
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// C++ double array
    /// </summary>
    public unsafe class ClDoubleArray : FastArr<double>
    {
        private double* pDouble;

        /// <summary>
        /// C++ double array
        /// </summary>
        /// <param name="n"></param>
        /// <param name="alignment"></param>
        public ClDoubleArray(int n, int alignment = 4096) : base(n, alignment)
        {
            pDouble = (double*)hAArr.ToPointer();
        }

        /// <summary>
        /// access like a C# array, don't cross boundaries, don't use after dispose()
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override double this[int i]
        {
            get
            {
                return *(pDouble + i);
            }
            set
            {
                *(pDouble + i) = value;
            }
        }

        /// <summary>
        /// get a copy of C++ array as a C# array
        /// </summary>
        /// <returns></returns>
        public override double[] ToArray()
        {
            int dL = Length;
            double[] f = new double[dL];
            unsafe
            {
                fixed (double* p = f)
                {
                    double* p2 = (double*)hAArr.ToPointer();
                    for (int i = 0; i < dL; i++)
                    {
                        f[i] = *(p2 + i);
                    }
                }

                return f;
            }
        }

        /// <summary>
        /// Copy from C++ array to C# array, must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyTo(double[] array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                Marshal.Copy(hAArr, array, arrayIndex, dL - arrayIndex);
                return;
            }
            throw new NotImplementedException();
        }


        /// <summary>
        /// Copy from C# array to C++ array, must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyFrom(double[] array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                Marshal.Copy(array, arrayIndex, hAArr, dL - arrayIndex);
                return;
            }
            throw new NotImplementedException();
        }


        /// <summary>
        /// Copy from C++ array to C++ array, must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyTo_(FastArr<double> array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                CSpaceArrays.CopyMemory(new IntPtr(((IMemoryOperations<double>)array).ha().ToInt64() + (long)arrayIndex),
                                   new IntPtr(ha().ToInt64() + (long)arrayIndex),
                                   (uint)((dL - arrayIndex) * sizeOf));
                return;
            }
            throw new NotImplementedException();
        }


        /// <summary>
        /// Copy from C++ array to C++ array, must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyFrom_(FastArr<double> array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                CSpaceArrays.CopyMemory(new IntPtr(ha().ToInt64() + (long)arrayIndex),
                                   new IntPtr(((IMemoryOperations<double>)array).ha().ToInt64() + (long)arrayIndex),
                                   (uint)((dL - arrayIndex) * sizeOf));
                return;
            }
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// C++ (C#-char) array
    /// </summary>
    public unsafe class ClCharArray : FastArr<char>
    {
        private char* pChar;

        /// <summary>
        /// C++ (C#-char) array
        /// </summary>
        /// <param name="n"></param>
        /// <param name="alignment"></param>
        public ClCharArray(int n, int alignment = 4096) : base(n, alignment)
        {

            pChar = (char*)hAArr.ToPointer();
        }

        /// <summary>
        /// access like a C# array, don't cross boundaries, don't use after dispose()
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override char this[int i]
        {
            get
            {
                return *(pChar + i);
            }
            set
            {
                *(pChar + i) = value;
            }
        }


        /// <summary>
        /// get a copy of C++ arrays as a C# array
        /// </summary>
        /// <returns></returns>
        public override char[] ToArray()
        {
            int dL = Length;
            char[] f = new char[dL];
            unsafe
            {
                fixed (char* p = f)
                {
                    char* p2 = (char*)hAArr.ToPointer();
                    for (int i = 0; i < dL; i++)
                    {
                        f[i] = *(p2 + i);
                    }
                }

                return f;
            }
        }


        /// <summary>
        /// copy C++ array to C# array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyTo(char[] array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                Marshal.Copy(hAArr, array, arrayIndex, dL - arrayIndex);
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy C# array to C++ array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyFrom(char[] array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                Marshal.Copy(array, arrayIndex, hAArr, dL - arrayIndex);
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy C++ array to C++ array, must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyTo_(FastArr<char> array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                CSpaceArrays.CopyMemory(new IntPtr(((IMemoryOperations<char>)array).ha().ToInt64() + (long)arrayIndex),
                                   new IntPtr(ha().ToInt64() + (long)arrayIndex),
                                   (uint)((dL - arrayIndex) * sizeOf));
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy C++ array to C++ array, must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyFrom_(FastArr<char> array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                CSpaceArrays.CopyMemory(new IntPtr(ha().ToInt64() + (long)arrayIndex),
                                   new IntPtr(((IMemoryOperations<char>)array).ha().ToInt64() + (long)arrayIndex),
                                   (uint)((dL - arrayIndex) * sizeOf));
                return;
            }
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// C++ long array
    /// </summary>
    public unsafe class ClLongArray : FastArr<long>
    {
        private long* pLong;

        /// <summary>
        /// C++ long array
        /// </summary>
        /// <param name="n"></param>
        /// <param name="alignment"></param>
        public ClLongArray(int n, int alignment = 4096) : base(n, alignment)
        {

            pLong = (long*)hAArr.ToPointer();
        }

        /// <summary>
        /// access like a C# array, don't cross boundaries, don't use after dispose()
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override long this[int i]
        {
            get
            {
                return *(pLong + i);
            }
            set
            {
                *(pLong + i) = value;
            }
        }


        /// <summary>
        /// get a copy of C++ array as a C# array
        /// </summary>
        /// <returns></returns>
        public override long[] ToArray()
        {
            int dL = Length;
            long[] f = new long[dL];
            unsafe
            {
                fixed (long* p = f)
                {
                    long* p2 = (long*)hAArr.ToPointer();
                    for (int i = 0; i < dL; i++)
                    {
                        f[i] = *(p2 + i);
                    }
                }

                return f;
            }
        }

        /// <summary>
        /// copy C++ array to C# array, must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyTo(long[] array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                Marshal.Copy(hAArr, array, arrayIndex, dL - arrayIndex);
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy C# array to C++ array, must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyFrom(long[] array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                Marshal.Copy(array, arrayIndex, hAArr, dL - arrayIndex);
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy C++ array to C++ array, must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyTo_(FastArr<long> array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                CSpaceArrays.CopyMemory(new IntPtr(((IMemoryOperations<long>)array).ha().ToInt64() + (long)arrayIndex),
                                   new IntPtr(ha().ToInt64() + (long)arrayIndex),
                                   (uint)((dL - arrayIndex) * sizeOf));
                return;
            }
            throw new NotImplementedException();
        }


        /// <summary>
        /// copy from C++ array to C++ array, must be same length
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyFrom_(FastArr<long> array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                CSpaceArrays.CopyMemory(new IntPtr(ha().ToInt64() + (long)arrayIndex),
                                   new IntPtr(((IMemoryOperations<long>)array).ha().ToInt64() + (long)arrayIndex),
                                   (uint)((dL - arrayIndex) * sizeOf));
                return;
            }
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// C++ (C#-uint) array
    /// </summary>
    public unsafe class ClUIntArray : FastArr<uint>
    {
        private uint* pUInt;

        /// <summary>
        /// C++ (C#-uint) array
        /// </summary>
        /// <param name="n"></param>
        /// <param name="alignment"></param>
        public ClUIntArray(int n, int alignment = 4096) : base(n, alignment)
        {
            pUInt = (uint*)hAArr.ToPointer();
        }

        /// <summary>
        /// access like a C# array, don't cross boundaries, don't use after dispose()
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public override uint this[int i]
        {
            get
            {
                return *(pUInt + i);
            }
            set
            {
                *(pUInt + i) = value;
            }
        }

        /// <summary>
        /// get a copy of C++ array as a C# array
        /// </summary>
        /// <returns></returns>
        public override uint[] ToArray()
        {
            int dL = Length;
            uint[] f = new uint[dL];
            unsafe
            {
                fixed (uint* p = f)
                {
                    uint* p2 = (uint*)hAArr.ToPointer();
                    for (int i = 0; i < dL; i++)
                    {
                        f[i] = *(p2 + i);
                    }
                }

                return f;
            }
        }

        /// <summary>
        /// copy from C++ array to C# array, both must be same size
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyTo(uint[] array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                Marshal.Copy(hAArr,(int[])(object) array, arrayIndex, dL - arrayIndex);
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy from C# array to C++ array, must be same size
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyFrom(uint[] array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                Marshal.Copy((int[])(object)array, arrayIndex, hAArr, dL - arrayIndex);
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy from C++ array to C++ array, must be same size
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyTo_(FastArr<uint> array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                CSpaceArrays.CopyMemory(new IntPtr(((IMemoryOperations<uint>)array).ha().ToInt64() + (long)arrayIndex),
                                   new IntPtr(ha().ToInt64() + (long)arrayIndex),
                                   (uint)((dL - arrayIndex) * sizeOf));
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// copy from C++ array to C++ array, must be same size
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public override void CopyFrom_(FastArr<uint> array, int arrayIndex)
        {
            int dL = Length;
            if (dL == array.Length)
            {
                CSpaceArrays.CopyMemory(new IntPtr(ha().ToInt64() + (long)arrayIndex),
                                   new IntPtr(((IMemoryOperations<uint>)array).ha().ToInt64() + (long)arrayIndex),
                                   (uint)((dL - arrayIndex) * sizeOf));
                return;
            }
            throw new NotImplementedException();
        }
    }

}
