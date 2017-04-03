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
using System.Text;

namespace Cekirdekler
{


    /// <summary>
    /// Parameter group to be crunched in kernel execution
    /// </summary>
    public interface ICanCompute
    {
        /// <summary>
        /// <para>blocking compute operation that does read + compute + write </para>
        /// </summary>
        /// <param name="cruncher"></param>
        /// <param name="computeId"></param>
        /// <param name="kernelNamesString">string that contains all kernel names(to be executed) separated by space or by , or by ;</param>
        /// <param name="globalRange">total workitems to be distributed to devices</param>
        /// <param name="localRange">workitems per local workgroup. default value is 256</param>
        /// <param name="ofsetGlobalRange">starting id for workitems.(for cluster add-on)</param>
        /// <param name="pipeline">true: pipeline is on</param>
        /// <param name="pipelineType">Cores.PIPELINE_EVENT means event-driven 3-queued pipelined read+compute+write operation. </param>
        /// <param name="pipelineBlobs">minimum 4, multiple of 4</param>
        void compute(ClNumberCruncher cruncher, int computeId, string kernelNamesString, int globalRange,
                            int localRange = 256, int ofsetGlobalRange = 0, bool pipeline = false,
                            bool pipelineType = Cores.PIPELINE_EVENT, int pipelineBlobs = 4);

    }


    /// <summary>
    /// can optimize the buffer copies 
    /// </summary>
    public interface IBufferOptimization
    {
        /// <summary>
        /// array to be used in computations. could be a C# array or FastArr{T} as a wrapper for C++ array 
        /// </summary>
        object array
        {
            get;

            set;
        }

        /// <summary>
        /// read whole array before compute
        /// </summary>
        bool read { get; set; }

        /// <summary>
        /// <para>change the read into a partial type so it can be pipelined to hide latency to make compute() faster</para>
        /// </summary>
        bool partialRead { get; set; }

        /// <summary>
        /// write results after kernel execution (possibly pipelined)
        /// </summary>
        bool write { get; set; }


        /// <summary>
        /// <para>number of array elements per workitem to be computed, to be buffer-copied. default=1</para>
        /// <para>also default=1 for C# arrays(without ClArray nor FastArr )</para>
        /// <para>number of global workitems * elements per work item must not be greater than buffer size </para>
        /// <para>number of local workitems * elements per work item * number of pipeline blob must not be greater than buffer size  </para>
        /// </summary>
        int numberOfElementsPerWorkItem { get; set; }
    }


    /// <summary>
    /// Holds ClArray instances, C# arrays and FastArr instances to be computed later
    /// </summary>
    public class ClParameterGroup : ICanCompute, ICanBind
    {
        /// <summary>
        /// not used, comes from interface
        /// </summary>
        public object array
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        // for adding more parameters and to be turned into an array later
        internal LinkedList<object> arrays = new LinkedList<object>();

        // read whole 
        internal LinkedList<bool> reads = new LinkedList<bool>();

        // read partial 
        internal LinkedList<bool> partialReads = new LinkedList<bool>();

        // write partial 
        internal LinkedList<bool> writes = new LinkedList<bool>();

        // elementsPerWorkItem 
        internal LinkedList<int> arrayElementsPerWorkItem = new LinkedList<int>();


        /// <summary>
        /// linked list to array conversion for all arrays
        /// </summary>
        /// <returns></returns>
        public object[] selectedArrays()
        {
            return arrays.ToArray();
        }

        /// <summary>
        /// <para>adds arrays as more parameters for kernel execution</para>
        /// <para>arrays order as parameters:</para>
        /// <para>this object - then - other params added to this object - then - parameters from .nextParam()</para>
        /// </summary>
        /// <param name="arrays_">C# arrays to add as kernel parameters</param>
        /// <returns>a ClParameterGroup that can crunch numbers or bind more parameters</returns>
        public ClParameterGroup nextParam(params Array[] arrays_)
        {
            ClParameterGroup gr = new ClParameterGroup();

            LinkedListNode<object> node = arrays.First;
            LinkedListNode<bool> node2 = reads.First;
            LinkedListNode<bool> node3 = partialReads.First;
            LinkedListNode<bool> node4 = writes.First;
            LinkedListNode<int> node5 = arrayElementsPerWorkItem.First;

            while(node!=null)
            {
                gr.arrays.AddLast(node.Value);
                gr.reads.AddLast(node2.Value);
                gr.partialReads.AddLast(node3.Value);
                gr.writes.AddLast(node4.Value);
                gr.arrayElementsPerWorkItem.AddLast(node5.Value);
                node = node.Next;
                node2 = node2.Next;
                node3 = node3.Next;
                node4 = node4.Next;
                node5 = node5.Next;
            }


            for (int i = 0; i < arrays_.Length; i++)
            {
                gr.arrays.AddLast(arrays_[i]);

                // default values for C# arrays (they need to be wrapped in ClArray to be modified)
                gr.reads.AddLast(true);
                gr.partialReads.AddLast(true);
                gr.writes.AddLast(true);
                gr.arrayElementsPerWorkItem.AddLast(1);
            }
            return gr;
        }

