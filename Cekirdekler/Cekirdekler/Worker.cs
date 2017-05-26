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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Cekirdekler;
using Cekirdekler.ClArrays;
namespace ClObject
{
    /// <summary>
    /// one worker per device is generated for explicit device management(but implicit for users)
    /// maybe more for implicit pipelining in later versions
    /// </summary>
    internal class Worker
    {

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern int compute(IntPtr hCommandQueue, IntPtr hKernel, IntPtr hGlobalRangeReference, IntPtr hGlobalRange, IntPtr hLocalRange);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern int computeRepeated(IntPtr hCommandQueue, IntPtr hKernel, IntPtr hGlobalRangeReference, IntPtr hGlobalRange, IntPtr hLocalRange, int repeats);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern int computeRepeatedWithSyncKernel(IntPtr hCommandQueue, IntPtr hKernel, IntPtr hGlobalRangeReference, IntPtr hGlobalRange, IntPtr hLocalRange, int repeats, IntPtr hSyncKernel, IntPtr hZeroRange);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern int computeEvent(IntPtr hCommandQueue, IntPtr hKernel, IntPtr hGlobalRangeReference, IntPtr hGlobalRange, IntPtr hLocalRange, IntPtr hEventArr, IntPtr hEvt);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void setKernelArgument(IntPtr hKernel, IntPtr hBuffer, int index);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void finish(IntPtr hCommandQueue);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void flush(IntPtr hCommandQueue);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void wait2(IntPtr hCommandQueue, IntPtr hCommandQueue2);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void wait3(IntPtr hCommandQueue, IntPtr hCommandQueue2, IntPtr hCommandQueue3);

        private ClDevice device = null;
        private ClContext context = null;

        /// <summary>
        /// <para>horizontal(driver-driven) pipelining opencl queue (read + compute +write)</para>
        /// <para>vertical(event-driven) pipelining opencl kernel execution queue</para>
        /// <para>and also non-pipelined simple work</para>
        /// </summary>
        public ClCommandQueue commandQueue = null;

        /// <summary>
        /// event-driven pipelining opencl buffer-write queue (read from C# array or C++ array)
        /// </summary>
        public ClCommandQueue commandQueueRead = null;

        /// <summary>
        /// event-driven pipelining opencl buffer-read queue (write to C# array or C++ array)
        /// </summary>
        public ClCommandQueue commandQueueWrite = null;

        /// <summary>
        /// <para>horizontal(driver-driven) pipelining opencl queue (read + compute +write)</para>
        /// <para>extra vertical(event-driven) pipelining opencl kernel execution queue</para>
        /// </summary>
        public ClCommandQueue commandQueue2 = null;

        /// <summary>
        /// event-driven pipelining opencl buffer-write queue (read from C# array or C++ array)
        /// </summary>
        public ClCommandQueue commandQueueRead2 = null;

        /// <summary>
        /// event-driven pipelining opencl buffer-read queue (write to C# array or C++ array)
        /// </summary>
        public ClCommandQueue commandQueueWrite2 = null;


        /// <summary>
        /// extra horizontal(driver-driven) pipelining opencl queue (read + compute +write) 
        /// </summary>
        public ClCommandQueue commandQueue3 = null;

        /// <summary>
        /// extra horizontal(driver-driven) pipelining opencl queue (read + compute +write) 
        /// </summary>
        public ClCommandQueue commandQueue4 = null;


        /// <summary>
        /// extra horizontal(driver-driven) pipelining opencl queue (read + compute +write) 
        /// </summary>
        public ClCommandQueue commandQueue5 = null;

        /// <summary>
        /// extra horizontal(driver-driven) pipelining opencl queue (read + compute +write) 
        /// </summary>
        public ClCommandQueue commandQueue6 = null;

        /// <summary>
        /// extra horizontal(driver-driven) pipelining opencl queue (read + compute +write) 
        /// </summary>
        public ClCommandQueue commandQueue7 = null;

        /// <summary>
        /// extra horizontal(driver-driven) pipelining opencl queue (read + compute +write) 
        /// </summary>
        public ClCommandQueue commandQueue8 = null;

        /// <summary>
        /// extra horizontal(driver-driven) pipelining opencl queue (read + compute +write) 
        /// </summary>
        public ClCommandQueue commandQueue9 = null;

        /// <summary>
        /// extra horizontal(driver-driven) pipelining opencl queue (read + compute +write) 
        /// </summary>
        public ClCommandQueue commandQueue10 = null;

        /// <summary>
        /// extra horizontal(driver-driven) pipelining opencl queue (read + compute +write) 
        /// </summary>
        public ClCommandQueue commandQueue11 = null;

        /// <summary>
        /// extra horizontal(driver-driven) pipelining opencl queue (read + compute +write) 
        /// </summary>
        public ClCommandQueue commandQueue12 = null;


        /// <summary>
        /// extra horizontal(driver-driven) pipelining opencl queue (read + compute +write) 
        /// </summary>
        public ClCommandQueue commandQueue13 = null;

        /// <summary>
        /// extra horizontal(driver-driven) pipelining opencl queue (read + compute +write) 
        /// </summary>
        public ClCommandQueue commandQueue14 = null;

        /// <summary>
        /// extra horizontal(driver-driven) pipelining opencl queue (read + compute +write) 
        /// </summary>
        public ClCommandQueue commandQueue15 = null;


        /// <summary>
        /// extra horizontal(driver-driven) pipelining opencl queue (read + compute +write) 
        /// </summary>
        public ClCommandQueue commandQueue16 = null;


