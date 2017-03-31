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

namespace Cekirdekler
{
    /// <summary>
    /// wrapper for cl::program
    /// </summary>
    public class ClProgram
    {

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr createProgram(IntPtr hContext, IntPtr hDevice, IntPtr hString);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void deleteProgram(IntPtr hProgram);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern int getProgramErr(IntPtr hProgram);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern string readProgramErrString(IntPtr hProgram);

        private IntPtr hProgram;
        private IntPtr hContext;
        private IntPtr hDevice;
        private IntPtr hString;
        private string strError_______;
        private bool isDeleted = false;

        /// <summary>
        /// error code that tells something wrong if not zero
        /// </summary>
        public int errorCode_______ = 0;

        /// <summary>
        /// creates a program from kernel string and context
        /// </summary>
        /// <param name="context"></param>
        /// <param name="clKernelString">kernel string to compile</param>
        public ClProgram(ClContext context, ClString clKernelString)
        {
            hContext = context.h();
            hDevice = context.hd();
            hString = clKernelString.h();
            hProgram = createProgram(hContext, hDevice, hString);
            errorCode_______ = getProgramErr(hProgram);
           // strHata = Encoding.UTF8.GetString(programHataStringiOku(hProgram));
            strError_______ = new StringBuilder( readProgramErrString(hProgram)).ToString();
           
        }

        /// <summary>
        /// error message from compiler
        /// </summary>
        /// <returns></returns>
        public string errMsg()
        {
            return strError_______;
        }

        /// <summary>
        /// handle to program object in "C" space
        /// </summary>
        /// <returns></returns>
        public IntPtr h()
        {
            return hProgram;
        }

        /// <summary>
        /// handle to context of program (in C space)
        /// </summary>
        /// <returns></returns>
        public IntPtr hc()
        {
            return hContext;
        }

        /// <summary>
        /// handle to device that was chosen to be compiled for 
        /// </summary>
        /// <returns></returns>
        public IntPtr hd()
        {
            return hDevice;
        }

        /// <summary>
        /// string pointer
        /// </summary>
        /// <returns></returns>
        public IntPtr hs()
        {
            return hString;
        }


        /// <summary>
        /// release C++ resources
        /// </summary>
        public void dispose()
        {
            if (!isDeleted)
                deleteProgram(hProgram);
            isDeleted = true;
        }
    }
}
