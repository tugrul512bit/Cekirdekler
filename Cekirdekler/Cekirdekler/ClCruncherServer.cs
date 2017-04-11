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
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ClCluster
{
    /// <summary>
    /// prealpha cluster feature
    /// </summary>
    public class ClCruncherServer
    {
        int PORT_NO;
        string SERVER_IP;
        int MAX_CLIENT_N;
        Dictionary<string, ClCruncherServerThread> clients;
        bool isWorking;
        Thread listenerThread;
        object lockObj;
        public ClCruncherServer(int port_no = 15000, string server_ip = "192.168.1.4", int maxClientN = 4)
        {
            isWorking = true;
            MAX_CLIENT_N = maxClientN;
            PORT_NO = port_no;
            SERVER_IP = new StringBuilder(server_ip).ToString();
            lockObj = new object();
            clients = new Dictionary<string, ClCruncherServerThread>();
        }

        public void stop()
        {
            lock (lockObj)
            {
                isWorking = false;
            }
        }

        private void openConnection(TcpListener listener)
        {
            TcpClient client = listener.AcceptTcpClient();
            client.ReceiveBufferSize = NetworkBuffer.receiveSendBufferSize;
            NetworkStream nwStream = client.GetStream();
            var sc = nwStream.GetType().GetProperty("Socket", BindingFlags.Instance | BindingFlags.NonPublic);
            var socketIp = ((Socket)sc.GetValue(nwStream, null)).RemoteEndPoint.ToString();
            Console.WriteLine("@@@" + socketIp);
            if (clients.ContainsKey(socketIp))
            {

            }
            else
            {
                ClCruncherServerThread cst = new ClCruncherServerThread(listener, client, socketIp, this);
                clients.Add(socketIp, cst);
            }




        }

        public void start()
        {
            listenerThread = new Thread(listen);
            listenerThread.Start();
            listenerThread.IsBackground = true;
        }

        public void wait()
        {
            lock (lockObj)
            {
                Monitor.PulseAll(lockObj);
                Monitor.Wait(lockObj);
            }
        }

        public void continueFromLastWait()
        {
            lock (lockObj)
            {
                Monitor.PulseAll(lockObj);
            }
        }

        private void listen()
        {
            int tmp_max = MAX_CLIENT_N;
            int clientCt = clients.Count;
            bool tmpWorking = true;
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(localAdd, PORT_NO);
            while (tmpWorking)
            {

                Console.WriteLine("tcp server is listening");
                listener.Server.ReceiveTimeout = 10000;
                listener.Server.SendTimeout = 10000;
                listener.Start();
                while (!listener.Pending())
                {
                    Thread.Sleep(1);
                }
                openConnection(listener);


                lock (lockObj)
                {
                    tmp_max = MAX_CLIENT_N;
                    clientCt = clients.Count;
                    tmpWorking = isWorking;
                    Console.WriteLine("a=" + clientCt + " b=" + tmp_max);
                }
            }
            listener.Stop();
        }



        public void dispose()
        {
            lock (lockObj)
            {
                isWorking = false;
                var clientsList = clients.ToList();
                foreach (var item in clientsList)
                {
                    item.Value.dispose();
                }
            }
        }
    }
}
