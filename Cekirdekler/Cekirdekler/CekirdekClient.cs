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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Cekirdekler;
namespace ClCluster
{
    /// <summary>
    /// prealpha cluster feature
    /// </summary>
    public class ClCruncherClient
    {
        TcpClient client;
        int PORT_NO;
        string SERVER_IP;
        Thread t;
        NetworkStream nwStream;
        public bool exception = false;
        public ClCruncherClient(int port_no, string server_ip)
        {
            PORT_NO = port_no;
            SERVER_IP = (new StringBuilder(server_ip)).ToString();
            try
            {
                Console.WriteLine("SERVER IP:"+SERVER_IP);
                Console.WriteLine("PORT NO:" + PORT_NO);
                client = new TcpClient(SERVER_IP, PORT_NO);
                client.ReceiveBufferSize = NetworkBuffer.receiveSendBufferSize;
                client.SendBufferSize = NetworkBuffer.receiveSendBufferSize;
                nwStream = client.GetStream();
            }
            catch (Exception ee)
            {
                exception = true;
                Console.WriteLine("Client exception:");
                Console.WriteLine(ee.StackTrace);
            }
            finally
            {
                if (exception)
                {
                    if(nwStream!=null)
                        nwStream.Dispose();
                }
            }
        }



        private void upload(NetworkStream nwStream, byte[] toBeSent)
        {

            byte endianness = BitConverter.IsLittleEndian ? (byte)0 : (byte)1;


            int pos = 0;
            while (pos < toBeSent.Length)
            {
                int l = Math.Min(toBeSent.Length - pos, NetworkBuffer.receiveSendBufferSize);
                nwStream.Write(toBeSent, pos, l);
                pos += l;
            }
        }


        private NetworkBuffer download(NetworkStream nwStream, object[] arraysParameter = null)
        {
            /* cevap alınıyor */
            int bytesRead = NetworkBuffer.receiveSendBufferSize;
            int length = -1;
            int bytesTotal = 0;
            byte[] buffer = new byte[NetworkBuffer.receiveSendBufferSize];
            byte[] serverBuffer = new byte[NetworkBuffer.receiveSendBufferSize];
            int counter = 0;
            NetworkBuffer nbAnswer = new NetworkBuffer();
            while (!nwStream.DataAvailable) { Thread.Sleep(1); }
            while (nwStream.CanRead && bytesRead > 0 && (length == -1 || bytesTotal < length))
            {

                bytesRead = nwStream.Read(buffer, 0, NetworkBuffer.receiveSendBufferSize);


                if (length == -1)
                    length = nbAnswer.readLengthOfBuffer(buffer);
                if (length == 0)
                {
                    Console.WriteLine("client: couldn't receive buffer.");
                    return nbAnswer;
                }
                else
                {
                    if (serverBuffer.Length < length)
                        serverBuffer = new byte[length];
                    Buffer.BlockCopy(buffer, 0, serverBuffer, bytesTotal, bytesRead);
                    bytesTotal += bytesRead;
                }
            }
            List<NetworkBuffer.HashIndisSiraDizi> resultingArrays = nbAnswer.oku(serverBuffer, "client", arraysParameter);

            return nbAnswer;
        }

        public void netSetup(string deviceTypesParameter = "",
             string kernelsString = "",
             string[] kernelNamesStringArray = null,
             int localWorkgroupSizeInWorkitems = 256,
             int numberOfGpusToUse = -1,
             bool GPU_STREAM_ = false,
             int MAX_CPU_ = -1)
        {
            NetworkBuffer nb = new NetworkBuffer(NetworkBuffer.SETUP);
            if (deviceTypesParameter.Equals(""))
                deviceTypesParameter = "cpu gpu";

            if (kernelsString.Equals(""))
                kernelsString = @"__kernel void serverDeneme(__global float *a){a[get_global_id(0)]+=3.14f;}";

            if (kernelNamesStringArray == null)
                kernelNamesStringArray = new string[] { "serverDeneme" };

            int[] localThreadNumber = new int[] { localWorkgroupSizeInWorkitems };
            int[] gpusToUse = new int[] { numberOfGpusToUse };
            bool[] GPU_STREAM = new bool[] { GPU_STREAM_ };
            int[] MAX_CPU = new int[] { MAX_CPU_ };
            string ki = String.Join(" ", kernelNamesStringArray);
            nb.addCompute(deviceTypesParameter.ToCharArray(), deviceTypesParameter.GetHashCode());
            nb.addCompute(kernelsString.ToCharArray(), kernelsString.GetHashCode());
            nb.addCompute(ki.ToCharArray(), ki.GetHashCode());
            nb.addComputeSteps(localThreadNumber, localThreadNumber.GetHashCode());
            nb.addComputeSteps(gpusToUse, gpusToUse.GetHashCode());
            nb.addPipeline(GPU_STREAM, GPU_STREAM.GetHashCode());
            nb.addComputeSteps(MAX_CPU, MAX_CPU.GetHashCode());

            upload(nwStream, nb.buf());
            Console.WriteLine(download(nwStream));
        }

