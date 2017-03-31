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

namespace Cekirdekler
{
    /// <summary>
    /// wrapper for opencl ndrange
    /// </summary>
    public class ClNdRange
    {
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr createNdRange(int n);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void deleteNdRange(IntPtr hRange);

        private IntPtr hRange;
        private int range;
        private bool isDeleted = false;

        /// <summary>
        /// creates a ndrange object in "C" space
        /// </summary>
        /// <param name="range_"></param>
        public ClNdRange(int range_)
        {
            range = range_;
            hRange = createNdRange(range);
        }

        /// <summary>
        /// handle to ndrange object in C++
        /// </summary>
        /// <returns></returns>
        public IntPtr h()
        {
            return hRange;
        }

        /// <summary>
        /// release C++ resources
        /// </summary>
        public void dispose()
        {
            if (!isDeleted)
                deleteNdRange(hRange);
            isDeleted = true;
        }

    }
}
