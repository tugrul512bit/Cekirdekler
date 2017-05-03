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



        /// <summary>
        /// <para>used to build stages of a pipeline</para>
        /// <para>inputs are interpreted as read-only(partial if multi device stage), outputs are interpreted as write-only</para>
        /// <para>hidden buffers are used for sequential logic (keeping xyz coordinates of an nbody algorithm - for example) </para>
        /// <para>addition order of inputs,hiddens,outputs must be same as kernel arguments </para>
        /// <para>multiple inputs can be bound to a single output(copies same data to all inputs), opposite can't </para>
        /// </summary>
        public class ClPipelineStage
        {

            // if this is true, it will compute after the input switch
            internal bool inputHasData = false;

            // just to inform push() of whole pipeline
            internal bool outputHasData = false;

            internal ClPipelineStageBuffer[] inputBuffers; // todo: List?
            internal List<ClPipelineStageBuffer> inputBuffersList;

            /// <summary>
            /// creates a stage to form a pipeline with other stages
            /// </summary>
            public ClPipelineStage()
            {
                inputBuffersList = new List<ClPipelineStageBuffer>();
            }

            // switch inputs(concurrently all stages) then compute(concurrently all stages, if they received input)
            internal void run()
            {

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

            }

            /// <summary>
            /// binds this stage to target stage's exit
            /// </summary>
            public void appendToStage(ClPipelineStage stage)
            {

            }

            /// <summary>
            /// setup devices that will run this stage(as parallel to speed-up this stage only, duplicated devices allowed too)
            /// </summary>
            public void addDevices(ClDevices devicesParameter)
            {

            }

            /// <summary>
            /// setup kernels to be used by this stage
            /// </summary>
            /// <param name="kernels">string containing auxiliary functions, kernel functions and constants</param>
            /// <param name="kernelNames">names of kernels to be used(in the order they run)</param>
            public void addKernels(string kernels, string kernelNames)
            {

            }

            /// <summary>
            /// not used for input or output, just for keeping sequential logic states (such as coordinates of particles)
            /// </summary>
            public void addHiddenBuffers(params Array[] inputsParameter)
            {

            }

            /// <summary>
            /// not used for input or output, just for keeping sequential logic states (such as coordinates of particles)
            /// </summary>
            public void addHiddenBuffers(params IBufferOptimization[] inputsParameter)
            {

            }

            /// <summary>
            /// not used for input or output, just for keeping sequential logic states (such as coordinates of particles)
            /// </summary>
            public void addHiddenBuffers(params IMemoryHandle[] inputsParameter)
            {

            }

            /// <summary>
            /// input arrays (ClArray, ClByteArray, byte[], ... ) to be pushed by user or to be connect to another stage's output
            /// </summary>
            public void addInputBuffers(params Array [] inputsParameter)
            {

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

            }



            /// <summary>
            /// output arrays (ClArray, ClByteArray, byte[], ... ) to be popped to user or to be connected another stage's input
            /// </summary>
            public void addOutputBuffers(params Array[] inputsParameter)
            {

            }

            /// <summary>
            /// output arrays (ClArray, ClByteArray, byte[], ... ) to be popped to user or to be connected another stage's input
            /// </summary>
            public void addOutputBuffers(params IMemoryHandle[] inputsParameter)
            {

            }

            /// <summary>
            /// output arrays (ClArray, ClByteArray, byte[], ... ) to be popped to user or to be connected another stage's input
            /// </summary>
            public void addOutputBuffers(params IBufferOptimization[] inputsParameter)
            {

            }
        }


        internal enum BufferType:int
        {
            BUF_ARRAY=0, BUF_FAST_ARRAY=1, BUF_CL_ARRAY=2
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
            private BufferType type;
            private ElementType eType;
            private object buf;
            private Array bufAsArray;
            private IMemoryHandle bufAsFastArr;
            private IBufferOptimization bufAsClArray;
            private object bufDuplicate;

            /// <summary>
            /// p: buffer to duplicate and double buffered in pipeline stages
            /// </summary>
            /// <param name="p"></param>
            public  ClPipelineStageBuffer(object p)
            {
                buf = p;
                bufAsArray = buf as Array;
                if (bufAsArray != null)
                {
                    type = BufferType.BUF_ARRAY;
                    if (buf.GetType() == typeof(float[]))
                        eType = ElementType.ELM_FLOAT;
                    else if (buf.GetType() == typeof(double[]))
                        eType = ElementType.ELM_DOUBLE;
                    else if (buf.GetType() == typeof(byte[]))
                        eType = ElementType.ELM_BYTE;
                    else if (buf.GetType() == typeof(char[]))
                        eType = ElementType.ELM_CHAR;
                    else if (buf.GetType() == typeof(int[]))
                        eType = ElementType.ELM_INT;
                    else if (buf.GetType() == typeof(long[]))
                        eType = ElementType.ELM_LONG;
                    /* to do: add struct array checking (maybe byte array) */
                }
                bufAsFastArr = buf as IMemoryHandle;
                if (bufAsFastArr != null)
                    type = BufferType.BUF_FAST_ARRAY;
                bufAsClArray = buf as IBufferOptimization;
                if (bufAsClArray != null)
                    type = BufferType.BUF_CL_ARRAY;

                if(type==BufferType.BUF_ARRAY)
                {
                    if(eType==ElementType.ELM_FLOAT)
                        bufDuplicate = new float[bufAsArray.Length];
                    else if (eType == ElementType.ELM_DOUBLE)
                        bufDuplicate = new double[bufAsArray.Length];
                    else if (eType == ElementType.ELM_BYTE)
                        bufDuplicate = new byte[bufAsArray.Length];
                    else if (eType == ElementType.ELM_CHAR)
                        bufDuplicate = new char[bufAsArray.Length];
                    else if (eType == ElementType.ELM_INT)
                        bufDuplicate = new int[bufAsArray.Length];
                    else if (eType == ElementType.ELM_LONG)
                        bufDuplicate = new long[bufAsArray.Length];
                    else if (eType == ElementType.ELM_UINT)
                        bufDuplicate = new uint[bufAsArray.Length];

                }
                else if(type==BufferType.BUF_FAST_ARRAY)
                {
                    Console.WriteLine("debug pipeline: alignment = "+ bufAsFastArr.alignmentBytes);
                    if (bufAsFastArr.arrType == CSpaceArrays.ARR_FLOAT)
                        bufDuplicate = new ClFloatArray(bufAsFastArr.Length,bufAsFastArr.alignmentBytes);
                    else if (bufAsFastArr.arrType == CSpaceArrays.ARR_DOUBLE)
                        bufDuplicate = new ClDoubleArray(bufAsFastArr.Length, bufAsFastArr.alignmentBytes);
                    else if (bufAsFastArr.arrType == CSpaceArrays.ARR_INT)
                        bufDuplicate = new ClIntArray(bufAsFastArr.Length, bufAsFastArr.alignmentBytes);
                    else if (bufAsFastArr.arrType == CSpaceArrays.ARR_LONG)
                        bufDuplicate = new ClLongArray(bufAsFastArr.Length, bufAsFastArr.alignmentBytes);
                    else if (bufAsFastArr.arrType == CSpaceArrays.ARR_BYTE)
                        bufDuplicate = new ClByteArray(bufAsFastArr.Length, bufAsFastArr.alignmentBytes);
                    else if (bufAsFastArr.arrType == CSpaceArrays.ARR_CHAR)
                        bufDuplicate = new ClCharArray(bufAsFastArr.Length, bufAsFastArr.alignmentBytes);
                    else if (bufAsFastArr.arrType == CSpaceArrays.ARR_UINT)
                        bufDuplicate = new ClUIntArray(bufAsFastArr.Length, bufAsFastArr.alignmentBytes);
                }
                else if(type==BufferType.BUF_CL_ARRAY)
                {
                    Console.WriteLine("debug pipeline: array length = "+bufAsClArray.arrayLength);
                    if (bufAsClArray.GetType() == typeof(ClArray<float>))
                        bufDuplicate = new ClArray<float>(bufAsClArray.arrayLength,bufAsClArray.alignmentBytes);
                    else if (bufAsClArray.GetType() == typeof(ClArray<double>))
                        bufDuplicate = new ClArray<double>(bufAsClArray.arrayLength, bufAsClArray.alignmentBytes);
                    else if (bufAsClArray.GetType() == typeof(ClArray<byte>))
                        bufDuplicate = new ClArray<byte>(bufAsClArray.arrayLength, bufAsClArray.alignmentBytes);
                    else if (bufAsClArray.GetType() == typeof(ClArray<char>))
                        bufDuplicate = new ClArray<char>(bufAsClArray.arrayLength, bufAsClArray.alignmentBytes);
                    else if (bufAsClArray.GetType() == typeof(ClArray<int>))
                        bufDuplicate = new ClArray<int>(bufAsClArray.arrayLength, bufAsClArray.alignmentBytes);
                    else if (bufAsClArray.GetType() == typeof(ClArray<long>))
                        bufDuplicate = new ClArray<long>(bufAsClArray.arrayLength, bufAsClArray.alignmentBytes);
                    else if (bufAsClArray.GetType() == typeof(ClArray<uint>))
                        bufDuplicate = new ClArray<uint>(bufAsClArray.arrayLength, bufAsClArray.alignmentBytes);
                }
            }

            /// <summary>
            /// double buffering for pipelining(overlap pci-e reads and writes on all pci-e bridges)
            /// </summary>
            internal void switchBuffers()
            {
                object tmp = buf;
                buf = bufDuplicate;
                bufDuplicate= tmp;
            }


            public object buffer()
            {
                return buf;
            }

           

            public object switchedBuffer()
            {
                return bufDuplicate;
            }

        }
    }
}