        /// <summary>
        /// <para>adds arrays as more parameters for kernel execution</para>
        /// <para>arrays order as parameters:</para>
        /// <para>this object - then - other params added to this object - then - parameters from .nextParam()</para>
        /// </summary>
        /// <param name="arrays_">FastArr type(ClFloatArray,ClUIntArray,...) arrays to add as kernel parameters</param>
        /// <returns>a ClParameterGroup that can crunch numbers or bind more parameters</returns>
        public ClParameterGroup nextParam(params IMemoryHandle[] arrays_)
        {

            ClParameterGroup gr = new ClParameterGroup();

            LinkedListNode<object> node = arrays.First;
            LinkedListNode<bool> node2 = reads.First;
            LinkedListNode<bool> node3 = partialReads.First;
            LinkedListNode<bool> node4 = writes.First;
            LinkedListNode<int> node5 = arrayElementsPerWorkItem.First;

            while (node != null)
            {
                gr.arrays.AddLast(node.Value);
                gr.reads.AddLast(node2.Value);
                gr.partialReads.AddLast(node3.Value);
                gr.writes.AddLast(node4.Value);
                gr.arrayElementsPerWorkItem.AddLast(node5.Value);
                node = node.Next;
                node2 = node2.Next;
                node3 = node3.Next;
                node4 = node4.Next;
                node5 = node5.Next;
            }


            for (int i = 0; i < arrays_.Length; i++)
            {
                gr.arrays.AddLast(arrays_[i]);

                // default values
                gr.reads.AddLast(true);
                gr.partialReads.AddLast(true);
                gr.writes.AddLast(true);
                gr.arrayElementsPerWorkItem.AddLast(1);

            }
            return gr;
        }

        /// <summary>
        /// <para>adds arrays as more parameters for kernel execution</para>
        /// <para>arrays order as parameters:</para>
        /// <para>this object - then - other params added to this object - then - parameters from .nextParam()</para>
        /// </summary>
        /// <param name="arrays_">ClArray type arrays(which can wrap C# or C++ arrays) to add as kernel parameters</param>
        /// <returns>a ClParameterGroup that can crunch numbers or bind more parameters</returns>
        public ClParameterGroup nextParam(params IBufferOptimization[] arrays_)
        {

            ClParameterGroup gr = new ClParameterGroup();

            LinkedListNode<object> node = arrays.First;
            LinkedListNode<bool> node2 = reads.First;
            LinkedListNode<bool> node3 = partialReads.First;
            LinkedListNode<bool> node4 = writes.First;
            LinkedListNode<int> node5 = arrayElementsPerWorkItem.First;

            while (node != null)
            {
                gr.arrays.AddLast(node.Value);
                gr.reads.AddLast(node2.Value);
                gr.partialReads.AddLast(node3.Value);
                gr.writes.AddLast(node4.Value);
                gr.arrayElementsPerWorkItem.AddLast(node5.Value);
                node = node.Next;
                node2 = node2.Next;
                node3 = node3.Next;
                node4 = node4.Next;
                node5 = node5.Next;
            }


            for (int i = 0; i < arrays_.Length; i++)
            {
                gr.arrays.AddLast(arrays_[i]);

              
                gr.reads.AddLast(arrays_[i].read);
                gr.partialReads.AddLast(arrays_[i].partialRead);
                gr.writes.AddLast(arrays_[i].write);
                gr.arrayElementsPerWorkItem.AddLast(arrays_[i].numberOfElementsPerWorkItem);
            }
            return gr;
        }


