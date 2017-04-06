using ClObject;
using System;
using System.Collections.Generic;
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
            /// <param name="n"></param>
            /// <returns></returns>
            ClDevices devicesWithMostComputeUnits();

            /// <summary>
            /// disrete GPUs, fpgas, ...
            /// </summary>
            /// <returns></returns>
            ClDevices devicesWithDedicatedMemory();

            /// <summary>
            /// iGPUs but not CPUs
            /// </summary>
            /// <returns></returns>
            ClDevices devicesWithHostMemorySharing();

            /// <summary>
            /// 16GB CPU > 8GB GPU
            /// </summary>
            /// <returns></returns>
            ClDevices devicesWithHighestMemoryAvailable();

            /// <summary>
            /// Amd devices
            /// </summary>
            /// <returns></returns>
            ClDevices devicesAmd();

            /// <summary>
            /// Nvidia devices
            /// </summary>
            /// <returns></returns>
            ClDevices devicesNvidia();

            /// <summary>
            /// Intel devices
            /// </summary>
            /// <returns></returns>
            ClDevices devicesIntel();

            /// <summary>
            /// Altera devices
            /// </summary>
            /// <returns></returns>
            ClDevices devicesAltera();

            /// <summary>
            /// Xilinx devices
            /// </summary>
            /// <returns></returns>
            ClDevices devicesXilinx();
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
                        if (platforms[i].platformName.ToLower().Trim().Contains(nameSearchStrings[j]) ||
                           platforms[i].vendorName.ToLower().Trim().Contains(nameSearchStrings[j]))
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
            /// <param name="devicePartitionEnabled"></param>
            /// <param name="streamingEnabled"></param>
            /// <param name="MAX_CPU_CORES"></param>
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
            /// <param name="streamingEnabled"></param>
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
            /// <param name="streamingEnabled"></param>
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
            /// returns devices ordered by decreasing number of compute units
            /// </summary>
            /// <returns></returns>
            public ClDevices devicesWithMostComputeUnits()
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
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_ACC(), j, false, false, -1);
                        totalDevices++;
                        deviceList.Add(tmp);
                    }
                    for (int j = 0; j < numCpu; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_CPU(), j, false, false, -1);
                        totalDevices++;
                        deviceList.Add(tmp);
                    }
                    for (int j = 0; j < numGpu; j++)
                    {
                        ClDevice tmp = new ClDevice(platforms[i], ClPlatform.CODE_GPU(), j, false, false, -1);
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

            public ClDevices devicesWithDedicatedMemory()
            {
                // get cpu gpu acc, put in same array, order by dedicated or not
                throw new NotImplementedException();
            }

            public ClDevices devicesWithHostMemorySharing()
            {
                // get cpu gpu acc, put in same array, order by not dedicated or dedicated
                throw new NotImplementedException();
            }

            public ClDevices devicesWithHighestMemoryAvailable()
            {
                // get cpu gpu acc, put in same array, order by mem size
                throw new NotImplementedException();
            }


            public ClDevices devicesAmd()
            {
                // get cpu gpu acc, get only amd named, put in same array
                throw new NotImplementedException();
            }

            public ClDevices devicesNvidia()
            {
                // get cpu gpu acc, get only nvidia named, put in same array
                throw new NotImplementedException();
            }

            public ClDevices devicesIntel()
            {
                // get cpu gpu acc, get only intel named, put in same array
                throw new NotImplementedException();
            }

            public ClDevices devicesAltera()
            {
                // get cpu gpu acc, get only altera named, put in same array
                throw new NotImplementedException();
            }

            public ClDevices devicesXilinx()
            {
                // get cpu gpu acc, get only xilinx named, put in same array
                throw new NotImplementedException();
            }

            /// <summary>
            /// info about platform details
            /// </summary>
            public void logInfo()
            {
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

            /// <summary>
            /// device details
            /// </summary>
            public void logInfo()
            {
                Console.WriteLine("---------");
                Console.WriteLine("Selected devices:");
                for(int i=0;i<devices.Length;i++)
                {
                    string stringToAddForDeviceName = "#" + i + ": " + devices[i].name().Trim();
                    int spaces = stringToAddForDeviceName.Length;
                    spaces = 48 - spaces;
                    if (spaces < 0)
                    {
                        spaces = 0;
                        stringToAddForDeviceName = stringToAddForDeviceName.Remove(48);
                    }
                    Console.WriteLine(stringToAddForDeviceName + (new string(' ',spaces))+"  number of compute units: "+String.Format("{0:###,###}", devices[i].numberOfComputeUnits).PadLeft(3,' ')+"    type:"+((devices[i].type()==ClPlatform.CODE_GPU())?"GPU":((devices[i].type() == ClPlatform.CODE_CPU())?"CPU":"ACCELERATOR")));
                }
                Console.WriteLine("---------");
            }

            public ClDevices accelerators(bool streamingEnabled = false)
            {
                throw new NotImplementedException();
            }

            public ClDevices cpus(bool devicePartitionEnabled = false, bool streamingEnabled = false, int MAX_CPU_CORES = -1)
            {
                throw new NotImplementedException();
            }

            public ClDevices gpus(bool streamingEnabled = false)
            {
                throw new NotImplementedException();
            }   

            public ClDevices devicesAltera()
            {
                throw new NotImplementedException();
            }

            public ClDevices devicesAmd()
            {
                throw new NotImplementedException();
            }

            public ClDevices devicesIntel()
            {
                throw new NotImplementedException();
            }

            public ClDevices devicesNvidia()
            {
                throw new NotImplementedException();
            }

            public ClDevices devicesWithDedicatedMemory()
            {
                throw new NotImplementedException();
            }

            public ClDevices devicesWithHighestMemoryAvailable()
            {
                throw new NotImplementedException();
            }

            public ClDevices devicesWithHostMemorySharing()
            {
                throw new NotImplementedException();
            }

            public ClDevices devicesWithMostComputeUnits()
            {
                throw new NotImplementedException();
            }

            public ClDevices devicesXilinx()
            {
                throw new NotImplementedException();
            }


        }

    }

}