        private ClString kernelStrings = null;
        private ClString[] kernelNames = null;
        private ClProgram program = null;

        private Dictionary<string, ClKernel> kernels = null;

        /// <summary>
        /// total time
        /// </summary>
        public Dictionary<int,double> benchmark;

        private object lock00 = new object();

        private Dictionary<object, ClBuffer> buffers = null;
        private Dictionary<int, ClNdRange> ranges = null;

        /// <summary>
        /// opencl device name that this worker is bound to
        /// </summary>
        public string deviceName;
        private int programAndKernelErrorCode = 0;
        private string tumHatalar="";

        /// <summary>
        /// <para>creates a worker for a device for a kernel string contains many kernel definitions and names of kernels given explicity</para>
        /// <para>(Cekirdekler API usage style-2 doesn't need kernel names explicitly)</para>
        /// </summary>
        /// <param name="device_">opencl device wrapper</param>
        /// <param name="kernels_">string wrapper containing all kernels</param>
        /// <param name="kernelNames_">names of kernels to be compiled on this device</param>
        /// <param name="noPipelining">if enabled, does not allocate multiple command queues(driver-driven pipelining can't be enabled). Useful for device-to-device pipelining with many stages(to overcome abundant resource usages)</param>
        public Worker(ClDevice device_, ClString kernels_, ClString[] kernelNames_, bool noPipelining = false)
        {
            {
                programAndKernelErrorCode = 0;
                device = device_;
                deviceName = device.name();
                context = new ClContext(device);

                // for event+driver driven pipelines and no-pipeline executions
                commandQueue = new ClCommandQueue(context);

                // for event driven pipelines
                commandQueueRead = new ClCommandQueue(context);

                // for event driven pipelines
                commandQueueWrite = new ClCommandQueue(context);

                // for driver-driven pipelines
                if (!noPipelining)
                {
                    commandQueue2 = new ClCommandQueue(context);
                    commandQueue3 = new ClCommandQueue(context);
                    commandQueue4 = new ClCommandQueue(context);
                    commandQueue5 = new ClCommandQueue(context);
                    commandQueue6 = new ClCommandQueue(context);
                    commandQueue7 = new ClCommandQueue(context);
                    commandQueue8 = new ClCommandQueue(context);
                    commandQueue9 = new ClCommandQueue(context);
                    commandQueue10 = new ClCommandQueue(context);
                    commandQueue11 = new ClCommandQueue(context);
                    commandQueue12 = new ClCommandQueue(context);
                    commandQueue13 = new ClCommandQueue(context);
                    commandQueue14 = new ClCommandQueue(context);
                    commandQueue15 = new ClCommandQueue(context);
                    commandQueue16 = new ClCommandQueue(context);
                }
                // for event driven pipelines
                commandQueueRead2 = new ClCommandQueue(context);
                commandQueueWrite2 = new ClCommandQueue(context);



                kernelStrings = new ClString(kernels_.read());
                kernelNames = new ClString[kernelNames_.Length];
                for (int i = 0; i < kernelNames_.Length; i++)
                {
                    kernelNames[i] = new ClString(kernelNames_[i].read());
                }
                program = new ClProgram(context, kernelStrings);
                tumHatalar += program.errMsg() + Environment.NewLine + "----" + Environment.NewLine;
                programAndKernelErrorCode += program.intProgramError;
                kernels = new Dictionary<string, ClKernel>();
                for (int i = 0; i < kernelNames_.Length; i++)
                {
                    kernels.Add(kernelNames_[i].read(), new ClKernel(program, kernelNames[i]));
                    programAndKernelErrorCode += kernels[kernelNames_[i].read()].intKernelError;
                }
            }
        }

        /// <summary>
        /// device type(code)
        /// </summary>
        /// <returns></returns>
        public int deviceType()
        {
            return device.type();
        }

        /// <summary>
        /// all errors by compiler
        /// </summary>
        /// <returns></returns>
        public string getAllErrors()
        {
            return tumHatalar;
        }

        /// <summary>
        /// indicator of error
        /// </summary>
        /// <returns></returns>
        public int getErrorCode()
        {
            return programAndKernelErrorCode;
        }

        ClUserEvent startingPointEvent = null;

        /// <summary>
        /// experimental user event for synchronized multiple-commandqueue starter, development cancelled 
        /// </summary>
        public void createUserEventForStartingPoint()
        {
            {
                startingPointEvent = new ClUserEvent(context);
            }
        }

        /// <summary>
        /// start synchronization
        /// </summary>
        public void bindStartPointToCommandQueues()
        {
            {
                startingPointEvent.addCommandQueue(commandQueue);
                startingPointEvent.addCommandQueue(commandQueueRead);
                startingPointEvent.addCommandQueue(commandQueueWrite);
            }
        }

        /// <summary>
        /// trigger start action
        /// </summary>
        public void triggerUserEventStartPoint()
        {
            {
                if (startingPointEvent != null)
                    startingPointEvent.trigger();
            }
        }

        /// <summary>
        /// decrement user event counter
        /// </summary>
        public void decrementUserEventCounter()
        {
            {
                if (startingPointEvent != null)
                    startingPointEvent.dec();
            }
        }

        /// <summary>
        /// increment user event counter
        /// </summary>
        public void incrementUserEventCounter()
        {
            {
                if (startingPointEvent != null)
                    startingPointEvent.inc();
            }
        }


        /// <summary>
        /// delete user event start point
        /// </summary>
        public void deleteUserEventStartPoint()
        {
            {
                if (startingPointEvent != null)
                {
                    startingPointEvent.dispose();
                    startingPointEvent = null;
                }
            }
        }