        /// <summary>
        /// <para>adds arrays as more parameters for kernel execution</para>
        /// <para>arrays order as parameters:</para>
        /// <para>this object - then - other params added to this object - then - parameters from .nextParam()</para>
        /// </summary>
        /// <param name="parameterGroups_">Other parameter groups to add as kernel parameters</param>
        /// <returns>a ClParameterGroup that can crunch numbers or bind more parameters</returns>
        public ClParameterGroup nextParam(params ClParameterGroup[] parameterGroups_)
        {

            ClParameterGroup gr = new ClParameterGroup();

            LinkedListNode<object> node0 = arrays.First;
            LinkedListNode<bool> node02 = reads.First;
            LinkedListNode<bool> node03 = partialReads.First;
            LinkedListNode<bool> node04 = writes.First;
            LinkedListNode<int> node05 = arrayElementsPerWorkItem.First;

            while (node0 != null)
            {
                gr.arrays.AddLast(node0.Value);
                gr.reads.AddLast(node02.Value);
                gr.partialReads.AddLast(node03.Value);
                gr.writes.AddLast(node04.Value);
                gr.arrayElementsPerWorkItem.AddLast(node05.Value);
                node0 = node0.Next;
                node02 = node02.Next;
                node03 = node03.Next;
                node04 = node04.Next;
                node05 = node05.Next;
            }


            for (int i = 0; i < parameterGroups_.Length; i++)
            {
                LinkedListNode<object> node = parameterGroups_[i].arrays.First;
                LinkedListNode<bool> node2 = parameterGroups_[i].reads.First;
                LinkedListNode<bool> node3 = parameterGroups_[i].partialReads.First;
                LinkedListNode<bool> node4 = parameterGroups_[i].writes.First;
                LinkedListNode<int> node5 = parameterGroups_[i].arrayElementsPerWorkItem.First;
                while(node!=null)
                {
                    if (node.Value != null)
                    {
                        gr.arrays.AddLast(node.Value);

                        
                        gr.reads.AddLast(node2.Value);
                        gr.partialReads.AddLast(node3.Value);
                        gr.writes.AddLast(node4.Value);
                        gr.arrayElementsPerWorkItem.AddLast(node5.Value);
                    }
                    node = node.Next;
                    node2 = node2.Next;
                    node3 = node3.Next;
                    node4 = node4.Next;
                    node5 = node5.Next;
                }
            }
            return gr;
        }
        

        /// <summary>
        /// <para>blocking compute operation that does read + compute + write </para>
        /// </summary>
        /// <param name="cruncher"></param>
        /// <param name="computeId"></param>
        /// <param name="kernelNamesString">string that contains all kernel names(to be executed) separated by space or by , or by ;</param>
        /// <param name="globalRange">total workitems to be distributed to devices</param>
        /// <param name="localRange">workitems per local workgroup. default value is 256</param>
        /// <param name="ofsetGlobalRange">starting id for workitems.(for cluster add-on)</param>
        /// <param name="pipeline">true: pipeline is on</param>
        /// <param name="pipelineType">Cores.PIPELINE_EVENT means event-driven 3-queued pipelined read+compute+write operation. </param>
        /// <param name="pipelineBlobs">minimum 4, multiple of 4</param>
        public void compute(ClNumberCruncher cruncher, int computeId, string kernelNamesString, int globalRange,
                            int localRange = 256, int ofsetGlobalRange = 0, bool pipeline = false,
                            bool pipelineType = Cores.PIPELINE_EVENT, int pipelineBlobs = 4)
        {
            string[] kernellerTmp = kernelNamesString.Split(new string[] { " ",",",";","-","\n" }, StringSplitOptions.RemoveEmptyEntries);

            object[] arrs_ = arrays.ToArray();
            string[] reads_ = reads.Select(x=> { return x? " read ":""; }).ToArray();
            string[] partialReads_ = partialReads.Select(x => { return x?" partial ":""; }).ToArray();
            string[] writes_ = writes.Select(x => { return x?" write ":""; }).ToArray();
            string[] readWrites_ = new string[reads_.Length];
            for(int i=0;i<readWrites_.Length;i++)
            {
                StringBuilder sb = new StringBuilder(partialReads_[i]);
                sb.Append(reads_[i]);
                sb.Append(writes_[i]);
                readWrites_[i] = sb.ToString();
            }
            int[] elemPerWorkItem_ = arrayElementsPerWorkItem.ToArray();
            cruncher.numberCruncher.compute(
                kernellerTmp, 0, "", 
                arrs_,readWrites_,elemPerWorkItem_, 
                globalRange, computeId, ofsetGlobalRange, 
                pipeline, pipelineBlobs, pipelineType, localRange);
            if (cruncher.performanceFeed)
                cruncher.numberCruncher.performanceReport(computeId);
        }
    }

    /// <summary>
    /// binds new parameters to earlier ones to be used in kernel executions
    /// </summary>
    public interface ICanBind
    {

