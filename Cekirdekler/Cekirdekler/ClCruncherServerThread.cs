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
    public class ClCruncherServerThread
    {
        private Thread dedicatedThread;
        private bool isWorking;
        private object lockObj;
        private Cores openclSystemToCrunchNumbers;
        private NetworkBuffer nbComputeAnswer;
        private TcpListener tcpListener;
        private TcpClient client;
        private byte[] serverBuffer;
        private string socketIp;
        private ClCruncherServer server;
        public ClCruncherServerThread(TcpListener listener, TcpClient client_, string socketIp_, ClCruncherServer cs)
        {
            server = cs;
            socketIp = (new StringBuilder(socketIp_)).ToString();
            tcpListener = listener;
            client = client_;
            lockObj = new object();
            isWorking = true;
            serverBuffer = new byte[1024];

            dedicatedThread = new Thread(() => f(listener, client));
            dedicatedThread.IsBackground = true;
            dedicatedThread.Start();
            Console.WriteLine("server thread started");
        }

        public void dispose()
        {
            lock (lockObj)
            {
                isWorking = false;
                client.Close();
            }
            dedicatedThread.Join();
            Console.WriteLine("server thread stopped");
        }

        Dictionary<int, object> d0 = new Dictionary<int, object>();
        Dictionary<object, int> d1 = new Dictionary<object, int>();
        NetworkBuffer nb = new NetworkBuffer();

        private void f(TcpListener listener, TcpClient client_)
        {
            bool tmpWorking = true;

            while (tmpWorking)
            {
                client.ReceiveBufferSize = NetworkBuffer.receiveSendBufferSize;
                client.SendBufferSize = NetworkBuffer.receiveSendBufferSize;
                byte[] buffer = new byte[NetworkBuffer.receiveSendBufferSize];

                //---read incoming stream---
                int bytesRead = NetworkBuffer.receiveSendBufferSize;
                int counter = 0;
                int length = -1;
                int bytesTotal = 0;
                nb.resetExceptArrays();
                NetworkStream nwStream = client.GetStream();
                while (!nwStream.DataAvailable) { Thread.Sleep(1); }
                while (nwStream.CanRead && bytesRead > 0 && (length == -1 || bytesTotal < length))
                {

                    bytesRead = nwStream.Read(buffer, 0, NetworkBuffer.receiveSendBufferSize);


                    if (length == -1)
                        length = nb.readLengthOfBuffer(buffer);
                    if (length == 0)
                    {
                        Console.WriteLine("Server: couldn't receive buffer.");
                        return;
                    }
                    else
                    {
                        if (serverBuffer.Length < length)
                            serverBuffer = new byte[length];
                        Buffer.BlockCopy(buffer, 0, serverBuffer, bytesTotal, bytesRead);
                        bytesTotal += bytesRead;
                    }
                }
                
                List<NetworkBuffer.HashIndisSiraDizi> arrays = nb.oku(serverBuffer, "server");
                if (nb.bufferCommand() == NetworkBuffer.SETUP)
                {

                    string deviceTypes = new string((char[])arrays[0].backingArray);
                    string kernelsString = new string((char[])arrays[1].backingArray);
                    string[] kernelNamesStringArray = (new string((char[])arrays[2].backingArray)).Split(" ".ToCharArray());
                    int localRAnge = ((int[])arrays[3].backingArray)[0];
                    int numberOfGPUsToUse = ((int[])arrays[4].backingArray)[0];
                    bool GPU_STREAM = ((bool[])arrays[5].backingArray)[0];
                    int MAX_CPU = ((int[])arrays[6].backingArray)[0];
                    openclSystemToCrunchNumbers = new Cores(
                            deviceTypes, kernelsString,
                            kernelNamesStringArray,false, localRAnge,
                            numberOfGPUsToUse, GPU_STREAM,
                            MAX_CPU);
                    if(openclSystemToCrunchNumbers.errorCode()!=0)
                    {
                        Console.WriteLine("Compiling error!");
                        Console.WriteLine(openclSystemToCrunchNumbers.errorMessage());
                        openclSystemToCrunchNumbers.dispose();
                        return;
                    }

                    NetworkBuffer nbAnswer = new NetworkBuffer(NetworkBuffer.ANSWER_SUCCESS);
                    byte[] bytesToSend = nbAnswer.buf();
                    int pos = 0;
                    while (pos < bytesToSend.Length)
                    {
                        int l = Math.Min(bytesToSend.Length - pos, client.ReceiveBufferSize);
                        nwStream.Write(bytesToSend, pos, l);
                        pos += l;
                    }
                    Console.WriteLine("--------------------------------------------------");
                }
                else if (nb.bufferCommand() == NetworkBuffer.COMPUTE)
                {
                    string[] kernelNamesString_ = new string((char[])arrays[0].backingArray).Split(" ".ToCharArray());
                    int numberOfSteps = ((int[])arrays[1].backingArray)[0];
                    string stepFunction = new string((char[])arrays[2].backingArray);

                    int numberOfArrays = ((int[])arrays[3].backingArray)[0];



                    string[] readWrite = new string[numberOfArrays];
                    for (int i = 0; i < numberOfArrays; i++)
                        readWrite[i] = new string((char[])arrays[numberOfArrays + 4 + i].backingArray);


                    int[] arrayElementsPerWorkItem = new int[numberOfArrays];
                    for (int i = 0; i < numberOfArrays; i++)
                        arrayElementsPerWorkItem[i] = ((int[])arrays[numberOfArrays * 2 + 4].backingArray)[i];

                    int totalGlobalRange = ((int[])arrays[numberOfArrays * 2 + 5].backingArray)[0];
                    int computeId = ((int[])arrays[numberOfArrays * 2 + 6].backingArray)[0];
                    int globalRangeOffset = ((int[])arrays[numberOfArrays * 2 + 7].backingArray)[0];
                    bool pipelineEnabled = ((bool[])arrays[numberOfArrays * 2 + 8].backingArray)[0];
                    int pipelineNumberOfBlobs = ((int[])arrays[numberOfArrays * 2 + 9].backingArray)[0];
                    bool pipelineType = ((bool[])arrays[numberOfArrays * 2 + 10].backingArray)[0];
                    object[] tmpArrays = new object[numberOfArrays];
                    int[] tmpHashValues = new int[numberOfArrays];
                    for (int o = 0; o < numberOfArrays; o++)
                    {
                        tmpArrays[o] = arrays[4 + o].backingArray;
                        tmpHashValues[o] = arrays[4 + o].hash_;
                        if(!d0.ContainsKey(tmpHashValues[o]))
                        {
                            d0.Add(tmpHashValues[o], tmpArrays[o]);
                            d1.Add(tmpArrays[o], tmpHashValues[o]);
                        }
                       
                    }

                    openclSystemToCrunchNumbers.compute(kernelNamesString_, numberOfSteps, stepFunction, tmpArrays, readWrite,
                        arrayElementsPerWorkItem, totalGlobalRange, computeId,
                        globalRangeOffset, pipelineEnabled, pipelineNumberOfBlobs, pipelineType);
                    openclSystemToCrunchNumbers.performanceReport(computeId);


                    // todo: "true ||" must be deleted but then array update is not working on same array because hash values don't match
                    //            /
                    //           /
                    //          /
                    //         /
                    //        |
                    //        v
                    if (true || nbComputeAnswer == null)
                    {
                        nbComputeAnswer = new NetworkBuffer(NetworkBuffer.ANSWER_COMPUTE_COMPLETE);
                        for (int m = 0; m < numberOfArrays; m++)
                        {
                            if (tmpArrays[m].GetType() == typeof(float[]))
                                nbComputeAnswer.addArray((float[])tmpArrays[m], tmpHashValues[m],
                                    globalRangeOffset,totalGlobalRange,arrayElementsPerWorkItem[m]);
                            else if (tmpArrays[m].GetType() == typeof(int[]))
                                nbComputeAnswer.addComputeSteps((int[])tmpArrays[m], tmpHashValues[m]);
                            else if (tmpArrays[m].GetType() == typeof(byte[]))
                                nbComputeAnswer.addArray((byte[])tmpArrays[m], tmpHashValues[m]);
                            else if (tmpArrays[m].GetType() == typeof(char[]))
                                nbComputeAnswer.addCompute((char[])tmpArrays[m], tmpHashValues[m]);
                            else if (tmpArrays[m].GetType() == typeof(double[]))
                                nbComputeAnswer.addArray((double[])tmpArrays[m], tmpHashValues[m]);
                            else if (tmpArrays[m].GetType() == typeof(long[]))
                                nbComputeAnswer.addArray((long[])tmpArrays[m], tmpHashValues[m]);
                        }
                    }
                    else
                    {

                        for (int m = 0; m < numberOfArrays; m++)
                        {
                            if (tmpArrays[m].GetType() == typeof(float[]))
                                nbComputeAnswer.update((float[])tmpArrays[m], tmpHashValues[m],
                                    globalRangeOffset, totalGlobalRange, arrayElementsPerWorkItem[m]);
                            else if (tmpArrays[m].GetType() == typeof(int[]))
                                nbComputeAnswer.update((int[])tmpArrays[m], tmpHashValues[m]);
                            else if (tmpArrays[m].GetType() == typeof(byte[]))
                                nbComputeAnswer.update((byte[])tmpArrays[m], tmpHashValues[m]);
                            else if (tmpArrays[m].GetType() == typeof(char[]))
                                nbComputeAnswer.update((char[])tmpArrays[m], tmpHashValues[m]);
                            else if (tmpArrays[m].GetType() == typeof(double[]))
                                nbComputeAnswer.update((double[])tmpArrays[m], tmpHashValues[m]);
                            else if (tmpArrays[m].GetType() == typeof(long[]))
                                nbComputeAnswer.update((long[])tmpArrays[m], tmpHashValues[m]);
                        }
                    }

                    byte[] bytesToSend = nbComputeAnswer.buf();
                    int pos = 0;
                    while (pos < bytesToSend.Length)
                    {
                        int l = Math.Min(bytesToSend.Length - pos, NetworkBuffer.receiveSendBufferSize);
                        nwStream.Write(bytesToSend, pos, l);
                        pos += l;
                    }
                }
                else if (nb.bufferCommand() == NetworkBuffer.DISPOSE)
                {
                    Console.WriteLine("Server-side Cekirdekler API is being deleted");
                    openclSystemToCrunchNumbers.dispose();

                    NetworkBuffer nbAnswer = new NetworkBuffer(NetworkBuffer.ANSWER_DELETED);
                    byte[] bytesToSend = nbAnswer.buf();
                    int pos = 0;
                    while (pos < bytesToSend.Length)
                    {
                        Console.WriteLine("server: writing segment");
                        int l = Math.Min(bytesToSend.Length - pos, NetworkBuffer.receiveSendBufferSize);
                        nwStream.Write(bytesToSend, pos, l);
                        pos += l;
                        Console.WriteLine("server: segment written: " + pos + "  " + bytesToSend.Length);
                    }
                    Console.WriteLine("--------------------------------------------------");
                    tmpWorking = false;
                    dispose();
                }
                else if (nb.bufferCommand() == NetworkBuffer.SERVER_STOP)
                {
                    Console.WriteLine("stopping server");
                    NetworkBuffer nbAnswer = new NetworkBuffer(NetworkBuffer.ANSWER_STOPPED);
                    byte[] bytesToSend = nbAnswer.buf();
                    int pos = 0;
                    while (pos < bytesToSend.Length)
                    {
                        Console.WriteLine("server: writing segment");
                        int l = Math.Min(bytesToSend.Length - pos, NetworkBuffer.receiveSendBufferSize);
                        nwStream.Write(bytesToSend, pos, l);
                        pos += l;
                        Console.WriteLine("server: segment written: " + pos + "  " + bytesToSend.Length);
                    }
                    Console.WriteLine("--------------------------------------------------");
                    tmpWorking = false;
                    server.stop();
                }
                else if (nb.bufferCommand() == NetworkBuffer.SERVER_CONTROL )
                {
                    Console.WriteLine("controlling server");
                    NetworkBuffer nbAnswer = new NetworkBuffer(NetworkBuffer.ANSWER_CONTROL );
                    byte[] bytesToSend = nbAnswer.buf();
                    int pos = 0;
                    while (pos < bytesToSend.Length)
                    {
                        Console.WriteLine("server: writing segment");
                        int l = Math.Min(bytesToSend.Length - pos, NetworkBuffer.receiveSendBufferSize);
                        nwStream.Write(bytesToSend, pos, l);
                        pos += l;
                        Console.WriteLine("server: segment written: " + pos + "  " + bytesToSend.Length);
                    }
                    Console.WriteLine("--------------------------------------------------");
                }
                else if (nb.bufferCommand() == NetworkBuffer.SERVER_NUMBER_OF_DEVICES)
                {
                    Console.WriteLine("receiving number of devices in server");
                    NetworkBuffer nbAnswer = new NetworkBuffer(NetworkBuffer.ANSWER_NUMBER_OF_DEVICES);
                    int[] numDevices_ = new int[1];
                    numDevices_[0] = openclSystemToCrunchNumbers.numberOfDevices();
                    nbAnswer.addComputeSteps(numDevices_,numDevices_.GetHashCode());
                    byte[] bytesToSend = nbAnswer.buf();
                    int pos = 0;
                    while (pos < bytesToSend.Length)
                    {
                        Console.WriteLine("server: writing segment");
                        int l = Math.Min(bytesToSend.Length - pos, NetworkBuffer.receiveSendBufferSize);
                        nwStream.Write(bytesToSend, pos, l);
                        pos += l;
                        Console.WriteLine("server: segment written: " + pos + "  " + bytesToSend.Length);
                    }
                    Console.WriteLine("--------------------------------------------------");
                }
            }

        }

        private void apiSetup()
        {

        }

        private void apiCompute()
        {

        }

        private void apiDispose()
        {

        }
    }
}
