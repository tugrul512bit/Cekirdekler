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

namespace ClObject
{

    /// <summary>
    /// wrapper for C++ strings 
    /// </summary>
    internal class ClString
    {
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr createString();

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr writeToString(IntPtr hString, IntPtr text);

        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void deleteString(IntPtr hString);

        private IntPtr hString;
        private bool isDeleted = false;
        private string str;

        /// <summary>
        /// creates a string(C++) from a string(C#) 
        /// </summary>
        /// <param name="strg"></param>
        public ClString(string strg)
        {
            str = new StringBuilder(strg).ToString();
            hString = createString();
            IntPtr tmp = Marshal.StringToHGlobalAnsi(new StringBuilder(str).ToString());
            writeToString(hString, tmp);
            Marshal.FreeHGlobal(tmp);
        }

        /// <summary>
        /// writes to C++ string
        /// </summary>
        /// <param name="strg"></param>
        public void write(string strg)
        {
            str = new StringBuilder(strg).ToString();
            IntPtr tmp = Marshal.StringToHGlobalAnsi(new StringBuilder(str).ToString());
            writeToString(hString, tmp);
            Marshal.FreeHGlobal(tmp);
        }

        /// <summary>
        /// handle to C++ string
        /// </summary>
        /// <returns></returns>
        public IntPtr h()
        {
            return hString;
        }

        /// <summary>
        /// not changed in C++ side so simply returns
        /// </summary>
        /// <returns></returns>
        public string read()
        {
            return str;
        }

        /// <summary>
        /// release resources in C++ side
        /// </summary>
        public void dispose()
        {
            if (!isDeleted)
                deleteString(hString);
            isDeleted = true;
        }
    }
}