        /// <summary>
        /// true: has dedicated memory
        /// </summary>
        /// <returns></returns>
        public bool gddr()
        {
            return device.isGddr();
        }

        /// <summary>
        /// prepares an opencl buffer for each C# array or other array-like object
        /// </summary>
        /// <param name="arr">float[], double[], long[], int[], byte[] gibi diziler olabilir</param>
        /// <param name="elementsPerThread">number of elements used in each workitem</param>
        public ClBuffer buffer(object arr,int elementsPerThread=1)
        {
            bool isGddr = device.isGddr();


            {
                if (buffers == null)
                    buffers = new Dictionary<object, ClBuffer>();
                if (arr.GetType() == typeof(float[]))
                {
                    int arrayLength = ((float[])arr).Length;
                    if (buffers.ContainsKey(arr))
                    {
                        // no action if already allocated.

                    }
                    else
                    {
                        // allocate once per C# side buffer object
                        ClBuffer buffer = new ClBuffer(context, arrayLength, ClBuffer.SizeOf.cl_float, isGddr);
                        buffers.Add(arr, buffer);
                    }

                }
                else if (Functions.isTypeOfFastArr(arr))
                {
                    // FastArr type array (C++)
                    int arrayLength = ((IMemoryHandle)arr).Length;
                    if (buffers.ContainsKey(arr))
                    {

                    }
                    else
                    {
                        var tmpSizeOf = new ClBuffer.SizeOf();
                        tmpSizeOf =(ClBuffer.SizeOf)( ((IMemoryHandle)arr).sizeOfEnum);
                        ClBuffer buffer = new ClBuffer(context, arrayLength,tmpSizeOf, isGddr,0, ((IMemoryHandle)arr).ha());
                        buffers.Add(arr, buffer);
                    }

                }
                else if (arr.GetType() == typeof(double[]))
                {
                    int arrayLength = ((double[])arr).Length;
                    if (buffers.ContainsKey(arr))
                    {

                    }
                    else
                    {
                        ClBuffer buffer = new ClBuffer(context, arrayLength, ClBuffer.SizeOf.cl_double, isGddr);
                        buffers.Add(arr, buffer);
                    }

                }
                else if (arr.GetType() == typeof(int[]))
                {
                    int arrayLength = ((int[])arr).Length;
                    if (buffers.ContainsKey(arr))
                    {

                    }
                    else
                    {
                        ClBuffer buffer = new ClBuffer(context, arrayLength, ClBuffer.SizeOf.cl_int, isGddr);
                        buffers.Add(arr, buffer);
                    }

                }
                else if (arr.GetType() == typeof(uint[]))
                {
                    int arrayLength = ((uint[])arr).Length;
                    if (buffers.ContainsKey(arr))
                    {

                    }
                    else
                    {
                        ClBuffer buffer = new ClBuffer(context, arrayLength, ClBuffer.SizeOf.cl_uint, isGddr);
                        buffers.Add(arr, buffer);
                    }

                }
                else if (arr.GetType() == typeof(long[]))
                {
                    int arrayLength = ((long[])arr).Length;
                    if (buffers.ContainsKey(arr))
                    {

                    }
                    else
                    {
                        ClBuffer buffer = new ClBuffer(context, arrayLength, ClBuffer.SizeOf.cl_long, isGddr);
                        buffers.Add(arr, buffer);
                    }

                }
                else if (arr.GetType() == typeof(byte[]))
                {
                    int arrayLength = ((byte[])arr).Length;
                    if (buffers.ContainsKey(arr))
                    {

                    }
                    else
                    {
                        ClBuffer buffer = new ClBuffer(context, arrayLength, ClBuffer.SizeOf.cl_char, isGddr);
                        buffers.Add(arr, buffer);
                    }

                }
                else if (arr.GetType() == typeof(char[]))
                {
                    int arrayLength = ((char[])arr).Length;
                    if (buffers.ContainsKey(arr))
                    {

                    }
                    else
                    {
                        ClBuffer buffer = new ClBuffer(context, arrayLength, ClBuffer.SizeOf.cl_half, isGddr);
                        buffers.Add(arr, buffer);
                    }

                }
                else
                {
                    // C++ array
                    // size is byte type
                    if (true)
                    {
                        int arrayLength = elementsPerThread;
                        if (buffers.ContainsKey(arr))
                        {

                        }
                        else
                        {
                            ClBuffer buffer = new ClBuffer(context, arrayLength, ClBuffer.SizeOf.cl_char, isGddr);
                            buffers.Add(arr, buffer);
                        }
                    }
                }
                return buffers[arr];
            }
        }


        /// <summary>
        /// checks if ndrange is already allocated, allocates if hasnt already
        /// </summary>
        /// <param name="num">range value</param>
        /// <returns></returns>
        public ClNdRange range(int num)
        {
            {
                if (ranges == null)
                    ranges = new Dictionary<int, ClNdRange>();


                if (ranges.ContainsKey(num))
                {
                    // no action if already allocated

                }
                else
                {
                    ClNdRange buffer = new ClNdRange(num);
                    ranges.Add(num, buffer);
                }



                return ranges[num];
            }
        }


        private Stopwatch tmpsw = new Stopwatch();
        private object[] arrsTmp = null;

        /// <summary>
        /// prepares benchmark variables for compute profiling
        /// </summary>
        /// <param name="computeId">compute id to differentiate from other compute actions</param>
        public void benchmark0(int computeId)
        {
            {
                if (benchmark == null)
                    benchmark = new Dictionary<int, double>();
                if (benchmark.ContainsKey(computeId))
                {
                    benchmark[computeId] = 0;
                }
                else
                {
                    benchmark.Add(computeId, 0);
                }
            }
        }

