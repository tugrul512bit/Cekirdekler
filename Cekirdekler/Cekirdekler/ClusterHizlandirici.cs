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
    /// <para>cluster server node sonradan alt cluster olarak kullanılabilir hale getirilecek</para>
    /// </summary>
    public class CluserHizlandirici:IHesapNode
    {
        // tüm alt katmanı tek merkezden kontrol eden,
        // fazlalık-paylaştırılamayan işlemleri hesaplayan bilgisayar
        Cores anaBilgisayar;

        // her bir server node kontrolü için bir client
        List<CekirdekClient> clientler;

        public class ServerBilgisi
        {
            public int port { get; set; }
            public string ip_ { get; set; }
            public string adi { get; set; }
            public double roundTripPerformans { get; set; }
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

        private bool serverCekirdekDeneme(string ip,int port)
        {
            CekirdekClient client = new CekirdekClient(port, ip);
            if (client == null || client.exception)
                return false;
            else
            {
                bool ret = client.kontrol();
                client.dur();
                return ret;
            }
        }

        /// <summary>
        /// clientte parametre ile belirlenen portları kontrol eder(ör: tüm serverların 50000 portları)
        /// </summary>
        private List<ServerBilgisi> serverBul(int port_,bool hizliArama)
        {
            string ip_lan = "192.168.1.";
            object kilit = new object();
            int ipSayisi = 0;
            List<ServerBilgisi> serverListesi = new List<ServerBilgisi>();
            int ustLimit = 255;
            if (hizliArama)
                ustLimit = 10;
            Parallel.For(1, ustLimit, i =>
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
                        var server = new ServerBilgisi() { port = port_, roundTripPerformans = 1.0 / (0.1 + (double)rep.RoundtripTime), ip_ = (new StringBuilder(ip)).ToString() };


                        Console.WriteLine(rep.RoundtripTime + "ms");
                      
                        try
                        {

                            //IPHostEntry ipHostEntry = Dns.GetHostEntry(ip);
                            //Console.WriteLine(ipHostEntry.HostName);
                            //server.adi = ipHostEntry.HostName;
                        }
                        catch (Exception ee)
                        {
                            // server olmayabilir
                            server.adi = "belirsiz server adı";
                        }


                        
                            if (serverCekirdekDeneme(ip, port_))
                            {
                                // cluster ağaç şeklindeyse, iletişim hızı test edilecek (MB/s)
                                // en yakın n tanesi alınacak
                                lock (kilit)
                                {
                                    ipSayisi++;
                                    serverListesi.Add(server);
                                }
                            }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("ping hatası");
                    Console.WriteLine(e.StackTrace);
                    
                }
                finally
                {
                    p.Dispose();
                    Console.WriteLine("______________________");

                }
            });
            Console.WriteLine(ipSayisi + " adet pc bağlı.");
            foreach (var item in serverListesi)
            {
                Console.WriteLine("---------------");
                Console.WriteLine(item.ip_);
                Console.WriteLine(item.roundTripPerformans);
                Console.WriteLine(item.adi);
                Console.WriteLine("---------------");
            }
            return serverListesi;
        }

        // hesap id --> menziller
        private bool pipelineAcik;
        private int pipelineParcaSayisi;
        private int localThreadSayisi_;
        private int[] adimlar;
        private int[] referanslar;
        private Stopwatch[] kronometreler;
        private Dictionary<int, int> anaBilgisayarThreadleri;
        private Dictionary<int, double> anaBilgisayarSureleri;
        private Dictionary<int, int[]> sonMenziller;
        private Dictionary<int, double[]> sonHesapSureleri;
        private Dictionary<int, ClusterLoadBalancer> yukDengeleyiciler;
        Stopwatch swHesap;
        Stopwatch swAnaBilgisayar;
        public void hesapla(string[] kernelAdi___ = null,
            int adimSayisi_ = 0, string adimFonksiyonu = "",
            object[] diziler_ = null, string[] oku_yaz = null,
            int[] enKucukElemanGrubundakiElemanSayisi = null,
            int toplamMenzil_ = 1024, int hesapId_ = 1,
            int threadReferans_ = 0, bool pipelineAcik_ = false,
            int pipelineParcaSayisi__ = 4, bool pipelineTuru_ = Cores.PIPELINE_EVENT)
        {
            pipelineAcik = pipelineAcik_;
            pipelineParcaSayisi = pipelineParcaSayisi__;
            if (adimlar == null || adimlar.Length != clientler.Count)
            {
                adimlar = new int[clientler.Count];
                for (int i = 0; i < clientler.Count; i++)
                {
                    adimlar[i] = clientler[i].aygitSayisi() * localThreadSayisi_;
                    if (pipelineAcik_)
                        adimlar[i] *= pipelineParcaSayisi__;
                }
            }
            if (swHesap == null)
                swHesap = new Stopwatch();
            if (referanslar == null || referanslar.Length != clientler.Count)
            {
                referanslar = new int[clientler.Count];
            }
            if (swAnaBilgisayar == null)
                swAnaBilgisayar = new Stopwatch();
            if (kronometreler == null)
            {
                kronometreler = new Stopwatch[clientler.Count];
                for (int i = 0; i < clientler.Count; i++)
                    kronometreler[i] = new Stopwatch();
            }
            if (sonMenziller == null)
                sonMenziller = new Dictionary<int, int[]>();
            if (sonHesapSureleri == null)
            {
                sonHesapSureleri = new Dictionary<int, double[]>();
                if (sonHesapSureleri.ContainsKey(hesapId_))
                {
                    for (int i = 0; i < clientler.Count; i++)
                    {

                        sonHesapSureleri[hesapId_][i] = 0.01;
                    }
                }
                else
                {
                    sonHesapSureleri.Add(hesapId_, new double[clientler.Count]);

                    for (int i = 0; i < clientler.Count; i++)
                    {
                        sonHesapSureleri[hesapId_][i] = 0.01;
                    }
                }
            }
        
            if (yukDengeleyiciler == null)
                yukDengeleyiciler = new Dictionary<int, ClusterLoadBalancer>();
            if (anaBilgisayarThreadleri == null)
                anaBilgisayarThreadleri = new Dictionary<int, int>();
            if (anaBilgisayarSureleri == null)
                anaBilgisayarSureleri = new Dictionary<int, double>();
            if (!yukDengeleyiciler.ContainsKey(hesapId_))
                yukDengeleyiciler.Add(hesapId_, new ClusterLoadBalancer());


            if (!sonMenziller.ContainsKey(hesapId_))
            {
                int[] tmpMenziller = new int[clientler.Count];
                if (anaBilgisayarThreadleri.ContainsKey(hesapId_))
                {
                    anaBilgisayarThreadleri[hesapId_] = yukDengeleyiciler[hesapId_].dengeleEsit(toplamMenzil_, tmpMenziller, adimlar);
                }
                else
                {
                    anaBilgisayarThreadleri.Add(hesapId_, yukDengeleyiciler[hesapId_].dengeleEsit(toplamMenzil_, tmpMenziller, adimlar));
                }
                sonMenziller.Add(hesapId_, tmpMenziller);
            }
            else
            {
                if (clientler.Count != sonMenziller[hesapId_].Length)
                {
                    int[] tmpMenziller = new int[clientler.Count];

                    if (anaBilgisayarThreadleri.ContainsKey(hesapId_))
                    {
                        anaBilgisayarThreadleri[hesapId_] = yukDengeleyiciler[hesapId_].dengeleEsit(toplamMenzil_, tmpMenziller, adimlar);
                    }
                    else
                    {
                        anaBilgisayarThreadleri.Add(hesapId_, yukDengeleyiciler[hesapId_].dengeleEsit(toplamMenzil_, tmpMenziller, adimlar));
                    }

                    sonMenziller[hesapId_] = tmpMenziller;

                }
                else
                {
                    if (anaBilgisayarThreadleri.ContainsKey(hesapId_))
                    {
                        anaBilgisayarThreadleri[hesapId_] =
                        yukDengeleyiciler[hesapId_].dengelePerformansaGore(sonHesapSureleri[hesapId_],
                            toplamMenzil_, sonMenziller[hesapId_],
                            adimlar, anaBilgisayarThreadleri[hesapId_],
                            anaBilgisayarSureleri[hesapId_]);
                    }
                    else
                    {
                        anaBilgisayarThreadleri.Add(hesapId_, yukDengeleyiciler[hesapId_].dengelePerformansaGore(sonHesapSureleri[hesapId_],
                            toplamMenzil_, sonMenziller[hesapId_],
                            adimlar, anaBilgisayarThreadleri[hesapId_],
                            anaBilgisayarSureleri[hesapId_]));
                    }
                }
            }

            int tmpRef = threadReferans_;
            for(int i=0;i<clientler.Count;i++)
            {
                referanslar[i] = tmpRef;
                tmpRef += sonMenziller[hesapId_][i];
            }


            swHesap.Reset();
            swHesap.Start();
            Parallel.For(0, clientler.Count + 1, i => {
                if (i < clientler.Count)
                {
                    kronometreler[i].Reset();
                    kronometreler[i].Start();
                        clientler[i].hesap(
                            kernelAdi___,
                            adimSayisi_, adimFonksiyonu,
                            diziler_, oku_yaz,
                            enKucukElemanGrubundakiElemanSayisi,
                            sonMenziller[hesapId_][i], hesapId_,
                            referanslar[i], pipelineAcik_,
                            pipelineParcaSayisi__, pipelineTuru_);
                    kronometreler[i].Stop();

                    if (sonHesapSureleri.ContainsKey(hesapId_))
                    {
                            sonHesapSureleri[hesapId_][i] = 0.01 + ((double)kronometreler[i].ElapsedMilliseconds);

                    }
                    else
                    {
                        sonHesapSureleri.Add(hesapId_, new double[clientler.Count]);

                        sonHesapSureleri[hesapId_][i] = 0.01 + ((double)kronometreler[i].ElapsedMilliseconds);
                    }
                }
                else
                {
                    
                    swAnaBilgisayar.Reset();
                    swAnaBilgisayar.Start();
                    if (anaBilgisayarThreadleri[hesapId_] > 0)
                    {
                        anaBilgisayar.compute(kernelAdi___,
                            adimSayisi_, adimFonksiyonu,
                            diziler_, oku_yaz,
                            enKucukElemanGrubundakiElemanSayisi,
                            anaBilgisayarThreadleri[hesapId_], hesapId_,
                            tmpRef, pipelineAcik_,
                            pipelineParcaSayisi__, pipelineTuru_);
                    }
                    swAnaBilgisayar.Stop();
                    if (anaBilgisayarSureleri.ContainsKey(hesapId_))
                    {
                        anaBilgisayarSureleri[hesapId_] = 0.01 + (double)swAnaBilgisayar.ElapsedMilliseconds;
                    }
                    else
                    {
                        anaBilgisayarSureleri.Add(hesapId_, 0.01 + (double)swAnaBilgisayar.ElapsedMilliseconds);

                    }
                }
            });
            swHesap.Stop();
            Console.WriteLine("Hesap süresi="+swHesap.ElapsedMilliseconds+"ms");
        }

        public double hesapSuresi()
        {
            throw new NotImplementedException();
        }



        public void kur(string aygitTurleri, string kerneller_, 
                        string[] kernelIsimleri_,int localThreadSayisi = 256, 
                        int kullanilacakGPUSayisi = -1, bool GPU_STREAM = true, 
                        int MAX_CPU = -1)
        {
            localThreadSayisi_ = localThreadSayisi;
            aygitTurleri = aygitTurleri.ToLower();
            kullanilacakGPUSayisi = 1;
            string anaBilgisayarClAygit = "";

            
            if (aygitTurleri.Contains("node0_g"))
                anaBilgisayarClAygit = "gpu";
            else if (aygitTurleri.Contains("node0_c"))
                anaBilgisayarClAygit = "cpu";
            int anaBilgisayarGPUSayisi = 1;
            anaBilgisayar = new Cores(anaBilgisayarClAygit, kerneller_, kernelIsimleri_,
                           localThreadSayisi, anaBilgisayarGPUSayisi, GPU_STREAM, MAX_CPU);
            List<int>portlar=new List<int>();
            if(aygitTurleri.Contains("cluster:"))
            {
                string[] strPortlar=aygitTurleri.Split(new string[] { "cluster:" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim().Split(new string[] { "port" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim().Split(new string[] { "," },StringSplitOptions.RemoveEmptyEntries);
                if (strPortlar == null)
                {
                    Console.WriteLine("Cluster kullanımı için port no belirtilmelidir.");
                    return;
                }
                else if(strPortlar.Length>0)
                {
                    for(int i=0;i<strPortlar.Length;i++)
                    {
                        if(strPortlar[i]!=null && !strPortlar[i].Trim().Equals("") && strPortlar[i].Trim().Length>0)
                            portlar.Add(int.Parse(strPortlar[i].Trim()));
                    }
                }
            }
            else
            {
                Console.WriteLine("Cluster kullanımı için aygıt türü parametresine, cluster:aaaaa şeklinde port numarası belirtilmelidir.");
                return;
            }
            bool hizliArama = (aygitTurleri.Contains("hizli-ara")) ? true : false;
            List<ServerBilgisi> clusterServerleri = new List<ServerBilgisi>();

            if (clientler == null)
                clientler = new List<CekirdekClient>();
            for (int i = 0; i < portlar.Count; i++)
            {
                Console.WriteLine("prt:"+portlar[i]);
                List<ServerBilgisi> tmpClusterServerleri = serverBul(portlar[i], hizliArama);
                if (tmpClusterServerleri != null && tmpClusterServerleri.Count > 0)
                    clusterServerleri.AddRange(tmpClusterServerleri);
            }
            
            foreach (var item in clusterServerleri)
            {
                clientler.Add(new CekirdekClient(item.port, item.ip_));
            }
            adimlar = new int[clientler.Count];
            for(int i=0;i< clientler.Count; i++) {
                string serverAygit = "";
                if (aygitTurleri.Contains("gpu"))
                    serverAygit += "gpu";
                if (aygitTurleri.Contains("cpu"))
                    serverAygit += "cpu";
                if (aygitTurleri.Contains("acc"))
                    serverAygit += "acc";

                clientler[i].kurulum(serverAygit, kerneller_,
                        kernelIsimleri_,localThreadSayisi,
                        kullanilacakGPUSayisi, GPU_STREAM,
                        MAX_CPU);

                // yapılacak: aygıt sayısı * localThreadSayisi kadar
                adimlar[i] = clientler[i].aygitSayisi()*localThreadSayisi;
                if (pipelineAcik)
                    adimlar[i] *= pipelineParcaSayisi;
            }

        }

        public void sil()
        {
            Parallel.For(0, clientler.Count, i =>
            {
                clientler[i].sil();
            });
            anaBilgisayar.dispose();
        }
    }
}
