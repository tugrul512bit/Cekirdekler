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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Cekirdekler;
namespace ClCluster
{
    /// <summary>
    /// <para>prealpha cluster add-on</para>
    /// <para>"cluster:xxxxx" then this class is used</para> 
    /// <para>todo: make this tree-like structure to enable grid computing or multi level cluster</para>
    /// </summary>
    public class ClusterAccelerator:IComputeNode
    {
        // controls whole sub-layer
        // computes remaining nuisance workload after load-balancing
        Cores mainframeComputer;

        List<ClCruncherClient> clients;

        public class ServerInfoSimple
        {
            public int port { get; set; }
            public string ipString { get; set; }
            public string nameOfServer { get; set; }
            public double roundTripPerformance { get; set; }
        }

        private string localIp()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("10.0.2.4", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }

        private bool serverTesting(string ip,int port)
        {
            ClCruncherClient client = new ClCruncherClient(port, ip);
            if (client == null || client.exception)
                return false;
            else
            {
                bool ret = client.control();
                client.stop();
                return ret;
            }
        }

        /// <summary>
        /// checks client with given port parameters (example:  50000 ports of all servers)
        /// </summary>
        private List<ServerInfoSimple> findServer(int port_,bool fasterSearch)
        {
            string ip_lan = "192.168.1.";
            object lockObjectTmp = new object();
            int numIps = 0;
            List<ServerInfoSimple> serverList = new List<ServerInfoSimple>();
            int upperLimit = 255;
            if (fasterSearch)
                upperLimit = 10;
            Parallel.For(1, upperLimit, i =>
            {
                string ip = ip_lan + i;
                Console.WriteLine(ip + ":");
                System.Net.NetworkInformation.Ping p = null;
                try
                {
                    p = new System.Net.NetworkInformation.Ping();
                    System.Net.NetworkInformation.PingReply rep = p.Send(ip);
                   
                    if (rep.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        var server = new ServerInfoSimple() { port = port_, roundTripPerformance = 1.0 / (0.1 + (double)rep.RoundtripTime), ipString = (new StringBuilder(ip)).ToString() };


                        Console.WriteLine(rep.RoundtripTime + "ms");
                      
                        try
                        {
                            // not works always
                            //IPHostEntry ipHostEntry = Dns.GetHostEntry(ip);
                            //Console.WriteLine(ipHostEntry.HostName);
                            //server.nameOfServer = ipHostEntry.HostName;
                        }
                        catch (Exception ee)
                        {
                            // server olmayabilir
                            server.nameOfServer = "unknown server name";
                        }


                        
                            if (serverTesting(ip, port_))
                            {
                                // todo
                                // need to test connection speed before tree-cluster compute
                                // test closes N nodes
                                lock (lockObjectTmp)
                                {
                                    numIps++;
                                    serverList.Add(server);
                                }
                            }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("ping error");
                    Console.WriteLine(e.StackTrace);
                    
                }
                finally
                {
                    p.Dispose();
                    Console.WriteLine("______________________");

                }
            });
            Console.WriteLine(numIps + " cluster PCs connected.");
            foreach (var item in serverList)
            {
                Console.WriteLine("---------------");
                Console.WriteLine(item.ipString);
                Console.WriteLine(item.roundTripPerformance);
                Console.WriteLine(item.nameOfServer);
                Console.WriteLine("---------------");
            }
            return serverList;
        }

        // compute id to global range conversion
        private bool pipelineEnabled;
        private int pipelineBlobCount;
        private int localRangeValue;
        private int[] stepsNum;
        private int[] offsetValues;
        private Stopwatch[] timers;
        private Dictionary<int, int> mainframeThreads;
        private Dictionary<int, double> mainframeTimings;
        private Dictionary<int, int[]> latestGlobalRanges;
        private Dictionary<int, double[]> latestTimings;
        private Dictionary<int, ClusterLoadBalancer> loadBalancers;
        Stopwatch swCompute;
        Stopwatch swMainframe;
        public void compute(string[] kernelNameStringArray = null,
            int numberOfSteps = 0, string stepFunction = "",
            object[] arrays_ = null, string[] readWrite_ = null,
            int[] arrayElementsPerWorkItem = null,
            int totalGlobalRange_ = 1024, int computeId_ = 1,
            int offsetValueForGlobalRange = 0, bool pipelineEnabled_ = false,
            int pipelineBlobCount = 4, bool pipelineType = Cores.PIPELINE_EVENT)
        {
            pipelineEnabled = pipelineEnabled_;
            this.pipelineBlobCount = pipelineBlobCount;
            if (stepsNum == null || stepsNum.Length != clients.Count)
            {
                stepsNum = new int[clients.Count];
                for (int i = 0; i < clients.Count; i++)
                {
                    stepsNum[i] = clients[i].numDevices() * localRangeValue;
                    if (pipelineEnabled_)
                        stepsNum[i] *= pipelineBlobCount;
                }
            }
            if (swCompute == null)
                swCompute = new Stopwatch();
            if (offsetValues == null || offsetValues.Length != clients.Count)
            {
                offsetValues = new int[clients.Count];
            }
            if (swMainframe == null)
                swMainframe = new Stopwatch();
            if (timers == null)
            {
                timers = new Stopwatch[clients.Count];
                for (int i = 0; i < clients.Count; i++)
                    timers[i] = new Stopwatch();
            }
            if (latestGlobalRanges == null)
                latestGlobalRanges = new Dictionary<int, int[]>();
            if (latestTimings == null)
            {
                latestTimings = new Dictionary<int, double[]>();
                if (latestTimings.ContainsKey(computeId_))
                {
                    for (int i = 0; i < clients.Count; i++)
                    {

                        latestTimings[computeId_][i] = 0.01;
                    }
                }
                else
                {
                    latestTimings.Add(computeId_, new double[clients.Count]);

                    for (int i = 0; i < clients.Count; i++)
                    {
                        latestTimings[computeId_][i] = 0.01;
                    }
                }
            }
        
            if (loadBalancers == null)
                loadBalancers = new Dictionary<int, ClusterLoadBalancer>();
            if (mainframeThreads == null)
                mainframeThreads = new Dictionary<int, int>();
            if (mainframeTimings == null)
                mainframeTimings = new Dictionary<int, double>();
            if (!loadBalancers.ContainsKey(computeId_))
                loadBalancers.Add(computeId_, new ClusterLoadBalancer());


            if (!latestGlobalRanges.ContainsKey(computeId_))
            {
                int[] tmpRanges = new int[clients.Count];
                if (mainframeThreads.ContainsKey(computeId_))
                {
                    mainframeThreads[computeId_] = loadBalancers[computeId_].dengeleEsit(totalGlobalRange_, tmpRanges, stepsNum);
                }
                else
                {
                    mainframeThreads.Add(computeId_, loadBalancers[computeId_].dengeleEsit(totalGlobalRange_, tmpRanges, stepsNum));
                }
                latestGlobalRanges.Add(computeId_, tmpRanges);
            }
            else
            {
                if (clients.Count != latestGlobalRanges[computeId_].Length)
                {
                    int[] tmpRanges = new int[clients.Count];

                    if (mainframeThreads.ContainsKey(computeId_))
                    {
                        mainframeThreads[computeId_] = loadBalancers[computeId_].dengeleEsit(totalGlobalRange_, tmpRanges, stepsNum);
                    }
                    else
                    {
                        mainframeThreads.Add(computeId_, loadBalancers[computeId_].dengeleEsit(totalGlobalRange_, tmpRanges, stepsNum));
                    }

                    latestGlobalRanges[computeId_] = tmpRanges;

                }
                else
                {
                    if (mainframeThreads.ContainsKey(computeId_))
                    {
                        mainframeThreads[computeId_] =
                        loadBalancers[computeId_].balanceOnPerformances(latestTimings[computeId_],
                            totalGlobalRange_, latestGlobalRanges[computeId_],
                            stepsNum, mainframeThreads[computeId_],
                            mainframeTimings[computeId_]);
                    }
                    else
                    {
                        mainframeThreads.Add(computeId_, loadBalancers[computeId_].balanceOnPerformances(latestTimings[computeId_],
                            totalGlobalRange_, latestGlobalRanges[computeId_],
                            stepsNum, mainframeThreads[computeId_],
                            mainframeTimings[computeId_]));
                    }
                }
            }

            int tmpRef = offsetValueForGlobalRange;
            for(int i=0;i<clients.Count;i++)
            {
                offsetValues[i] = tmpRef;
                tmpRef += latestGlobalRanges[computeId_][i];
            }


            swCompute.Reset();
            swCompute.Start();
            Parallel.For(0, clients.Count + 1, i => {
                if (i < clients.Count)
                {
                    timers[i].Reset();
                    timers[i].Start();
                        clients[i].compute(
                            kernelNameStringArray,
                            numberOfSteps, stepFunction,
                            arrays_, readWrite_,
                            arrayElementsPerWorkItem,
                            latestGlobalRanges[computeId_][i], computeId_,
                            offsetValues[i], pipelineEnabled_,
                            pipelineBlobCount, pipelineType);
                    timers[i].Stop();

                    if (latestTimings.ContainsKey(computeId_))
                    {
                            latestTimings[computeId_][i] = 0.01 + ((double)timers[i].ElapsedMilliseconds);

                    }
                    else
                    {
                        latestTimings.Add(computeId_, new double[clients.Count]);

                        latestTimings[computeId_][i] = 0.01 + ((double)timers[i].ElapsedMilliseconds);
                    }
                }
                else
                {
                    
                    swMainframe.Reset();
                    swMainframe.Start();
                    if (mainframeThreads[computeId_] > 0)
                    {
                        mainframeComputer.compute(kernelNameStringArray,
                            numberOfSteps, stepFunction,
                            arrays_, readWrite_,
                            arrayElementsPerWorkItem,
                            mainframeThreads[computeId_], computeId_,
                            tmpRef, pipelineEnabled_,
                            pipelineBlobCount, pipelineType);
                    }
                    swMainframe.Stop();
                    if (mainframeTimings.ContainsKey(computeId_))
                    {
                        mainframeTimings[computeId_] = 0.01 + (double)swMainframe.ElapsedMilliseconds;
                    }
                    else
                    {
                        mainframeTimings.Add(computeId_, 0.01 + (double)swMainframe.ElapsedMilliseconds);

                    }
                }
            });
            swCompute.Stop();
            Console.WriteLine("Hesap süresi="+swCompute.ElapsedMilliseconds+"ms");
        }

        public double computeTiming()
        {
            throw new NotImplementedException();
        }



        public void setupNodes(string deviceTypes, string kernelsString, 
                        string[] kernelNamesStringArray,int localRangeValue = 256, 
                        int numGPUsToUse = -1, bool GPU_STREAM = true, 
                        int MAX_CPU = -1)
        {
            this.localRangeValue = localRangeValue;
            deviceTypes = deviceTypes.ToLower();
            numGPUsToUse = 1;
            string mainframeDevices = "";

            
            if (deviceTypes.Contains("node0_g"))
                mainframeDevices = "gpu";
            else if (deviceTypes.Contains("node0_c"))
                mainframeDevices = "cpu";
            int mainframeNumberOfGPUs = 1;
            mainframeComputer = new Cores(mainframeDevices, kernelsString, kernelNamesStringArray,
                           localRangeValue, mainframeNumberOfGPUs, GPU_STREAM, MAX_CPU);
            List<int>ports=new List<int>();
            if(deviceTypes.Contains("cluster:"))
            {
                string[] strPorts=deviceTypes.Split(new string[] { "cluster:" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim().Split(new string[] { "port" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim().Split(new string[] { "," },StringSplitOptions.RemoveEmptyEntries);
                if (strPorts == null)
                {
                    Console.WriteLine("Cluster setup needs a port definition");
                    return;
                }
                else if(strPorts.Length>0)
                {
                    for(int i=0;i<strPorts.Length;i++)
                    {
                        if(strPorts[i]!=null && !strPorts[i].Trim().Equals("") && strPorts[i].Trim().Length>0)
                            ports.Add(int.Parse(strPorts[i].Trim()));
                    }
                }
            }
            else
            {
                Console.WriteLine("For cluster seetup, deice type parameter must be 'cluster:aaaaa' where aaaaa is port number.");
                return;
            }
            bool fastSearch = (deviceTypes.Contains("fast-search")) ? true : false;
            List<ServerInfoSimple> clusterServers = new List<ServerInfoSimple>();

            if (clients == null)
                clients = new List<ClCruncherClient>();
            for (int i = 0; i < ports.Count; i++)
            {
                Console.WriteLine("prt:"+ports[i]);
                List<ServerInfoSimple> tmpClusterServers = findServer(ports[i], fastSearch);
                if (tmpClusterServers != null && tmpClusterServers.Count > 0)
                    clusterServers.AddRange(tmpClusterServers);
            }
            
            foreach (var item in clusterServers)
            {
                clients.Add(new ClCruncherClient(item.port, item.ipString));
            }
            stepsNum = new int[clients.Count];
            for(int i=0;i< clients.Count; i++) {
                string serverDevice = "";
                // these can change in future (tree-like cluster: node0_g instead of gpu, for example)
                if (deviceTypes.Contains("gpu"))
                    serverDevice += "gpu";
                if (deviceTypes.Contains("cpu"))
                    serverDevice += "cpu";
                if (deviceTypes.Contains("acc"))
                    serverDevice += "acc";

                clients[i].netSetup(serverDevice, kernelsString,
                        kernelNamesStringArray,localRangeValue,
                        numGPUsToUse, GPU_STREAM,
                        MAX_CPU);

                stepsNum[i] = clients[i].numDevices()*localRangeValue;
                if (pipelineEnabled)
                    stepsNum[i] *= pipelineBlobCount;
            }

        }

        public void dispose()
        {
            Parallel.For(0, clients.Count, i =>
            {
                clients[i].dispose();
            });
            mainframeComputer.dispose();
        }
    }
}
