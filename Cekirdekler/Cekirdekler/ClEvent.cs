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
    /// wrapper for opencl event objects
    /// </summary>
    internal class ClEvent
    {
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr createEvent();

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void deleteEvent(IntPtr hEvent);

        IntPtr hEvent;

        /// <summary>
        /// creates an event to be used in commands
        /// </summary>
        public ClEvent()
        {
            hEvent = createEvent();
        }

        /// <summary>
        /// handle to event object
        /// </summary>
        /// <returns></returns>
        public IntPtr h()
        {
            return hEvent;
        }

        /// <summary>
        /// releases C++ resources
        /// </summary>
        public void dispose()
        {
            if (hEvent != IntPtr.Zero)
            {
                deleteEvent(hEvent);
                hEvent = IntPtr.Zero;
            }
        }
    }
}
