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
using System.Web;
using System.Web.Script.Serialization;

namespace ClObject
{
    /// <summary>
    /// reads string from C space and converts json
    /// </summary>
    internal class JsonCPPCS
    {
        public static JavaScriptSerializer json = new JavaScriptSerializer();
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr readFromString(IntPtr hStringInfo);

        /// <summary>
        /// not a real callback actually
        /// </summary>
        /// <param name="hStringInfo"></param>
        [DllImport("KutuphaneCL", CallingConvention = CallingConvention.Cdecl)]
        private static extern void jsonStringCallBack(IntPtr hStringInfo);

        /// <summary>
        /// read string from C space string and parse by javascript serializer
        /// </summary>
        /// <param name="hStringInfo"></param>
        /// <returns></returns>
        public static string readWithDispose(IntPtr hStringInfo)
        {
            IntPtr t = readFromString(hStringInfo);
            string str = Marshal.PtrToStringAnsi(t);
            jsonStringCallBack(hStringInfo);
            return str;
        }

        /// <summary>
        /// (without delete )read string from C space string and parse by javascript serializer
        /// </summary>
        /// <param name="hStringInfo"></param>
        /// <returns></returns>
        public static string readWithoutDispose(IntPtr hStringInfo)
        {
            IntPtr t = readFromString(hStringInfo);
            string str = Marshal.PtrToStringAnsi(t);
            return str;
        }
    }
}
