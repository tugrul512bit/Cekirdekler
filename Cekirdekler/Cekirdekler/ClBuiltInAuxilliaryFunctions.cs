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
using System.Text;

namespace Cekirdekler
{
    namespace ClBuiltInHelper
    {
        /// <summary>
        /// <para>Only setting some flags will add some functions to beginning of OpenCL kernel string before compile</para>
        /// <para>Not implemented(yet)</para>
        /// </summary>
        public class ClBuiltInAuxilliaryFunctions
        {

            private StringBuilder selectedFunctionsBuilder = new StringBuilder(@"");
            private bool prepared = false;

            private string exampleFunctionString = Environment.NewLine+ @"void exampleFunction(int a,int b){return a+b;}"+Environment.NewLine;
            private bool exampleFunctionPrivate = false;
            /// <summary>
            /// adds "void exampleFunction(int a,int b){return a+b;}" in the beginning of kernel string
            /// </summary>
            public bool exampleFunction {
                get { if (!prepared) { selectedFunctionsBuilder.Append(exampleFunctionString); } return exampleFunctionPrivate; }
                set { exampleFunctionPrivate = value; }
            }
        }
    }
}
