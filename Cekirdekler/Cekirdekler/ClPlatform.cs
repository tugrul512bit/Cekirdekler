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

using System.Runtime.InteropServices;
using System.Text;

using System.Web.Script.Serialization;

namespace ClObject
{
    /// <summary>
    /// cl::platform wrapper with simple methods
    /// </summary>
    internal class ClPlatform
    {
        public static JavaScriptSerializer json = new JavaScriptSerializer();

        [DllImport("KutuphaneCL.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr createPlatform(IntPtr hList, int index);

        [DllImport("KutuphaneCL.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void deletePlatform(IntPtr hPlatform);

        [DllImport("KutuphaneCL.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr getPlatformInfo(IntPtr hPlatform);

        /// <summary>
        /// takes cpu flag to be used later in other wrappers
        /// </summary>
        /// <returns></returns>
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CODE_CPU();

        /// <summary>
        /// takes gpu flag to be used later in other wrappers
        /// </summary>
        /// <returns></returns>
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CODE_GPU();


        /// <summary>
        /// takes accelerator flag to be used in other wrappers
        /// </summary>
        /// <returns></returns>
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CODE_ACC();

        /// <summary>
        /// number of cpus in a platform
        /// </summary>
        /// <param name="hPlatform"></param>
        /// <returns></returns>
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int numberOfCpusInPlatform(IntPtr hPlatform);

        /// <summary>
        /// number of gpus in a platform
        /// </summary>
        /// <param name="hPlatform"></param>
        /// <returns></returns>
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int numberOfGpusInPlatform(IntPtr hPlatform);


        /// <summary>
        /// number of accelerators in platorm
        /// </summary>
        /// <param name="hPlatform"></param>
        /// <returns></returns>
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        public static extern int numberOfAcceleratorsInPlatform(IntPtr hPlatform);


        private IntPtr hPlatform;


        private bool isDeleted = false;
        private int intPlatformErrorCode = 0;

        public List<ClDevice> selectedDevices;

        /// <summary>
        /// gets a platform from a list of platforms in "C" space using an index
        /// </summary>
        /// <param name="hPlatformList_">C space array with N platforms</param>
        /// <param name="index_">0 to N-1</param>
        public ClPlatform(IntPtr hPlatformList_, int index_)
        {
            hPlatform = createPlatform(hPlatformList_,index_);
            selectedDevices = new List<ClDevice>();
        }

        /// <summary>
        /// number of gpus in this platform
        /// </summary>
        /// <returns></returns>
        public int numberOfGpus()
        {
            return numberOfGpusInPlatform(hPlatform);
        }

        /// <summary>
        /// number of cpus in this platform
        /// </summary>
        /// <returns></returns>
        public int numberOfCpus()
        {
            return numberOfCpusInPlatform(hPlatform);
        }

        /// <summary>
        /// number of accelerators in this platform
        /// </summary>
        /// <returns></returns>
        public int numberOfAccelerators()
        {
            return numberOfAcceleratorsInPlatform(hPlatform);
        }

        /// <summary>
        /// handle to this platform in C space
        /// </summary>
        /// <returns></returns>
        public IntPtr h()
        {
            return hPlatform;
        }

        /// <summary>
        /// disposes C space platform objects
        /// </summary>
        public void dispose()
        {
            if (!isDeleted)
                deletePlatform(hPlatform);
            isDeleted = true;
        }

        private class PlatformInformation
        {
            public int totalCPUs;
            public int totalGPUs;
            public int totalACCs;
            public float[] testVariables;
            public string strTest;
        }
    }
}