        /// <summary>
        /// <para>adds arrays as more parameters for kernel execution</para>
        /// <para>arrays order as parameters:</para>
        /// <para>this object - then - other params added to this object - then - parameters from .nextParam()</para>
        /// </summary>
        /// <param name="parameterGroups_">Other parameter groups to add as kernel parameters</param>
        /// <returns>a ClParameterGroup that can crunch numbers or bind more parameters</returns>
        ClParameterGroup nextParam(params ClParameterGroup[] parameterGroups_);

        /// <summary>
        /// <para>adds arrays as more parameters for kernel execution</para>
        /// <para>arrays order as parameters:</para>
        /// <para>this object - then - other params added to this object - then - parameters from .nextParam()</para>
        /// </summary>
        /// <param name="arrays_">ClArray type arrays(which can wrap C# or C++ arrays) to add as kernel parameters</param>
        /// <returns>a ClParameterGroup that can crunch numbers or bind more parameters</returns>
        ClParameterGroup nextParam(params IBufferOptimization []arrays_);

        /// <summary>
        /// <para>adds arrays as more parameters for kernel execution</para>
        /// <para>arrays order as parameters:</para>
        /// <para>this object - then - other params added to this object - then - parameters from .nextParam()</para>
        /// </summary>
        /// <param name="arrays_">FastArr type(ClFloatArray,ClUIntArray,...) arrays to add as kernel parameters</param>
        /// <returns>a ClParameterGroup that can crunch numbers or bind more parameters</returns>
        ClParameterGroup nextParam(params IMemoryHandle[] arrays_);

        /// <summary>
        /// <para>adds arrays as more parameters for kernel execution</para>
        /// <para>arrays order as parameters:</para>
        /// <para>this object - then - other params added to this object - then - parameters from .nextParam()</para>
        /// </summary>
        /// <param name="arrays_">C# arrays to add as kernel parameters</param>
        /// <returns>a ClParameterGroup that can crunch numbers or bind more parameters</returns>
        ClParameterGroup nextParam(params Array[] arrays_);


        /// <summary>
        /// C++ or C# array of current object
        /// </summary>
        object array { get; set; }
    }

    /// <summary>
    /// <para>float,byte,...</para>
    /// <para>double,long</para>
    /// <para>int,uint</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClArray<T>: IList<T>,ICanBind,IBufferOptimization, ICanCompute
    {
        private int n;


        /// <summary>
        /// gets a copy as C# array
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            if (array == null)
                return null;

            if(isCSharpArr)
            {
                // C# array to C# array
                return ((T[])array).ToArray();
            }
            else
            {
                // C++ array to C# array (overridden implemented method of FastArr)
                // so same method can be used (only a future ToFastArr() could need a different method here)
                return ((FastArr<T>)array).ToArray(); 
            }
        }

        /// <summary>
        /// <para>number of elements of this array(C++ or C# side)</para>
        /// <para>when set to a different value, a new array is allocated(or created), old data is copied, remaining elements are defaulted</para>
        /// <para>if new value is smaller, elements at the end of array are lost</para>
        /// </summary>
        public int N
        {
            get { return n; }

            set
            {
                if(array==null || n==value)
                {
                    // no array no change 
                }
                else
                {
                    if(isCSharpArr)
                    {
                        T[] aNewArray = new T[N];
                        int iL = Math.Min(n, value);
                        var tmpT = array as T[];

                        Array.ConstrainedCopy((T[])array, 0, aNewArray, 0, iL);
                        if(iL<value)
                        {
                            for (int i = iL; i < value; i++)
                            {
                                aNewArray[i] = default(T);
                            }
                        }
                        array = aNewArray;
                    }
                    else
                    {
                        object aNewArray = createFastArr(value);
                        int iL = Math.Min(n, value);
                        var tmpT = array as FastArr<T>;
                        var tmpIList = aNewArray as IList<T>;
                        for (int i = 0; i < iL; i++)
                        {
                            tmpIList[i] = tmpT[i];
                        }
                        if (iL < value)
                        {
                            for (int i = iL; i < value; i++)
                            {
                                tmpIList[i] = default(T);
                            }
                        }
                        array = aNewArray;
                    }
                }

                n = value;
            }
        }



