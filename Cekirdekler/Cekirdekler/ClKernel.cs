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
    /// wrapper for opencl kernel object
    /// </summary>
    internal class ClKernel
    {
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr createKernel(IntPtr hProgram, IntPtr hString);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void deleteKernel(IntPtr hKernel);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern int getKernelErr(IntPtr hKernel);

        private IntPtr hKernel;
        private IntPtr hProgram;
        private IntPtr hString;
        private bool isDeleted = false;
        public int intKernelError = 0;

        /// <summary>
        /// takes program and a kernel name and prepares a kernel to be used.
        /// </summary>
        /// <param name="program">opencl wrapper of cl::program</param>
        /// <param name="kernelName">string wrapper of kernel name</param>
        public ClKernel(ClProgram program, ClString kernelName)
        {
            hProgram = program.h();
            hString = kernelName.h();
            hKernel = createKernel(hProgram, hString);
            intKernelError = getKernelErr(hKernel);
        }

        /// <summary>
        /// handle for kernel object in "C" space
        /// </summary>
        /// <returns></returns>
        public IntPtr h()
        {
            return hKernel;
        }

        /// <summary>
        /// kernel name string pointer
        /// </summary>
        /// <returns></returns>
        public IntPtr hs()
        {
            return hString;
        }

        /// <summary>
        /// releases resources that are allocated in "C" space
        /// </summary>
        public void dispose()
        {
            if (!isDeleted)
                deleteKernel(hKernel);
            isDeleted = true;
        }
    }
}