        public void compute(string[] kernelNameStringArray = null,
            int numberOfSteps = 0, string stepFunction = "",
            object[] arrays = null, string[] readWrite = null,
            int[] arrayElementsPerWorkItem = null,
            int globalRange = 1024, int computeId = 1,
            int globalOffset = 0, bool enablePipeline = false,
            int numberOfPipelineBlobs = 4, bool typeOfPipeline = Cores.PIPELINE_EVENT)
        {
            NetworkBuffer nbCompute = new NetworkBuffer(NetworkBuffer.COMPUTE);
            if (kernelNameStringArray == null)
                kernelNameStringArray = new string[] { "serverDeneme" };
            string kernelName = String.Join(" ", kernelNameStringArray);

            int[] numberOfStepsArray = new int[] { numberOfSteps };

            if (arrays == null)
                arrays = new object[] { new float[1024] };

            int[] numberOfArrays = new int[] { arrays.Length };

            if (readWrite == null)
                readWrite = new string[] { "read write" }; // unoptimized for now

            if (arrayElementsPerWorkItem == null)
                arrayElementsPerWorkItem = new int[] { 1 };

            int[] totalGlobalRange = new int[] { globalRange };

            int[] computeIdArray = new int[] { computeId };


            int[] globalOffsetArray = new int[] { globalOffset };

            bool[] pipelineEnabledArray = new bool[] { enablePipeline };

            int[] pipelineBlobsArray = new int[] { numberOfPipelineBlobs/* default 4 but unimportant when pipelining is disabled*/};

            bool[] pipelineTypeArray = new bool[] { typeOfPipeline };

            nbCompute.addCompute(kernelName.ToCharArray(), kernelName.GetHashCode());
            nbCompute.addComputeSteps(numberOfStepsArray, numberOfStepsArray.GetHashCode());
            nbCompute.addCompute(stepFunction.ToCharArray(), stepFunction.GetHashCode());
            nbCompute.addComputeSteps(numberOfArrays, numberOfArrays.GetHashCode());

            for (int m = 0; m < arrays.Length; m++)
            {
                if (readWrite[m].Contains("partial"))
                {
                    // not all array types are covered here, todo: add all types
                    if (arrays[m].GetType() == typeof(float[]))
                        nbCompute.addArray((float[])arrays[m], arrays[m].GetHashCode(),
                            globalOffset, globalRange, arrayElementsPerWorkItem[m]);
                    else if (arrays[m].GetType() == typeof(int[]))
                        nbCompute.addComputeSteps((int[])arrays[m], arrays[m].GetHashCode(),
                            globalOffset, globalRange, arrayElementsPerWorkItem[m]);
                    else if (arrays[m].GetType() == typeof(byte[]))
                        nbCompute.addArray((byte[])arrays[m], arrays[m].GetHashCode(),
                            globalOffset, globalRange, arrayElementsPerWorkItem[m]);
                    else if (arrays[m].GetType() == typeof(char[]))
                        nbCompute.addCompute((char[])arrays[m], arrays[m].GetHashCode(),
                            globalOffset, globalRange, arrayElementsPerWorkItem[m]);
                    else if (arrays[m].GetType() == typeof(double[]))
                        nbCompute.addArray((double[])arrays[m], arrays[m].GetHashCode(),
                            globalOffset, globalRange, arrayElementsPerWorkItem[m]);
                    else if (arrays[m].GetType() == typeof(long[]))
                        nbCompute.addArray((long[])arrays[m], arrays[m].GetHashCode(),
                            globalOffset, globalRange, arrayElementsPerWorkItem[m]);
                }
                else if (readWrite[m].Contains("read"))
                {
                    if (arrays[m].GetType() == typeof(float[]))
                        nbCompute.addArray((float[])arrays[m], arrays[m].GetHashCode());
                    else if (arrays[m].GetType() == typeof(int[]))
                        nbCompute.addComputeSteps((int[])arrays[m], arrays[m].GetHashCode());
                    else if (arrays[m].GetType() == typeof(byte[]))
                        nbCompute.addArray((byte[])arrays[m], arrays[m].GetHashCode());
                    else if (arrays[m].GetType() == typeof(char[]))
                        nbCompute.addCompute((char[])arrays[m], arrays[m].GetHashCode());
                    else if (arrays[m].GetType() == typeof(double[]))
                        nbCompute.addArray((double[])arrays[m], arrays[m].GetHashCode());
                    else if (arrays[m].GetType() == typeof(long[]))
                        nbCompute.addArray((long[])arrays[m], arrays[m].GetHashCode());
                }
            }


            for (int m = 0; m < arrays.Length; m++)
                nbCompute.addCompute(readWrite[m].ToCharArray(), readWrite[m].GetHashCode());
            nbCompute.addComputeSteps(arrayElementsPerWorkItem, arrayElementsPerWorkItem.GetHashCode());
            nbCompute.addComputeSteps(totalGlobalRange, totalGlobalRange.GetHashCode());
            nbCompute.addComputeSteps(computeIdArray, computeIdArray.GetHashCode());
            nbCompute.addComputeSteps(globalOffsetArray, globalOffsetArray.GetHashCode());
            nbCompute.addPipeline(pipelineEnabledArray, pipelineEnabledArray.GetHashCode());
            nbCompute.addComputeSteps(pipelineBlobsArray, pipelineBlobsArray.GetHashCode());
            nbCompute.addPipeline(pipelineTypeArray, pipelineTypeArray.GetHashCode());

            upload(nwStream, nbCompute.buf());
            NetworkBuffer nbCevap = new NetworkBuffer();

            download(nwStream, arrays);
        }



