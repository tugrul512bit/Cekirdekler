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

namespace ClObject
{
    /// <summary>
    /// wrapper for opencl user event
    /// </summary>
    internal class ClUserEvent
    {
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr createUserEvent(IntPtr hContext);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void deleteUserEvent(IntPtr hUserEvent);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void triggerUserEvent(IntPtr hUserEvent);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void addUserEvent(IntPtr hCommandQueue,IntPtr hUserEvent);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void decrementUserEvent(IntPtr hUserEvent, IntPtr hContext);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void incrementUserEvent(IntPtr hUserEvent);

        IntPtr hUserEvent;
        IntPtr hContext;

        /// <summary>
        /// creates user event for fine grained synchronization
        /// </summary>
        /// <param name="ct"></param>
        public ClUserEvent(ClContext ct)
        {
            hUserEvent = createUserEvent(ct.h());
            hContext = ct.h();
        }
        private object lockObj = new object();
        private int ctr = 0;

        /// <summary>
        /// decrement user event counter
        /// </summary>
        public void dec()
        {
            lock(lockObj)
            {
                ctr--;
                decrementUserEvent(hUserEvent, hContext);
            }
        }

        /// <summary>
        /// increment user event counter
        /// </summary>
        public void inc()
        {
            lock (lockObj)
            {
                ctr++;
                incrementUserEvent(hUserEvent);
            }
        }

        /// <summary>
        /// release C++ resources
        /// </summary>
        public void dispose()
        {
            if (hUserEvent != IntPtr.Zero)
            {
                deleteUserEvent(hUserEvent);
                hUserEvent = IntPtr.Zero;
            }
        }

        /// <summary>
        /// trigger user event
        /// </summary>
        public void trigger()
        {
            triggerUserEvent(hUserEvent);
        }

        /// <summary>
        /// add command queue to user event
        /// </summary>
        /// <param name="cq"></param>
        public void addCommandQueue(ClCommandQueue cq)
        {
            addUserEvent(cq.h(), hUserEvent);
            lock (lockObj)
            {
                ctr++;
            }
        }

    }
}
