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
using Cekirdekler;
using ClObject;
using Cekirdekler.Pipeline.Pool;

namespace Cekirdekler
{
    namespace ClArrays
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
            /// <param name="readWritesReady">
            /// <para>used by ClTask. compute() uses predetermined array fields in this array instead of current values.</para>
            /// <para>null = current values will be used</para>
            /// </param>
            /// <param name="elementsPerItemReady">
            /// <para>used by ClTask. compute() uses predetermined per-item-elements-value in this array instead of current values.</para>
            /// <para>null = current values will be used</para>
            /// </param>
            void compute(ClNumberCruncher cruncher, int computeId, string kernelNamesString, int globalRange,
                                int localRange = 256, int ofsetGlobalRange = 0, bool pipeline = false,
                                bool pipelineType = Cores.PIPELINE_EVENT, int pipelineBlobs = 4, string[] readWritesReady = null, int[] elementsPerItemReady = null);


            /// <summary>
            /// <para>creates task to compute later, meant to be used in pools mainly</para>
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
            ClTask task(ClNumberCruncher cruncher, int computeId, string kernelNamesString, int globalRange,
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
            /// <para>if the device used in cruncher supports zero copy buffer access, this field determines its usage. true=direct RAM access, false=dedicated memory</para>
            /// </summary>
            bool zeroCopy { get; set; }

            /// <summary>
            /// <para>buffers created in devices will be read by kernel and written by host, always</para>
            /// <para>prevents usage of write/writeAll/writeOnly, until it is set to false again</para>
            /// </summary>
            bool readOnly { get; set; }

            /// <summary>
            /// <para>buffers created in devices will be written by kernel and read by host, always</para>
            /// <para>prevents usage of read/partialRead/readOnly, until its set to false again</para>
            /// </summary>
            bool writeOnly { get; set; }

            /// <summary>
            /// read whole array before compute
            /// </summary>
            bool read { get; set; }

            /// <summary>
            /// <para>change the read into a partial type so it can be pipelined to hide latency to make compute() faster (possibly pipelined)</para>
            /// </summary>
            bool partialRead { get; set; }

            /// <summary>
            /// writes partial results after kernel execution (possibly pipelined)
            /// </summary>
            bool write { get; set; }

            /// <summary>
            /// write results after kernel execution but all of array instead of a part of it
            /// </summary>
            bool writeAll { get; set; }


            /// <summary>
            /// just to return typeof(T) instead of using many if-else in client code
            /// </summary>
            int arrayLength { get; }

            /// <summary>
            /// <para>number of array elements per workitem to be computed, to be buffer-copied. default=1</para>
            /// <para>also default=1 for C# arrays(without ClArray nor FastArr )</para>
            /// <para>number of global workitems * elements per work item must not be greater than buffer size </para>
            /// <para>number of local workitems * elements per work item * number of pipeline blob must not be greater than buffer size  </para>
            /// </summary>
            int numberOfElementsPerWorkItem { get; set; }

            /// <summary>
            /// if contained array is a C++ array, this value is the address alignment of it in bytes
            /// </summary>
            int alignmentBytes { get; set; }
        }


        /// <summary>
        /// Holds ClArray instances, C# arrays and FastArr instances to be computed later
        /// </summary>
        public class ClParameterGroup : ICanCompute, ICanBind
        {
            /// <summary>
            /// not implemented
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

            // lengths of arrays (instead of casting and finding cast type later)
            internal LinkedList<int> arrayLengths = new LinkedList<int>();


            // read whole 
            internal LinkedList<bool> reads = new LinkedList<bool>();

            // read partial 
            internal LinkedList<bool> partialReads = new LinkedList<bool>();

            // write whole
            internal LinkedList<bool> writeAlls = new LinkedList<bool>();


            // write partial 
            internal LinkedList<bool> writes = new LinkedList<bool>();

            // elementsPerWorkItem 
            internal LinkedList<int> arrayElementsPerWorkItem = new LinkedList<int>();

            // read only mode "ro", write only mode "wo"
            internal LinkedList<bool> readOnlys = new LinkedList<bool>();
            internal LinkedList<bool> writeOnlys = new LinkedList<bool>();
            internal LinkedList<bool> zeroCopys = new LinkedList<bool>();

            
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
                LinkedListNode<int> node6 = arrayLengths.First;
                LinkedListNode<bool> node7 = writeAlls.First;
                LinkedListNode<bool> node8 = readOnlys.First;
                LinkedListNode<bool> node9 = writeOnlys.First;
                LinkedListNode<bool> node10 = zeroCopys.First;

                while (node != null)
                {
                    gr.arrays.AddLast(node.Value);
                    gr.reads.AddLast(node2.Value);
                    gr.partialReads.AddLast(node3.Value);
                    gr.writes.AddLast(node4.Value);
                    gr.arrayElementsPerWorkItem.AddLast(node5.Value);
                    gr.arrayLengths.AddLast(node6.Value);
                    gr.writeAlls.AddLast(node7.Value);
                    gr.readOnlys.AddLast(node8.Value);
                    gr.writeOnlys.AddLast(node9.Value);
                    gr.zeroCopys.AddLast(node10.Value);

                    node = node.Next;
                    node2 = node2.Next;
                    node3 = node3.Next;
                    node4 = node4.Next;
                    node5 = node5.Next;
                    node6 = node6.Next;
                    node7 = node7.Next;
                    node8 = node8.Next;
                    node9 = node9.Next;
                    node10 = node10.Next;
                }