        /// <summary>
        /// start benchmarking for load balancing
        /// </summary>
        public void startBench()
        {
            {
                tmpsw.Start();
            }
        }

        /// <summary>
        /// end benchmarking for load balancing
        /// </summary>
        /// <param name="computeId">unique id for compute action</param>
        public void endBench(int computeId)
        {
            {
                tmpsw.Start();
                tmpsw.Stop();
                if (benchmark == null)
                    benchmark = new Dictionary<int, double>();
                if (benchmark.ContainsKey(computeId))
                {
                    benchmark[computeId] += tmpsw.Elapsed.TotalMilliseconds;
                }
                else
                {
                    benchmark.Add(computeId, tmpsw.Elapsed.TotalMilliseconds);
                }
                tmpsw.Reset();
            }
        }


        /// <summary>
        /// read from client array and write to cl buffer (partially or all of it)
        /// </summary>
        /// <param name="arrays">kernel's parameters - arrays from C# or wrappers of C++ arrays</param>
        /// <param name="reference">start point of writing</param>
        /// <param name="range">range of writing</param>
        /// <param name="computeId">compute id of the writing</param>
        /// <param name="readWrite">"read"=read all array, "partial read"=read device share only, "write"=write partial(devie share) kernel results</param>
        /// <param name="elementsPerWorkItem">elements per workitem. example: streaming float4*2 means size=4</param>
        /// <param name="enqueueMode">if true, no synchronization</param>
        public void writeToBuffer(object[] arrays, int reference, int range, int computeId, string[] readWrite, int[] elementsPerWorkItem, bool enqueueMode=false)
        {
            {
                arrsTmp = arrays;
                for (int i = 0; i < arrays.Length; i++)
                {
                    if (readWrite[i].Contains("partial"))
                    {
                        buffer(arrays[i]).writeRanged(commandQueue, reference * elementsPerWorkItem[i], range * elementsPerWorkItem[i], arrays[i]);
                        continue;
                    }
                    if (readWrite[i].Contains("read"))
                        buffer(arrays[i]).write(commandQueue, arrays[i]);

                }
                if(!enqueueMode)
                    finish(commandQueue.h());
            }
        }

        /// <summary>
        /// non partial writes to buffer (read from array)
        /// </summary>
        /// <param name="arrs"></param>
        /// <param name="readWrite"></param>
        public void writeToBufferWithoutPartial(object[] arrs, string[] readWrite)
        {
            {
                arrsTmp = arrs;
                for (int i = 0; i < arrs.Length; i++)
                {
                    if (readWrite[i].Contains("read") && !readWrite[i].Contains("partial"))
                    {
                        buffer(arrs[i]).write(commandQueue, arrs[i]);
                    }
                }
                finish(commandQueue.h());
            }
        }

        // TO DO: add "write all" for non-pipelined execution modes too, add "char []" support wherever byte[] exists

        /// <summary>
        /// non partial reads from buffer (write to array)
        /// </summary>
        /// <param name="arrs"></param>
        /// <param name="readWrite"></param>
        public void readFromBufferAllData(object[] arrs, string[] readWrite)
        {
            {
                arrsTmp = arrs;
                for (int i = 0; i < arrs.Length; i++)
                {
                    if (readWrite[i].Contains("write") && readWrite[i].Contains("all"))
                    {
                        buffer(arrs[i]).write(commandQueue, arrs[i]);
                    }
                }
                finish(commandQueue.h());
            }
        }
        

        /// <summary>
        /// write to buffer(read from array) using queueRead queue
        /// </summary>
        /// <param name="arrs"></param>
        /// <param name="reference"></param>
        /// <param name="range"></param>
        /// <param name="computeId"></param>
        /// <param name="readWrite"></param>
        /// <param name="elementsPerWorkItem"></param>
        public void writeToBufferUsingQueueRead(object[] arrs, int reference, int range, int computeId, string[] readWrite, int[] elementsPerWorkItem)
        {
            {
                arrsTmp = arrs;
                for (int i = 0; i < arrs.Length; i++)
                {
                    if (readWrite[i].Contains("partial"))
                    {
                        buffer(arrs[i]).writeRanged(commandQueueRead, reference * elementsPerWorkItem[i], range * elementsPerWorkItem[i], arrs[i]);
                        continue;
                    }
                    if (readWrite[i].Contains("read"))
                        buffer(arrs[i]).write(commandQueueRead, arrs[i]);

                }
                //finish(commandQueueOku.h());
            }
        }

        /// <summary>
        /// write to buffer, read from array, using queueRead queue, partially
        /// </summary>
        /// <param name="arrs"></param>
        /// <param name="reference"></param>
        /// <param name="range"></param>
        /// <param name="computeId"></param>
        /// <param name="readWrite"></param>
        /// <param name="elementsPerWorkItem"></param>
        public void writeToBufferUsingQueueReadPartial(object[] arrs, int reference, int range, int computeId, string[] readWrite, int[] elementsPerWorkItem)
        {
            {
                arrsTmp = arrs;
                for (int i = 0; i < arrs.Length; i++)
                {
                    if (readWrite[i].Contains("partial"))
                    {
                        buffer(arrs[i]).writeRanged(commandQueueRead, reference * elementsPerWorkItem[i], range * elementsPerWorkItem[i], arrs[i]);
                    }
                }
                //finish(commandQueueOku.h());
            }
        }

        /// <summary>
        /// clFinish for commandqueueread
        /// </summary>
        public void writeToBufferUsingQueueReadFinish()
        {
            {
                finish(commandQueueRead.h());
            }
        }

