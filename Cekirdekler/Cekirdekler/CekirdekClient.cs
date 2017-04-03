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
    public class CekirdekClient
    {
        TcpClient client;
        int PORT_NO;
        string SERVER_IP;
        Thread t;
        NetworkStream nwStream;
        public bool exception = false;
        public CekirdekClient(int port_no, string server_ip)
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



        private void upload(NetworkStream nwStream, byte[] gonderilecek)
        {

            byte endianness = BitConverter.IsLittleEndian ? (byte)0 : (byte)1;


            int pos = 0;
            while (pos < gonderilecek.Length)
            {
                int l = Math.Min(gonderilecek.Length - pos, NetworkBuffer.receiveSendBufferSize);
                nwStream.Write(gonderilecek, pos, l);
                pos += l;
            }
        }


        private NetworkBuffer download(NetworkStream nwStream, object[] diziler_ = null)
        {
            /* cevap alınıyor */
            int bytesRead = NetworkBuffer.receiveSendBufferSize;
            int uzunluk = -1;
            int byteToplam = 0;
            byte[] buffer = new byte[NetworkBuffer.receiveSendBufferSize];
            byte[] serverBuffer = new byte[NetworkBuffer.receiveSendBufferSize];
            int sayac = 0;
            NetworkBuffer nbCevap = new NetworkBuffer();
            while (!nwStream.DataAvailable) { Thread.Sleep(1); }
            while (nwStream.CanRead && bytesRead > 0 && (uzunluk == -1 || byteToplam < uzunluk))
            {

                bytesRead = nwStream.Read(buffer, 0, NetworkBuffer.receiveSendBufferSize);


                if (uzunluk == -1)
                    uzunluk = nbCevap.bufferBoyuOku(buffer);
                if (uzunluk == 0)
                {
                    Console.WriteLine("client: buffer alınamadı.");
                    return nbCevap;
                }
                else
                {
                    if (serverBuffer.Length < uzunluk)
                        serverBuffer = new byte[uzunluk];
                    Buffer.BlockCopy(buffer, 0, serverBuffer, byteToplam, bytesRead);
                    byteToplam += bytesRead;
                }
            }
            List<NetworkBuffer.HashIndisSiraDizi> sonucDizileri = nbCevap.oku(serverBuffer, "client", diziler_);

            return nbCevap;
        }

        public void kurulum(string aygitTurleri = "",
             string kerneller_ = "",
             string[] kernelIsimleri_ = null,
             int localThreadSayisi_ = 256,
             int kullanilacakGPUSayisi_ = -1,
             bool GPU_STREAM_ = false,
             int MAX_CPU_ = -1)
        {
            NetworkBuffer nb = new NetworkBuffer(NetworkBuffer.KURULUM);
            if (aygitTurleri.Equals(""))
                aygitTurleri = "cpu gpu";

            if (kerneller_.Equals(""))
                kerneller_ = @"__kernel void serverDeneme(__global float *a){a[get_global_id(0)]+=3.14f;}";

            if (kernelIsimleri_ == null)
                kernelIsimleri_ = new string[] { "serverDeneme" };

            int[] localThreadSayisi = new int[] { localThreadSayisi_ };
            int[] kullanilacakGPUSayisi = new int[] { kullanilacakGPUSayisi_ };
            bool[] GPU_STREAM = new bool[] { GPU_STREAM_ };
            int[] MAX_CPU = new int[] { MAX_CPU_ };
            string ki = String.Join(" ", kernelIsimleri_);
            nb.ekle(aygitTurleri.ToCharArray(), aygitTurleri.GetHashCode());
            nb.ekle(kerneller_.ToCharArray(), kerneller_.GetHashCode());
            nb.ekle(ki.ToCharArray(), ki.GetHashCode());
            nb.ekle(localThreadSayisi, localThreadSayisi.GetHashCode());
            nb.ekle(kullanilacakGPUSayisi, kullanilacakGPUSayisi.GetHashCode());
            nb.ekle(GPU_STREAM, GPU_STREAM.GetHashCode());
            nb.ekle(MAX_CPU, MAX_CPU.GetHashCode());

            upload(nwStream, nb.buf());
            Console.WriteLine(download(nwStream));
        }

        public void hesap(string[] kernelAdi___ = null,
            int adimSayisi_ = 0, string adimFonksiyonu = "",
            object[] diziler_ = null, string[] oku_yaz = null,
            int[] enKucukElemanGrubundakiElemanSayisi = null,
            int toplamMenzil_ = 1024, int hesapId_ = 1,
            int threadReferans_ = 0, bool pipelineAcik_ = false,
            int pipelineParcaSayisi__ = 4, bool pipelineTuru_ = Cores.PIPELINE_EVENT)
        {
            NetworkBuffer nbHesap = new NetworkBuffer(NetworkBuffer.HESAP);
            if (kernelAdi___ == null)
                kernelAdi___ = new string[] { "serverDeneme" };
            string kernelAdi__ = String.Join(" ", kernelAdi___);

            int[] adimSayisi = new int[] { adimSayisi_ };

            if (diziler_ == null)
                diziler_ = new object[] { new float[1024] };

            int[] diziler_adet = new int[] { diziler_.Length };

            if (oku_yaz == null)
                oku_yaz = new string[] { "oku yaz" };

            if (enKucukElemanGrubundakiElemanSayisi == null)
                enKucukElemanGrubundakiElemanSayisi = new int[] { 1 };

            int[] toplamMenzil = new int[] { toplamMenzil_ };

            int[] hesapId = new int[] { hesapId_ };


            int[] threadReferans = new int[] { threadReferans_ };

            bool[] pipelineAcik = new bool[] { pipelineAcik_ };

            int[] pipelineParcaSayisi_ = new int[] { pipelineParcaSayisi__/* default 4 ama pipeline kapalıyken önemsiz*/};

            bool[] pipelineTuru = new bool[] { pipelineTuru_ };

            nbHesap.ekle(kernelAdi__.ToCharArray(), kernelAdi__.GetHashCode());
            nbHesap.ekle(adimSayisi, adimSayisi.GetHashCode());
            nbHesap.ekle(adimFonksiyonu.ToCharArray(), adimFonksiyonu.GetHashCode());
            nbHesap.ekle(diziler_adet, diziler_adet.GetHashCode());

            for (int m = 0; m < diziler_.Length; m++)
            {
                if (oku_yaz[m].Contains("partial"))
                {
                    // yapılacak: FloatDizi türü de eklenecek
                    if (diziler_[m].GetType() == typeof(float[]))
                        nbHesap.ekle((float[])diziler_[m], diziler_[m].GetHashCode(),
                            threadReferans_, toplamMenzil_, enKucukElemanGrubundakiElemanSayisi[m]);
                    else if (diziler_[m].GetType() == typeof(int[]))
                        nbHesap.ekle((int[])diziler_[m], diziler_[m].GetHashCode(),
                            threadReferans_, toplamMenzil_, enKucukElemanGrubundakiElemanSayisi[m]);
                    else if (diziler_[m].GetType() == typeof(byte[]))
                        nbHesap.ekle((byte[])diziler_[m], diziler_[m].GetHashCode(),
                            threadReferans_, toplamMenzil_, enKucukElemanGrubundakiElemanSayisi[m]);
                    else if (diziler_[m].GetType() == typeof(char[]))
                        nbHesap.ekle((char[])diziler_[m], diziler_[m].GetHashCode(),
                            threadReferans_, toplamMenzil_, enKucukElemanGrubundakiElemanSayisi[m]);
                    else if (diziler_[m].GetType() == typeof(double[]))
                        nbHesap.ekle((double[])diziler_[m], diziler_[m].GetHashCode(),
                            threadReferans_, toplamMenzil_, enKucukElemanGrubundakiElemanSayisi[m]);
                    else if (diziler_[m].GetType() == typeof(long[]))
                        nbHesap.ekle((long[])diziler_[m], diziler_[m].GetHashCode(),
                            threadReferans_, toplamMenzil_, enKucukElemanGrubundakiElemanSayisi[m]);
                    /*
                    else if (diziler_[m].GetType() == typeof(FloatDizi))
                        nbHesap.ekle((FloatDizi)diziler_[m], diziler_[m].GetHashCode(),
                            threadReferans_, toplamMenzil_, enKucukElemanGrubundakiElemanSayisi[m]);
                    else if (diziler_[m].GetType() == typeof(ByteDizi))
                        nbHesap.ekle((ByteDizi)diziler_[m], diziler_[m].GetHashCode(),
                            threadReferans_, toplamMenzil_, enKucukElemanGrubundakiElemanSayisi[m]);
                    */
                }
                else if (oku_yaz[m].Contains("read"))
                {
                    // yapılacak: FloatDizi türü de eklenecek
                    if (diziler_[m].GetType() == typeof(float[]))
                        nbHesap.ekle((float[])diziler_[m], diziler_[m].GetHashCode());
                    else if (diziler_[m].GetType() == typeof(int[]))
                        nbHesap.ekle((int[])diziler_[m], diziler_[m].GetHashCode());
                    else if (diziler_[m].GetType() == typeof(byte[]))
                        nbHesap.ekle((byte[])diziler_[m], diziler_[m].GetHashCode());
                    else if (diziler_[m].GetType() == typeof(char[]))
                        nbHesap.ekle((char[])diziler_[m], diziler_[m].GetHashCode());
                    else if (diziler_[m].GetType() == typeof(double[]))
                        nbHesap.ekle((double[])diziler_[m], diziler_[m].GetHashCode());
                    else if (diziler_[m].GetType() == typeof(long[]))
                        nbHesap.ekle((long[])diziler_[m], diziler_[m].GetHashCode());
                    /*
                    else if (diziler_[m].GetType() == typeof(FloatDizi))
                        nbHesap.ekle((FloatDizi)diziler_[m], diziler_[m].GetHashCode());
                    else if (diziler_[m].GetType() == typeof(ByteDizi))
                        nbHesap.ekle((ByteDizi)diziler_[m], diziler_[m].GetHashCode());
                        */
                }
            }


            for (int m = 0; m < diziler_.Length; m++)
                nbHesap.ekle(oku_yaz[m].ToCharArray(), oku_yaz[m].GetHashCode());
            nbHesap.ekle(enKucukElemanGrubundakiElemanSayisi, enKucukElemanGrubundakiElemanSayisi.GetHashCode());
            nbHesap.ekle(toplamMenzil, toplamMenzil.GetHashCode());
            nbHesap.ekle(hesapId, hesapId.GetHashCode());
            nbHesap.ekle(threadReferans, threadReferans.GetHashCode());
            nbHesap.ekle(pipelineAcik, pipelineAcik.GetHashCode());
            nbHesap.ekle(pipelineParcaSayisi_, pipelineParcaSayisi_.GetHashCode());
            nbHesap.ekle(pipelineTuru, pipelineTuru.GetHashCode());

            // "oku" olanların hepsi,
            // "parçalı oku" olanların kendi menzilleri yazılacak
            upload(nwStream, nbHesap.buf());
            NetworkBuffer nbCevap = new NetworkBuffer();

            // sadece "yaz" olanlar, kendi menzilleri kadar okunacak
            download(nwStream, diziler_);
        }



        public void sil()
        {
            NetworkBuffer nb2 = new NetworkBuffer(NetworkBuffer.SIL);
            upload(nwStream, nb2.buf());
            Console.WriteLine(download(nwStream));

            nwStream.Close(1000);
            nwStream.Dispose();
            client.Close();

            // .Net 4.6
            //client.Dispose();
        }


        public bool kontrol()
        {
            NetworkBuffer nb2 = new NetworkBuffer(NetworkBuffer.SERVER_SINAMA);
            try
            {
                if (!exception)
                {
                    upload(nwStream, nb2.buf());
                    return download(nwStream).bufferKomut() == NetworkBuffer.CEVAP_SINAMA;
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

        public int aygitSayisi()
        {
            NetworkBuffer nb2 = new NetworkBuffer(NetworkBuffer.SERVER_AYGIT_SAYISI);
            upload(nwStream, nb2.buf());
            int[] aygitSayisi_ = new int[1];
            NetworkBuffer nb3= download(nwStream, new object[] { aygitSayisi_ });
            if (nb3.bufferKomut() == NetworkBuffer.CEVAP_AYGIT_SAYISI)
                return aygitSayisi_[0];
            else
                return -1;
        }

        public void dur()
        {
            NetworkBuffer nb2 = new NetworkBuffer(NetworkBuffer.SERVER_DURDUR);
            upload(nwStream, nb2.buf());
            Console.WriteLine(download(nwStream));

        }
    }
}
