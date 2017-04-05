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
            IDeviceQueryable cpus();

            /// <summary>
            /// select gpus
            /// </summary>
            /// <returns></returns>
            IDeviceQueryable gpus();

            /// <summary>
            /// select accelerators
            /// </summary>
            /// <returns></returns>
            IDeviceQueryable accelerators();

            /// <summary>
            /// orders devices with most numerous compute units first, least ones last 
            /// </summary>
            /// <param name="n"></param>
            /// <returns></returns>
            IDeviceQueryable devicesWithMostComputeUnits();

            /// <summary>
            /// disrete GPUs, fpgas, ...
            /// </summary>
            /// <returns></returns>
            IDeviceQueryable devicesWithDedicatedMemory();

            /// <summary>
            /// iGPUs but not CPUs
            /// </summary>
            /// <returns></returns>
            IDeviceQueryable devicesWithHostMemorySharing();

            /// <summary>
            /// 16GB CPU > 8GB GPU
            /// </summary>
            /// <returns></returns>
            IDeviceQueryable devicesWithHighestMemoryAvailable();

            /// <summary>
            /// Amd devices
            /// </summary>
            /// <returns></returns>
            IDeviceQueryable devicesAmd();

            /// <summary>
            /// Nvidia devices
            /// </summary>
            /// <returns></returns>
            IDeviceQueryable devicesNvidia();

            /// <summary>
            /// Intel devices
            /// </summary>
            /// <returns></returns>
            IDeviceQueryable devicesIntel();

            /// <summary>
            /// Altera devices
            /// </summary>
            /// <returns></returns>
            IDeviceQueryable devicesAltera();

            /// <summary>
            /// Xilinx devices
            /// </summary>
            /// <returns></returns>
            IDeviceQueryable devicesXilinx();
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


            public IDeviceQueryable cpus()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable gpus()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable accelerators()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesWithMostComputeUnits()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesWithDedicatedMemory()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesWithHostMemorySharing()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesWithHighestMemoryAvailable()
            {
                throw new NotImplementedException();
            }


            public IDeviceQueryable devicesAmd()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesNvidia()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesIntel()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesAltera()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesXilinx()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// contains all devices of this platform, overloads [], indexing
            /// </summary>
            internal ClDevices devices { get;  }
        }


        /// <summary>
        /// has devices of a selected platform or all platforms, overloads [] indexing to pick one by one
        /// </summary>
        public class ClDevices : IDeviceQueryable
        {
            private ClDevice[] devices;

            public IDeviceQueryable accelerators()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable cpus()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable gpus()
            {
                throw new NotImplementedException();
            }   

            public IDeviceQueryable devicesAltera()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesAmd()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesIntel()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesNvidia()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesWithDedicatedMemory()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesWithHighestMemoryAvailable()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesWithHostMemorySharing()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesWithMostComputeUnits()
            {
                throw new NotImplementedException();
            }

            public IDeviceQueryable devicesXilinx()
            {
                throw new NotImplementedException();
            }


        }

    }

}
