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
    /// wrapper for opencl context
    /// </summary>
    internal class ClContext
    {

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr createContext(IntPtr hPlatform, IntPtr hDevice);


        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void deleteContext(IntPtr hContext);

        private IntPtr hContext;
        private IntPtr hDevice;
        private IntPtr hPlatform;
        private bool isDeleted = false;

        /// <summary>
        /// creates a context for a device, this library makes use of explicit multi device control for compute
        /// </summary>
        /// <param name="device">device to use for context</param>
        public ClContext(ClDevice device)
        {
            hDevice = device.h();
            hPlatform = device.hp();
            hContext = createContext(hPlatform, hDevice);
        }

        /// <summary>
        /// handle to context object in C++
        /// </summary>
        /// <returns></returns>
        public IntPtr h()
        {
            return hContext;
        }


        /// <summary>
        /// handle to device object in C++ which is used in this context
        /// </summary>
        /// <returns></returns>
        public IntPtr hd()
        {
            return hDevice;
        }

        /// <summary>
        /// handle to platform object in C++ which holds the device for this context
        /// </summary>
        /// <returns></returns>
        public IntPtr hp()
        {
            return hPlatform;
        }

        /// <summary>
        /// release C++ resources
        /// </summary>
        public void dispose()
        {

            if (!isDeleted)
                deleteContext(hContext);
            isDeleted = true;
        }



    }
}