        /// <summary>
        /// <para>Wrapper for C++ and C# arrays, can optimize their copies to devices(or devices' access to them)</para>
        /// </summary>
        /// <param name="n_">number of elements(not bytes), default=-1 (no array allocated)</param>
        /// <param name="a_">alignment value(in bytes), default=4096</param>
        public ClArray(int n_=-1,int a_=4096)
        {
            N = n_;
            if(n_>0)
            {
                if(typeof(T) == typeof(float))
                    array = new ClFloatArray(n_,a_);
                else if (typeof(T) == typeof(double))
                    array = new ClDoubleArray(n_, a_);
                else if (typeof(T) == typeof(int))
                    array = new ClIntArray(n_, a_);
                else if (typeof(T) == typeof(long))
                    array = new ClLongArray(n_, a_);
                else if (typeof(T) == typeof(uint))
                    array = new ClUIntArray(n_, a_);
                else if (typeof(T) == typeof(char))
                    array = new ClCharArray(n_, a_);
                else if (typeof(T) == typeof(byte))
                    array = new ClByteArray(n_, a_);

                arrayAsIList = array_ as IList<T>;
            }
            isCSharpArr = false;

            // ClArray,FastArr<T>,T[]  default values
            read = true; // reads arrays as whole, not pipelined
            partialRead = false; // no partial reads
            write = true; // partial writes
            numberOfElementsPerWorkItem = 1; // 1 element per workitem 
        }

        /// <summary>
        /// release C++ resources, callable multiple times
        /// </summary>
        public void dispose()
        {
            if (array != null)
            {
                if (!isCSharpArr)
                {
                    ((IMemoryHandle)array).dispose();
                }
            }
        }

        /// <summary>
        /// release C++ resources
        /// </summary>
        ~ClArray()
        {
            if(array!=null)
            {
                if(!isCSharpArr)
                {
                    ((IMemoryHandle)array).dispose();
                }
            }
        }

        /// <summary>
        /// <para>if given true, allocates a C++ array(if C# array exists, copies from it)</para>
        /// <para>if given false, creates a C# array (if C++ array exists, copies from it)</para>
        /// <para>if array property is set from client code, this is changed accordingly with that array type</para>
        /// </summary>
        public bool fastArr
        {
            get
            {
                if (array == null)
                    return false;

                return !isCSharpArr;
            }


            set
            {
                
                // C++ array is requested
                if(value)
                {
                    // if C# array exists, put C++ array in place of it and copy old values
                    if(isCSharpArr)
                    {
                        if (N > 0)
                        {
                            if (array != null)
                            {
                                object tmp_ = createFastArr();
                                ((IMemoryOperations<T>)tmp_).CopyFrom((T[])array, 0);
                                array = tmp_;
                            }
                            else
                            {
                                array = createFastArr();
                            }
                        }
                    }
                    // if C++ array already exists, no action, if no array, create new C++ array 
                    else if(!isCSharpArr)
                    {
                        if(array==null)
                        {
                            if (N > 0)
                            {
                                array = createFastArr();
                            }
                        }    
                    }

                }
                // C# array is requested
                else
                {

                    // if C++ array exists, delete. Put C# array in place of it and copy old values from it
                    if (array != null && !isCSharpArr)
                    {
                        if (N > 0)
                        {
                            T[] tmp_ = new T[N];
                            ((FastArr<T>)array).CopyTo(tmp_, 0);
                            ((FastArr<T>)array).dispose();
                            array = tmp_;
                        }
                    }
                    else if (array == null)
                    {
                        if(N>0)
                            array = new T[N];
                    }
                }
            }
        }

        private object createFastArr(int arrayElements=0)
        {
            if (arrayElements == 0)
                arrayElements = N;

            if (typeof(T) == typeof(float))
                return new ClFloatArray(arrayElements);
            else if (typeof(T) == typeof(double))
                return new ClDoubleArray(arrayElements);
            else if (typeof(T) == typeof(int))
                return new ClIntArray(arrayElements);
            else if (typeof(T) == typeof(uint))
                return new ClUIntArray(arrayElements);
            else if (typeof(T) == typeof(long))
                return new ClLongArray(arrayElements);
            else if (typeof(T) == typeof(char))
                return new ClCharArray(arrayElements);
            else if (typeof(T) == typeof(byte))
                return new ClByteArray(arrayElements);
            else
                return null;
        }

        /// <summary>
        /// <para>float[], byte[], FastArr{T} ... </para>
        /// </summary>
        private object array_;

        private IList<T> arrayAsIList;

        /// <summary>
        /// C++(FastArr{T}) or C# (T[]) arrays
        /// </summary>
        public object array
        {
            get { return array_; }
            set
            {
                if (value.GetType() == typeof(T[]))
                    isCSharpArr = true;
                else
                    isCSharpArr = false;


                // when C++ array wrapper has no scope, destructor deletes it, can simmply set this:
                array_ = value;
                arrayAsIList = array_ as IList<T>;
            }
        }