        /// <summary>
        /// clFlush for commandqueueread
        /// </summary>
        public void writeToBufferUsingQueueReadFlush()
        {
            {
                flush(commandQueueRead.h());
            }
        }

        /// <summary>
        /// clFinish for commandqueueread2
        /// </summary>        
        public void writeToBufferUsingQueueReadFinish2()
        {
            {
                finish(commandQueueRead2.h());
            }
        }

        /// <summary>
        /// clFlush for commandqueueread2
        /// </summary>
        public void writeToBufferUsingQueueReadFlush2()
        {
            {
                flush(commandQueueRead2.h());
            }
        }


        /// <summary>
        /// binds arrays to a kernel as arguments
        /// </summary>
        /// <param name="kernelName"></param>
        /// <param name="arrs"></param>
        /// <param name="numberOfElementsOrBytes"></param>
        public void kernelArgument(string kernelName, object[] arrs, int[] numberOfElementsOrBytes)
        {
            {
                for (int i = 0; i < arrs.Length; i++)
                {
                    setKernelArgument(kernels[kernelName].h(), buffer(arrs[i], numberOfElementsOrBytes[i]).h(), i);
                }
            }
        }

        /// <summary>
        /// compute a kernel without events
        /// </summary>
        /// <param name="kernelName"></param>
        /// <param name="reference"></param>
        /// <param name="globalRange"></param>
        /// <param name="localRange"></param>
        /// <param name="computeId"></param>
        /// <param name="enqueueMode"></param>
        public void compute(string kernelName, int reference, int globalRange, int localRange, int computeId,bool enqueueMode=false)
        {
            {
                int err = compute(commandQueue.h(), kernels[kernelName].h(), this.range(reference).h(), this.range(globalRange).h(), this.range(localRange).h());
            }
        }

        /// <summary>
        /// compute a kernel without events
        /// </summary>
        /// <param name="kernelName"></param>
        /// <param name="reference"></param>
        /// <param name="globalRange"></param>
        /// <param name="localRange"></param>
        /// <param name="computeId"></param>
        /// <param name="repeats"></param>
        /// <param name="enqueueMode"></param>
        public void computeRepeated(string kernelName, int reference, int globalRange, int localRange, int computeId,int repeats,bool enqueueMode=false)
        {
            {
                int err = computeRepeated(commandQueue.h(), kernels[kernelName].h(), this.range(reference).h(), this.range(globalRange).h(), this.range(localRange).h(),repeats);
            }
        }

        /// <summary>
        /// compute a kernel without events
        /// </summary>
        /// <param name="kernelName"></param>
        /// <param name="reference"></param>
        /// <param name="globalRange"></param>
        /// <param name="localRange"></param>
        /// <param name="computeId"></param>
        /// <param name="repeats"></param>
        /// <param name="syncKernelName"></param>
        /// <param name="enqueueMode"></param>
        public void computeRepeatedWithSyncKernel(string kernelName, int reference, int globalRange, int localRange, int computeId, int repeats, string syncKernelName, bool enqueueMode = false)
        {
            {
                int err = computeRepeatedWithSyncKernel(commandQueue.h(), kernels[kernelName].h(), 
                    this.range(reference).h(), this.range(globalRange).h(), 
                    this.range(localRange).h(), repeats,
                    kernels[syncKernelName].h(), this.range(0).h());
            }
        }

        /// <summary>
        /// compute with extra queue1
        /// </summary>
        /// <param name="kernelName"></param>
        /// <param name="reference"></param>
        /// <param name="globalRange"></param>
        /// <param name="localRange"></param>
        /// <param name="computeId"></param>
        public void computeQueue(string kernelName, int reference, int globalRange, int localRange, int computeId)
        {
            {
                int err = compute(commandQueue.h(), kernels[kernelName].h(), this.range(reference).h(), this.range(globalRange).h(), this.range(localRange).h());
                //finish(commandQueue.h());
            }
        }

        /// <summary>
        /// compute with extra queue2 or chosen queue
        /// </summary>
        /// <param name="kernelName"></param>
        /// <param name="reference"></param>
        /// <param name="globalRange"></param>
        /// <param name="localRange"></param>
        /// <param name="computeId"></param>
        /// <param name="eArr"></param>
        /// <param name="e"></param>
        /// <param name="pipelineOrder">pipeline order: 0=1st, 1=2nd pipeline</param>
        /// <param name="cqSelection"></param>
        public void computeQueueEvent(string kernelName, int reference, int globalRange, int localRange, int computeId, ClEventArray eArr, ClEvent e, int pipelineOrder=0,ClCommandQueue cqSelection=null)
        {
            {
                int err = computeEvent(pipelineOrder == 0 ? (cqSelection == null ? commandQueue.h() : cqSelection.h()) : commandQueue2.h(), kernels[kernelName].h(), this.range(reference).h(), this.range(globalRange).h(), this.range(localRange).h(), eArr.h(), e.h());
                //finish(commandQueue.h());
            }
        }

        /// <summary>
        /// clfinish for commandqueue
        /// </summary>
        public void computeQueueFinish()
        {
            {
                finish(commandQueue.h());
            }
        }

        /// <summary>
        /// clflush for commandqueue
        /// </summary>
        public void computeQueueFlush()
        {
            {
                flush(commandQueue.h());
            }
        }

        /// <summary>
        /// clfinish for commandqueue2
        /// </summary>
        public void computeQueueFinish2()
        {
            {
                finish(commandQueue2.h());
            }
        }