        public void dispose()
        {
            NetworkBuffer nb2 = new NetworkBuffer(NetworkBuffer.DISPOSE);
            upload(nwStream, nb2.buf());
            Console.WriteLine(download(nwStream));

            nwStream.Close(1000);
            nwStream.Dispose();
            client.Close();

            // .Net 4.6
            //client.Dispose();
        }


        public bool control()
        {
            NetworkBuffer nb2 = new NetworkBuffer(NetworkBuffer.SERVER_CONTROL);
            try
            {
                if (!exception)
                {
                    upload(nwStream, nb2.buf());
                    return download(nwStream).bufferCommand() == NetworkBuffer.ANSWER_CONTROL;
                }
                else
                    return false;
            }
            catch(Exception e)
            {
                exception = true;
                Console.WriteLine("upload download exception:");
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                if (exception)
                {
                    if(this.client!=null)
                        this.client.Close();
                }
            }
            return false;
        }

        public int numDevices()
        {
            NetworkBuffer nb2 = new NetworkBuffer(NetworkBuffer.SERVER_NUMBER_OF_DEVICES);
            upload(nwStream, nb2.buf());
            int[] numberOfDevices = new int[1];
            NetworkBuffer nb3= download(nwStream, new object[] { numberOfDevices });
            if (nb3.bufferCommand() == NetworkBuffer.ANSWER_NUMBER_OF_DEVICES)
                return numberOfDevices[0];
            else
                return -1;
        }

        public void stop()
        {
            NetworkBuffer nb2 = new NetworkBuffer(NetworkBuffer.SERVER_STOP);
            upload(nwStream, nb2.buf());
            Console.WriteLine(download(nwStream));

        }
    }
}
