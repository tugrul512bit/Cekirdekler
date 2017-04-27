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

using Cekirdekler.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cekirdekler
{

    /// <summary>
    /// CPU|GPU means all GPUs and all CPUs
    /// GPU|ACC means all GPUs and all ACCs
    /// GPU means only GPUs are used
    /// </summary>
    public enum AcceleratorType : int
    {
        /// <summary>
        /// sadece işlemcileri kullanır
        /// </summary>
        CPU = 1,

        /// <summary>
        /// sadece ekran kartlarını ve entegre grafik çiplerini kullanır
        /// </summary>
        GPU = 2,

        /// <summary>
        /// sadece özel hızlandırıcıları kullanır
        /// </summary>
        ACC = 4,

    }
    


    /// <summary>
    /// compiles kernel strings for the selected devices then computes with array(C#,C++) parameters later 
    /// </summary>
    public class ClNumberCruncher
    {
        internal Cores numberCruncher {get;set;}
        internal int errorNotification { get; set; }
        internal int numberOfErrorsHappened { get; set; }
        /// <summary>
        /// outputs to console: each device's performance(and memory target type) results per compute() operation
        /// </summary>
        public bool performanceFeed { get; set; }

        /// <summary>
        /// to ease balancing against performance spikes, interrupts, hiccups
        /// </summary>
        public bool smoothLoadBalancer { get { return numberCruncher.smoothLoadBalancer; } set { numberCruncher.smoothLoadBalancer = value; } }

        /// <summary>
        /// <para>prepares devices and compiles kernels in the kernel string</para>
        /// <para>does optionally pipelined kernel execution load balancing between multiple devices</para>
        /// </summary>
        /// <param name="cpuGpu">AcceleratorType.CPU|AcceleratorType.GPU or similar</param>
        /// <param name="kernelString">something like: @"multi-line C# string that has multiple kernel definitions"</param>
        /// <param name="numberofCPUCoresToUseAsDeviceFission">AcceleratorType.CPU uses number of threads for an N-core CPU(between 1 and N-1)(-1 means N-1)</param>
        /// <param name="numberOfGPUsToUse">AcceleratorType.GPU uses number of GPUs equal to this parameter. Between 1 and N(-1 means N)</param>
        /// <param name="stream">devices that share RAM with CPU will not do extra copies. Devices that don't share RAM will directly access RAM and reduce number of copies</param>
        public ClNumberCruncher(AcceleratorType cpuGpu, string kernelString,
                            int numberofCPUCoresToUseAsDeviceFission = -1,
                            int numberOfGPUsToUse = -1, bool stream = true)
        {
            numberOfErrorsHappened = 0;
            StringBuilder cpuGpu_ = new StringBuilder("");
            if (((int)cpuGpu & ((int)AcceleratorType.CPU)) > 0)
                cpuGpu_.Append("cpu ");
            if (((int)cpuGpu & ((int)AcceleratorType.GPU)) > 0)
                cpuGpu_.Append("gpu ");
            if (((int)cpuGpu & ((int)AcceleratorType.ACC)) > 0)
                cpuGpu_.Append("acc ");

            List<string> kernelNames_ = new List<string>();

            // extracting patterns kernel _ _ _ void _ _ name _ _ (
            string kernelVoidRegex = "(kernel[\\s]+void[\\s]+[a-zA-Z\\d_]+[^\\(])";
            Regex regex = new Regex(kernelVoidRegex);
            MatchCollection match = regex.Matches(kernelString);
            for (int i = 0; i < match.Count; i++)
            {
                // extracting name
                Regex rgx = new Regex("([\\s]+[a-zA-Z\\d_]+)");
                MatchCollection mc = rgx.Matches(match[i].Value.Trim());
                kernelNames_.Add(mc[mc.Count - 1].Value.Trim());
            }
            if (kernelNames_.Count == 0)
            {
                Console.WriteLine("Error: no kernel definitions are found in string. Kernel string: \n" + kernelString);
                errorNotification = 1;
                return;
            }
            numberCruncher = new Cores(cpuGpu_.ToString(), kernelString, kernelNames_.ToArray(), 256,
                numberOfGPUsToUse, stream, numberofCPUCoresToUseAsDeviceFission);
            if (numberCruncher.errorCode() != 0)
            {
                errorMessage_ = numberCruncher.errorMessage();
                Console.WriteLine(numberCruncher.errorMessage());
                errorNotification = numberCruncher.errorCode();
                numberCruncher.dispose();
                numberOfErrorsHappened++;
                return;
            }
            

        }

        /// <summary>
        /// returns relative compute power of each device
        /// </summary>
        /// <returns></returns>
        public double[] normalizedComputePowersOfDevices()
        {
            if ((numberCruncher != null) && (numberCruncher.tmpThroughputs != null) && (numberCruncher.tmpThroughputs.Length > 0))
            {
                double[] result = new double[numberCruncher.tmpThroughputs.Length];
                double total = 0;
                for(int i=0;i<numberCruncher.tmpThroughputs.Length;i++)
                {
                    result[i] = numberCruncher.tmpThroughputs[i];
                    total += numberCruncher.tmpThroughputs[i];
                }
                for (int i = 0; i < numberCruncher.tmpThroughputs.Length; i++)
                    result[i] /= total;
                    return result;
            }
            else
                return null;
        }

        /// <summary>
        /// returns relative global range of each device
        /// </summary>
        /// <returns></returns>
        public double[] normalizedGlobalRangesOfDevices(int id)
        {
            if ((numberCruncher != null) && (numberCruncher.tmpThroughputs != null) && (numberCruncher.tmpThroughputs.Length > 0))
            {
                double[] result = new double[numberCruncher.tmpThroughputs.Length];
                double total = 0;
                for (int i = 0; i < numberCruncher.tmpThroughputs.Length; i++)
                {
                    result[i] = numberCruncher.globalRanges[id][i];
                    total += numberCruncher.globalRanges[id][i];
                }
                for (int i = 0; i < numberCruncher.tmpThroughputs.Length; i++)
                    result[i] /= total;
                return result;
            }
            else
                return null;
        }

        /// <summary>
        /// <para>prepares devices and compiles kernels in the kernel string</para>
        /// <para>does optionally pipelined kernel execution load balancing between multiple devices</para>
        /// </summary>
        /// <param name="devicesForGPGPU">one or more devices for GPGPU</param>
        /// <param name="kernelString">something like: @"multi-line C# string that has multiple kernel definitions"</param>
        public ClNumberCruncher(ClDevices devicesForGPGPU, string kernelString)
        {
            numberOfErrorsHappened = 0;
            List<string> kernelNames_ = new List<string>();

            // extracting patterns kernel _ _ _ void _ _ name _ _ (
            string kernelVoidRegex = "(kernel[\\s]+void[\\s]+[a-zA-Z\\d_]+[^\\(])";
            Regex regex = new Regex(kernelVoidRegex);
            MatchCollection match = regex.Matches(kernelString);
            for (int i = 0; i < match.Count; i++)
            {
                // extracting name
                Regex rgx = new Regex("([\\s]+[a-zA-Z\\d_]+)");
                MatchCollection mc = rgx.Matches(match[i].Value.Trim());
                kernelNames_.Add(mc[mc.Count - 1].Value.Trim());
            }
            if (kernelNames_.Count == 0)
            {
                Console.WriteLine("Error: no kernel definitions are found in string. Kernel string: \n" + kernelString);
                errorNotification = 1;
                return;
            }
            numberCruncher = new Cores(devicesForGPGPU, kernelString, kernelNames_.ToArray());
            if (numberCruncher.errorCode() != 0)
            {
                errorMessage_ = numberCruncher.errorMessage();
                Console.WriteLine(numberCruncher.errorMessage());
                errorNotification = numberCruncher.errorCode();
                numberCruncher.dispose();
                numberOfErrorsHappened++;
                return;
            }


        }

        private string errorMessage_;

        /// <summary>
        /// kernel compile error
        /// </summary>
        /// <returns></returns>
        public string errorMessage()
        {
            return errorMessage_;
        }

        /// <summary>
        /// not zero means error
        /// </summary>
        /// <returns></returns>
        public int errorCode()
        {
            return errorNotification;
        }

        /// <summary>
        /// releases C++ resources
        /// </summary>
        public void dispose()
        {

            if (numberCruncher != null)
                numberCruncher.dispose();
            numberCruncher = null;
        }

        /// <summary>
        /// releases C++ resources
        /// </summary>
        ~ClNumberCruncher()
        {
            if(numberCruncher!=null)
                numberCruncher.dispose();
            numberCruncher = null;
        }
        
    }
}
