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
    public class CekirdekServer
    {
        int PORT_NO;
        string SERVER_IP;
        int MAX_CLIENT_N;
        Dictionary<string, CekirdekServerThread> clientler;
        bool calisiyor;
        Thread listenerThread;
        object kilit;
        public CekirdekServer(int port_no = 15000, string server_ip = "192.168.1.4", int maxClientN = 4)
        {
            calisiyor = true;
            MAX_CLIENT_N = maxClientN;
            PORT_NO = port_no;
            SERVER_IP = new StringBuilder(server_ip).ToString();
            kilit = new object();
            clientler = new Dictionary<string, CekirdekServerThread>();
        }

        public void dur()
        {
            lock (kilit)
            {
                calisiyor = false;
            }
        }

        private void baglantiAc(TcpListener listener)
        {
            //---incoming client connected---
            TcpClient client = listener.AcceptTcpClient();
            client.ReceiveBufferSize = NetworkBuffer.receiveSendBufferSize;
            //---get the incoming data through a network stream---
            NetworkStream nwStream = client.GetStream();
            var sc = nwStream.GetType().GetProperty("Socket", BindingFlags.Instance | BindingFlags.NonPublic);
            var socketIp = ((Socket)sc.GetValue(nwStream, null)).RemoteEndPoint.ToString();
            Console.WriteLine("@@@" + socketIp);
            if (clientler.ContainsKey(socketIp))
            {

            }
            else
            {
                CekirdekServerThread cst = new CekirdekServerThread(listener, client, socketIp, this);
                clientler.Add(socketIp, cst);
            }




        }

        public void basla()
        {
            listenerThread = new Thread(listen);
            listenerThread.Start();
            listenerThread.IsBackground = true;
        }

        public void bekle()
        {
            lock (kilit)
            {
                Monitor.PulseAll(kilit);
                Monitor.Wait(kilit);
            }
        }

        public void devamEt()
        {
            lock (kilit)
            {
                Monitor.PulseAll(kilit);
            }
        }

        private void listen()
        {
            int tmp_max = MAX_CLIENT_N;
            int clientCt = clientler.Count;
            bool tmpCalisiyor = true;
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(localAdd, PORT_NO);
            while (tmpCalisiyor)
            {

                Console.WriteLine("tcp server dinlemede");
                listener.Server.ReceiveTimeout = 10000;
                listener.Server.SendTimeout = 10000;
                listener.Start();
                while (!listener.Pending())
                {
                    Thread.Sleep(1);
                }
                baglantiAc(listener);


                lock (kilit)
                {
                    tmp_max = MAX_CLIENT_N;
                    clientCt = clientler.Count;
                    tmpCalisiyor = calisiyor;
                    Console.WriteLine("a=" + clientCt + " b=" + tmp_max);
                }
            }
            listener.Stop();
        }



        public void sil()
        {
            lock (kilit)
            {
                calisiyor = false;
                var liste = clientler.ToList();
                foreach (var item in liste)
                {
                    item.Value.sil();
                }
            }
        }
    }
}
