using Cekirdekler.ClArrays;
using Cekirdekler.Hardware;
using System;
using System.Collections.Generic;
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

            /// <summary>
            /// pushes data from entrance stage, returns true if exit stage has result data on its output(and its target arrays)
            /// </summary>
            /// <returns></returns>
            public bool pushData()
            {
                if (debug)
                {
                    Console.WriteLine("Pipeline running.");
                    if (stages.Length == 0)
                        Console.WriteLine("Zero pipeline stages.");
                }


                Parallel.For(0, stages.Length, i => {
                    for (int j = 0; j < stages[i].Length; j++)
                    {
                        stages[i][j].run();
                        // to do: copy from output(duplicate, from last iteration) to next stage's input(duplicate, for next iteration)
                    }
                });

                Parallel.For(0, stages.Length, i => {
                    for (int j = 0; j < stages[i].Length; j++)
                    {
                        stages[i][j].switchInputBuffers();
                    }
                });
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
            }
           
        }

        internal class KernelParameters
        {
            public int globalRange { get; set; }
            public int localRange { get; set; }
            public object[] buffers { get; set; } // kernel parameters ClArray Array ClFastArray(ClByteArray,ClFloatArray,...)
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


            // switch inputs(concurrently all stages) then compute(concurrently all stages, if they received input)
            internal void run()
            {
                if (debug)
                    Console.WriteLine("pipeline stage running.");
                // initialize buffers and number cruncher
                if (!initComplete)
                {
                    lock (syncObj)
                    {
                        if (numberCruncher == null)
                        {
                            numberCruncher = new ClNumberCruncher(devices, kernelsToCompile);
                            if (debug)
                            {
                                numberCruncher.performanceFeed = true;
                            }
                        }

                        for (int i = 0; i < inputBuffers.Length; i++)
                            inputBuffers[i].write = false;

                        for (int i = 0; i < outputBuffers.Length; i++)
                        {
                            outputBuffers[i].read = false;
                            outputBuffers[i].partialRead = false;
                        }

                        // hidden buffers don't write/read unless its multi gpu
                        // todo: multi-gpu stage buffers will sync
                        for (int i = 0; i < hiddenBuffers.Length; i++)
                        {
                            hiddenBuffers[i].read = false;
                            hiddenBuffers[i].partialRead = false;
                            hiddenBuffers[i].write = false;
                        }

                        initComplete = true;
                    }
                }

                // to do: move parameter building to initializing
                ClParameterGroup bufferParameters = null;
                ClPipelineStageBuffer ib = null;
                int inputStart = 0, hiddenStart = 0, outputStart = 0;
                if (inputBuffers.Length > 0)
                {
                    ib = inputBuffers[0];
                    inputStart = 1;
                }
                else if (hiddenBuffers.Length > 0)
                {
                    ib = hiddenBuffers[0];
                    hiddenStart = 1;
                }
                else if (outputBuffers.Length > 0)
                {
                    ib = outputBuffers[0];
                    outputStart = 1;
                }

                bool parameterStarted = false;



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
                }

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
                }

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
                }
                // parameter building end

                // running kernel
                for (int i = 0; i < kernelNamesToRun.Length; i++)
                {
                    bufferParameters.compute(numberCruncher, 1,
                        kernelNamesToRun[i],
                        kernelNameToParameters[kernelNamesToRun[i]].globalRange,
                        kernelNameToParameters[kernelNamesToRun[i]].localRange);
                    if (debug)
                        Console.WriteLine("kernel complete: "+i);
                }
            }



            // double buffering for overlapped stages for multi device usage
            internal void switchInputBuffers()
            {
                for (int i = 0; i < inputBuffers.Length; i++)
                    inputBuffers[0].switchBuffers();
            }

            /// <summary>
            /// creates a pipeline out of all bound stages, ready to compute, currently only 1 stage per layer (as parallel)
            /// </summary>
            /// <returns></returns>
            public ClPipeline makePipeline()
            {
                if (debug)
                    Console.WriteLine("Creating a pipeline from a group of stages");
                // find starting stages with tracking back
                ClPipelineStage currentStage = findInputStages();
                int numberOfLayers = currentStage.findOutputStagesCount(0);
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
                        return valuesToCompare[0];
                    }
                    else
                        return startValue;
                }
                else
                    return startValue;
            }

            internal int stageOrder { get; set; }

            /// <summary>
            /// kernel function name to run once before beginning, empty string = no initializing needed
            /// </summary>
            public void initializerKernel(string initKernelNames)
            {

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
            public void addKernels(string kernels, string kernelNames, int [] globalRanges, int localRanges)
            {
                kernelsToCompile = new StringBuilder(kernels).ToString();
                kernelNamesToRun = kernelNames.Split(new string[] {" ",",",";","-",Environment.NewLine },StringSplitOptions.RemoveEmptyEntries);
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
            private ElementType eType;
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
