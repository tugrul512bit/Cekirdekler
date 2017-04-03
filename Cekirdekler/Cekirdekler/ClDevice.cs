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
        private static extern bool deviceGDDR(IntPtr hDevice);

        private IntPtr hDevice;
        private IntPtr hPlatform;
        private ClString deviceName__ = null;
        private bool isDeleted = false;
        public string deviceName_ = "";
        private int typeOfDevice_ = -1;
        private bool GDDR = false;
        public ClDevice(ClPlatform clPlatform,int type_, int i,bool devicePartition,bool GPU_STREAM,int MAX_CPU)
        {
            deviceName__ = new ClString(" ");
            hPlatform = clPlatform.h();
            if (type_ == ClPlatform.CODE_CPU() && devicePartition)
            {
                int epc1 = Environment.ProcessorCount-1;
                if (MAX_CPU != -1)
                {
                    epc1 = Math.Max(Math.Min(MAX_CPU, epc1), 1);
                }
                Console.WriteLine(epc1 + " cores are chosen for compute(equals to device partition cores).");

                hDevice = createDeviceAsPartition(hPlatform, type_, i, epc1);
            }
            else
                hDevice = createDevice(hPlatform, type_, i);
            getDeviceName(hDevice, deviceName__.h());
            deviceName_ = JsonCPPCS.read(deviceName__.h());
            typeOfDevice_ = type_;
            if (GPU_STREAM)
                GDDR = false;
            else
                GDDR = deviceGDDR(hDevice);
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
            return typeOfDevice_;
        }

        /// <summary>
        /// device name
        /// </summary>
        /// <returns></returns>
        public string name()
        {
            return deviceName_;
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
        /// releases resources taken in C++ C space functions
        /// </summary>
        public void dispose()
        {
            if (!isDeleted)
                deleteDevice(hDevice);
            isDeleted = true;
        }
    }
}
