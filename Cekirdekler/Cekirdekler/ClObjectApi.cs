using ClObject;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Cekirdekler
{
    namespace Hardware
    {


        /// <summary>
        /// for selecting a subset from hardware
        /// </summary>
        public interface IDeviceQueryable
        {
            /// <summary>
            /// select  all gpus from container
            /// </summary>
            /// <returns></returns>
            ClDevices cpus(bool devicePartitionEnabled=false, bool streamingEnabled=false, int MAX_CPU_CORES=-1);

            /// <summary>
            /// select gpus
            /// </summary>
            /// <returns></returns>
            ClDevices gpus( bool streamingEnabled = false);

            /// <summary>
            /// select accelerators
            /// </summary>
            /// <returns></returns>
            ClDevices accelerators( bool streamingEnabled = false);

            /// <summary>
            /// orders devices with most numerous compute units first, least ones last 
            /// </summary>
            /// <returns></returns>
            ClDevices devicesWithMostComputeUnits(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1);

            /// <summary>
            /// disrete GPUs, fpgas, ...
            /// </summary>
            /// <returns></returns>
            ClDevices devicesWithDedicatedMemory(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1);

            /// <summary>
            /// iGPUs but not CPUs
            /// </summary>
            /// <returns></returns>
            ClDevices devicesWithHostMemorySharing(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1);

            /// <summary>
            /// 16GB CPU > 8GB GPU
            /// </summary>
            /// <returns></returns>
            ClDevices devicesWithHighestMemoryAvailable(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1);

            /// <summary>
            /// Amd devices
            /// </summary>
            /// <returns></returns>
            ClDevices devicesAmd(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1);

            /// <summary>
            /// Nvidia devices
            /// </summary>
            /// <returns></returns>
            ClDevices devicesNvidia(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1);

            /// <summary>
            /// Intel devices
            /// </summary>
            /// <returns></returns>
            ClDevices devicesIntel(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1);

            /// <summary>
            /// Altera devices
            /// </summary>
            /// <returns></returns>
            ClDevices devicesAltera(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1);

            /// <summary>
            /// Xilinx devices
            /// </summary>
            /// <returns></returns>
            ClDevices devicesXilinx(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1);
        }

        /// <summary>
        /// select subset of platforms
        /// </summary>
        public interface IPlatformQueryable
        {
            /// <summary>
            /// 2 GPU platform > 1 accelerator platform
            /// </summary>
            /// <returns></returns>
            ClPlatforms platformsWithMostDevices();

            /// <summary>
            /// platforms with a description string containing "Intel" 
            /// </summary>
            /// <returns></returns>
            ClPlatforms platformsIntel();

            /// <summary>
            /// Amd platforms
            /// </summary>
            /// <returns></returns>
            ClPlatforms platformsAmd();

            /// <summary>
            /// Nvidia platforms
            /// </summary>
            /// <returns></returns>
            ClPlatforms platformsNvidia();

            /// <summary>
            /// Altera platforms
            /// </summary>
            /// <returns></returns>
            ClPlatforms platformsAltera();

            /// <summary>
            /// Xilinx platforms
            /// </summary>
            /// <returns></returns>
            ClPlatforms platformsXilinx();
        }



        /// <summary>
        /// contains all platforms, overloads [] operator, indexing
        /// </summary>
        public class ClPlatforms:IDeviceQueryable,IPlatformQueryable
        {
            internal ClPlatform[] platforms;
            internal ClPlatforms()
            {

            }

            /// <summary>
            /// gets platform and vendor names separately
            /// {platform1,platform2,....}
            /// platform1: {platformName,vendorName}
            /// </summary>
            /// <returns></returns>
            public string [][]platformVendorNames()
            {
                string[][] tmp = new string[platforms.Length][];
                for(int i=0;i<platforms.Length;i++)
                {
                    tmp[i] = new string[] { platforms[i].platformName, platforms[i].vendorName };
                }
                return tmp;
            }

            /// <summary>
            /// disposes all platforms
            /// </summary>
            ~ClPlatforms()
            {
                if(platforms!=null)
                {
                    for(int i=0;i<platforms.Length;i++)
                    {
                        platforms[i].dispose();
                    }
                }
            }

            /// <summary>
            /// get list of all platforms
            /// </summary>
            /// <returns></returns>
            public static ClPlatforms all()
            {
                ClPlatforms result = new ClPlatforms();
                IntPtr hList= Cores.platformList();
                int num=Cores.numberOfPlatforms(hList);
                result.platforms = new ClPlatform[num];
                for (int i=0;i<num;i++)
                {
                    result.platforms[i] = new ClPlatform(hList, i);
                }
                Cores.deletePlatformList(hList);
                return result;
            }

            internal ClPlatforms copy(int [] j=null)
            {
                ClPlatforms tmp = new ClPlatforms();
                if (this.platforms != null && this.platforms.Length > 0)
                {
                    if (j==null ||j.Length==0)
                    {
                        tmp.platforms = new ClPlatform[this.platforms.Length];
                        for (int i = 0; i < this.platforms.Length; i++)
                        {
                            tmp.platforms[i] = this.platforms[i].copy();
                        }
                    }
                    else
                    {
                        tmp.platforms = new ClPlatform[j.Length];
                        for(int k=0;k<j.Length;k++)
                            tmp.platforms[k] = this.platforms[j[k]].copy();
                    }
                }
                return tmp;
            }

            /// <summary>
            /// get 1 platform
            /// </summary>
            /// <param name="i"></param>
            /// <returns></returns>
            public ClPlatforms this[int i]
            {
                get
                {
                    ClPlatforms tmp = this.copy(new int[] { i });
                    return tmp;
                }
            }
            
            /// <summary>
            /// returns number of selected platforms
            /// </summary>
            public int Length
            {
                get { return platforms.Length; }
            }

            /// <summary>
            /// gets a copy of platforms tat are sorted on their number of devices
            /// </summary>
            /// <returns></returns>
            public ClPlatforms platformsWithMostDevices()
            {
                ClPlatforms tmp = this.copy();
                int[] deviceNum = new int[platforms.Length];
                for(int i=0;i<platforms.Length;i++)
                {
                    deviceNum[i] = 1000 -( platforms[i].numberOfAccelerators()+ platforms[i].numberOfGpus()+ platforms[i].numberOfCpus());
                }
                Array.Sort(deviceNum,tmp.platforms);
               
                return tmp;
            }


            private ClPlatforms platformsSearchName(string [] nameSearchStrings)
            {
                int counter = 0;
                int[] indices = new int[platforms.Length];
                for (int i = 0; i < platforms.Length; i++)
                {
                    for (int j = 0; j < nameSearchStrings.Length; j++)
                    {
                        if (platforms[i].platformName.ToLowerInvariant().Trim().Contains(nameSearchStrings[j]) ||
                           platforms[i].vendorName.ToLowerInvariant().Trim().Contains(nameSearchStrings[j]))
                        {
                            indices[counter] = i;
                            counter++;
                            break;
                        }
                    }
                }
                if (counter > 0)
                {
                    ClPlatforms result = this.copy(indices);
                    return result;
                }
                else
                {
                    Console.WriteLine("Platform not found: "+nameSearchStrings[0]+" ");
                    return null;
                }
            }


            /// <summary>
            /// gets platforms that have "Intel" in vendor or platform name string
            /// </summary>
            /// <returns></returns>
            public ClPlatforms platformsIntel()
            {
                return platformsSearchName(new string[]{ "intel","ıntel"});
            }

            /// <summary>
            /// gets platforms that have "Amd" or "Advanced Micro Devices" in vendor or platform name string
            /// </summary>
            /// <returns></returns>
            public ClPlatforms platformsAmd()
            {
                return platformsSearchName(new string[] {"amd", "advanced micro devices", "advanced mıcro devıces" });
            }

            /// <summary>
            /// gets platforms that have "Nvidia" in vendor or platform name string
            /// </summary>
            /// <returns></returns>
            public ClPlatforms platformsNvidia()
            {
                return platformsSearchName(new string[] {"nvidia", "nvıdıa" });
            }

            /// <summary>
            /// gets platforms that have "Altera" in vendor or platform name string
            /// </summary>
            /// <returns></returns>
            public ClPlatforms platformsAltera()
            {
                return platformsSearchName(new string[] {"altera" });
            }

            /// <summary>
            /// gets platforms that have "Xilinx" in vendor or platform name string
            /// </summary>
            /// <returns></returns>
            public ClPlatforms platformsXilinx()
            {
                return platformsSearchName(new string[] {"xilinx", "xılınx" });
            }


            /// <summary>
            /// get all cpu devices from selected platforms
            /// </summary>
            /// <param name="devicePartitionEnabled">cpus can be generally partitioned for smaller workloads or data locality</param>
            /// <param name="streamingEnabled">streaming makes device access to RAM with a zero-copy way if C++ wrapper array is given</param>
            /// <param name="MAX_CPU_CORES">limit number of cpu cores if device partition is enabled</param>
            /// <returns></returns>
            public ClDevices cpus(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                ClDevices cpuDevices = new ClDevices();
                List<ClDevice> deviceList = new List<ClDevice>();
                for (int i = 0; i < platforms.Length; i++)
                {
                    for (int j = 0; j < platforms[i].numberOfCpus(); j++)
                    {
                        ClDevice device = new ClDevice(platforms[i], ClPlatform.CODE_CPU(), j, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
                        deviceList.Add(device);
                    }
                }
                cpuDevices.devices = deviceList.ToArray();
                return cpuDevices;
            }

            /// <summary>
            /// get all gpus from selected platforms
            /// </summary>
            /// <param name="streamingEnabled">access to RAM with a zero-copy way when C++ wrapper array is given</param>
            /// <returns></returns>
            public ClDevices gpus(bool streamingEnabled = false)
            {
                ClDevices gpuDevices = new ClDevices();
                List<ClDevice> deviceList = new List<ClDevice>();
                for (int i = 0; i < platforms.Length; i++)
                {
                    for (int j = 0; j < platforms[i].numberOfGpus(); j++)
                    {
                        ClDevice device = new ClDevice(platforms[i], ClPlatform.CODE_GPU(), j, false, streamingEnabled, -1);
                        deviceList.Add(device);
                    }
                }
                gpuDevices.devices = deviceList.ToArray();
                return gpuDevices;
            }

            /// <summary>
            /// selects all accelerators from platforms inside
            /// </summary>
            /// <param name="streamingEnabled">access to RAM with zero-copy buffers when C++ array wrapper is given</param>
            /// <returns></returns>
            public ClDevices accelerators(bool streamingEnabled = false)
            {
                ClDevices accDevices = new ClDevices();
                List<ClDevice> deviceList = new List<ClDevice>();
                for (int i = 0; i < platforms.Length; i++)
                {
                    for (int j = 0; j < platforms[i].numberOfAccelerators(); j++)
                    {
                        ClDevice device = new ClDevice(platforms[i], ClPlatform.CODE_ACC(), j, false, streamingEnabled, -1);
                        deviceList.Add(device);
                    }
                }
                accDevices.devices = deviceList.ToArray();
                return accDevices;
            }

            /// <summary>
            /// get devices ordered by decreasing number of compute units
            /// </summary>
            /// <param name="devicePartitionEnabled">for each CPU device, this enables or disables device partition</param>
            /// <param name="streamingEnabled">for each device, enables zero-copy buffer access for C++ array wrapper parameters</param>
            /// <param name="MAX_CPU_CORES">for each CPU device with device partitioning enabled, limits max number of cores for the sub-device created</param>
            /// <returns></returns>
            public ClDevices devicesWithMostComputeUnits(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                int totalDevices = 0;
                List<ClDevice> deviceList = new List<ClDevice>();
                for (int i=0;i<platforms.Length;i++)
                {
                    int numAcc=platforms[i].numberOfAccelerators();
                    int numCpu=platforms[i].numberOfCpus();
                    int numGpu=platforms[i].numberOfGpus();
                    for(int j=0;j<numAcc;j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_ACC(), j, false, streamingEnabled, -1);
                        totalDevices++;
                        deviceList.Add(tmp);
                    }
                    for (int j = 0; j < numCpu; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_CPU(), j, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
                        totalDevices++;
                        deviceList.Add(tmp);
                    }
                    for (int j = 0; j < numGpu; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_GPU(), j, false, streamingEnabled, -1);
                        totalDevices++;
                        deviceList.Add(tmp);
                    }
                }
                int[] keysComputeUnits = new int[deviceList.Count];
                ClDevice[] devices = deviceList.ToArray();
                for(int i=0;i<totalDevices;i++)
                {
                    keysComputeUnits[i] =1000000 - devices[i].numberOfComputeUnits;
                }
                Array.Sort(keysComputeUnits, devices);
                // get cpu gpu acc, put in same array, order by decreasing CU

                ClDevices clDevices = new ClDevices();
                clDevices.devices = devices;
                return clDevices;
            }

            /// <summary>
            /// get devices that don't share RAM with CPU
            /// </summary>
            /// <param name="devicePartitionEnabled">for each CPU device, this enables or disables device partition</param>
            /// <param name="streamingEnabled">for each device, enables zero-copy buffer access for C++ array wrapper parameters</param>
            /// <param name="MAX_CPU_CORES">for each CPU device with device partitioning enabled, limits max number of cores for the sub-device created</param>
            /// <returns></returns>
            public ClDevices devicesWithDedicatedMemory(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                int totalDevices = 0;
                List<ClDevice> deviceList = new List<ClDevice>();
                for (int i = 0; i < platforms.Length; i++)
                {
                    int numAcc = platforms[i].numberOfAccelerators();
                    int numCpu = platforms[i].numberOfCpus();
                    int numGpu = platforms[i].numberOfGpus();
                    for (int j = 0; j < numAcc; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_ACC(), j, false, streamingEnabled, -1);
                        if (tmp.isGddr())
                        {
                            totalDevices++;
                            deviceList.Add(tmp);
                        }
                    }
                    for (int j = 0; j < numCpu; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_CPU(), j, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
                        if (tmp.isGddr())
                        {
                            totalDevices++;
                            deviceList.Add(tmp);
                        }
                    }
                    for (int j = 0; j < numGpu; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_GPU(), j, false, streamingEnabled, -1);
                        if (tmp.isGddr())
                        {
                            totalDevices++;
                            deviceList.Add(tmp);
                        }
                    }
                }
                ClDevice[] devices = deviceList.ToArray();

                ClDevices clDevices = new ClDevices();
                clDevices.devices = devices;
                return clDevices;
            }

            /// <summary>
            /// get devices that share RAM with CPU
            /// </summary>
            /// <param name="devicePartitionEnabled">for each CPU device, this enables or disables device partition</param>
            /// <param name="streamingEnabled">for each device, enables zero-copy buffer access for C++ array wrapper parameters</param>
            /// <param name="MAX_CPU_CORES">for each CPU device with device partitioning enabled, limits max number of cores for the sub-device created</param>
            /// <returns></returns>
            public ClDevices devicesWithHostMemorySharing(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                int totalDevices = 0;
                List<ClDevice> deviceList = new List<ClDevice>();
                for (int i = 0; i < platforms.Length; i++)
                {
                    int numAcc = platforms[i].numberOfAccelerators();
                    int numCpu = platforms[i].numberOfCpus();
                    int numGpu = platforms[i].numberOfGpus();
                    for (int j = 0; j < numAcc; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_ACC(), j, false, streamingEnabled, -1);
                        if (!tmp.isGddr())
                        {
                            totalDevices++;
                            deviceList.Add(tmp);
                        }
                    }
                    for (int j = 0; j < numCpu; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_CPU(), j, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
                        if (!tmp.isGddr())
                        {
                            totalDevices++;
                            deviceList.Add(tmp);
                        }
                    }
                    for (int j = 0; j < numGpu; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_GPU(), j, false, streamingEnabled, -1);
                        if (!tmp.isGddr())
                        {
                            totalDevices++;
                            deviceList.Add(tmp);
                        }
                    }
                }
                ClDevice[] devices = deviceList.ToArray();

                ClDevices clDevices = new ClDevices();
                clDevices.devices = devices;
                return clDevices;
            }

            public ClDevices devicesWithHighestMemoryAvailable(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                int totalDevices = 0;
                List<ClDevice> deviceList = new List<ClDevice>();
                for (int i = 0; i < platforms.Length; i++)
                {
                    int numAcc = platforms[i].numberOfAccelerators();
                    int numCpu = platforms[i].numberOfCpus();
                    int numGpu = platforms[i].numberOfGpus();
                    for (int j = 0; j < numAcc; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_ACC(), j, false, streamingEnabled, -1);
                        totalDevices++;
                        deviceList.Add(tmp);
                    }
                    for (int j = 0; j < numCpu; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_CPU(), j, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
                        totalDevices++;
                        deviceList.Add(tmp);
                    }
                    for (int j = 0; j < numGpu; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_GPU(), j, false, streamingEnabled, -1);
                        totalDevices++;
                        deviceList.Add(tmp);
                    }
                }
                ulong[] keysComputeUnits = new ulong[deviceList.Count];
                ClDevice[] devices = deviceList.ToArray();
                for (int i = 0; i < totalDevices; i++)
                {
                    keysComputeUnits[i] = ulong.MaxValue - devices[i].memorySize;
                }
                Array.Sort(keysComputeUnits, devices);

                ClDevices clDevices = new ClDevices();
                clDevices.devices = devices;
                return clDevices;
            }

            private ClDevices devicesNameSearch(string[]names, bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                int totalDevices = 0;
                List<ClDevice> deviceList = new List<ClDevice>();
                for (int i = 0; i < platforms.Length; i++)
                {
                    int numAcc = platforms[i].numberOfAccelerators();
                    int numCpu = platforms[i].numberOfCpus();
                    int numGpu = platforms[i].numberOfGpus();
                    for (int j = 0; j < numAcc; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_ACC(), j, false, streamingEnabled, -1);
                        for (int k = 0; k < names.Length; k++)
                        {
                            if (tmp.name().ToLowerInvariant().Trim().Contains(names[k]) ||
                                tmp.vendorName().ToLowerInvariant().Trim().Contains(names[k]))
                            {
                                totalDevices++;
                                deviceList.Add(tmp);
                                break;
                            }
                        }
                    }
                    for (int j = 0; j < numCpu; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_CPU(), j, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
                        for (int k = 0; k < names.Length; k++)
                        {
                            if (tmp.name().ToLowerInvariant().Trim().Contains(names[k]) ||
                                tmp.vendorName().ToLowerInvariant().Trim().Contains(names[k]))
                            {
                                totalDevices++;
                                deviceList.Add(tmp);
                                break;
                            }
                        }
                    }
                    for (int j = 0; j < numGpu; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_GPU(), j, false, streamingEnabled, -1);
                        for (int k = 0; k < names.Length; k++)
                        {
                            if (tmp.name().ToLowerInvariant().Trim().Contains(names[k])||
                                tmp.vendorName().ToLowerInvariant().Trim().Contains(names[k]))
                            {
                                totalDevices++;
                                deviceList.Add(tmp);
                                break;
                            }
                        }
                    }
                }
                ClDevice[] devices = deviceList.ToArray();
                if (devices.Length > 0)
                {
                    ClDevices clDevices = new ClDevices();
                    clDevices.devices = devices;
                    return clDevices;
                }
                else
                    return null;
            }

            public ClDevices devicesAmd(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                return devicesNameSearch(new string[] { "amd", "advanced micro devices", "advanced mıcro devıces" },devicePartitionEnabled, streamingEnabled,MAX_CPU_CORES);
            }

            public ClDevices devicesNvidia(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                return devicesNameSearch(new string[] { "nvidia", "nvıdıa", "gtx","titan","tıtan" }, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
            }

            public ClDevices devicesIntel(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                return devicesNameSearch(new string[] { "intel", "ıntel" }, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
            }

            public ClDevices devicesAltera(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                return devicesNameSearch(new string[] { "altera" }, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
            }

            public ClDevices devicesXilinx(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                return devicesNameSearch(new string[] { "xilinx", "xılınx" }, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
            }

            /// <summary>
            /// info about platform details
            /// </summary>
            public void logInfo()
            {
                if (Console.WindowWidth < 150)
                    Console.WindowWidth = 150;
               
                Console.WriteLine("--------------");
                Console.WriteLine("Selected platforms:");
                string[][] names = platformVendorNames();
                for (int i = 0; i < names.Length; i++)
                {
                    Console.WriteLine("#"+i+":");
                    Console.WriteLine("Platform name: "+ names[i][0]);
                    Console.WriteLine("Vendor name..: "+names[i][1]);
                    Console.WriteLine("Devices......: CPUs=" + platforms[i].numberOfCpus()+"      GPUs="+ platforms[i].numberOfGpus()+"        Accelerators="+ platforms[i].numberOfAccelerators());
                }
            }
        }


        /// <summary>
        /// has devices of a selected platform or all platforms, overloads [] indexing to pick one by one
        /// </summary>
        public class ClDevices : IDeviceQueryable
        {
            internal ClDevice[] devices;

            internal ClDevices copy(int[] j = null, bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                ClDevices tmp = new ClDevices();
                if (this.devices != null && this.devices.Length > 0)
                {
                    if (j == null || j.Length == 0)
                    {
                        tmp.devices = new ClDevice[this.devices.Length];
                        for (int i = 0; i < this.devices.Length; i++)
                        {
                            tmp.devices[i] = this.devices[i].copy(devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
                        }
                    }
                    else
                    {
                        tmp.devices = new ClDevice[j.Length];
                        for (int k = 0; k < j.Length; k++)
                            tmp.devices[k] = this.devices[j[k]].copy(devicePartitionEnabled ,streamingEnabled, MAX_CPU_CORES);
                    }
                }
                return tmp;
            }

            /// <summary>
            /// returns number of selected devices 
            /// </summary>
            public int Length
            {
                get { return devices.Length; }
            }

            /// <summary>
            /// device details
            /// </summary>
            public void logInfo()
            {
                if (Console.WindowWidth < 150)
                    Console.WindowWidth = 150;
                Console.WriteLine("---------");
                Console.WriteLine("Selected devices:");
                for(int i=0;i<devices.Length;i++)
                {
                    string stringToAddForDeviceName = "#" + i + ": " + devices[i].name().Trim()+"("+devices[i].vendorName().Trim()+")";
                    int spaces = stringToAddForDeviceName.Length;
                    spaces = 70 - spaces;
                    if (spaces < 0)
                    {
                        spaces = 0;
                        stringToAddForDeviceName = stringToAddForDeviceName.Remove(70);
                    }
                    Console.WriteLine(stringToAddForDeviceName + (new string(' ',spaces))+"  number of compute units: "+String.Format("{0:###,###}", devices[i].numberOfComputeUnits).PadLeft(3,' ')+"    type:"+((devices[i].type()==ClPlatform.CODE_GPU())?"GPU":((devices[i].type() == ClPlatform.CODE_CPU())?"CPU":"ACCELERATOR"))+"      memory: "+String.Format(CultureInfo.InvariantCulture,"{0:###,###.##}",devices[i].memorySize/(1024.0*1024.0*1024.0))+"GB");
                }
                Console.WriteLine("---------");
            }

            public ClDevices accelerators(bool streamingEnabled = false)
            {
                int counter = 0;
                int[] indices = new int[devices.Length];
                for(int i=0;i<devices.Length;i++)
                {
                    if(devices[i].type()==ClPlatform.CODE_ACC())
                    {
                        indices[counter] = i;
                        counter++;
                    }
                }
                if (counter > 0)
                {
                    ClDevices result = this.copy(indices, false, streamingEnabled, -1);
                    return result;
                }
                else
                    return null;
            }

            public ClDevices cpus(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                int counter = 0;
                int[] indices = new int[devices.Length];
                for (int i = 0; i < devices.Length; i++)
                {
                    if (devices[i].type() == ClPlatform.CODE_CPU())
                    {
                        indices[counter] = i;
                        counter++;
                    }
                }
                if (counter > 0)
                {
                    ClDevices result = this.copy(indices, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
                    return result;
                }
                else
                    return null;
            }

            public ClDevices gpus(bool streamingEnabled = false)
            {
                int counter = 0;
                int[] indices = new int[devices.Length];
                for (int i = 0; i < devices.Length; i++)
                {
                    if (devices[i].type() == ClPlatform.CODE_GPU())
                    {
                        indices[counter] = i;
                        counter++;
                    }
                }
                if (counter > 0)
                {
                    ClDevices result = this.copy(indices, false, streamingEnabled, -1);
                    return result;
                }
                else
                    return null;
            }   

            private ClDevices devicesNameSearch(string[]names,bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                int counter = 0;
                int[] indices = new int[devices.Length];
                for (int i = 0; i < devices.Length; i++)
                {
                    for (int j = 0; j < names.Length; j++)
                    {
                        if (devices[i].name().ToLowerInvariant().Trim().Contains(names[j]) ||
                            devices[i].vendorName().ToLowerInvariant().Trim().Contains(names[j]))
                        {
                            indices[counter] = i;
                            counter++;
                        }
                    }
                }
                if (counter > 0)
                {
                    ClDevices result = this.copy(indices, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
                    return result;
                }
                else
                    return null;
            }

            public ClDevices devicesAltera(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
               return devicesNameSearch(new string[]{"altera"}, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
            }

            public ClDevices devicesAmd(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
               return devicesNameSearch(new string[]{"amd","advanced micro devices","advanced mıcro devıces"}, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
            }

            public ClDevices devicesIntel(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
               return devicesNameSearch(new string[]{"intel","ıntel"}, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
            }

            public ClDevices devicesNvidia(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
               return devicesNameSearch(new string[]{"nvidia","nvıdıa"}, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
            }

            public ClDevices devicesXilinx(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
               return devicesNameSearch(new string[]{"xilinx","xılınx"}, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
            }

            public ClDevices devicesWithDedicatedMemory(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                int counter = 0;
                int[] indices = new int[devices.Length];
                for (int i = 0; i < devices.Length; i++)
                {
                    if (devices[i].isGddr())
                    {
                        indices[counter] = i;
                        counter++;
                    }
                }
                if (counter > 0)
                {
                    ClDevices result = this.copy(indices, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
                    return result;
                }
                else
                    return null;
            }

            public ClDevices devicesWithHighestMemoryAvailable(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                int counter = 0;
                ulong[] keys = new ulong[devices.Length];
                for (int i = 0; i < devices.Length; i++)
                {
                    keys[i] = ulong.MaxValue - devices[i].memorySize;
                }
                if (counter > 0)
                {
                    ClDevices result = this.copy(null, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
                    Array.Sort(keys, result.devices);
                    return result;
                }
                else
                    return null;
            }

            public ClDevices devicesWithHostMemorySharing(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                int counter = 0;
                int[] indices = new int[devices.Length];
                for (int i = 0; i < devices.Length; i++)
                {
                    if (!devices[i].isGddr())
                    {
                        indices[counter] = i;
                        counter++;
                    }
                }
                if (counter > 0)
                {
                    ClDevices result = this.copy(indices, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
                    return result;
                }
                else
                    return null;
            }

            /// <summary>
            /// get devices ordered by decreasing number of compute units
            /// </summary>
            /// <param name="devicePartitionEnabled">for each CPU device, this enables or disables device partition</param>
            /// <param name="streamingEnabled">for each device, enables zero-copy buffer access for C++ array wrapper parameters</param>
            /// <param name="MAX_CPU_CORES">for each CPU device with device partitioning enabled, limits max number of cores for the sub-device created</param>
            /// <returns></returns>
            public ClDevices devicesWithMostComputeUnits(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                int counter = 0;
                int[] keys = new int[devices.Length];
                for (int i = 0; i < devices.Length; i++)
                {
                    keys[i] = int.MaxValue - devices[i].numberOfComputeUnits;
                }
                if (counter > 0)
                {
                    ClDevices result = this.copy(null, devicePartitionEnabled, streamingEnabled, MAX_CPU_CORES);
                    Array.Sort(keys, result.devices);
                    return result;
                }
                else
                    return null;
            }




        }

    }

}