        /// <summary>
        /// clflush for commandqueue2
        /// </summary>
        public void computeQueueFlush2()
        {
            {
                flush(commandQueue2.h());
            }
        }

        /// <summary>
        /// clfinish for commandqueue3
        /// </summary>
        public void computeQueueFinish3()
        {
            {
                finish(commandQueue3.h());
            }
        }

        /// <summary>
        /// clflush for commandqueue3
        /// </summary>
        public void computeQueueFlush3()
        {
            {
                flush(commandQueue3.h());
            }
        }


        public void computeQueueFinish4()
        {
            {
                finish(commandQueue4.h());
            }
        }
        public void computeQueueFlush4()
        {
            {
                flush(commandQueue4.h());
            }
        }

        public void computeQueueFinish5()
        {
            {
                finish(commandQueue5.h());
            }
        }
        public void computeQueueFlush5()
        {
            {
                flush(commandQueue5.h());
            }
        }

        public void computeQueueFinish6()
        {
            {
                finish(commandQueue6.h());
            }
        }

        public void computeQueueFlush6()
        {
            { flush(commandQueue6.h()); }
        }

        public void computeQueueFinish7()
        {
            { finish(commandQueue7.h()); }
        }
        public void computeQueueFlush7()
        {
            { flush(commandQueue7.h()); }
        }

        public void computeQueueFinish8()
        {
            { finish(commandQueue8.h()); }
        }
        public void computeQueueFlush8()
        {
            { flush(commandQueue8.h()); }
        }

        public void computeQueueFinish9()
        {
            { finish(commandQueue9.h()); }
        }
        public void computeQueueFlush9()
        {
            { flush(commandQueue9.h()); }
        }

        public void computeQueueFinish10()
        {
            { finish(commandQueue10.h()); }
        }
        public void computeQueueFlush10()
        {
            { flush(commandQueue10.h()); }
        }

        public void computeQueueFinish11()
        {
            { finish(commandQueue11.h()); }
        }
        public void computeQueueFlush11()
        {
            { flush(commandQueue11.h()); }
        }

        public void computeQueueFinish12()
        {
            { finish(commandQueue12.h()); }
        }
        public void computeQueueFlush12()
        {
            { flush(commandQueue12.h()); }
        }

        public void computeQueueFinish13()
        {
            { finish(commandQueue13.h()); }
        }
        public void computeQueueFlush13()
        {
            { flush(commandQueue13.h()); }
        }

        public void computeQueueFinish14()
        {
            { finish(commandQueue14.h()); }
        }
        public void computeQueueFlush14()
        {
            { flush(commandQueue14.h()); }
        }

        public void computeQueueFinish15()
        {
            { finish(commandQueue15.h()); }
        }
        public void computeQueueFlush15()
        {
            { flush(commandQueue15.h()); }
        }


        public void computeQueueFinish16()
        {
            { finish(commandQueue16.h()); }
        }
        public void computeQueueFlush16()
        {
            { flush(commandQueue16.h()); }
        }

        public void waitComputeAndRead()
        {
            { wait2(commandQueueRead.h(), commandQueue.h()); }
            //bekle2(commandQueue.h(), commandQueueOku.h());
        }

        public void waitComputeAndWrite()
        {
            { wait2(commandQueueWrite.h(), commandQueue.h()); }
            //bekle2(commandQueue.h(), commandQueueYaz.h());
        }

        public void waitReadAndWrite()
        {
            { wait2(commandQueueWrite.h(), commandQueueRead.h()); }
            //bekle2(commandQueueOku.h(), commandQueueYaz.h());
        }

        public void waitReadComputeWrite()
        {
            { wait3(commandQueueWrite.h(), commandQueueRead.h(), commandQueue.h()); }
            //bekle3(commandQueue.h(), commandQueueOku.h(), commandQueueYaz.h());
        }

        /// <summary>
        ///  reads from buffer, writes to array
        /// </summary>
        /// <param name="arrs"></param>
        /// <param name="reference"></param>
        /// <param name="globalRange"></param>
        /// <param name="computeId"></param>
        /// <param name="readWrite"></param>
        /// <param name="elementsPerWorkItem"></param>
        /// <param name="enqueueMode"></param>
        public void readFromBuffer(object[] arrs, int reference, int globalRange, int computeId, string[] readWrite, int[] elementsPerWorkItem,bool enqueueMode=false)
        {

            {
                for (int i = 0; i < arrs.Length; i++)
                    if (readWrite[i].Contains("write") && !readWrite[i].Contains("all"))
                        buffer(arrs[i]).read(commandQueue, reference * elementsPerWorkItem[i], globalRange * elementsPerWorkItem[i], arrs[i]);

                if(!enqueueMode)
                    finish(commandQueue.h());
            }
        }

        
        /// <summary>
        /// reads from buffer, writes to array, uses commandqueuewrite
        /// </summary>
        /// <param name="arrs"></param>
        /// <param name="reference"></param>
        /// <param name="globalRange"></param>
        /// <param name="computeId"></param>
        /// <param name="readWrite"></param>
        /// <param name="elementsPerWorkItem"></param>
        public void readFromBufferUsingQueueWrite(object[] arrs, int reference, int globalRange, int computeId, string[] readWrite, int[] elementsPerWorkItem)
        {
            {
                for (int i = 0; i < arrs.Length; i++)
                    if (readWrite[i].Contains("write") && !readWrite[i].Contains("all") )
                        buffer(arrs[i]).read(commandQueueWrite, reference * elementsPerWorkItem[i], globalRange * elementsPerWorkItem[i], arrs[i]);
                //finish(commandQueueYaz.h());
            }
        }