                for (int i = 0; i < arrays_.Length; i++)
                {
                    gr.arrays.AddLast(arrays_[i]);

                    // default values for C# arrays (they need to be wrapped in ClArray to be modified)
                    gr.reads.AddLast(true);
                    gr.partialReads.AddLast(true);
                    gr.writes.AddLast(true);
                    gr.arrayElementsPerWorkItem.AddLast(1);
                    gr.arrayLengths.AddLast(arrays_[i].Length);
                    gr.writeAlls.AddLast(false);
                    gr.readOnlys.AddLast(false);
                    gr.writeOnlys.AddLast(false);
                    gr.zeroCopys.AddLast(false);
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
                LinkedListNode<int> node6 = arrayLengths.First;
                LinkedListNode<bool> node7 = writeAlls.First;
                LinkedListNode<bool> node8 = readOnlys.First;
                LinkedListNode<bool> node9 = writeOnlys.First;
                LinkedListNode<bool> node10 = zeroCopys.First;

                while (node != null)
                {
                    gr.arrays.AddLast(node.Value);
                    gr.reads.AddLast(node2.Value);
                    gr.partialReads.AddLast(node3.Value);
                    gr.writes.AddLast(node4.Value);
                    gr.arrayElementsPerWorkItem.AddLast(node5.Value);
                    gr.arrayLengths.AddLast(node6.Value);
                    gr.writeAlls.AddLast(node7.Value);
                    gr.readOnlys.AddLast(node8.Value);
                    gr.writeOnlys.AddLast(node9.Value);
                    gr.zeroCopys.AddLast(node10.Value);

                    node = node.Next;
                    node2 = node2.Next;
                    node3 = node3.Next;
                    node4 = node4.Next;
                    node5 = node5.Next;
                    node6 = node6.Next;
                    node7 = node7.Next;
                    node8 = node8.Next;
                    node9 = node9.Next;
                    node10 = node10.Next;
                }


                for (int i = 0; i < arrays_.Length; i++)
                {
                    gr.arrays.AddLast(arrays_[i]);

                    // default values
                    gr.reads.AddLast(true);
                    gr.partialReads.AddLast(true);
                    gr.writes.AddLast(true);
                    gr.arrayElementsPerWorkItem.AddLast(1);
                    gr.arrayLengths.AddLast(arrays_[i].Length);
                    gr.writeAlls.AddLast(false);
                    gr.readOnlys.AddLast(false);
                    gr.writeOnlys.AddLast(false);
                    gr.zeroCopys.AddLast(false);

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
                LinkedListNode<int> node6 = arrayLengths.First;
                LinkedListNode<bool> node7 = writeAlls.First;
                LinkedListNode<bool> node8 = readOnlys.First;
                LinkedListNode<bool> node9 = writeOnlys.First;
                LinkedListNode<bool> node10 = zeroCopys.First;

                while (node != null)
                {
                    gr.arrays.AddLast(node.Value);
                    gr.reads.AddLast(node2.Value);
                    gr.partialReads.AddLast(node3.Value);
                    gr.writes.AddLast(node4.Value);
                    gr.arrayElementsPerWorkItem.AddLast(node5.Value);
                    gr.arrayLengths.AddLast(node6.Value);
                    gr.writeAlls.AddLast(node7.Value);
                    gr.readOnlys.AddLast(node8.Value);
                    gr.writeOnlys.AddLast(node9.Value);
                    gr.zeroCopys.AddLast(node10.Value);
                    node = node.Next;
                    node2 = node2.Next;
                    node3 = node3.Next;
                    node4 = node4.Next;
                    node5 = node5.Next;
                    node6 = node6.Next;
                    node7 = node7.Next;
                    node8 = node8.Next;
                    node9 = node9.Next;
                    node10 = node10.Next;
                }


                for (int i = 0; i < arrays_.Length; i++)
                {
                    gr.arrays.AddLast(arrays_[i]);
                    gr.reads.AddLast(arrays_[i].read);
                    gr.partialReads.AddLast(arrays_[i].partialRead);
                    gr.writes.AddLast(arrays_[i].write);
                    gr.arrayElementsPerWorkItem.AddLast(arrays_[i].numberOfElementsPerWorkItem);
                    gr.arrayLengths.AddLast(arrays_[i].arrayLength);
                    gr.writeAlls.AddLast(arrays_[i].writeAll);
                    gr.readOnlys.AddLast(arrays_[i].readOnly);
                    gr.writeOnlys.AddLast(arrays_[i].writeOnly);
                    gr.zeroCopys.AddLast(arrays_[i].zeroCopy);
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
                LinkedListNode<int> node06 = arrayLengths.First;
                LinkedListNode<bool> node07 = writeAlls.First;
                LinkedListNode<bool> node08 = readOnlys.First;
                LinkedListNode<bool> node09 = writeOnlys.First;
                LinkedListNode<bool> node010 = zeroCopys.First;

                while (node0 != null)
                {
                    gr.arrays.AddLast(node0.Value);
                    gr.reads.AddLast(node02.Value);
                    gr.partialReads.AddLast(node03.Value);
                    gr.writes.AddLast(node04.Value);
                    gr.arrayElementsPerWorkItem.AddLast(node05.Value);
                    gr.arrayLengths.AddLast(node06.Value);
                    gr.writeAlls.AddLast(node07.Value);
                    gr.readOnlys.AddLast(node08.Value);
                    gr.writeOnlys.AddLast(node09.Value);
                    gr.zeroCopys.AddLast(node010.Value);
                    node0 = node0.Next;
                    node02 = node02.Next;
                    node03 = node03.Next;
                    node04 = node04.Next;
                    node05 = node05.Next;
                    node06 = node06.Next;
                    node07 = node07.Next;
                    node08 = node08.Next;
                    node09 = node09.Next;
                    node010 = node010.Next;
                }


                for (int i = 0; i < parameterGroups_.Length; i++)
                {
                    LinkedListNode<object> node = parameterGroups_[i].arrays.First;
                    LinkedListNode<bool> node2 = parameterGroups_[i].reads.First;
                    LinkedListNode<bool> node3 = parameterGroups_[i].partialReads.First;
                    LinkedListNode<bool> node4 = parameterGroups_[i].writes.First;
                    LinkedListNode<int> node5 = parameterGroups_[i].arrayElementsPerWorkItem.First;
                    LinkedListNode<int> node6 = parameterGroups_[i].arrayLengths.First;
                    LinkedListNode<bool> node7 = parameterGroups_[i].writeAlls.First;
                    LinkedListNode<bool> node8 = parameterGroups_[i].readOnlys.First;
                    LinkedListNode<bool> node9 = parameterGroups_[i].writeOnlys.First;
                    LinkedListNode<bool> node10 = parameterGroups_[i].zeroCopys.First;
                    while (node != null)
                    {
                        if (node.Value != null)
                        {
                            gr.arrays.AddLast(node.Value);
                            gr.reads.AddLast(node2.Value);
                            gr.partialReads.AddLast(node3.Value);
                            gr.writes.AddLast(node4.Value);
                            gr.arrayElementsPerWorkItem.AddLast(node5.Value);
                            gr.arrayLengths.AddLast(node6.Value);
                            gr.writeAlls.AddLast(node7.Value);
                            gr.readOnlys.AddLast(node8.Value);
                            gr.writeOnlys.AddLast(node9.Value);
                            gr.zeroCopys.AddLast(node10.Value);
                        }
                        node = node.Next;
                        node2 = node2.Next;
                        node3 = node3.Next;
                        node4 = node4.Next;
                        node5 = node5.Next;
                        node6 = node6.Next;
                        node7 = node7.Next;
                        node8 = node8.Next;
                        node9 = node9.Next;
                        node10 = node10.Next;
                    }
                }
                return gr;
            }


            /// <summary>
            /// <para>creates task to compute later, meant to be used in pools mainly</para>
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
            public ClTask task(ClNumberCruncher cruncher, int computeId, string kernelNamesString, int globalRange,
                                int localRange = 256, int ofsetGlobalRange = 0, bool pipeline = false,
                                bool pipelineType = Cores.PIPELINE_EVENT, int pipelineBlobs = 4)
            {

                throw new NotImplementedException();
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
            /// <param name="readWritesReady">
            /// <para>used by ClTask. compute() uses predetermined array fields in this array instead of current values.</para>
            /// <para>null = current values will be used</para>
            /// </param>
            /// <param name="elementsPerItemReady">
            /// <para>used by ClTask. compute() uses predetermined per-item-elements-value in this array instead of current values.</para>
            /// <para>null = current values will be used</para>
            /// </param>
            public void compute(ClNumberCruncher cruncher, int computeId, string kernelNamesString, int globalRange,
                                int localRange = 256, int ofsetGlobalRange = 0, bool pipeline = false,
                                bool pipelineType = Cores.PIPELINE_EVENT, int pipelineBlobs = 4, string[] readWritesReady = null, int[] elementsPerItemReady = null)
            {
                if (cruncher.numberOfErrorsHappened > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error: there has been " + (cruncher.numberOfErrorsHappened) + " error(s) before, cannot compute.");
                    Console.WriteLine();
                    return;
                }
                if (cruncher.errorCode() != 0)
                {
                    Console.WriteLine("Number-cruncher C99-device compiling error:");
                    Console.WriteLine(cruncher.errorMessage());
                    cruncher.numberOfErrorsHappened++;
                    return;
                }

                if ((globalRange % localRange) != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Work-size error: global range(" + globalRange + ") is not an integer multiple of local range. Global range has to be exact multiple of local range(" + localRange + ").  This is obliged by OpenCL rules.");
                    Console.WriteLine();
                    cruncher.numberOfErrorsHappened++;
                    return;
                }

                if (pipeline)
                {
                    if ((globalRange % (localRange * pipelineBlobs)) != 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Work-size error: global range(" + globalRange + ") is not an integer multiple of (local range)*(number of pipeline blobs)=(" + (localRange * cruncher.numberCruncher.workers.Length) + ").");
                        Console.WriteLine();
                        cruncher.numberOfErrorsHappened++;
                        return;
                    }
                }

                if (!pipeline)
                {
                    if ((globalRange < (localRange * cruncher.numberCruncher.workers.Length)))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Work-size error: global work size(" + globalRange + ") must be equal to or greater than (number of selected devices)*(local worksize)=(" + (cruncher.numberCruncher.workers.Length * localRange) + ")");
                        Console.WriteLine();
                        cruncher.numberOfErrorsHappened++;
                        return;
                    }
                }
                else
                {
                    if ((globalRange < (localRange * cruncher.numberCruncher.workers.Length * pipelineBlobs)))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Work-size error: global work size must be equal to or greater than (number of selected devices)*(local worksize)*(number of pipeline blobs)=(" + (cruncher.numberCruncher.workers.Length * localRange * pipelineBlobs) + ") if pipelining is enabled.");
                        Console.WriteLine();
                        cruncher.numberOfErrorsHappened++;
                        return;
                    }
                }


                string[] kernelsTmp = kernelNamesString.Split(new string[] { " ", ",", ";", "-", "\n","@" }, StringSplitOptions.RemoveEmptyEntries);

                object[] arrs_ = arrays.ToArray();
                int[] lengths_ = arrayLengths.ToArray();
                string[] reads_ = reads.Select(x => { return x ? " read " : ""; }).ToArray();
                string[] partialReads_ = partialReads.Select(x => { return x ? " partial " : ""; }).ToArray();
                string[] writes_ = writes.Select(x => { return x ? " write " : ""; }).ToArray();
                string[] writeAlls_ = writeAlls.Select(x => { return x ? " all " : ""; }).ToArray();
                string[] readWrites_ = new string[reads_.Length];
                string[] readOnlys_ = readOnlys.Select(x => { return x ? " ro " : ""; }).ToArray();
                string[] writeOnlys_ = writeOnlys.Select(x => { return x ? " wo " : ""; }).ToArray();
                string[] zeroCopys_ = zeroCopys.Select(x => { return x ? " zc " : ""; }).ToArray();
                for (int i = 0; i < readWrites_.Length; i++)
                {
                    StringBuilder sb = new StringBuilder(partialReads_[i]);
                    sb.Append(reads_[i]);
                    sb.Append(writes_[i]);
                    sb.Append(writeAlls_[i]);
                    sb.Append(readOnlys_[i]);
                    sb.Append(writeOnlys_[i]);
                    sb.Append(zeroCopys_[i]);
                    readWrites_[i] = sb.ToString();
                }
                int[] elemPerWorkItem_ = arrayElementsPerWorkItem.ToArray();
                for (int ar = 0; ar < arrs_.Length; ar++)
                {
                    // if there is only "full read" or "full writes"s, no need to check number of elements. Simply whole array is copied
                    if ((partialReads_[ar].Length != 0) || ((writes_[ar].Length != 0) && (writeAlls_[ar].Length == 0))) // write is a full write, writeAll is not
                    {
                        if (lengths_[ar] < (globalRange * elemPerWorkItem_[ar]))
                        {
                            Console.WriteLine();
                            Console.WriteLine("Array-size error: (global range)*(number of array elements per work item)=(" + (globalRange * elemPerWorkItem_[ar]) + ") must be equal to or less than array length (" + (lengths_[ar]) + ").");
                            Console.WriteLine();
                            cruncher.numberOfErrorsHappened++;
                            return;
                        }
                    }
                }

                cruncher.numberCruncher.compute(
                    kernelsTmp, cruncher.repeatCount, cruncher.repeatCount > 1 ? cruncher.repeatKernelName : "",
                    arrs_, readWrites_, elemPerWorkItem_,
                    globalRange, computeId, ofsetGlobalRange,
                    pipeline, pipelineBlobs, pipelineType, localRange);
                if (cruncher.performanceFeed)
                {
                    if ((cruncher.numberCruncher != null))
                        cruncher.numberCruncher.performanceReport(computeId);
                    else
                        Console.WriteLine("Error: Number cruncher core object was not allocated properly.");
                }
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
            ClParameterGroup nextParam(params IBufferOptimization[] arrays_);

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
        public class ClArray<T> : IList<T>, ICanBind, IBufferOptimization, ICanCompute
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

                if (isCSharpArr)
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
                    if (array == null || n == value)
                    {
                        // no array no change 
                    }
                    else
                    {
                        if (isCSharpArr)
                        {
                            T[] aNewArray = new T[N];
                            int iL = Math.Min(n, value);
                            var tmpT = array as T[];

                            Array.ConstrainedCopy((T[])array, 0, aNewArray, 0, iL);
                            if (iL < value)
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
            public ClArray(int n_ = -1, int a_ = 4096)
            {
                isDeleted = false;
                alignmentBytes = a_;
                N = n_;
                if (n_ > 0)
                {
                    if (typeof(T) == typeof(float))
                        array = new ClFloatArray(n_, a_);
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
                readOnly = false; // def
                writeOnly = false; // def
                zeroCopy = false; // def
                write = true; // partial writes
                writeAll = false; // make this true only when single gpu and all elements are needed
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
                        isDeleted = true;
                        ((IMemoryHandle)array).dispose();
                    }
                }
            }

            /// <summary>
            /// if C++ resources are released or not
            /// </summary>
            public bool isDeleted { get; set; }

            /// <summary>
            /// release C++ resources
            /// </summary>
            ~ClArray()
            {
                if (array != null)
                {
                    if (!isCSharpArr)
                    {
                        isDeleted = true;
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
                    if (value)
                    {
                        // if C# array exists, put C++ array in place of it and copy old values
                        if (isCSharpArr)
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
                        else if (!isCSharpArr)
                        {
                            if (array == null)
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
                            if (N > 0)
                                array = new T[N];
                        }
                    }
                }
            }

            private object createFastArr(int arrayElements = 0)
            {
                if (arrayElements == 0)
                    arrayElements = N;

                if (typeof(T) == typeof(float))
                    return new ClFloatArray(arrayElements, alignmentBytes);
                else if (typeof(T) == typeof(double))
                    return new ClDoubleArray(arrayElements, alignmentBytes);
                else if (typeof(T) == typeof(int))
                    return new ClIntArray(arrayElements, alignmentBytes);
                else if (typeof(T) == typeof(uint))
                    return new ClUIntArray(arrayElements, alignmentBytes);
                else if (typeof(T) == typeof(long))
                    return new ClLongArray(arrayElements, alignmentBytes);
                else if (typeof(T) == typeof(char))
                    return new ClCharArray(arrayElements, alignmentBytes);
                else if (typeof(T) == typeof(byte))
                    return new ClByteArray(arrayElements, alignmentBytes);
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
            public static implicit operator ClArray<T>(T[] b)
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

           

            // todo: float int double, ... can work too, with checked boundaries.
            /// <summary>
            /// <para>since there is not array of struct to generic implicit conversion</para>
            /// <para>needs this method to generate a byte array wrapper</para>
            /// <para>pins the wrapped array</para>
            /// </summary>
            /// <param name="s"></param>
            /// <returns></returns>
            public static ClArray<byte> wrapArrayOfStructs(object s)
            {
                if (Functions.isUserDefinedStructArray(s))
                {
                    GCHandle gc = GCHandle.Alloc(s, GCHandleType.Pinned);
                    var asList = s as IList;
                    int n = Marshal.SizeOf(asList[0].GetType()) * asList.Count; // number of total bytes
                    ClArray<byte> result = new ClByteArray(gc.AddrOfPinnedObject(), n,gc);
                    result.numberOfElementsPerWorkItem = Marshal.SizeOf(asList[0].GetType());
                    return result;
                }
                else
                {
                    Console.WriteLine("Error: parameter is not a user-defined struct array");
                    return null;
                }
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
                if (!isCSharpArr)
                {
                    if (!array.isCSharpArr)
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

                bd.arrays.AddLast(this);
                bd.reads.AddLast(read);
                bd.partialReads.AddLast(partialRead);
                bd.writes.AddLast(write);
                bd.arrayElementsPerWorkItem.AddLast(numberOfElementsPerWorkItem);
                bd.arrayLengths.AddLast(Length);
                bd.writeAlls.AddLast(writeAll);
                bd.readOnlys.AddLast(readOnly);
                bd.writeOnlys.AddLast(writeOnly);
                bd.zeroCopys.AddLast(zeroCopy);
                for (int i = 0; i < arrays_.Length; i++)
                {
                    bd.arrays.AddLast(arrays_[i]);

                    // default
                    bd.reads.AddLast(true);
                    bd.partialReads.AddLast(false);
                    bd.writes.AddLast(true);
                    bd.arrayElementsPerWorkItem.AddLast(1);
                    bd.arrayLengths.AddLast(arrays_[i].Length);
                    bd.writeAlls.AddLast(false);
                    bd.readOnlys.AddLast(false);
                    bd.writeOnlys.AddLast(false);
                    bd.zeroCopys.AddLast(false);
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

                bd.arrays.AddLast(this);
                bd.reads.AddLast(read);
                bd.partialReads.AddLast(partialRead);
                bd.writes.AddLast(write);
                bd.arrayElementsPerWorkItem.AddLast(numberOfElementsPerWorkItem);
                bd.arrayLengths.AddLast(Length);
                bd.writeAlls.AddLast(writeAll);
                bd.readOnlys.AddLast(readOnly);
                bd.writeOnlys.AddLast(writeOnly);
                bd.zeroCopys.AddLast(zeroCopy);
                for (int i = 0; i < arrays_.Length; i++)
                {
                    bd.arrays.AddLast(arrays_[i]);

                    // default
                    bd.reads.AddLast(true);
                    bd.partialReads.AddLast(false);
                    bd.writes.AddLast(true);
                    bd.arrayElementsPerWorkItem.AddLast(1);
                    bd.arrayLengths.AddLast(arrays_[i].Length);
                    bd.writeAlls.AddLast(false);
                    bd.readOnlys.AddLast(false);
                    bd.writeOnlys.AddLast(false);
                    bd.zeroCopys.AddLast(false);
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

                bd.arrays.AddLast(this);
                bd.reads.AddLast(read);
                bd.partialReads.AddLast(partialRead);
                bd.writes.AddLast(write);
                bd.arrayElementsPerWorkItem.AddLast(numberOfElementsPerWorkItem);
                bd.arrayLengths.AddLast(Length);
                bd.writeAlls.AddLast(writeAll);
                bd.readOnlys.AddLast(readOnly);
                bd.writeOnlys.AddLast(writeOnly);
                bd.zeroCopys.AddLast(zeroCopy);
                for (int i = 0; i < arrays_.Length; i++)
                {
                    bd.arrays.AddLast(arrays_[i]);

                    // default
                    bd.reads.AddLast(arrays_[i].read);
                    bd.partialReads.AddLast(arrays_[i].partialRead);
                    bd.writes.AddLast(arrays_[i].write);
                    bd.arrayElementsPerWorkItem.AddLast(arrays_[i].numberOfElementsPerWorkItem);
                    bd.arrayLengths.AddLast(arrays_[i].arrayLength);
                    bd.writeAlls.AddLast(arrays_[i].writeAll);
                    bd.readOnlys.AddLast(arrays_[i].readOnly);
                    bd.writeOnlys.AddLast(arrays_[i].writeOnly);
                    bd.zeroCopys.AddLast(arrays_[i].zeroCopy);
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

                bd.arrays.AddLast(this);
                bd.reads.AddLast(read);
                bd.partialReads.AddLast(partialRead);
                bd.writes.AddLast(write);
                bd.arrayElementsPerWorkItem.AddLast(numberOfElementsPerWorkItem);
                bd.arrayLengths.AddLast(Length);
                bd.writeAlls.AddLast(writeAll);
                bd.readOnlys.AddLast(readOnly);
                bd.writeOnlys.AddLast(writeOnly);
                bd.zeroCopys.AddLast(zeroCopy);

                for (int i = 0; i < parameterGroups_.Length; i++)
                {
                    LinkedListNode<object> node = parameterGroups_[i].arrays.First;
                    LinkedListNode<bool> node2 = parameterGroups_[i].reads.First;
                    LinkedListNode<bool> node3 = parameterGroups_[i].partialReads.First;
                    LinkedListNode<bool> node4 = parameterGroups_[i].writes.First;
                    LinkedListNode<int> node5 = parameterGroups_[i].arrayElementsPerWorkItem.First;
                    LinkedListNode<int> node6 = parameterGroups_[i].arrayLengths.First;
                    LinkedListNode<bool> node7 = parameterGroups_[i].writeAlls.First;
                    LinkedListNode<bool> node8 = parameterGroups_[i].readOnlys.First;
                    LinkedListNode<bool> node9 = parameterGroups_[i].writeOnlys.First;
                    LinkedListNode<bool> node10 = parameterGroups_[i].zeroCopys.First;
                    while (node != null)
                    {
                        if (node.Value != null)
                        {
                            bd.arrays.AddLast(node.Value);
                            bd.reads.AddLast(node2.Value);
                            bd.partialReads.AddLast(node3.Value);
                            bd.writes.AddLast(node4.Value);
                            bd.arrayElementsPerWorkItem.AddLast(node5.Value);
                            bd.arrayLengths.AddLast(node6.Value);
                            bd.writeAlls.AddLast(node7.Value);
                            bd.readOnlys.AddLast(node8.Value);
                            bd.writeOnlys.AddLast(node9.Value);
                            bd.zeroCopys.AddLast(node10.Value);
                        }
                        node = node.Next;
                        node2 = node2.Next;
                        node3 = node3.Next;
                        node4 = node4.Next;
                        node5 = node5.Next;
                        node6 = node6.Next;
                        node7 = node7.Next;
                        node8 = node8.Next;
                        node9 = node9.Next;
                        node10 = node10.Next;
                    }
                }
                return bd;
            }



            /// <summary>
            /// <para>creates task to compute later, meant to be used in pools mainly</para>
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
            public ClTask task(ClNumberCruncher cruncher, int computeId, string kernelNamesString, int globalRange,
                                int localRange = 256, int ofsetGlobalRange = 0, bool pipeline = false,
                                bool pipelineType = Cores.PIPELINE_EVENT, int pipelineBlobs = 4)
            {
                string[] kernelsTmp = kernelNamesString.Split(new string[] { " ", ",", ";", "-", "\n", "@" }, StringSplitOptions.RemoveEmptyEntries);

                object[] arrs_ = new object[] { this };
                int[] lengths_ = new int[] { this.Length };
                string[] reads_ = new string[] { read ? " read " : "" };
                string[] partialReads_ = new string[] { partialRead ? " partial " : "" };
                string[] writes_ = new string[] { write ? " write " : "" };
                string[] writeAlls_ = new string[] { writeAll ? " all " : "" };
                string[] readWrites_ = new string[reads_.Length];
                string[] readOnlys_ = new string[] { readOnly ? " ro " : "" };
                string[] writeOnlys_ = new string[] { writeOnly ? " wo " : "" };
                string[] zeroCopys_ = new string[] { zeroCopy ? " zc " : "" };
                for (int i = 0; i < readWrites_.Length; i++)
                {
                    StringBuilder sb = new StringBuilder(partialReads_[i]);
                    sb.Append(reads_[i]);
                    sb.Append(writes_[i]);
                    sb.Append(writeAlls_[i]);
                    sb.Append(readOnlys_[i]);
                    sb.Append(writeOnlys_[i]);
                    sb.Append(zeroCopys_[i]);
                    readWrites_[i] = sb.ToString();
                }
                int[] elemsPerWorkItem_ = new int[] { numberOfElementsPerWorkItem };
                throw new NotImplementedException();
            }

            /// <summary>
            /// <para>blocking compute operation that does read + compute + write unless enqueueMode is enabled in number cruncher </para>
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
            /// <param name="readWritesReady">
            /// <para>used by ClTask. compute() uses predetermined array fields in this array instead of current values.</para>
            /// <para>null = current values will be used</para>
            /// </param>
            /// <param name="elementsPerItemReady">
            /// <para>used by ClTask. compute() uses predetermined per-item-elements-value in this array instead of current values.</para>
            /// <para>null = current values will be used</para>
            /// </param>
            public void compute(ClNumberCruncher cruncher, int computeId, string kernelNamesString, int globalRange,
                                int localRange = 256, int ofsetGlobalRange = 0, bool pipeline = false,
                                bool pipelineType = Cores.PIPELINE_EVENT, int pipelineBlobs = 4, string [] readWritesReady=null, int [] elementsPerItemReady=null)
            {
                
                if(cruncher.numberOfErrorsHappened>0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error: there has been "+(cruncher.numberOfErrorsHappened)+" error(s) before, cannot compute.");
                    Console.WriteLine();
                    return;
                }
                if (cruncher.errorCode() != 0)
                {
                    Console.WriteLine("Number-cruncher C99-device compiling error:");
                    Console.WriteLine(cruncher.errorMessage());
                    cruncher.numberOfErrorsHappened++;
                    return;
                }

                if ((globalRange % localRange) != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Work-size error: global range(" + globalRange + ") is not an integer multiple of local range. Global range has to be exact multiple of local range(" + localRange + ").  This is obliged by OpenCL rules.");
                    Console.WriteLine();
                    cruncher.numberOfErrorsHappened++;
                    return;
                }

                if (pipeline)
                {
                    if ((globalRange % (localRange * pipelineBlobs)) != 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Work-size error: global range(" + globalRange + ") is not an integer multiple of (local range)*(number of pipeline blobs)=(" + (localRange * cruncher.numberCruncher.workers.Length) + ").");
                        Console.WriteLine();
                        cruncher.numberOfErrorsHappened++;
                        return;
                    }
                }

                if (!pipeline)
                {
                    if ((globalRange < (localRange * cruncher.numberCruncher.workers.Length)))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Work-size error: global work size(" + globalRange + ") must be equal to or greater than (number of selected devices)*(local worksize)=(" + (cruncher.numberCruncher.workers.Length * localRange) + ")");
                        Console.WriteLine();
                        cruncher.numberOfErrorsHappened++;
                        return;
                    }
                }
                else
                {
                    if ((globalRange < (localRange * cruncher.numberCruncher.workers.Length * pipelineBlobs)))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Work-size error: global range(" + globalRange + ") must be equal to or greater than (number of selected devices)*(local worksize)*(number of pipeline blobs)=(" + (cruncher.numberCruncher.workers.Length * localRange * pipelineBlobs) + ") if pipelining is enabled.");
                        Console.WriteLine();
                        cruncher.numberOfErrorsHappened++;
                        return;
                    }
                }
                // if there is only "full read" or "full writes" s, no need to check number of elements. Simply whole array is copied
                if (partialRead || (write && !writeAll))
                {
                    if (Length < (globalRange * numberOfElementsPerWorkItem))
                    {
                        Console.WriteLine();
                        Console.WriteLine("Array-size error: (global range)*(number of array elements per work item)=(" + (globalRange * numberOfElementsPerWorkItem) + ") must be equal to or less than array length (" + (Length) + ").");
                        Console.WriteLine();
                        cruncher.numberOfErrorsHappened++;
                        return;
                    }
                }

                string[] kernelsTmp = kernelNamesString.Split(new string[] { " ", ",", ";", "-", "\n","@" }, StringSplitOptions.RemoveEmptyEntries);

                object[] arrs_ = new object[] { this };
                int[] lengths_ = new int[] {this.Length };
                string[] reads_ = new string[] { read ? " read " : "" };
                string[] partialReads_ = new string[] { partialRead ? " partial " : "" };
                string[] writes_ = new string[] { write ? " write " : "" };
                string[] writeAlls_ = new string[] { writeAll  ? " all " : "" };
                string[] readWrites_ = new string[reads_.Length];
                string[] readOnlys_ = new string[] { readOnly ? " ro " : "" };
                string[] writeOnlys_ = new string[] { writeOnly ? " wo " : "" };
                string[] zeroCopys_ = new string[] { zeroCopy ? " zc " : "" };
                for (int i = 0; i < readWrites_.Length; i++)
                {
                    StringBuilder sb = new StringBuilder(partialReads_[i]);
                    sb.Append(reads_[i]);
                    sb.Append(writes_[i]);
                    sb.Append(writeAlls_[i]);
                    sb.Append(readOnlys_[i]);
                    sb.Append(writeOnlys_[i]);
                    sb.Append(zeroCopys_[i]);
                    readWrites_[i] = sb.ToString();
                }
                int[] elemsPerWorkItem_ = new int[] { numberOfElementsPerWorkItem };

                for (int ar = 0; ar < arrs_.Length; ar++)
                {
                    // if there is only "full read" or "full write"s, no need to check number of elements. Simply whole array is copied
                    if ((partialReads_[ar].Length != 0) || ((writes_[ar].Length != 0) && (writeAlls_[ar].Length == 0)))
                    {
                        if (lengths_[ar] < (globalRange * elemsPerWorkItem_[ar]))
                        {
                            Console.WriteLine();
                            Console.WriteLine("Array-size error: (global range)*(number of array elements per work item)=(" + (globalRange * elemsPerWorkItem_[ar]) + ") must be equal to or less than array length (" + (lengths_[ar]) + ").");
                            Console.WriteLine();
                            cruncher.numberOfErrorsHappened++;
                            return;
                        }
                    }
                }

                cruncher.numberCruncher.compute(
                    kernelsTmp, cruncher.repeatCount, cruncher.repeatCount > 1 ? cruncher.repeatKernelName : "",
                    arrs_, readWritesReady==null?readWrites_: readWritesReady, elementsPerItemReady==null? elemsPerWorkItem_: elementsPerItemReady,
                    globalRange, computeId, ofsetGlobalRange,
                    pipeline, pipelineBlobs, pipelineType, localRange);

                if (cruncher.performanceFeed)
                {
                    if ((cruncher.numberCruncher != null))
                        cruncher.numberCruncher.performanceReport(computeId);
                    else
                        Console.WriteLine("Error: Number cruncher core object was not allocated properly.");
                }
                
            }


            /// <summary>
            /// <para>if the device used in cruncher supports zero copy buffer access, this field determines its usage. true=direct RAM access, false=dedicated memory</para>
            /// </summary>
            public bool zeroCopy { get; set; }

            private bool readOnlyPrivate = false;

            /// <summary>
            /// <para>buffers created in devices will be read by kernel and written by host, always</para>
            /// <para>prevents usage of write/writeAll/writeOnly, until it is set to false again</para>
            /// </summary>
            public bool readOnly {
                get { return readOnlyPrivate; }
                set {
                    if (value && !writeOnly)
                    {
                        write = false;
                        writeAll = false;
                        writeOnly = false;
                        readOnlyPrivate = value;
                    }
                    else if(!value)
                    {
                        readOnlyPrivate = value;
                    }
                }
            }


            private bool writeOnlyPrivate = false;

            /// <summary>
            /// <para>buffers created in devices will be written by kernel and read by host, always</para>
            /// <para>prevents usage of read/partialRead/readOnly, until its set to false again</para>
            /// </summary>
            public bool writeOnly {
                get { return writeOnlyPrivate; }
                set {
                    if (value && !readOnly)
                    {
                        read = false;
                        partialRead = false;
                        readOnly = false;
                        writeOnlyPrivate = value;
                    }
                    else if (!value)
                    {
                        writeOnlyPrivate = value;
                    }
                }
            }


            private bool readPrivate = true;

            /// <summary>
            /// reads whole array before compute (no pipelining)
            /// </summary>
            public bool read {
                get { return readPrivate; }
                set {
                    if (!writeOnly)
                    {
                        readPrivate = value;
                    }
                }
            }


            private bool partialReadPrivate = false;


            /// <summary>
            /// <para>partial reads(possibly pipelined)</para>
            /// </summary>
            public bool partialRead
            {
                get { return partialReadPrivate; }
                set
                {
                    if (!writeOnly)
                    {
                        partialReadPrivate = value;
                    }
                }
            }



            private bool writePrivate = true;


            /// <summary>
            /// partial writes (possibly pipelined)
            /// </summary>
            public bool write
            {
                get { return writePrivate; }
                set
                {
                    if (!readOnly)
                    {
                        writePrivate = value;
                    }
                }
            }


            private bool writeAllPrivate = true;


            /// <summary>
            /// whole writes
            /// </summary>
            public bool writeAll
            {
                get { return writeAllPrivate; }
                set
                {
                    if (!readOnly)
                    {
                        writeAllPrivate = value;
                    }
                }
            }

            /// <summary>
            /// <para>number of array elements per workitem, default=1</para>
            /// <para>global range * this number must be smaller than or equal to array size </para>
            /// </summary>
            public int numberOfElementsPerWorkItem { get; set; }

            /// <summary>
            /// <para>for more communication between interfaces</para>
            /// </summary>
            public int arrayLength
            {
                get
                {
                    return Length;
                }
            }

            /// <summary>
            ///  address alignment in bytes, if contained array is a C++ array
            /// </summary>
            public int alignmentBytes
            {
                get; set;
            }

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
}
