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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ClObject
{
    /// <summary>
    /// wrapper for opencl device objects
    /// </summary>
    internal class ClDevice
    {
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr createDevice(IntPtr hPlatform,int type_, int index_);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr createDeviceAsPartition(IntPtr hPlatform, int type_, int index_,int numberOfCpuCores_);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void deleteDevice(IntPtr hDevice);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void getDeviceName(IntPtr hDevice, IntPtr hString);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void getDeviceVendorName(IntPtr hDevice, IntPtr hString);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool deviceGDDR(IntPtr hDevice);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern int deviceComputeUnits(IntPtr hDevice);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong deviceMemSize(IntPtr hDevice);

        private IntPtr hDevice;
        private IntPtr hPlatform;
        private ClString deviceNameClString = null;
        private ClString deviceVendorNameClString = null;
        private bool isDeleted = false;
        public string deviceNameStringFromOpenclCSpace = "";
        public string deviceVendorNameStringFromOpenclCSpace = "";
        private int typeOfUsedDeviceInClPlatform = -1;
        private int numberOfComputeUnitsPrivate = 0;
        public int numberOfComputeUnits {get { return numberOfComputeUnitsPrivate; } }
        private bool GDDR = false;
        private ulong memorySizePrivate = 0;
        public ulong memorySize { get { return memorySizePrivate; }  }
        internal ClPlatform clPlatformForCopy;
        private int deviceTypeCodeInClPlatformForCopy;
        private int iForCopy;
        private bool devicePartitionForCopy;
        private bool GPU_STREAMForCopy;
        private int MAX_CPUForCopy;
        public ClDevice(ClPlatform clPlatform,int deviceTypeCodeInClPlatform, int i,bool devicePartition,bool GPU_STREAM,int MAX_CPU)
        {
            clPlatformForCopy = clPlatform;
            deviceTypeCodeInClPlatformForCopy = deviceTypeCodeInClPlatform;
            iForCopy = i;
            devicePartitionForCopy = devicePartition;
            GPU_STREAMForCopy = GPU_STREAM;
            MAX_CPUForCopy = MAX_CPU;
            deviceNameClString = new ClString(" ");
            deviceVendorNameClString = new ClString(" ");
            hPlatform = clPlatform.h();
            if (deviceTypeCodeInClPlatform == ClPlatform.CODE_CPU() && devicePartition)
            {
                int epc1 = Environment.ProcessorCount-1;
                if (MAX_CPU != -1)
                {
                    epc1 = Math.Max(Math.Min(MAX_CPU, epc1), 1);
                }
                Console.WriteLine(epc1 + " cores are chosen for compute(equals to device partition cores).");

                hDevice = createDeviceAsPartition(hPlatform, deviceTypeCodeInClPlatform, i, epc1);
            }
            else
                hDevice = createDevice(hPlatform, deviceTypeCodeInClPlatform, i);
            getDeviceName(hDevice, deviceNameClString.h());
            getDeviceVendorName(hDevice, deviceVendorNameClString.h());
            numberOfComputeUnitsPrivate = deviceComputeUnits(hDevice);
            memorySizePrivate = deviceMemSize(hDevice);
            deviceNameStringFromOpenclCSpace = JsonCPPCS.read(deviceNameClString.h());
            deviceVendorNameStringFromOpenclCSpace = JsonCPPCS.read(deviceVendorNameClString.h());
            typeOfUsedDeviceInClPlatform = deviceTypeCodeInClPlatform;
            if (GPU_STREAM)
                GDDR = false;
            else
                GDDR = deviceGDDR(hDevice);
        }

        internal ClDevice copy(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
        {
            ClDevice result = new ClDevice(clPlatformForCopy,
                deviceTypeCodeInClPlatformForCopy,
                iForCopy,
                devicePartitionForCopy | devicePartitionEnabled,
                GPU_STREAMForCopy | streamingEnabled,
                ((MAX_CPU_CORES<=0)? MAX_CPUForCopy:MAX_CPU_CORES));


            return result;
        }

        internal ClDevice copyExact()
        {
            ClDevice result = new ClDevice(clPlatformForCopy,
                deviceTypeCodeInClPlatformForCopy,
                iForCopy,
                devicePartitionForCopy,
                GPU_STREAMForCopy,
                MAX_CPUForCopy);


            return result;
        }

        /// <summary>
        /// if device has dedicated memory, returns true
        /// </summary>
        /// <returns></returns>
        public bool isGddr()
        {
            return GDDR;
        }

        /// <summary>
        /// type of device
        /// </summary>
        /// <returns></returns>
        public int type()
        {
            return typeOfUsedDeviceInClPlatform;
        }

        /// <summary>
        /// device name
        /// </summary>
        /// <returns></returns>
        public string name()
        {
            return deviceNameStringFromOpenclCSpace;
        }

        /// <summary>
        /// device vendor name
        /// </summary>
        /// <returns></returns>
        public string vendorName()
        {
            return deviceVendorNameStringFromOpenclCSpace;
        }

        /// <summary>
        /// handle for device object in C space
        /// </summary>
        /// <returns>address</returns>
        public IntPtr h()
        {
            return hDevice;
        }

        /// <summary>
        /// handle for platform object (that contains this device object) in C space
        /// </summary>
        /// <returns></returns>
        public IntPtr hp()
        {
            return hPlatform;
        }

        /// <summary>
        /// this device and its platform is used in number crunching and will be disposed in there
        /// </summary>
        public void cruncherWillDispose()
        {
            isDeleted = true;
            clPlatformForCopy.cruncherWillDispose();
        }

        /// <summary>
        /// releases resources taken in C++ C space functions
        /// </summary>
        public void dispose()
        {
            if (!isDeleted)
                deleteDevice(hDevice);
            isDeleted = true;
        }

        ~ClDevice()
        {
            dispose();
        }
    }
}