        // 1= yazma işlemi oldu, 0 = olmadı
        //  2 pipeline var. 0 ve 1

        /// <summary>
        /// write to buffer using queueread with event
        /// </summary>
        /// <param name="arrs"></param>
        /// <param name="reference"></param>
        /// <param name="globalRange"></param>
        /// <param name="computeId"></param>
        /// <param name="readWrite"></param>
        /// <param name="elementsPerWorkItem"></param>
        /// <param name="eArr"></param>
        /// <param name="e"></param>
        /// <param name="pipelineOrder">0=1st pipeline, 1=2nd pipeline</param>
        /// <param name="cqSelection"></param>
        /// <returns></returns> 
        public int writeToBufferUsingQueueReadPartialEvent(object[] arrs, int reference, int globalRange,
            int computeId, string[] readWrite, int[] elementsPerWorkItem, ClEventArray eArr, ClEvent e, int pipelineOrder = 0, ClCommandQueue cqSelection = null)
        {
            {
                arrsTmp = arrs;
                int result_ = 0;
                int ctr01 = 0;
                int ctr01start = -1;
                int ctr01end = -1;
                for (int i = 0; i < arrs.Length; i++)
                {
                    if (readWrite[i].Contains("partial"))
                    {
                        if (ctr01start == -1)
                            ctr01start = i;
                        ctr01++;
                        ctr01end = i;
                    }
                }
                for (int i = 0; i < arrs.Length; i++)
                {

                    if (readWrite[i].Contains("partial"))
                    {
                        if (ctr01 == 1)
                        {

                            buffer(arrs[i]).writeRangedEvent(pipelineOrder == 0 ? (cqSelection == null ? commandQueueRead : cqSelection) : commandQueueRead2,
                                reference * elementsPerWorkItem[i],
                                globalRange * elementsPerWorkItem[i],
                                arrs[i], eArr, e);
                            result_ = 1;
                        }
                        else if (ctr01 > 1)
                        {
                            if (i == ctr01start)
                            {
                                ClEvent emptyEvent = new ClEvent();
                                buffer(arrs[i]).writeRangedEvent(pipelineOrder == 0 ? (cqSelection == null ? commandQueueRead : cqSelection) : commandQueueRead2,
                                    reference * elementsPerWorkItem[i],
                                    globalRange * elementsPerWorkItem[i],
                                    arrs[i], eArr, emptyEvent);
                                emptyEvent.dispose();
                            }
                            else if (i == ctr01end)
                            {
                                ClEventArray emptyEventArr = new ClEventArray();
                                buffer(arrs[i]).writeRangedEvent(pipelineOrder == 0 ? (cqSelection == null ? commandQueueRead : cqSelection) : commandQueueRead2,
                                    reference * elementsPerWorkItem[i],
                                    globalRange * elementsPerWorkItem[i],
                                    arrs[i], emptyEventArr, e);
                                emptyEventArr.dispose();
                            }
                            else
                            {
                                ClEvent emptyEvent = new ClEvent();
                                ClEventArray emptyEventArr = new ClEventArray();
                                buffer(arrs[i]).writeRangedEvent(pipelineOrder == 0 ? (cqSelection == null ? commandQueueRead : cqSelection) : commandQueueRead2,
                                    reference * elementsPerWorkItem[i],
                                    globalRange * elementsPerWorkItem[i],
                                    arrs[i], emptyEventArr, emptyEvent);
                                emptyEventArr.dispose();
                                emptyEvent.dispose();
                            }
                            result_ = 1;
                        }
                    }
                }
                return result_;
                //finish(commandQueueOku.h());
            }

        }


        /// <summary>
        /// read from buffer, write to array, use commandqueueWrite with events
        /// </summary>
        /// <param name="arrs"></param>
        /// <param name="reference"></param>
        /// <param name="globalRange"></param>
        /// <param name="computeId"></param>
        /// <param name="readWrite"></param>
        /// <param name="elementsPerWorkItem"></param>
        /// <param name="eArr"></param>
        /// <param name="e"></param>
        /// <param name="pipelineOrder">0:first pipeline, 1:second pipeline</param>
        /// <param name="cqSelection"></param>
        /// <returns>1=has written, 0=no write</returns>
        public int readFromBufferUsingQueueWriteEvent(object[] arrs, int reference, int globalRange, int computeId, string[] readWrite, int[] elementsPerWorkItem, ClEventArray eArr, ClEvent e, int pipelineOrder = 0, ClCommandQueue cqSelection = null)
        {
            {
                int result_ = 0;
                int ctr01 = 0;
                int ctr01start = -1;
                int ctr01end = -1;
                for (int i = 0; i < arrs.Length; i++)
                {
                    // to do: add cancellation of array length check when "all" is selected
                    if (readWrite[i].Contains("write") && !readWrite[i].Contains("all")) // write all = non partial write
                    {
                        if (ctr01start == -1)
                            ctr01start = i;
                        ctr01++;
                        ctr01end = i;
                    }
                }

                for (int i = 0; i < arrs.Length; i++)
                {
                    if (readWrite[i].Contains("write") && !readWrite[i].Contains("all"))
                    {
                        if (ctr01 == 1)
                        {
                            buffer(arrs[i]).readEvent(pipelineOrder == 0 ? (cqSelection == null ? commandQueueWrite : cqSelection) : commandQueueWrite2, reference * elementsPerWorkItem[i],
                                globalRange * elementsPerWorkItem[i], arrs[i],
                                eArr, e);
                            result_ = 1;
                        }
                        else if (ctr01 > 1)
                        {
                            if (i == ctr01start)
                            {
                                ClEvent emptyEvent = new ClEvent();
                                buffer(arrs[i]).readEvent(pipelineOrder == 0 ? (cqSelection == null ? commandQueueWrite : cqSelection) : commandQueueWrite2, reference * elementsPerWorkItem[i],
                                globalRange * elementsPerWorkItem[i], arrs[i],
                                eArr, emptyEvent);
                                emptyEvent.dispose();
                            }
                            else if (i == (ctr01end))
                            {
                                ClEventArray emptyEventarr = new ClEventArray();
                                buffer(arrs[i]).readEvent(pipelineOrder == 0 ? (cqSelection == null ? commandQueueWrite : cqSelection) : commandQueueWrite2, reference * elementsPerWorkItem[i],
                                    globalRange * elementsPerWorkItem[i], arrs[i],
                                     emptyEventarr, e);
                                emptyEventarr.dispose();
                            }
                            else
                            {
                                ClEvent emptyEvent = new ClEvent();
                                ClEventArray emptyEventArr = new ClEventArray();
                                buffer(arrs[i]).readEvent(pipelineOrder == 0 ? (cqSelection == null ? commandQueueWrite : cqSelection) : commandQueueWrite2, reference * elementsPerWorkItem[i],
                                    globalRange * elementsPerWorkItem[i], arrs[i],
                                    emptyEventArr, emptyEvent);
                                emptyEventArr.dispose();
                                emptyEvent.dispose();
                            }
                            result_ = 1;
                        }

                    }
                    //finish(commandQueueYaz.h());
                }
                return result_;

            }
        }



