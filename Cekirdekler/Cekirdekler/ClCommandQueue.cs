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
    /// wrapper for opencl command queue
    /// </summary>
    internal class ClCommandQueue
    {
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr createCommandQueue(IntPtr hContext, IntPtr hDevice, int async);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void deleteCommandQueue(IntPtr hCommandQueue);

        private IntPtr hCommandQueue;
        private IntPtr hContext;
        private IntPtr hDevice;

        /// <summary>
        /// creates a command queue in a context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="async">async!=0 means out-of-order command queue</param>
        public ClCommandQueue(ClContext context, int async=0)
        {
            hContext = context.h();
            hDevice = context.hd();
            hCommandQueue = createCommandQueue(hContext, hDevice,async);
        }

        /// <summary>
        /// handle to command queue object in C++
        /// </summary>
        /// <returns></returns>
        public IntPtr h()
        {
            return hCommandQueue;
        }

        /// <summary>
        /// handle to context object in C++ which holds this command queue
        /// </summary>
        /// <returns></returns>
        public IntPtr hc()
        {
            return hContext;
        }

        /// <summary>
        /// handle to device in C++ which is used for this command queue
        /// </summary>
        /// <returns></returns>
        public IntPtr hd()
        {

            return hDevice;
        }

        /// <summary>
        /// releases C++ resources for this commmand queue
        /// </summary>
        public void dispose()
        {

            if (hCommandQueue!=IntPtr.Zero)
                deleteCommandQueue(hCommandQueue);
            hCommandQueue = IntPtr.Zero;
        }
    }
}
