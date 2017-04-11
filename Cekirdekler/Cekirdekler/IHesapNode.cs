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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cekirdekler;
namespace ClCluster
{
    /// <summary>
    /// <para>prealpha cluster add-on</para>
    /// <para>hesaplayacak nesne, Cekirdekler api de olabilir,</para> 
    /// <para>bir pc(ip tcp) grubunda çalışan Hizlandirici api de(içinde Cekirdekler var) olabilir</para> 
    /// </summary>
    interface IComputeNode
    {
        // kurulumun verilen parametrelere göre yapılışı
        void setupNodes(string deviceTypesParameter, string kernelsStringParameter,
                        string[] kernelNamesStringArray, int localRangeParameter = 256,
                        int numGPUsToUse = -1, bool GPU_STREAM = true,
                        int MAX_CPU = -1);

        /// <summary>
        /// compute
        /// </summary>
        /// <returns></returns>
        void compute(string[] kernelNamesStringArray = null,
            int numSteps = 0, string stepFunction = "",
            object[] arrays = null, string[] readWrite = null,
            int[] arrayElementsPerWorkItem = null,
            int globalRange = 1024, int computeId = 1,
            int globalOffset = 0, bool pipelineEnabled = false,
            int pipelineBlobCount = 4, bool pipelineType = Cores.PIPELINE_EVENT);

        /// <summary>
        /// total time for read compute write
        /// </summary>
        /// <returns></returns>
        double computeTiming();

        /// <summary>
        /// release C++ resources
        /// </summary>
        void dispose();
    }
}
