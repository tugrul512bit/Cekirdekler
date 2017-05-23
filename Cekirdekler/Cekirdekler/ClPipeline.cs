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



using Cekirdekler.ClArrays;
using Cekirdekler.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cekirdekler
{
    namespace Pipeline
    {
        /// <summary>
        /// <para>Explicit pipelining where each device works on a different stage of pipeline, concurrently, after each push()</para>
        /// <para>To be able to push at each iteration, double-buffering is used for inputs</para>
        /// <para>1 push      (1 thread) = (switch buffer):false</para>
        /// <para>1 more push (1 thread) = (switch buffer)(read+compute+write)[stage-0]:false</para>
        /// <para>1 more push (2 threads)= (switch buffer)(read+compute+write)[stage-0] ++ (switch buffer):false</para>
        /// <para>1 more push (2 threads)= (switch buffer)(read+compute+write)[stage-0] ++ (switch buffer)(read+compute+write)[stage-1]:true</para>
        /// </summary>
        public class ClPipeline
        {
            internal static object syncObj = new object();
            internal int counter { get; set; }
            /// <summary>
            /// pushes data(arrays) from entrance stage, returns true if exit stage has result data on its output(and its target arrays)
            /// </summary>
            /// <returns></returns>
            public bool pushData(object [] data = null, object [] popResultsHere=null)
            {
                if (debug)
                {
                    Console.WriteLine("Pipeline running.");
                    if ((stages==null)||(stages.Length == 0))
                    {
                        Console.WriteLine("Zero pipeline stages.");
                        return false;
                    }
                }

                Parallel.For(0, stages.Length*2, i => {
                    if (i < stages.Length)
                    {
                        for (int j = 0; j < stages[i].Length; j++)
                        {
                            stages[i][j].run(); // input(stage i) --> output(stage i)
                            if (debug)
                            {
                                lock (syncObj)
                                {
                                    Console.WriteLine("stage-"+i+"-"+j+" compute time: "+ stages[i][j].elapsedTime);
                                }
                            }
                        }
                    }
                    else
                    {
                        int k = i - stages.Length;
                        for (int j = 0; j < stages[k].Length; j++)
                        {
                            stages[k][j].forwardResults(k,stages.Length-1,data,popResultsHere); // duplicate output(stage i) --> duplicate input(stage i+1)
                        }
                    }
                });

                Parallel.For(0, stages.Length, i => {
                    for (int j = 0; j < stages[i].Length; j++)
                    {
                        if (data == null)
                        {
                            if (i != 0)
                                stages[i][j].switchInputBuffers(); // switch all duplicates with real buffers
                        }
                        else
                        {
                            stages[i][j].switchInputBuffers(); // switch all duplicates with real buffers
                        }

                        if (popResultsHere == null)
                        {
                            if (i != (stages.Length - 1))
                                stages[i][j].switchOutputBuffers(); // switch all duplicates with real buffers
                        }
                        else
                        {
                            stages[i][j].switchOutputBuffers(); // switch all duplicates with real buffers
                        }
                    }
                });

                
                counter++;
                if ((counter > (stages.Length * 2 - 2)) && (data == null) && (popResultsHere == null))
                    return true;
                else if ( (counter > (stages.Length*2 - 1)) && (data != null) && (popResultsHere == null))
                    return true;
                else if ((counter > (stages.Length * 2 - 1)) && (data == null) && (popResultsHere != null))
                    return true;
                else if ((counter > (stages.Length*2)) && (data != null) && (popResultsHere != null))
                    return true;

                return false;
            }

            // multiple layers per stage, each stage push data to next stage
            internal ClPipelineStage[][] stages;
            internal bool debug { get; set; }
            /// <summary>
            /// only created by one of the stages that are bound together
            /// </summary>
            internal ClPipeline(bool debugLog=false)
            {
                debug = debugLog;
                counter = 0;
            }
           
        }

        internal class KernelParameters
        {
            public int globalRange { get; set; }
            public int localRange { get; set; }
        }

        /// <summary>
        /// <para>used to build stages of a pipeline</para>
        /// <para>inputs are interpreted as read-only(partial if multi device stage), outputs are interpreted as write-only</para>
        /// <para>hidden buffers are used for sequential logic (keeping xyz coordinates of an nbody algorithm - for example) </para>
        /// <para>addition order of inputs,hiddens,outputs must be same as kernel arguments </para>
        /// <para>multiple inputs can be bound to a single output(copies same data to all inputs), opposite can't </para>
        /// </summary>
        public class ClPipelineStage
        {
            internal ClNumberCruncher numberCruncher;

            // if this is true, it will compute after the input switch
            internal bool inputHasData = false;

            // just to inform push() of whole pipeline
            internal bool outputHasData = false;

            internal ClPipelineStageBuffer[] inputBuffers; 
            internal List<ClPipelineStageBuffer> inputBuffersList;

            internal ClPipelineStageBuffer[] outputBuffers; 
            internal List<ClPipelineStageBuffer> outputBuffersList;

            internal ClPipelineStageBuffer[] hiddenBuffers;
            internal List<ClPipelineStageBuffer> hiddenBuffersList;

            internal Dictionary<string,  KernelParameters> kernelNameToParameters;
            internal Dictionary<string,  KernelParameters> initKernelNameToParameters;
            internal ClDevices devices;

            internal string [] kernelNamesToRun;
            internal string kernelsToCompile;

            internal ClPipelineStage previousStage;
            internal ClPipelineStage[] nextStages;
            internal List<ClPipelineStage> nextStagesList;
            internal static object syncObj=new object();
            internal bool initComplete;

            private bool debug { get; set; }

            /// <summary>
            /// creates a stage to form a pipeline with other stages
            /// </summary>
            public ClPipelineStage(bool debugLog=false)
            {
                enqueueMode = false;
                initializerKernelNames = null;
                debug = debugLog;
                initComplete = false;
                nextStagesList = new List<ClPipelineStage>();
                inputBuffersList = new List<ClPipelineStageBuffer>();
                outputBuffersList = new List<ClPipelineStageBuffer>();
                hiddenBuffersList = new List<ClPipelineStageBuffer>();
                kernelNameToParameters = new Dictionary<string, KernelParameters>();
                initKernelNameToParameters = new Dictionary<string, KernelParameters>();
                stageOrder = -1;
            }

            internal Stopwatch timer { get; set; }
            internal double elapsedTime { get; set; }

            /// <summary>
            /// enables enqueued execution of all kernels given as a list, false by default(not enabled)
            /// </summary>
            public bool enqueueMode { get; set; }
            // switch inputs(concurrently all stages) then compute(concurrently all stages, if they received input)
            /// <summary>
            /// runs kernels attached to this stage consecutively (one after another)
            /// </summary>
            /// <param name="initializerKernels">runs only the initializer kernels given by initializerKernel() method</param>
            internal void run(bool initializerKernels=false)
            {
                if (timer == null)
                    timer = new Stopwatch();
                if (debug)
                    Console.WriteLine("pipeline stage running.");
                // initialize buffers and number cruncher
                if (!initComplete)
                {
                    lock (syncObj)
                    {
                        if (numberCruncher == null)
                        {
                            numberCruncher = new ClNumberCruncher(devices, kernelsToCompile,true/* can't enable driver-pipelining but can have more device-pipeline-stages */);
                            if (debug)
                            {
                                numberCruncher.performanceFeed = true;
                                Console.WriteLine("number cruncher setup complete");
                            }
                        }

                        if (inputBuffers != null)
                        {
                            for (int i = 0; i < inputBuffers.Length; i++)
                            {
                                inputBuffers[i].write = false;
                                inputBuffers[i].partialRead = false;
                                inputBuffers[i].read = true;
                            }

                            if (debug)
                                Console.WriteLine("input buffer write flag is unset, read is set, partialread is unset");
                        }

                        if (outputBuffers != null)
                        {
                            for (int i = 0; i < outputBuffers.Length; i++)
                            {
                                outputBuffers[i].write = true;
                                outputBuffers[i].read = false;
                                outputBuffers[i].partialRead = false;
                            }

                            if (debug)
                                Console.WriteLine("output buffer read-partialread flag is unset, write is set");
                        }

                        if (hiddenBuffers != null)
                        {
                            // hidden buffers don't write/read unless its multi gpu
                            // todo: multi-gpu stage buffers will sync
                            for (int i = 0; i < hiddenBuffers.Length; i++)
                            {
                                hiddenBuffers[i].read = false;
                                hiddenBuffers[i].partialRead = false;
                                hiddenBuffers[i].write = false;
                            }

                            if (debug)
                                Console.WriteLine("hidden buffer read-partialread-write flag is unset");
                        }

                        initComplete = true;

                        if (debug)
                            Console.WriteLine("pipeline initialization complete.");
                    }
                }

                timer.Start();

                // to do: move parameter building to initializing
                ClParameterGroup bufferParameters = null;
                ClPipelineStageBuffer ib = null;
                int inputStart = 0, hiddenStart = 0, outputStart = 0;
                if ((inputBuffers != null) && (inputBuffers.Length > 0))
                {
                    ib = inputBuffers[0];
                    inputStart = 1;
                }
                else if ((hiddenBuffers != null) && (hiddenBuffers.Length > 0))
                {
                    ib = hiddenBuffers[0];
                    hiddenStart = 1;
                }
                else if ((outputBuffers != null) && (outputBuffers.Length > 0))
                {
                    ib = outputBuffers[0];
                    outputStart = 1;
                }
                else
                {
                    Console.WriteLine("no buffer found.");
                }

                if(debug)
                {
                    Console.WriteLine("input start = "+inputStart);
                    Console.WriteLine("hidden start = "+hiddenStart);
                    Console.WriteLine("output start = "+outputStart);
                }

                bool parameterStarted = false;
                bool moreThanOneParameter = false;

                if (inputBuffers != null)
                {
                    for (int i = inputStart; i < inputBuffers.Length; i++)
                    {
                        if (!parameterStarted)
                        {
                            bufferParameters = ib.nextParam(inputBuffers[i].buf);

                            parameterStarted = true;
                        }
                        else
                        {
                            bufferParameters = bufferParameters.nextParam(inputBuffers[i].buf);
                        }
                        moreThanOneParameter = true;
                    }
                }

                if (hiddenBuffers != null)
                {
                    for (int i = hiddenStart; i < hiddenBuffers.Length; i++)
                    {
                        if (!parameterStarted)
                        {
                            bufferParameters = ib.nextParam(hiddenBuffers[i].buf);
                            parameterStarted = true;
                        }
                        else
                        {
                            bufferParameters = bufferParameters.nextParam(hiddenBuffers[i].buf);
                        }
                        moreThanOneParameter = true;
                    }
                }

                if (outputBuffers != null)
                {
                    for (int i = outputStart; i < outputBuffers.Length; i++)
                    {
                        if (!parameterStarted)
                        {
                            bufferParameters = ib.nextParam(outputBuffers[i].buf);
                            parameterStarted = true;
                        }
                        else
                        {
                            bufferParameters = bufferParameters.nextParam(outputBuffers[i].buf);
                        }
                        moreThanOneParameter = true;
                    }
                }

                if (debug)
                    Console.WriteLine("kernel parameters are set");

                // parameter building end

                // running kernel
                if (!initializerKernels)
                {
                    if (enqueueMode)
                        if (hiddenBuffers != null)
                        {
                            // hidden buffers don't write/read unless its multi gpu
                            // todo: multi-gpu stage buffers will sync
                            for (int i = 0; i < hiddenBuffers.Length; i++)
                            {
                                hiddenBuffers[i].read = false;
                                hiddenBuffers[i].partialRead = false;
                                hiddenBuffers[i].write = false;

                                // to do: test, delete
                                var rd = bufferParameters.reads.First;
                                var wr = bufferParameters.writes.First;
                                var rp = bufferParameters.partialReads.First;
                                var arrs = bufferParameters.arrays.First;
                                for (int k = 0; k < bufferParameters.reads.Count; k++)
                                {
                                    if ((arrs.Value == hiddenBuffers[i].buf) || (arrs.Value == hiddenBuffers[i].bufDuplicate))
                                    {
                                        rd.Value = true; wr.Value = false; rp.Value = false;
                                    }
                                    rd = rd.Next; wr = wr.Next; arrs = arrs.Next; rp = rp.Next;
                                }
                            }

                        }

                    // normal kernel execution
                    if (kernelNamesToRun != null)
                    {
                        if (enqueueMode)
                            numberCruncher.enqueueMode = true;
                        for (int i = 0; i < kernelNamesToRun.Length; i++)
                        {
                            if (enqueueMode)
                            {
                                if (i == 0)
                                {
                                    if (inputBuffers != null)
                                    {
                                        for (int j = 0; j < inputBuffers.Length; j++)
                                        {
                                            inputBuffers[j].write = false;
                                            inputBuffers[j].partialRead = false;
                                            inputBuffers[j].read = true;

                                            // to do: test, delete
                                            var rd = bufferParameters.reads.First;
                                            var wr = bufferParameters.writes.First;
                                            var rp = bufferParameters.partialReads.First;
                                            var arrs = bufferParameters.arrays.First;
                                            for (int k = 0; k < bufferParameters.reads.Count; k++)
                                            {
                                                if ((arrs.Value == inputBuffers[j].buf) || (arrs.Value == inputBuffers[j].bufDuplicate))
                                                {
                                                    rd.Value = true; wr.Value = false;rp.Value = false;
                                                }
                                                rd = rd.Next; wr = wr.Next; arrs = arrs.Next; rp = rp.Next;
                                            }
                                        }
                                    }

                                    if (outputBuffers != null)
                                    {
                                        for (int j = 0; j < outputBuffers.Length; j++)
                                        {
                                            outputBuffers[j].write = false;
                                            outputBuffers[j].read = false;
                                            outputBuffers[j].partialRead = false;

                                            // to do: test, delete
                                            var rd = bufferParameters.reads.First;
                                            var wr = bufferParameters.writes.First;
                                            var rp = bufferParameters.partialReads.First;
                                            var arrs = bufferParameters.arrays.First;
                                            for (int k = 0; k < bufferParameters.reads.Count; k++)
                                            {
                                                if ((arrs.Value == outputBuffers[j].buf) || (arrs.Value == outputBuffers[j].bufDuplicate))
                                                {
                                                    rd.Value = false; wr.Value = false; rp.Value = false;
                                                }
                                                rd = rd.Next; wr = wr.Next; arrs = arrs.Next; rp = rp.Next;
                                            }
                                        }
                                    }
                                }
                                else if (i == 1)
                                {
                                    if (inputBuffers != null)
                                    {
                                        for (int j = 0; j < inputBuffers.Length; j++)
                                        {
                                            inputBuffers[j].write = false;
                                            inputBuffers[j].partialRead = false;
                                            inputBuffers[j].read = false;

                                            // to do: test, delete
                                            var rd = bufferParameters.reads.First;
                                            var wr = bufferParameters.writes.First;
                                            var rp = bufferParameters.partialReads.First;
                                            var arrs = bufferParameters.arrays.First;
                                            for (int k = 0; k < bufferParameters.reads.Count; k++)
                                            {
                                                if ((arrs.Value == inputBuffers[j].buf) || (arrs.Value == inputBuffers[j].bufDuplicate))
                                                {
                                                    rd.Value = false; wr.Value = false; rp.Value = false;
                                                }
                                                rd = rd.Next; wr = wr.Next; arrs = arrs.Next; rp = rp.Next;
                                            }
                                        }
                                    }
                                }

                                if (i == (kernelNamesToRun.Length - 1))
                                {
                                    if (outputBuffers != null)
                                    {
                                        for (int j = 0; j < outputBuffers.Length; j++)
                                        {
                                            outputBuffers[j].write = true;
                                            outputBuffers[j].read = false;
                                            outputBuffers[j].partialRead = false;

                                            // to do: test, delete
                                            var rd = bufferParameters.reads.First;
                                            var wr = bufferParameters.writes.First;
                                            var rp = bufferParameters.partialReads.First;
                                            var arrs = bufferParameters.arrays.First;
                                            for (int k = 0; k < bufferParameters.reads.Count; k++)
                                            {
                                                if ((arrs.Value == outputBuffers[j].buf) || (arrs.Value == outputBuffers[j].bufDuplicate))
                                                {
                                                    rd.Value = true; wr.Value = true; rp.Value = false;
                                                }
                                                rd = rd.Next; wr = wr.Next; arrs = arrs.Next; rp = rp.Next;
                                            }
                                        }

                                    }
                                }
                            }

                            if (debug)
                            {
                                Console.WriteLine("running kernel: " + i);
                                Console.WriteLine("more than one parameter: " + moreThanOneParameter);
                            }



                            // normal run
                            if (moreThanOneParameter)
                            {
                                bufferParameters.compute(numberCruncher, i + 1,
                                    kernelNamesToRun[i],
                                    kernelNameToParameters[kernelNamesToRun[i]].globalRange,
                                    kernelNameToParameters[kernelNamesToRun[i]].localRange);
                            }
                            else
                            {
                                (ib.buf as ICanCompute).compute(numberCruncher, i * 123456 + 1,
                                    kernelNamesToRun[i],
                                    kernelNameToParameters[kernelNamesToRun[i]].globalRange,
                                    kernelNameToParameters[kernelNamesToRun[i]].localRange);
                            }


                            if (debug)
                                Console.WriteLine("kernel complete: " + i);
                        }
                        if (enqueueMode)
                            numberCruncher.enqueueMode = false;
                    }
                    else
                    {
                        if (debug)
                            Console.WriteLine("no kernel names to run");
                    }
                }
                else
                {
                    // initializing stage
                    if (initializerKernelNames != null)
                    {
                        for (int i = 0; i < initializerKernelNames.Length; i++)
                        {
                            if (debug)
                            {
                                Console.WriteLine("running kernel: " + i);
                                Console.WriteLine("more than one parameter: " + moreThanOneParameter);
                            }


                            // normal run
                            if (moreThanOneParameter)
                            {
                                
                                bufferParameters.compute(numberCruncher, i*500 + 1,
                                    initializerKernelNames[i],
                                    initializerKernelGlobalRanges[i],
                                    initializerKernelLocalRanges[i]);
                            }
                            else
                            {
                                (ib.buf as ICanCompute).compute(numberCruncher, i * 12345678 + 1,
                                    initializerKernelNames[i],
                                    initializerKernelGlobalRanges[i],
                                    initializerKernelLocalRanges[i]);
                            }


                            if (debug)
                                Console.WriteLine("kernel complete: " + i);
                        }
                    }
                    else
                    {
                        if (debug)
                            Console.WriteLine("no kernel names to run");
                    }
                }
                timer.Stop();
                elapsedTime = timer.Elapsed.TotalMilliseconds;
                timer.Reset();
            }


            // double buffering for overlapped stages for multi device usage
            internal void switchInputBuffers()
            {
                for (int i = 0; i < inputBuffers.Length; i++)
                    inputBuffers[i].switchBuffers();
            }

            // double buffering for overlapped stages for multi device usage
            internal void switchOutputBuffers()
            {
                for (int i = 0; i < outputBuffers.Length; i++)
                    outputBuffers[i].switchBuffers();
            }

            // copy from output duplicates to input duplicates while real outputs and real inputs are computed concurrently
            // index is current index to check against zero or maxIndex
            // if it is zero and if data is given, gets data to its input (duplicate one)
            // if it is maxIndex and if result is given, gets output to result
            internal void forwardResults(int index,int maxIndex,object []data,object [] result)
            {
                // has data to be pushed to duplicated input because real input is in use
                if((index==0) && (data!=null))
                {
                    if(data.Length!=inputBuffers.Length)
                    {
                        Console.WriteLine("error: inconsistent number of input arrays and data arrays.");
                        // to do: add error code whenever error happened. Then don't run pipeline if error code is not zero
                        return;
                    }

                    // to do: if there are enough threads, can make this a parallel.for loop
                    for(int i=0;i<data.Length;i++)
                    {
                        var asArray = data[i] as Array;
                        if(asArray!=null)
                        {
                            // given element is a C# array(of float,int,..byte,struct)
                            if(data[i].GetType()==typeof(float[]))
                            {
                                if(asArray.Length!=inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if(inputBuffers[i].eType!=ElementType.ELM_FLOAT)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<float>;
                                destination.CopyFrom((float [])asArray, 0);
                            }
                            else if (data[i].GetType() == typeof(double[]))
                            {
                                if (asArray.Length != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_DOUBLE)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }
                                var destination = inputBuffers[i].switchedBuffer() as ClArray<double>;
                                destination.CopyFrom((double[])asArray, 0);
                            }
                            else if (data[i].GetType() == typeof(byte[]))
                            {
                                if (asArray.Length != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_BYTE)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }
                                var destination = inputBuffers[i].switchedBuffer() as ClArray<byte>;
                                destination.CopyFrom((byte[])asArray, 0);
                            }
                            else if (data[i].GetType() == typeof(char[]))
                            {
                                if (asArray.Length != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_CHAR)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }
                                var destination = inputBuffers[i].switchedBuffer() as ClArray<char>;
                                destination.CopyFrom((char[])asArray, 0);
                            }
                            else if (data[i].GetType() == typeof(int[]))
                            {
                                if (asArray.Length != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_INT)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }
                                var destination = inputBuffers[i].switchedBuffer() as ClArray<int>;
                                destination.CopyFrom((int[])asArray, 0);
                            }
                            else if (data[i].GetType() == typeof(uint[]))
                            {
                                if (asArray.Length != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_UINT)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }
                                var destination = inputBuffers[i].switchedBuffer() as ClArray<uint>;
                                destination.CopyFrom((uint[])asArray, 0);
                            }
                            else if (data[i].GetType() == typeof(long[]))
                            {
                                if (asArray.Length != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_LONG)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }
                                var destination = inputBuffers[i].switchedBuffer() as ClArray<long>;
                                destination.CopyFrom((long[])asArray, 0);
                            }
                            else
                            {
                                Console.WriteLine("error: array of structs for device-to-device pipeline not implemented yet.");
                                throw new NotImplementedException();
                            }




                        }

                        var asFastArray = data[i] as IMemoryHandle;
                        if(asFastArray!=null)
                        {
                            if (data[i].GetType() == typeof(ClFloatArray))
                            {
                                if (asFastArray.Length != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_FLOAT)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<float>;
                                destination.CopyFrom((ClFloatArray)asFastArray, 0);
                            }
                            else if (data[i].GetType() == typeof(ClDoubleArray))
                            {
                                if (asFastArray.Length != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_DOUBLE)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<double>;
                                destination.CopyFrom((ClDoubleArray)asFastArray, 0);
                            }
                            else if (data[i].GetType() == typeof(ClByteArray))
                            {
                                if (asFastArray.Length != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_BYTE)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<byte>;
                                destination.CopyFrom((ClByteArray)asFastArray, 0);
                            }
                            else if (data[i].GetType() == typeof(ClCharArray))
                            {
                                if (asFastArray.Length != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_CHAR)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<char>;
                                destination.CopyFrom((ClCharArray)asFastArray, 0);
                            }
                            else if (data[i].GetType() == typeof(ClIntArray))
                            {
                                if (asFastArray.Length != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_INT)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<int>;
                                destination.CopyFrom((ClIntArray)asFastArray, 0);
                            }
                            else if (data[i].GetType() == typeof(ClUIntArray))
                            {
                                if (asFastArray.Length != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_UINT)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<uint>;
                                destination.CopyFrom((ClUIntArray)asFastArray, 0);
                            }
                            else if (data[i].GetType() == typeof(ClLongArray))
                            {
                                if (asFastArray.Length != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_LONG)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<long>;
                                destination.CopyFrom((ClLongArray)asFastArray, 0);
                            }

                        }

                        var asClArray = data[i] as IBufferOptimization;
                        if (asClArray != null)
                        {
                            if (data[i].GetType() == typeof(ClArray<float>))
                            {
                                if (asClArray.arrayLength != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_FLOAT)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<float>;
                                destination.CopyFrom((ClArray<float>)asClArray, 0);
                            }
                            else if (data[i].GetType() == typeof(ClArray<double>))
                            {
                                if (asClArray.arrayLength != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_DOUBLE)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<double>;
                                destination.CopyFrom((ClArray<double>)asClArray, 0);
                            }
                            else if (data[i].GetType() == typeof(ClArray<byte>))
                            {
                                if (asClArray.arrayLength != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_BYTE)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<byte>;
                                destination.CopyFrom((ClArray<byte>)asClArray, 0);
                            }
                            else if (data[i].GetType() == typeof(ClArray<char>))
                            {
                                if (asClArray.arrayLength != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_CHAR)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<char>;
                                destination.CopyFrom((ClArray<char>)asClArray, 0);
                            }
                            else if (data[i].GetType() == typeof(ClArray<int>))
                            {
                                if (asClArray.arrayLength != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_INT)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<int>;
                                destination.CopyFrom((ClArray<int>)asClArray, 0);
                            }
                            else if (data[i].GetType() == typeof(ClArray<uint>))
                            {
                                if (asClArray.arrayLength != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_UINT)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<uint>;
                                destination.CopyFrom((ClArray<uint>)asClArray, 0);
                            }
                            else if (data[i].GetType() == typeof(ClArray<long>))
                            {
                                if (asClArray.arrayLength != inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of input arrays and length of data arrays.");
                                    return;
                                }

                                if (inputBuffers[i].eType != ElementType.ELM_LONG)
                                {
                                    Console.WriteLine("error: inconsistent types of input and data arrays.");
                                    return;
                                }

                                var destination = inputBuffers[i].switchedBuffer() as ClArray<long>;
                                destination.CopyFrom((ClArray<long>)asClArray, 0);
                            }
                        }


                    }
                }

                // has result arrays to be received data from duplicated outputs because real output is in use
                if((index==maxIndex) && (result!=null))
                {
                    // to do: convert to output version from this input version
                    // *******************************************************************************************
                    // *******************************************************************************************
                    // *******************************************************************************************
                    if (result.Length != outputBuffers.Length)
                    {
                        Console.WriteLine("error: inconsistent number of output arrays and result arrays.");
                        // to do: add error code whenever error happened. Then don't run pipeline if error code is not zero
                        return;
                    }

                    // to do: if there are enough threads, can make this a parallel.for loop
                    for (int i = 0; i < result.Length; i++)
                    {
                        var asArray = result[i] as Array;
                        if (asArray != null)
                        {
                            // given element is a C# array(of float,int,..byte,struct)
                            if (result[i].GetType() == typeof(float[]))
                            {
                                if (asArray.Length != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_FLOAT)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<float>;
                                source.CopyTo((float[])asArray, 0);
                            }
                            else if (result[i].GetType() == typeof(double[]))
                            {
                                if (asArray.Length != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_DOUBLE)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }
                                var source = outputBuffers[i].switchedBuffer() as ClArray<double>;
                                source.CopyTo((double[])asArray, 0);
                            }
                            else if (result[i].GetType() == typeof(byte[]))
                            {
                                if (asArray.Length != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_BYTE)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }
                                var source = outputBuffers[i].switchedBuffer() as ClArray<byte>;
                                source.CopyTo((byte[])asArray, 0);
                            }
                            else if (result[i].GetType() == typeof(char[]))
                            {
                                if (asArray.Length != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_CHAR)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }
                                var source = outputBuffers[i].switchedBuffer() as ClArray<char>;
                                source.CopyTo((char[])asArray, 0);
                            }
                            else if (result[i].GetType() == typeof(int[]))
                            {
                                if (asArray.Length != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_INT)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }
                                var source = outputBuffers[i].switchedBuffer() as ClArray<int>;
                                source.CopyTo((int[])asArray, 0);
                            }
                            else if (result[i].GetType() == typeof(uint[]))
                            {
                                if (asArray.Length != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_UINT)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }
                                var source = outputBuffers[i].switchedBuffer() as ClArray<uint>;
                                source.CopyTo((uint[])asArray, 0);
                            }
                            else if (result[i].GetType() == typeof(long[]))
                            {
                                if (asArray.Length != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_LONG)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }
                                var source = outputBuffers[i].switchedBuffer() as ClArray<long>;
                                source.CopyTo((long[])asArray, 0);
                            }
                            else
                            {
                                Console.WriteLine("error: array of structs for device-to-device pipeline not implemented yet.");
                                throw new NotImplementedException();
                            }




                        }

                        var asFastArray = result[i] as IMemoryHandle;
                        if (asFastArray != null)
                        {
                            if (result[i].GetType() == typeof(ClFloatArray))
                            {
                                if (asFastArray.Length != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_FLOAT)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<float>;
                                source.CopyTo((ClFloatArray)asFastArray, 0);
                            }
                            else if (result[i].GetType() == typeof(ClDoubleArray))
                            {
                                if (asFastArray.Length != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_DOUBLE)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<double>;
                                source.CopyTo((ClDoubleArray)asFastArray, 0);
                            }
                            else if (result[i].GetType() == typeof(ClByteArray))
                            {
                                if (asFastArray.Length != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_BYTE)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<byte>;
                                source.CopyTo((ClByteArray)asFastArray, 0);
                            }
                            else if (result[i].GetType() == typeof(ClCharArray))
                            {
                                if (asFastArray.Length != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_CHAR)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<char>;
                                source.CopyTo((ClCharArray)asFastArray, 0);
                            }
                            else if (result[i].GetType() == typeof(ClIntArray))
                            {
                                if (asFastArray.Length != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_INT)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<int>;
                                source.CopyTo((ClIntArray)asFastArray, 0);
                            }
                            else if (result[i].GetType() == typeof(ClUIntArray))
                            {
                                if (asFastArray.Length != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_UINT)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<uint>;
                                source.CopyTo((ClUIntArray)asFastArray, 0);
                            }
                            else if (result[i].GetType() == typeof(ClLongArray))
                            {
                                if (asFastArray.Length != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_LONG)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<long>;
                                source.CopyTo((ClLongArray)asFastArray, 0);
                            }

                        }

                        var asClArray = result[i] as IBufferOptimization;
                        if (asClArray != null)
                        {
                            if (result[i].GetType() == typeof(ClArray<float>))
                            {
                                if (asClArray.arrayLength != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_FLOAT)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<float>;
                                source.CopyTo((ClArray<float>)asClArray, 0);
                            }
                            else if (result[i].GetType() == typeof(ClArray<double>))
                            {
                                if (asClArray.arrayLength != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_DOUBLE)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<double>;
                                source.CopyTo((ClArray<double>)asClArray, 0);
                            }
                            else if (result[i].GetType() == typeof(ClArray<byte>))
                            {
                                if (asClArray.arrayLength != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_BYTE)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<byte>;
                                source.CopyTo((ClArray<byte>)asClArray, 0);
                            }
                            else if (result[i].GetType() == typeof(ClArray<char>))
                            {
                                if (asClArray.arrayLength != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_CHAR)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<char>;
                                source.CopyTo((ClArray<char>)asClArray, 0);
                            }
                            else if (result[i].GetType() == typeof(ClArray<int>))
                            {
                                if (asClArray.arrayLength != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_INT)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<int>;
                                source.CopyTo((ClArray<int>)asClArray, 0);
                            }
                            else if (result[i].GetType() == typeof(ClArray<uint>))
                            {
                                if (asClArray.arrayLength != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_UINT)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<uint>;
                                source.CopyTo((ClArray<uint>)asClArray, 0);
                            }
                            else if (result[i].GetType() == typeof(ClArray<long>))
                            {
                                if (asClArray.arrayLength != outputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: inconsistent length of output arrays and length of result arrays.");
                                    return;
                                }

                                if (outputBuffers[i].eType != ElementType.ELM_LONG)
                                {
                                    Console.WriteLine("error: inconsistent types of output and result arrays.");
                                    return;
                                }

                                var source = outputBuffers[i].switchedBuffer() as ClArray<long>;
                                source.CopyTo((ClArray<long>)asClArray, 0);
                            }
                        }


                    }
                    // *********************************************************************************************
                    // *********************************************************************************************
                    // *********************************************************************************************
                }

                // to do: complete this method
                if ((nextStages != null) && (nextStages.Length > 0))
                {

                    for (int i = 0; i < outputBuffers.Length; i++)
                    {
                        if (outputBuffers[i].eType == ElementType.ELM_FLOAT)
                        {
                            var source = outputBuffers[i].bufDuplicate as ClArray<float>;
                            // to do: if number of free threads greater than nextStages length, use parallel.for loop
                            for (int j = 0; j < nextStages.Length; j++)
                            {
                                if(source.GetType()!=nextStages[j].inputBuffers[i].switchedBuffer().GetType())
                                {
                                    Console.WriteLine("error: output - input buffer type mismatch");
                                    return;
                                }

                                if(source.Length!= nextStages[j].inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: output - input buffer length mismatch");
                                    return;
                                }
                                source.CopyTo((ClArray<float>)nextStages[j].inputBuffers[i].bufDuplicate,0);
                            }
                        }

                        if (outputBuffers[i].eType == ElementType.ELM_DOUBLE)
                        {
                            var source = outputBuffers[i].bufDuplicate as ClArray<double>;
                            // to do: if number of free threads greater than nextStages length, use parallel.for loop
                            for (int j = 0; j < nextStages.Length; j++)
                            {
                                if (source.GetType() != nextStages[j].inputBuffers[i].switchedBuffer().GetType())
                                {
                                    Console.WriteLine("error: output - input buffer type mismatch");
                                    return;
                                }

                                if (source.Length != nextStages[j].inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: output - input buffer length mismatch");
                                    return;
                                }
                                source.CopyTo((ClArray<double>)nextStages[j].inputBuffers[i].bufDuplicate, 0);
                            }
                        }

                        if (outputBuffers[i].eType == ElementType.ELM_BYTE)
                        {
                            var source = outputBuffers[i].bufDuplicate as ClArray<byte>;
                            // to do: if number of free threads greater than nextStages length, use parallel.for loop
                            for (int j = 0; j < nextStages.Length; j++)
                            {
                                if (source.GetType() != nextStages[j].inputBuffers[i].switchedBuffer().GetType())
                                {
                                    Console.WriteLine("error: output - input buffer type mismatch");
                                    return;
                                }

                                if (source.Length != nextStages[j].inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: output - input buffer length mismatch");
                                    return;
                                }
                                source.CopyTo((ClArray<byte>)nextStages[j].inputBuffers[i].bufDuplicate, 0);
                            }
                        }

                        if (outputBuffers[i].eType == ElementType.ELM_CHAR)
                        {
                            var source = outputBuffers[i].bufDuplicate as ClArray<char>;
                            // to do: if number of free threads greater than nextStages length, use parallel.for loop
                            for (int j = 0; j < nextStages.Length; j++)
                            {
                                if (source.GetType() != nextStages[j].inputBuffers[i].switchedBuffer().GetType())
                                {
                                    Console.WriteLine("error: output - input buffer type mismatch");
                                    return;
                                }

                                if (source.Length != nextStages[j].inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: output - input buffer length mismatch");
                                    return;
                                }
                                source.CopyTo((ClArray<char>)nextStages[j].inputBuffers[i].bufDuplicate, 0);
                            }
                        }

                        if (outputBuffers[i].eType == ElementType.ELM_INT)
                        {
                            var source = outputBuffers[i].bufDuplicate as ClArray<int>;
                            // to do: if number of free threads greater than nextStages length, use parallel.for loop
                            for (int j = 0; j < nextStages.Length; j++)
                            {
                                if (source.GetType() != nextStages[j].inputBuffers[i].switchedBuffer().GetType())
                                {
                                    Console.WriteLine("error: output - input buffer type mismatch");
                                    return;
                                }

                                if (source.Length != nextStages[j].inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: output - input buffer length mismatch");
                                    return;
                                }
                                source.CopyTo((ClArray<int>)nextStages[j].inputBuffers[i].bufDuplicate, 0);
                            }
                        }

                        if (outputBuffers[i].eType == ElementType.ELM_UINT)
                        {
                            var source = outputBuffers[i].bufDuplicate as ClArray<uint>;
                            // to do: if number of free threads greater than nextStages length, use parallel.for loop
                            for (int j = 0; j < nextStages.Length; j++)
                            {
                                if (source.GetType() != nextStages[j].inputBuffers[i].switchedBuffer().GetType())
                                {
                                    Console.WriteLine("error: output - input buffer type mismatch");
                                    return;
                                }

                                if (source.Length != nextStages[j].inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: output - input buffer length mismatch");
                                    return;
                                }
                                source.CopyTo((ClArray<uint>)nextStages[j].inputBuffers[i].bufDuplicate, 0);
                            }
                        }

                        if (outputBuffers[i].eType == ElementType.ELM_LONG)
                        {
                            var source = outputBuffers[i].bufDuplicate as ClArray<long>;
                            // to do: if number of free threads greater than nextStages length, use parallel.for loop
                            for (int j = 0; j < nextStages.Length; j++)
                            {
                                if (source.GetType() != nextStages[j].inputBuffers[i].switchedBuffer().GetType())
                                {
                                    Console.WriteLine("error: output - input buffer type mismatch");
                                    return;
                                }

                                if (source.Length != nextStages[j].inputBuffers[i].bufDuplicate.arrayLength)
                                {
                                    Console.WriteLine("error: output - input buffer length mismatch");
                                    return;
                                }
                                source.CopyTo((ClArray<long>)nextStages[j].inputBuffers[i].bufDuplicate, 0);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// <para>creates a pipeline out of all bound stages, ready to compute, currently only 1 stage per layer (as parallel)</para>
            /// <para>executes the initializer kernels of each stage once</para>
            /// </summary>
            /// <returns></returns>
            public ClPipeline makePipeline()
            {
                if (debug)
                    Console.WriteLine("Creating a pipeline from a group of stages");
                // find starting stages with tracking back
                ClPipelineStage currentStage = findInputStages();
                int numberOfLayers = currentStage.findOutputStagesCount(1);
                int currentOrder = 0;
                if (debug)
                    Console.WriteLine("Number of stages="+ numberOfLayers);

                ClPipelineStage[][] pipelineStages = new ClPipelineStage[numberOfLayers][];
                // enumerate orders and add all stages as array elements for pipeline

                for (int i=0;i<numberOfLayers;i++)
                {
                    // currently supports only linear-horizontal bound stages, only 1 stage per layer, no parallel stages (yet)
                    currentStage.stageOrder = i;
                    pipelineStages[i] = new ClPipelineStage[1];
                    pipelineStages[i][0] = currentStage;
                    if(i<numberOfLayers-1)
                        currentStage = currentStage.nextStages[0];
                }

                ClPipeline pipeline = new ClPipeline(debug);
                pipeline.stages = pipelineStages;
                // initialize stage buffers

                for (int i = 0; i < pipeline.stages.Length; i++)
                {
                    for (int j = 0; j < pipeline.stages[i].Length; j++)
                    {
                        pipeline.stages[i][j].run(true); // initialize normal buffers
                        pipeline.stages[i][j].switchInputBuffers();
                        pipeline.stages[i][j].switchOutputBuffers();
                        pipeline.stages[i][j].run(true); // initialize duplicate buffers
                        pipeline.stages[i][j].switchInputBuffers();
                        pipeline.stages[i][j].switchOutputBuffers();
                    }
                }
                    return pipeline;
            }

            /// <summary>
            /// finds all stages and puts them in layers to be computed in parallel
            /// </summary>
            /// <param name="root"></param>
            /// <returns></returns>
            internal ClPipelineStage findInputStages()
            {
                if (previousStage != null)
                {
                    return previousStage.findInputStages();
                }
                else
                    return this;
            }

            /// <summary>
            /// finds total number of horizontally bound stages (also number of steps before output has data)
            /// </summary>
            /// <returns></returns>
            internal int findOutputStagesCount(int startValue)
            {
                ClPipelineStage currentStage = this;
                if (currentStage.nextStages != null)
                {
                    if (currentStage.nextStages.Length > 0)
                    {
                        int[] valuesToCompare = new int[currentStage.nextStages.Length];
                        for (int i = 0; i < currentStage.nextStages.Length; i++)
                        {
                            valuesToCompare[i] = currentStage.nextStages[i].findOutputStagesCount(startValue);
                        }
                        Array.Sort(valuesToCompare);
                        return startValue+valuesToCompare[0];
                    }
                    else
                        return startValue;
                }
                else
                    return startValue;
            }

            internal int stageOrder { get; set; }

            internal string []initializerKernelNames { get; set; }
            internal int []initializerKernelGlobalRanges { get; set; }
            internal int []initializerKernelLocalRanges { get; set; }



            /// <summary>
            /// <para>kernel function name to run once before beginning, empty string = no initializing needed</para>
            /// <para></para>
            /// </summary>
            public void initializerKernel(string initKernelNames, int[] globalRanges, int [] localRanges)
            {
                if (initKernelNames != null)
                    initializerKernelNames = initKernelNames.Split(new string[] { " ", ",", ";", "-", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if ((initKernelNames == null)  || (globalRanges==null) || (localRanges==null) ||
                    (initializerKernelNames.Length != globalRanges.Length) || (initializerKernelNames.Length != localRanges.Length) || (localRanges.Length != globalRanges.Length))
                {
                    initializerKernelNames = null;
                    Console.WriteLine("Warning: Number of initializer kernels and number of range values do not match or one of them is null. Initializer kernels will not run.");
                    return;
                }
                
                
                initializerKernelGlobalRanges = new int[globalRanges.Length];
                initializerKernelLocalRanges = new int[localRanges.Length];

                for (int i=0;i< initializerKernelGlobalRanges.Length;i++)
                {
                    initializerKernelGlobalRanges[i] = globalRanges[i];
                    initializerKernelLocalRanges[i] = localRanges[i];
                }
            }

            /// <summary>
            /// binds this stage to target stage's entrance
            /// </summary>
            public void prependToStage(ClPipelineStage stage)
            {
                // this stage
                nextStagesList.Add(stage);
                nextStages = nextStagesList.ToArray();

                // next stage
                stage.previousStage = this;
            }

            /// <summary>
            /// binds this stage to target stage's exit
            /// </summary>
            public void appendToStage(ClPipelineStage stage)
            {
                // this stage
                previousStage = stage;

                // previous stage
                previousStage.nextStagesList.Add(this);
                previousStage.nextStages = previousStage.nextStagesList.ToArray();
            }

            /// <summary>
            /// <para>setup devices that will compute this stage(as parallel to speed-up this stage only, duplicated devices allowed too)</para>
            /// <para>copies device object</para>
            /// </summary>
            public void addDevices(ClDevices devicesParameter)
            {
                devices = devicesParameter.copyExact();
            }

            /// <summary>
            /// setup kernels to be used by this stage
            /// 
            /// </summary>
            /// <param name="kernels">string containing auxiliary functions, kernel functions and constants</param>
            /// <param name="kernelNames">names of kernels to be used(in the order they run)</param>
            /// <param name="globalRanges">total workitems per kernel name in kernelNames parameter</param>
            /// <param name="localRanges">workgroup workitems per kernel name in kernelNames parameter</param>
            public void addKernels(string kernels, string kernelNames, int [] globalRanges, int[] localRanges)
            {
                kernelsToCompile = new StringBuilder(kernels).ToString();
                kernelNamesToRun = kernelNames.Split(new string[] {" ",",",";","-",Environment.NewLine },StringSplitOptions.RemoveEmptyEntries);
                if (debug)
                {
                    Console.WriteLine(kernelNamesToRun != null ? (kernelNamesToRun.Length > 0 ? kernelNamesToRun[0] : ("kernel name error: " + kernelNames)) : ("kernel name error: " + kernelNames));
                    if (globalRanges.Length < kernelNamesToRun.Length)
                        Console.WriteLine("number of global ranges is not equal to kernel names listed in \"kernels\" parameter ");

                    if (localRanges.Length < kernelNamesToRun.Length)
                        Console.WriteLine("number of local ranges is not equal to kernel names listed in \"kernels\" parameter ");
                }

                for (int i=0;i<kernelNamesToRun.Length;i++)
                {
                    if(kernelNameToParameters.ContainsKey(kernelNamesToRun[i]))
                    {

                    }
                    else
                    {
                        KernelParameters kernelParameters = new KernelParameters();
                        kernelParameters.globalRange = globalRanges[i];
                        kernelParameters.localRange = localRanges[i];
                        kernelNameToParameters.Add(kernelNamesToRun[i], kernelParameters);
                    }
                }
            }

            /// <summary>
            /// not used for input or output, just for keeping sequential logic states (such as coordinates of particles)
            /// </summary>
            public void addHiddenBuffers(params Array[] hiddensParameter)
            {
                for (int i = 0; i < hiddensParameter.Length; i++)
                    hiddenBuffersList.Add(new ClPipelineStageBuffer(hiddensParameter[i]));

                hiddenBuffers = hiddenBuffersList.ToArray();
            }

            /// <summary>
            /// not used for input or output, just for keeping sequential logic states (such as coordinates of particles)
            /// </summary>
            public void addHiddenBuffers(params IBufferOptimization[] hiddensParameter)
            {
                for (int i = 0; i < hiddensParameter.Length; i++)
                    hiddenBuffersList.Add(new ClPipelineStageBuffer(hiddensParameter[i]));

                hiddenBuffers = hiddenBuffersList.ToArray();
            }

            /// <summary>
            /// not used for input or output, just for keeping sequential logic states (such as coordinates of particles)
            /// </summary>
            public void addHiddenBuffers(params IMemoryHandle[] hiddensParameter)
            {
                for (int i = 0; i < hiddensParameter.Length; i++)
                    hiddenBuffersList.Add(new ClPipelineStageBuffer(hiddensParameter[i]));

                hiddenBuffers = hiddenBuffersList.ToArray();
            }

            /// <summary>
            /// input arrays (ClArray, ClByteArray, byte[], ... ) to be pushed by user or to be connect to another stage's output
            /// </summary>
            public void addInputBuffers(params Array [] inputsParameter)
            {
                for (int i = 0; i < inputsParameter.Length; i++)
                    inputBuffersList.Add(new ClPipelineStageBuffer(inputsParameter[i]));

                inputBuffers = inputBuffersList.ToArray();
            }

            /// <summary>
            /// input arrays (ClArray, ClByteArray, byte[], ... ) to be pushed by user or to be connect to another stage's output
            /// </summary>
            public void addInputBuffers(params IBufferOptimization[] inputsParameter)
            {
                for(int i=0;i<inputsParameter.Length;i++)
                    inputBuffersList.Add(new ClPipelineStageBuffer(inputsParameter[i]));

                inputBuffers = inputBuffersList.ToArray();
            }

            /// <summary>
            /// input arrays (ClArray, ClByteArray, byte[], ... ) to be pushed by user or to be connect to another stage's output
            /// </summary>
            public void addInputBuffers(params IMemoryHandle[] inputsParameter)
            {
                for (int i = 0; i < inputsParameter.Length; i++)
                    inputBuffersList.Add(new ClPipelineStageBuffer(inputsParameter[i]));

                inputBuffers = inputBuffersList.ToArray();
            }



            /// <summary>
            /// output arrays (ClArray, ClByteArray, byte[], ... ) to be popped to user or to be connected another stage's input
            /// </summary>
            public void addOutputBuffers(params Array[] outputsParameter)
            {
                for (int i = 0; i < outputsParameter.Length; i++)
                    outputBuffersList.Add(new ClPipelineStageBuffer(outputsParameter[i]));

                outputBuffers = outputBuffersList.ToArray();
            }

            /// <summary>
            /// output arrays (ClArray, ClByteArray, byte[], ... ) to be popped to user or to be connected another stage's input
            /// </summary>
            public void addOutputBuffers(params IMemoryHandle[] outputsParameter)
            {
                for (int i = 0; i < outputsParameter.Length; i++)
                    outputBuffersList.Add(new ClPipelineStageBuffer(outputsParameter[i]));

                outputBuffers = outputBuffersList.ToArray();
            }

            /// <summary>
            /// output arrays (ClArray, ClByteArray, byte[], ... ) to be popped to user or to be connected another stage's input
            /// </summary>
            public void addOutputBuffers(params IBufferOptimization[] outputsParameter)
            {
                for (int i = 0; i < outputsParameter.Length; i++)
                    outputBuffersList.Add(new ClPipelineStageBuffer(outputsParameter[i]));

                outputBuffers = outputBuffersList.ToArray();
            }
        }



        internal enum ElementType:int
        {
            ELM_FLOAT=0, ELM_DOUBLE = 1, ELM_BYTE = 2, ELM_CHAR = 3, ELM_INT = 4, ELM_LONG = 5, ELM_UINT = 6, ELM_STRUCT = 7,
        }

        /// <summary>
        /// Wraps Array, FastArr, ClArray types so that it can be switched by its duplicate, read, write, ...
        /// </summary>
        internal class ClPipelineStageBuffer
        {
            // buffers are always wrapped as ClArray
            internal ElementType eType;
            private ClArray<byte> bufByte;
            private ClArray<byte> bufByteDuplicate;
            private ClArray<char> bufChar;
            private ClArray<char> bufCharDuplicate;
            private ClArray<int> bufInt;
            private ClArray<int> bufIntDuplicate;
            private ClArray<long> bufLong;
            private ClArray<long> bufLongDuplicate;
            private ClArray<float> bufFloat;
            private ClArray<float> bufFloatDuplicate;
            private ClArray<double> bufDouble;
            private ClArray<double> bufDoubleDuplicate;
            private ClArray<uint> bufUInt;
            private ClArray<uint> bufUIntDuplicate;
            internal IBufferOptimization buf;
            internal IBufferOptimization bufDuplicate;
            /// <summary>
            /// p: buffer to duplicate and double buffered in pipeline stages
            /// </summary>
            /// <param name="p"></param>
            public  ClPipelineStageBuffer(object p)
            {
                var bufAsArray = p as Array;
                if (bufAsArray != null)
                {
                    if (p.GetType() == typeof(float[]))
                    {
                        eType = ElementType.ELM_FLOAT;
                        bufFloat = (float[])p;
                        bufFloatDuplicate = new ClArray<float>(bufFloat.Length, bufFloat.alignmentBytes);
                        buf = bufFloat as IBufferOptimization;
                        bufDuplicate = bufFloatDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(double[]))
                    {
                        eType = ElementType.ELM_DOUBLE;
                        bufDouble = (double[])p;
                        bufDoubleDuplicate = new ClArray<double>(bufDouble.Length, bufDouble.alignmentBytes);
                        buf = bufDouble as IBufferOptimization;
                        bufDuplicate = bufDoubleDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(byte[]))
                    {
                        eType = ElementType.ELM_BYTE;
                        bufByte = (byte[])p;
                        bufByteDuplicate = new ClArray<byte>(bufByte.Length, bufByte.alignmentBytes);
                        buf = bufByte as IBufferOptimization;
                        bufDuplicate = bufByteDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(char[]))
                    {
                        eType = ElementType.ELM_CHAR;
                        bufChar = (char[])p;
                        bufCharDuplicate = new ClArray<char>(bufChar.Length, bufChar.alignmentBytes);
                        buf = bufChar as IBufferOptimization;
                        bufDuplicate = bufCharDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(int[]))
                    {
                        eType = ElementType.ELM_INT;
                        bufInt = (int[])p;
                        bufIntDuplicate = new ClArray<int>(bufInt.Length, bufInt.alignmentBytes);
                        buf = bufInt as IBufferOptimization;
                        bufDuplicate = bufIntDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(uint[]))
                    {
                        eType = ElementType.ELM_UINT;
                        bufUInt = (uint[])p;
                        bufUIntDuplicate = new ClArray<uint>(bufUInt.Length, bufUInt.alignmentBytes);
                        buf = bufUInt as IBufferOptimization;
                        bufDuplicate = bufUIntDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(long[]))
                    {
                        eType = ElementType.ELM_LONG;
                        bufLong = (long[])p;
                        bufLongDuplicate = new ClArray<long>(bufLong.Length, bufLong.alignmentBytes);
                        buf = bufLong as IBufferOptimization;
                        bufDuplicate = bufLongDuplicate as IBufferOptimization;
                    }
                    else
                    {
                        // then it has to be a struct array
                        eType = ElementType.ELM_BYTE;
                        bufByte = ClArray<byte>.wrapArrayOfStructs(p);
                        bufByteDuplicate = new ClArray<byte>(bufByte.Length, bufByte.alignmentBytes);
                        bufByteDuplicate.numberOfElementsPerWorkItem = bufByte.numberOfElementsPerWorkItem;
                        buf = bufByte as IBufferOptimization;
                        bufDuplicate = bufByteDuplicate as IBufferOptimization;
                    }
                }
                var bufAsFastArr = p as IMemoryHandle;
                if (bufAsFastArr != null)
                {
                    if(p.GetType()==typeof(ClByteArray))
                    {
                        eType = ElementType.ELM_BYTE;
                        bufByte = (ClByteArray)p;
                        bufByteDuplicate = new ClArray<byte>(bufByte.Length,bufByte.alignmentBytes>0? bufByte.alignmentBytes:4096);
                        buf = bufByte as IBufferOptimization;
                        bufDuplicate = bufByteDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(ClCharArray))
                    {
                        eType = ElementType.ELM_CHAR;
                        bufChar = (ClCharArray)p;
                        bufCharDuplicate = new ClArray<char>(bufChar.Length, bufChar.alignmentBytes > 0 ? bufChar.alignmentBytes : 4096);
                        buf = bufChar as IBufferOptimization;
                        bufDuplicate = bufCharDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(ClIntArray))
                    {
                        eType = ElementType.ELM_INT;
                        bufInt = (ClIntArray)p;
                        bufIntDuplicate = new ClArray<int>(bufInt.Length, bufInt.alignmentBytes > 0 ? bufInt.alignmentBytes : 4096);
                        buf = bufInt as IBufferOptimization;
                        bufDuplicate = bufIntDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(ClUIntArray))
                    {
                        eType = ElementType.ELM_UINT;
                        bufUInt = (ClUIntArray)p;
                        bufUIntDuplicate = new ClArray<uint>(bufUInt.Length, bufUInt.alignmentBytes > 0 ? bufUInt.alignmentBytes : 4096);
                        buf = bufUInt as IBufferOptimization;
                        bufDuplicate = bufUIntDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(ClLongArray))
                    {
                        eType = ElementType.ELM_LONG;
                        bufLong = (ClLongArray)p;
                        bufLongDuplicate = new ClArray<long>(bufLong.Length, bufLong.alignmentBytes > 0 ? bufLong.alignmentBytes : 4096);
                        buf = bufLong as IBufferOptimization;
                        bufDuplicate = bufLongDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(ClFloatArray))
                    {
                        eType = ElementType.ELM_FLOAT;
                        bufFloat = (ClFloatArray)p;
                        bufFloatDuplicate = new ClArray<float>(bufFloat.Length, bufFloat.alignmentBytes > 0 ? bufFloat.alignmentBytes : 4096);
                        buf = bufFloat as IBufferOptimization;
                        bufDuplicate = bufFloatDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(ClDoubleArray))
                    {
                        eType = ElementType.ELM_DOUBLE;
                        bufDouble = (ClDoubleArray)p;
                        bufDoubleDuplicate = new ClArray<double>(bufDouble.Length, bufDouble.alignmentBytes > 0 ? bufDouble.alignmentBytes : 4096);
                        buf = bufDouble as IBufferOptimization;
                        bufDuplicate = bufDoubleDuplicate as IBufferOptimization;
                    }
                }
                var bufAsClArray = p as IBufferOptimization;
                if (bufAsClArray != null)
                {
                    if(p.GetType() == typeof(ClArray<byte>))
                    {
                        eType = ElementType.ELM_BYTE;
                        bufByte = (ClArray<byte>)p;
                        bufByteDuplicate = new ClArray<byte>(bufByte.Length, bufByte.alignmentBytes > 0 ? bufByte.alignmentBytes : 4096);
                        bufByteDuplicate.numberOfElementsPerWorkItem = bufByte.numberOfElementsPerWorkItem;
                        buf = bufByte as IBufferOptimization;
                        bufDuplicate = bufByteDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(ClArray<char>))
                    {
                        eType = ElementType.ELM_CHAR;
                        bufChar = (ClArray<char>)p;
                        bufCharDuplicate = new ClArray<char>(bufChar.Length, bufChar.alignmentBytes > 0 ? bufChar.alignmentBytes : 4096);
                        bufCharDuplicate.numberOfElementsPerWorkItem = bufChar.numberOfElementsPerWorkItem;
                        buf = bufChar as IBufferOptimization;
                        bufDuplicate = bufCharDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(ClArray<int>))
                    {
                        eType = ElementType.ELM_INT; 
                        bufInt = (ClArray<int>)p;
                        bufIntDuplicate = new ClArray<int>(bufInt.Length, bufInt.alignmentBytes > 0 ? bufInt.alignmentBytes : 4096);
                        bufIntDuplicate.numberOfElementsPerWorkItem = bufInt.numberOfElementsPerWorkItem;
                        buf = bufInt as IBufferOptimization;
                        bufDuplicate = bufIntDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(ClArray<uint>))
                    {
                        eType = ElementType.ELM_UINT;
                        bufUInt = (ClArray<uint>)p;
                        bufUIntDuplicate = new ClArray<uint>(bufUInt.Length, bufUInt.alignmentBytes > 0 ? bufUInt.alignmentBytes : 4096);
                        bufUIntDuplicate.numberOfElementsPerWorkItem = bufUInt.numberOfElementsPerWorkItem;
                        buf = bufUInt as IBufferOptimization;
                        bufDuplicate = bufUIntDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(ClArray<long>))
                    {
                        eType = ElementType.ELM_LONG;
                        bufLong = (ClArray<long>)p;
                        bufLongDuplicate = new ClArray<long>(bufLong.Length, bufLong.alignmentBytes > 0 ? bufLong.alignmentBytes : 4096);
                        bufLongDuplicate.numberOfElementsPerWorkItem = bufLong.numberOfElementsPerWorkItem;
                        buf = bufLong as IBufferOptimization;
                        bufDuplicate = bufLongDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(ClArray<float>))
                    {
                        eType = ElementType.ELM_FLOAT;
                        bufFloat = (ClArray<float>)p;
                        bufFloatDuplicate = new ClArray<float>(bufFloat.Length, bufFloat.alignmentBytes > 0 ? bufFloat.alignmentBytes : 4096);
                        bufFloatDuplicate.numberOfElementsPerWorkItem = bufFloat.numberOfElementsPerWorkItem;
                        buf = bufFloat as IBufferOptimization;
                        bufDuplicate = bufFloatDuplicate as IBufferOptimization;
                    }
                    else if (p.GetType() == typeof(ClArray<double>))
                    {
                        eType = ElementType.ELM_DOUBLE;
                        bufDouble = (ClArray<double>)p;
                        bufDoubleDuplicate = new ClArray<double>(bufDouble.Length, bufDouble.alignmentBytes > 0 ? bufDouble.alignmentBytes : 4096);
                        bufDoubleDuplicate.numberOfElementsPerWorkItem = bufDouble.numberOfElementsPerWorkItem;
                        buf = bufDouble as IBufferOptimization;
                        bufDuplicate = bufDoubleDuplicate as IBufferOptimization;
                    }
                }
            }

            /// <summary>
            /// double buffering for pipelining(overlap pci-e reads and writes on all pci-e bridges)
            /// </summary>
            internal void switchBuffers()
            {
                object tmp = bufByte;
                bufByte = bufByteDuplicate;
                bufByteDuplicate = (ClArray<byte>)tmp;
                tmp = bufChar;
                bufChar = bufCharDuplicate;
                bufCharDuplicate = (ClArray<char>)tmp;
                tmp = bufInt;
                bufInt = bufIntDuplicate;
                bufIntDuplicate = (ClArray<int>)tmp;
                tmp = bufUInt;
                bufUInt = bufUIntDuplicate;
                bufUIntDuplicate = (ClArray<uint>)tmp;
                tmp = bufLong;
                bufLong = bufLongDuplicate;
                bufLongDuplicate = (ClArray<long>)tmp;
                tmp = bufFloat;
                bufFloat = bufFloatDuplicate;
                bufFloatDuplicate = (ClArray<float>)tmp;
                tmp = bufDouble;
                bufDouble = bufDoubleDuplicate;
                bufDoubleDuplicate = (ClArray<double>)tmp;
                tmp = buf;
                buf = bufDuplicate;
                bufDuplicate =(IBufferOptimization) tmp;
            }

            public int numberOfElementsPerWorkItem
            {
                get
                {
                    return buf.numberOfElementsPerWorkItem;
                }

                set
                {
                    buf.numberOfElementsPerWorkItem = value;
                    bufDuplicate.numberOfElementsPerWorkItem = value;
                }
            }

            internal ClParameterGroup nextParam(params IBufferOptimization[] bufs)
            {
                if (bufByte != null)
                    return bufByte.nextParam(bufs);
                else if (bufChar != null)
                    return bufChar.nextParam(bufs);
                else if (bufInt != null)
                    return bufInt.nextParam(bufs);
                else if (bufUInt != null)
                    return bufUInt.nextParam(bufs);
                else if (bufLong != null)
                    return bufLong.nextParam(bufs);
                else if (bufFloat != null)
                    return bufFloat.nextParam(bufs);
                else if (bufDouble != null)
                    return bufDouble.nextParam(bufs);
                else
                    return null;
            }

            internal ClParameterGroup nextParamDuplicate(params IBufferOptimization[] bufs)
            {
                if (bufByteDuplicate != null)
                    return bufByteDuplicate.nextParam(bufs);
                else if (bufCharDuplicate != null)
                    return bufCharDuplicate.nextParam(bufs);
                else if (bufIntDuplicate != null)
                    return bufIntDuplicate.nextParam(bufs);
                else if (bufUIntDuplicate != null)
                    return bufUIntDuplicate.nextParam(bufs);
                else if (bufLongDuplicate != null)
                    return bufLongDuplicate.nextParam(bufs);
                else if (bufFloatDuplicate != null)
                    return bufFloatDuplicate.nextParam(bufs);
                else if (bufDoubleDuplicate != null)
                    return bufDoubleDuplicate.nextParam(bufs);
                else
                    return null;
            }

            public bool read
            {
                get
                {
                    return buf.read;
                }

                set
                {
                    buf.read = value;
                    bufDuplicate.read = value;
                }
            }

            public bool partialRead
            {
                get
                {
                    return buf.partialRead;
                }

                set
                {
                    buf.partialRead = value;
                    bufDuplicate.partialRead = value;
                }
            }

            public bool write
            {
                get
                {
                    return buf.write;
                }

                set
                {
                    buf.write = value;
                    bufDuplicate.write = value;
                }
            }

            public object buffer()
            {
                if (bufByte != null)
                    return bufByte;
                else if (bufChar != null)
                    return bufChar;
                else if (bufInt != null)
                    return bufInt;
                else if (bufUInt != null)
                    return bufUInt;
                else if (bufLong != null)
                    return bufLong;
                else if (bufFloat != null)
                    return bufFloat;
                else if (bufDouble != null)
                    return bufDouble;
                else
                    return null;
            }

           

            public object switchedBuffer()
            {
                if (bufByteDuplicate != null)
                    return bufByteDuplicate;
                else if (bufCharDuplicate != null)
                    return bufCharDuplicate;
                else if (bufIntDuplicate != null)
                    return bufIntDuplicate;
                else if (bufUIntDuplicate != null)
                    return bufUIntDuplicate;
                else if (bufLongDuplicate != null)
                    return bufLongDuplicate;
                else if (bufFloatDuplicate != null)
                    return bufFloatDuplicate;
                else if (bufDoubleDuplicate != null)
                    return bufDoubleDuplicate;
                else
                    return null;
            }

        }
    }
}
