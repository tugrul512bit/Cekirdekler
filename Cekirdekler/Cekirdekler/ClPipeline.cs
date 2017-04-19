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
        /// <para>1 push      = 1 operation (read)</para>
        /// <para>1 more push = 2 operations(read + compute)</para>
        /// <para>1 more push = 3 operations(read + compute + write)</para>
        /// <para>1 more push = 4 operations(read + compute + write)+(read on next stage)</para>
        /// <para>1 more push = 5 operations(read + compute + write)+(read on next stage + compute)</para>
        /// <para>1 more push = 6 operations(read + compute + write)+(read on next stage + compute + write)</para>
        /// <para>1 more push = 7 operations(read + compute + write)+(read on next stage + compute + write)+(...)</para>
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
            public void addHiddenBuffer(params Array[] inputsParameter)
            {

            }

            /// <summary>
            /// not used for input or output, just for keeping sequential logic states (such as coordinates of particles)
            /// </summary>
            public void addHiddenBuffer(params IBufferOptimization[] inputsParameter)
            {

            }

            /// <summary>
            /// not used for input or output, just for keeping sequential logic states (such as coordinates of particles)
            /// </summary>
            public void addHiddenBuffer(params IMemoryHandle[] inputsParameter)
            {

            }

            /// <summary>
            /// input arrays (ClArray, ClByteArray, byte[], ... ) to be pushed by user or to be connect to another stage's output
            /// </summary>
            public void addInputs(params Array [] inputsParameter)
            {

            }

            /// <summary>
            /// input arrays (ClArray, ClByteArray, byte[], ... ) to be pushed by user or to be connect to another stage's output
            /// </summary>
            public void addInputs(params IBufferOptimization[] inputsParameter)
            {

            }

            /// <summary>
            /// input arrays (ClArray, ClByteArray, byte[], ... ) to be pushed by user or to be connect to another stage's output
            /// </summary>
            public void addInputs(params IMemoryHandle[] inputsParameter)
            {

            }



            /// <summary>
            /// output arrays (ClArray, ClByteArray, byte[], ... ) to be popped to user or to be connected another stage's input
            /// </summary>
            public void addOutputs(params Array[] inputsParameter)
            {

            }

            /// <summary>
            /// output arrays (ClArray, ClByteArray, byte[], ... ) to be popped to user or to be connected another stage's input
            /// </summary>
            public void addOutputs(params IMemoryHandle[] inputsParameter)
            {

            }

            /// <summary>
            /// output arrays (ClArray, ClByteArray, byte[], ... ) to be popped to user or to be connected another stage's input
            /// </summary>
            public void addOutputs(params IBufferOptimization[] inputsParameter)
            {

            }
        }
    }
}
