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
    /// wrapper for event array in C++
    /// </summary>
    internal class ClEventArray
    {
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr createEventArr(bool cpy);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void addToEventArr(IntPtr hArr,IntPtr hEvent, bool isCopy);

        
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void deleteEventArr(IntPtr hArr);

        IntPtr hArr;

        /// <summary>
        /// creates event array for opencl commands
        /// </summary>
        /// <param name="isCopy"></param>
        public ClEventArray(bool isCopy=false)
        {
            hArr = createEventArr(isCopy);
        }

        /// <summary>
        /// adds event to event array
        /// </summary>
        /// <param name="e"></param>
        /// <param name="isCopy"></param>
        public void add(ClEvent e,bool isCopy=false)
        {
            addToEventArr(hArr,e.h(), isCopy);
        }

        /// <summary>
        /// handle to event array object
        /// </summary>
        /// <returns></returns>
        public IntPtr h()
        {
            return hArr;
        }

        /// <summary>
        /// releases C++ resources
        /// </summary>
        public void dispose()
        {
            if (hArr != IntPtr.Zero)
            {
                deleteEventArr(hArr);
                hArr = IntPtr.Zero;
            }
        }
    }
}