        /// <summary>
        /// clfinish for commandqueuewrite
        /// </summary>
        public void readBufferQueueWriteFinish()
        {
            {
                finish(commandQueueWrite.h());
            }
        }

        /// <summary>
        /// clflush for commandqueuewrite
        /// </summary>
        public void bufferReadQueueWriteFlush()
        {
            {
                flush(commandQueueWrite.h());
            }
        }




        /// <summary>
        /// clfinish for commandqueuewrite2
        /// </summary>
        public void readBufferQueueWriteFinish2()
        {
            { finish(commandQueueWrite2.h()); }
        }

        /// <summary>
        /// clflush for commandqueuewrite2
        /// </summary>
        public void readBufferQueueWriteFlush2()
        {
            { flush(commandQueueWrite2.h()); }
        }


        /// <summary>
        /// release C++ resources
        /// </summary>
        public void dispose()
        {

            {
                if (ranges != null)
                    ranges.Values.ToList<ClNdRange>().ForEach(s => { if (s != null) { s.dispose();s = null; } });
                ranges = null;
                if (buffers != null)
                    buffers.Values.ToList<ClBuffer>().ForEach(s => { if (s != null) { s.dispose();s = null; } });
                buffers = null;
                if (kernels != null)
                    kernels.Values.ToList<ClKernel>().ForEach(s => { if (s != null) { s.dispose();s = null; } });
                kernels = null;
                if (program != null)
                    program.dispose();
                program = null;
                if (kernelNames != null)
                {
                    for (int i = 0; i < kernelNames.Length; i++)
                    {
                        if (kernelNames[i] != null)
                        {
                            kernelNames[i].dispose();
                            kernelNames[i] = null;
                        }
                    }
                }
                kernelNames = null;
                if (kernelStrings != null)
                    kernelStrings.dispose();
                kernelStrings = null;
                if (commandQueue != null)
                    commandQueue.dispose();
                commandQueue = null;
                if (commandQueueRead != null)
                    commandQueueRead.dispose();
                commandQueueRead = null;
                if (commandQueueWrite != null)
                    commandQueueWrite.dispose();
                commandQueueWrite = null;

                if (commandQueue2 != null)
                    commandQueue2.dispose();
                commandQueue2 = null;
                if (commandQueue3 != null)
                    commandQueue3.dispose();
                commandQueue3 = null;
                if (commandQueue4 != null)
                    commandQueue4.dispose();
                commandQueue4 = null;

                if (commandQueue5 != null)
                    commandQueue5.dispose();
                commandQueue5 = null;

                if (commandQueue6 != null)
                    commandQueue6.dispose();
                commandQueue6 = null;

                if (commandQueue7 != null)
                    commandQueue7.dispose();
                commandQueue7 = null;

                if (commandQueue8 != null)
                    commandQueue8.dispose();
                commandQueue8 = null;

                if (commandQueue9 != null)
                    commandQueue9.dispose();
                commandQueue9 = null;

                if (commandQueue10 != null)
                    commandQueue10.dispose();
                commandQueue10 = null;

                if (commandQueue11 != null)
                    commandQueue11.dispose();
                commandQueue11 = null;

                if (commandQueue12 != null)
                    commandQueue12.dispose();
                commandQueue12 = null;

                if (commandQueue13 != null)
                    commandQueue13.dispose();
                commandQueue13 = null;

                if (commandQueue14 != null)
                    commandQueue14.dispose();
                commandQueue14 = null;

                if (commandQueue15 != null)
                    commandQueue15.dispose();
                commandQueue15 = null;

                if (commandQueue16 != null)
                    commandQueue16.dispose();
                commandQueue16 = null;

                if (commandQueueRead2 != null)
                    commandQueueRead2.dispose();
                commandQueueRead2 = null;
                if (commandQueueWrite2 != null)
                    commandQueueWrite2.dispose();
                commandQueueWrite2 = null;


                if (context != null)
                    context.dispose();
                context = null;
                if (device != null)
                    device.dispose();
                device = null;

            }

        }

    }
}
