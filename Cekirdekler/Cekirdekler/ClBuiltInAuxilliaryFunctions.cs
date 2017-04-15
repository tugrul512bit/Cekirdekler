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
