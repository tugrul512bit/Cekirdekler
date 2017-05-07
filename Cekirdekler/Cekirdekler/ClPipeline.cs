using Cekirdekler.ClArrays;
using Cekirdekler.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            /// <summary>
            /// pushes data from entrance stage, returns true if exit stage has result data on its output(and its target arrays)
            /// </summary>
            /// <returns></returns>
            public bool pushData()
            {
                return false;
            }


            /// <summary>
            /// only created by one of the stages that are bound together
            /// </summary>
            internal ClPipeline()
            {

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

            internal string kernelsToRun;
            internal string kernelsToCompile;

            internal ClPipelineStage previousStage;
            internal ClPipelineStage[] nextStages;
            internal List<ClPipelineStage> nextStagesList;
            internal static object syncObj;
            internal bool initComplete;
            /// <summary>
            /// creates a stage to form a pipeline with other stages
            /// </summary>
            public ClPipelineStage()
            {
                initComplete = false;
                if (syncObj == null)
                    syncObj = new object();
                nextStagesList = new List<ClPipelineStage>();
                inputBuffersList = new List<ClPipelineStageBuffer>();
                outputBuffersList = new List<ClPipelineStageBuffer>();
                hiddenBuffersList = new List<ClPipelineStageBuffer>();
                kernelNameToParameters = new Dictionary<string, KernelParameters>();
            }


            // switch inputs(concurrently all stages) then compute(concurrently all stages, if they received input)
            internal void run()
            {
                if (!initComplete)
                {
                    lock (syncObj)
                    {
                        if (numberCruncher == null)
                            numberCruncher = new ClNumberCruncher(devices, kernelsToCompile);

                        // todo: input: only reads
                        // hidden: no read write
                        // output: only writes
                        //for(int i=0;i<inputBuffers.Length;i++)
                        //    inputBuffers[i].buffer().write=false;
                        initComplete = true;
                    }
                }

            }

            

            // double buffering for overlapped stages for multi device usage
            internal void switchInputBuffers()
            {

            }

            /// <summary>
            /// creates a pipeline out of all bound stages, ready to compute 
            /// </summary>
            /// <returns></returns>
            public ClPipeline makePipeline()
            {
                return null;
            }

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
                kernelsToRun = new StringBuilder(kernelNames).ToString();
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
                    }
                    else if (p.GetType() == typeof(double[]))
                    {
                        eType = ElementType.ELM_DOUBLE;
                        bufDouble = (double[])p;
                        bufDoubleDuplicate = new ClArray<double>(bufDouble.Length, bufDouble.alignmentBytes);
                    }
                    else if (p.GetType() == typeof(byte[]))
                    {
                        eType = ElementType.ELM_BYTE;
                        bufByte = (byte[])p;
                        bufByteDuplicate = new ClArray<byte>(bufByte.Length, bufByte.alignmentBytes);
                    }
                    else if (p.GetType() == typeof(char[]))
                    {
                        eType = ElementType.ELM_CHAR;
                        bufChar = (char[])p;
                        bufCharDuplicate = new ClArray<char>(bufChar.Length, bufChar.alignmentBytes);
                    }
                    else if (p.GetType() == typeof(int[]))
                    {
                        eType = ElementType.ELM_INT;
                        bufInt = (int[])p;
                        bufIntDuplicate = new ClArray<int>(bufInt.Length, bufInt.alignmentBytes);
                    }
                    else if (p.GetType() == typeof(uint[]))
                    {
                        eType = ElementType.ELM_UINT;
                        bufUInt = (uint[])p;
                        bufUIntDuplicate = new ClArray<uint>(bufUInt.Length, bufUInt.alignmentBytes);
                    }
                    else if (p.GetType() == typeof(long[]))
                    {
                        eType = ElementType.ELM_LONG;
                        bufLong = (long[])p;
                        bufLongDuplicate = new ClArray<long>(bufLong.Length, bufLong.alignmentBytes);
                    }
                    else
                    {
                        // then it has to be a struct array
                        eType = ElementType.ELM_BYTE;
                        bufByte = ClArray<byte>.wrapArrayOfStructs(p);
                        bufByteDuplicate = new ClArray<byte>(bufByte.Length, bufByte.alignmentBytes);
                        bufByteDuplicate.numberOfElementsPerWorkItem = bufByte.numberOfElementsPerWorkItem;
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
                    }
                    else if (p.GetType() == typeof(ClCharArray))
                    {
                        eType = ElementType.ELM_CHAR;
                        bufChar = (ClCharArray)p;
                        bufCharDuplicate = new ClArray<char>(bufChar.Length, bufChar.alignmentBytes > 0 ? bufChar.alignmentBytes : 4096);
                    }
                    else if (p.GetType() == typeof(ClIntArray))
                    {
                        eType = ElementType.ELM_INT;
                        bufInt = (ClIntArray)p;
                        bufIntDuplicate = new ClArray<int>(bufInt.Length, bufInt.alignmentBytes > 0 ? bufInt.alignmentBytes : 4096);
                    }
                    else if (p.GetType() == typeof(ClUIntArray))
                    {
                        eType = ElementType.ELM_UINT;
                        bufUInt = (ClUIntArray)p;
                        bufUIntDuplicate = new ClArray<uint>(bufUInt.Length, bufUInt.alignmentBytes > 0 ? bufUInt.alignmentBytes : 4096);
                    }
                    else if (p.GetType() == typeof(ClLongArray))
                    {
                        eType = ElementType.ELM_LONG;
                        bufLong = (ClLongArray)p;
                        bufLongDuplicate = new ClArray<long>(bufLong.Length, bufLong.alignmentBytes > 0 ? bufLong.alignmentBytes : 4096);
                    }
                    else if (p.GetType() == typeof(ClFloatArray))
                    {
                        eType = ElementType.ELM_FLOAT;
                        bufFloat = (ClFloatArray)p;
                        bufFloatDuplicate = new ClArray<float>(bufFloat.Length, bufFloat.alignmentBytes > 0 ? bufFloat.alignmentBytes : 4096);
                    }
                    else if (p.GetType() == typeof(ClDoubleArray))
                    {
                        eType = ElementType.ELM_DOUBLE;
                        bufDouble = (ClDoubleArray)p;
                        bufDoubleDuplicate = new ClArray<double>(bufDouble.Length, bufDouble.alignmentBytes > 0 ? bufDouble.alignmentBytes : 4096);
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
                    }
                    else if (p.GetType() == typeof(ClArray<char>))
                    {
                        eType = ElementType.ELM_CHAR;
                        bufChar = (ClArray<char>)p;
                        bufCharDuplicate = new ClArray<char>(bufChar.Length, bufChar.alignmentBytes > 0 ? bufChar.alignmentBytes : 4096);
                        bufCharDuplicate.numberOfElementsPerWorkItem = bufChar.numberOfElementsPerWorkItem;
                    }
                    else if (p.GetType() == typeof(ClArray<int>))
                    {
                        eType = ElementType.ELM_INT; 
                        bufInt = (ClArray<int>)p;
                        bufIntDuplicate = new ClArray<int>(bufInt.Length, bufInt.alignmentBytes > 0 ? bufInt.alignmentBytes : 4096);
                        bufIntDuplicate.numberOfElementsPerWorkItem = bufInt.numberOfElementsPerWorkItem;
                    }
                    else if (p.GetType() == typeof(ClArray<uint>))
                    {
                        eType = ElementType.ELM_UINT;
                        bufUInt = (ClArray<uint>)p;
                        bufUIntDuplicate = new ClArray<uint>(bufUInt.Length, bufUInt.alignmentBytes > 0 ? bufUInt.alignmentBytes : 4096);
                        bufUIntDuplicate.numberOfElementsPerWorkItem = bufUInt.numberOfElementsPerWorkItem;
                    }
                    else if (p.GetType() == typeof(ClArray<long>))
                    {
                        eType = ElementType.ELM_LONG;
                        bufLong = (ClArray<long>)p;
                        bufLongDuplicate = new ClArray<long>(bufLong.Length, bufLong.alignmentBytes > 0 ? bufLong.alignmentBytes : 4096);
                        bufLongDuplicate.numberOfElementsPerWorkItem = bufLong.numberOfElementsPerWorkItem;
                    }
                    else if (p.GetType() == typeof(ClArray<float>))
                    {
                        eType = ElementType.ELM_FLOAT;
                        bufFloat = (ClArray<float>)p;
                        bufFloatDuplicate = new ClArray<float>(bufFloat.Length, bufFloat.alignmentBytes > 0 ? bufFloat.alignmentBytes : 4096);
                        bufFloatDuplicate.numberOfElementsPerWorkItem = bufFloat.numberOfElementsPerWorkItem;
                    }
                    else if (p.GetType() == typeof(ClArray<double>))
                    {
                        eType = ElementType.ELM_DOUBLE;
                        bufDouble = (ClArray<double>)p;
                        bufDoubleDuplicate = new ClArray<double>(bufDouble.Length, bufDouble.alignmentBytes > 0 ? bufDouble.alignmentBytes : 4096);
                        bufDoubleDuplicate.numberOfElementsPerWorkItem = bufDouble.numberOfElementsPerWorkItem;
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