        /// <summary>
        /// <para>initialize this wrapper from T[] or FastArr{T}</para> 
        /// </summary>
        /// <param name="b"></param>
        public static implicit operator ClArray<T> (T[] b)
        {

            if (typeof(T) == typeof(int) ||
                typeof(T) == typeof(uint) ||
                typeof(T) == typeof(byte) ||
                typeof(T) == typeof(char) ||
                typeof(T) == typeof(double) ||
                typeof(T) == typeof(long) ||
                typeof(T) == typeof(float))
            {
                ClArray<T> clArray = new ClArray<T>();
                clArray.array = b;
                clArray.isCSharpArr = true;
                return clArray;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// <para>initialize this wrapper from T[] or FastArr{T}</para> 
        /// </summary>
        /// <param name="b"></param>
        public static implicit operator ClArray<T>(FastArr<T> b)
        {
            ClArray<T> clArray = new ClArray<T>();
            clArray.array = b;
            clArray.isCSharpArr = false;
            return clArray;
        }


        // for sub steps
        private T arrayForSubSteps { get; set; }

        /// <summary>
        /// IList{T} compatibility, gives number of elements of C# or C++ arrays
        /// </summary>
        public int Count
        {
            get
            {
                return ((IList<T>)array).Count;
            }
        }

        /// <summary>
        /// IList{T} compatibility, gives number of elements of C# or C++ arrays
        /// </summary>
        public int Length
        {
            get
            {
                return ((IList<T>)array).Count;
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

        private bool isCSharpArr_;

        /// <summary>
        /// true = C# array
        /// false = C++ array (wrapper)
        /// </summary>
        internal bool isCSharpArr
        {
            get { return isCSharpArr_; }
            set { isCSharpArr_ = value; }
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
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
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
        /// copies to C# array (from its C++ or C# array)  
        /// </summary>
        /// <param name="array">C# array</param>
        /// <param name="arrayIndex">element index</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (!isCSharpArr)
            {
                    // using IList interface
                    ((IList<T>)this.array).CopyTo(array, arrayIndex);
            }
            else
            {
                    // using C# array
                    ((IList<T>)this.array).CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// copies to other ClArray objects' arrays
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(ClArray<T> array, int arrayIndex)
        {
            if(!isCSharpArr)
            {
                if(!array.isCSharpArr)
                {
                    // both C++ arrays
                    ((IMemoryOperations<T>)this.array).CopyTo_((FastArr<T>)array.array, arrayIndex);
                }
                else
                {
                    // uses IList interface
                    ((IList<T>)this.array).CopyTo((T[])array.array, arrayIndex);

                }
            }
            else
            {
                // this object has C# array

                if (!array.isCSharpArr)
                {
                    // other object has C++ array
                    ((IMemoryOperations<T>)array.array).CopyFrom((T[])this.array, arrayIndex);
                }
                else
                {
                    // both C# arrays
                    ((IList<T>)this.array).CopyTo((T[])array.array, arrayIndex);
                }
            }
        }

        /// <summary>
        /// copy to C++ array (FastArr)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(FastArr<T> array, int arrayIndex)
        {
            if (!isCSharpArr)
            {
                    // both C++ arrays
                    ((IMemoryOperations<T>)this.array).CopyTo_(array, arrayIndex);
            }
            else
            {
                    // this is C# array, parameter is C++ array
                    array.CopyFrom((T[])this.array, arrayIndex);
            }
        }

        /// <summary>
        /// copies from C# array(to C++ or C# array of itself)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyFrom(T[] array, int arrayIndex)
        {
            if (!isCSharpArr)
            {
                    // this is C++ array, other is C# array
                    ((IMemoryOperations<T>)this.array).CopyFrom((T[])array, arrayIndex);
            }
            else
            {
                    // both C# arrays
                    ((IList<T>)array).CopyTo((T[])this.array, arrayIndex);
            }
        }

        /// <summary>
        /// copies from other ClArray objects
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyFrom(ClArray<T> array, int arrayIndex)
        {
            if (!isCSharpArr)
            {
                if (!array.isCSharpArr)
                {
                    // both C++ arrays
                    ((IMemoryOperations<T>)this.array).CopyFrom_((FastArr<T>)array.array, arrayIndex);
                }
                else
                {
                    // this is C++ array, other is C# array
                    ((IMemoryOperations<T>)this.array).CopyFrom((T[])array.array, arrayIndex);
                }
            }
            else
            {
                if (!array.isCSharpArr)
                {
                    // this is C# array, other is C++ array
                    ((IList<T>)array.array).CopyTo((T[])this.array, arrayIndex);
                }
                else
                {
                    // both C# arrays
                    ((IList<T>)array.array).CopyTo((T[])this.array, arrayIndex);
                }
            }
        }

        /// <summary>
        /// copies from C++ array(to its own C++ or C# array)
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyFrom(FastArr<T> array, int arrayIndex)
        {
            if (!isCSharpArr)
            {
                    // both C++ arrays
                    ((IMemoryOperations<T>)this.array).CopyFrom_(array, arrayIndex);
            }
            else
            {
                    // this is C# array, other is C++ array
                    ((IList<T>)array).CopyTo((T[])this.array, arrayIndex);
            }
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
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
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
        /// <para>adds arrays as more parameters for kernel execution</para>
        /// <para>arrays order as parameters:</para>
        /// <para>this object - then - other params added to this object - then - parameters from .nextParam()</para>
        /// </summary>
        /// <param name="arrays_">C# arrays to add as kernel parameters</param>
        /// <returns>a ClParameterGroup that can crunch numbers or bind more parameters</returns>
        public ClParameterGroup nextParam(params Array[] arrays_)
        {
            ClParameterGroup bd = new ClParameterGroup();

            bd.arrays.AddLast(array);
            bd.reads.AddLast(read);
            bd.partialReads.AddLast(partialRead);
            bd.writes.AddLast(write);
            bd.arrayElementsPerWorkItem.AddLast(numberOfElementsPerWorkItem);
            for (int i = 0; i < arrays_.Length; i++)
            {
                bd.arrays.AddLast(arrays_[i]);

                // default
                bd.reads.AddLast(true);
                bd.partialReads.AddLast(false);
                bd.writes.AddLast(true);
                bd.arrayElementsPerWorkItem.AddLast(1);
            }
            return bd;
        }

        /// <summary>
        /// <para>adds arrays as more parameters for kernel execution</para>
        /// <para>arrays order as parameters:</para>
        /// <para>this object - then - other params added to this object - then - parameters from .nextParam()</para>
        /// </summary>
        /// <param name="arrays_">FastArr type(ClFloatArray,ClUIntArray,...) arrays to add as kernel parameters</param>
        /// <returns>a ClParameterGroup that can crunch numbers or bind more parameters</returns>
        public ClParameterGroup nextParam(params IMemoryHandle[] arrays_)
        {
            ClParameterGroup bd = new ClParameterGroup();
            
            bd.arrays.AddLast(array);
            bd.reads.AddLast(read);
            bd.partialReads.AddLast(partialRead);
            bd.writes.AddLast(write);
            bd.arrayElementsPerWorkItem.AddLast(numberOfElementsPerWorkItem);
            for (int i = 0; i < arrays_.Length; i++)
            {
                bd.arrays.AddLast(arrays_[i]);

                // default
                bd.reads.AddLast(true);
                bd.partialReads.AddLast(false);
                bd.writes.AddLast(true);
                bd.arrayElementsPerWorkItem.AddLast(1);
            }
            return bd;
        }

        /// <summary>
        /// <para>adds arrays as more parameters for kernel execution</para>
        /// <para>arrays order as parameters:</para>
        /// <para>this object - then - other params added to this object - then - parameters from .nextParam()</para>
        /// </summary>
        /// <param name="arrays_">ClArray type arrays(which can wrap C# or C++ arrays) to add as kernel parameters</param>
        /// <returns>a ClParameterGroup that can crunch numbers or bind more parameters</returns>
        public ClParameterGroup nextParam(params IBufferOptimization[] arrays_)
        {
            ClParameterGroup bd = new ClParameterGroup();
            
            bd.arrays.AddLast(array);
            bd.reads.AddLast(read);
            bd.partialReads.AddLast(partialRead);
            bd.writes.AddLast(write);
            bd.arrayElementsPerWorkItem.AddLast(numberOfElementsPerWorkItem);
            for (int i = 0; i < arrays_.Length; i++)
            {
                bd.arrays.AddLast(arrays_[i]);

                // default
                bd.reads.AddLast(arrays_[i].read);
                bd.partialReads.AddLast(arrays_[i].partialRead);
                bd.writes.AddLast(arrays_[i].write);
                bd.arrayElementsPerWorkItem.AddLast(arrays_[i].numberOfElementsPerWorkItem);
            }
            return bd;
        }

        /// <summary>
        /// <para>adds arrays as more parameters for kernel execution</para>
        /// <para>arrays order as parameters:</para>
        /// <para>this object - then - other params added to this object - then - parameters from .nextParam()</para>
        /// </summary>
        /// <param name="parameterGroups_">Other parameter groups to add as kernel parameters</param>
        /// <returns>a ClParameterGroup that can crunch numbers or bind more parameters</returns>
        public ClParameterGroup nextParam(params ClParameterGroup[] parameterGroups_)
        {
            ClParameterGroup bd = new ClParameterGroup();

            bd.arrays.AddLast(array);
            bd.reads.AddLast(read);
            bd.partialReads.AddLast(partialRead);
            bd.writes.AddLast(write);
            bd.arrayElementsPerWorkItem.AddLast(numberOfElementsPerWorkItem);

            for (int i = 0; i < parameterGroups_.Length; i++)
            {
                LinkedListNode<object> node = parameterGroups_[i].arrays.First;
                LinkedListNode<bool> node2 = parameterGroups_[i].reads.First;
                LinkedListNode<bool> node3 = parameterGroups_[i].partialReads.First;
                LinkedListNode<bool> node4 = parameterGroups_[i].writes.First;
                LinkedListNode<int> node5 = parameterGroups_[i].arrayElementsPerWorkItem.First;
                while (node!=null)
                {
                    if (node.Value != null)
                    {
                        bd.arrays.AddLast(node.Value);
                        bd.reads.AddLast(node2.Value);
                        bd.partialReads.AddLast(node3.Value);
                        bd.writes.AddLast(node4.Value);
                        bd.arrayElementsPerWorkItem.AddLast(node5.Value);
                    }
                    node = node.Next;
                    node2 = node2.Next;
                    node3 = node3.Next;
                    node4 = node4.Next;
                    node5 = node5.Next;
                }
            }
            return bd;
        }

        /// <summary>
        /// <para>blocking compute operation that does read + compute + write </para>
        /// </summary>
        /// <param name="cruncher"></param>
        /// <param name="computeId"></param>
        /// <param name="kernelNamesString">string that contains all kernel names(to be executed) separated by space or by , or by ;</param>
        /// <param name="globalRange">total workitems to be distributed to devices</param>
        /// <param name="localRange">workitems per local workgroup. default value is 256</param>
        /// <param name="ofsetGlobalRange">starting id for workitems.(for cluster add-on)</param>
        /// <param name="pipeline">true: pipeline is on</param>
        /// <param name="pipelineType">Cores.PIPELINE_EVENT means event-driven 3-queued pipelined read+compute+write operation. </param>
        /// <param name="pipelineBlobs">minimum 4, multiple of 4</param>
        public void compute(ClNumberCruncher cruncher, int computeId, string kernelNamesString, int globalRange,
                            int localRange = 256, int ofsetGlobalRange = 0, bool pipeline = false,
                            bool pipelineType = Cores.PIPELINE_EVENT, int pipelineBlobs = 4)
        {
            string[] kernellerTmp = kernelNamesString.Split(new string[] { " ", ",", ";", "-", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            object[] arrs_ = new object[] { array };
            string[] reads_ = new string[] { read ? " read " : "" };
            string[] partialReads_ = new string[]{ partialRead ? " partial " : ""};
            string[] writes_ = new string[] { write ? " write " : ""};
            string[] readWrites_ = new string[reads_.Length];
            for (int i = 0; i < readWrites_.Length; i++)
            {
                StringBuilder sb = new StringBuilder(partialReads_[i]);
                sb.Append(reads_[i]);
                sb.Append(writes_[i]);
                readWrites_[i] = sb.ToString();
            }
            int[] elemsPerWorkItem_ = new int[]{ numberOfElementsPerWorkItem };
            cruncher.numberCruncher.compute(
                kernellerTmp, 0, "",
                arrs_, readWrites_, elemsPerWorkItem_,
                globalRange, computeId, ofsetGlobalRange,
                pipeline, pipelineBlobs, pipelineType, localRange);
            if (cruncher.performanceFeed)
                cruncher.numberCruncher.performanceReport(computeId);
        }



        /// <summary>
        /// reads whole array before compute (no pipelining)
        /// </summary>
        public bool read { get; set; }

        /// <summary>
        /// <para>partial reads(possibly pipelined)</para>
        /// </summary>
        public bool partialRead { get; set; }

        /// <summary>
        /// partial writes (possibly pipelined)
        /// </summary>
        public bool write { get; set; }


        /// <summary>
        /// <para>gnumber of array elements per workitem, default=1</para>
        /// <para>global range * this number must be smaller than or equal to array size </para>
        /// </summary>
        public int numberOfElementsPerWorkItem { get; set; }

        /// <summary>
        /// <para>direct access to C++ array elements just like C# arrays</para>
        /// <para>don't cross boundaries, don't use after dispose()</para>
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public T this[int i]
        {
            get
            {
                return arrayAsIList[i];
            }
            set
            {
                arrayAsIList[i] = value;
            }
        }
    }
}
