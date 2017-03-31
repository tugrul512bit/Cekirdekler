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

namespace Cekirdekler
{
    /// <summary>
    /// prealpha cluster feature
    /// </summary>
    public class CekirdekServerThread
    {
        private Thread t;
        private bool calisiyor;
        private object kilit;
        private Cores opencl;
        private NetworkBuffer nbHesapCevap;
        private TcpListener tl;
        private TcpClient client;
        private byte[] serverBuffer;
        private string socketIp;
        private CekirdekServer server;
        public CekirdekServerThread(TcpListener listener, TcpClient client_, string socketIp_, CekirdekServer cs)
        {
            server = cs;
            socketIp = (new StringBuilder(socketIp_)).ToString();
            tl = listener;
            client = client_;
            kilit = new object();
            calisiyor = true;
            serverBuffer = new byte[1024];

            t = new Thread(() => f(listener, client));
            t.IsBackground = true;
            t.Start();
            Console.WriteLine("server thread başlatıldı");
        }

        public void sil()
        {
            lock (kilit)
            {
                calisiyor = false;
                client.Close();
            }
            t.Join();
            Console.WriteLine("server thread durduruldu");
        }

        Dictionary<int, object> d0 = new Dictionary<int, object>();
        Dictionary<object, int> d1 = new Dictionary<object, int>();
        NetworkBuffer nb = new NetworkBuffer();

        private void f(TcpListener listener, TcpClient client_)
        {
            bool tmpCalisiyor = true;

            while (tmpCalisiyor)
            {
                client.ReceiveBufferSize = NetworkBuffer.receiveSendBufferSize;
                client.SendBufferSize = NetworkBuffer.receiveSendBufferSize;
                byte[] buffer = new byte[NetworkBuffer.receiveSendBufferSize];

                //---read incoming stream---
                int bytesRead = NetworkBuffer.receiveSendBufferSize;
                int sayac = 0;
                int uzunluk = -1;
                int byteToplam = 0;
                nb.resetDiziHaric();
                NetworkStream nwStream = client.GetStream();
                while (!nwStream.DataAvailable) { Thread.Sleep(1); }
                while (nwStream.CanRead && bytesRead > 0 && (uzunluk == -1 || byteToplam < uzunluk))
                {

                    bytesRead = nwStream.Read(buffer, 0, NetworkBuffer.receiveSendBufferSize);


                    if (uzunluk == -1)
                        uzunluk = nb.bufferBoyuOku(buffer);
                    if (uzunluk == 0)
                    {
                        Console.WriteLine("Server: buffer alınamadı.");
                        return;
                    }
                    else
                    {
                        if (serverBuffer.Length < uzunluk)
                            serverBuffer = new byte[uzunluk];
                        Buffer.BlockCopy(buffer, 0, serverBuffer, byteToplam, bytesRead);
                        byteToplam += bytesRead;
                    }
                }
                
                List<NetworkBuffer.HashIndisSiraDizi> diziler = nb.oku(serverBuffer, "server");
                //alınan verilere göre komut uygulanacak
                if (nb.bufferKomut() == NetworkBuffer.KURULUM)
                {

                    string aygitTurleri = new string((char[])diziler[0].dizi_);
                    string kerneller_ = new string((char[])diziler[1].dizi_);
                    string[] kernelIsimleri_ = (new string((char[])diziler[2].dizi_)).Split(" ".ToCharArray());
                    int localThreadSayisi = ((int[])diziler[3].dizi_)[0];
                    int kullanilacakGPUSayisi = ((int[])diziler[4].dizi_)[0];
                    bool GPU_STREAM = ((bool[])diziler[5].dizi_)[0];
                    int MAX_CPU = ((int[])diziler[6].dizi_)[0];
                    opencl = new Cores(
                            aygitTurleri, kerneller_,
                            kernelIsimleri_, localThreadSayisi,
                            kullanilacakGPUSayisi, GPU_STREAM,
                            MAX_CPU);
                    if(opencl.errorCode()!=0)
                    {
                        Console.WriteLine("Derleme hatası!");
                        Console.WriteLine(opencl.errorMessage());
                        opencl.dispose();
                        return;
                    }

                    NetworkBuffer nbCevap = new NetworkBuffer(NetworkBuffer.CEVAP_BASARILI);
                    byte[] bytesToSend = nbCevap.buf();
                    int pos = 0;
                    while (pos < bytesToSend.Length)
                    {
                        int l = Math.Min(bytesToSend.Length - pos, client.ReceiveBufferSize);
                        nwStream.Write(bytesToSend, pos, l);
                        pos += l;
                    }
                    Console.WriteLine("--------------------------------------------------");
                }
                else if (nb.bufferKomut() == NetworkBuffer.HESAP)
                {
                    string[] kernelAdi = new string((char[])diziler[0].dizi_).Split(" ".ToCharArray());
                    int adimSayisi = ((int[])diziler[1].dizi_)[0];
                    string adimFonksiyonu = new string((char[])diziler[2].dizi_);

                    int diziler_adet = ((int[])diziler[3].dizi_)[0];



                    string[] okuYaz = new string[diziler_adet];
                    for (int i = 0; i < diziler_adet; i++)
                        okuYaz[i] = new string((char[])diziler[diziler_adet + 4 + i].dizi_);


                    int[] enKucukElemanGrubundakiElemanSayisi = new int[diziler_adet];
                    for (int i = 0; i < diziler_adet; i++)
                        enKucukElemanGrubundakiElemanSayisi[i] = ((int[])diziler[diziler_adet * 2 + 4].dizi_)[i];

                    int toplamMenzil = ((int[])diziler[diziler_adet * 2 + 5].dizi_)[0];
                    int hesapId = ((int[])diziler[diziler_adet * 2 + 6].dizi_)[0];
                    int threadReferans = ((int[])diziler[diziler_adet * 2 + 7].dizi_)[0];
                    bool pipelineAcik = ((bool[])diziler[diziler_adet * 2 + 8].dizi_)[0];
                    int pipelineParcaSayisi_ = ((int[])diziler[diziler_adet * 2 + 9].dizi_)[0];
                    bool pipelineTuru = ((bool[])diziler[diziler_adet * 2 + 10].dizi_)[0];
                    object[] tmpDiziler = new object[diziler_adet];
                    int[] tmpHashler = new int[diziler_adet];
                    for (int o = 0; o < diziler_adet; o++)
                    {
                        tmpDiziler[o] = diziler[4 + o].dizi_;
                        tmpHashler[o] = diziler[4 + o].hash_;
                        if(!d0.ContainsKey(tmpHashler[o]))
                        {
                            d0.Add(tmpHashler[o], tmpDiziler[o]);
                            d1.Add(tmpDiziler[o], tmpHashler[o]);
                        }
                       
                    }

                    opencl.compute(kernelAdi, adimSayisi, adimFonksiyonu, tmpDiziler, okuYaz,
                        enKucukElemanGrubundakiElemanSayisi, toplamMenzil, hesapId,
                        threadReferans, pipelineAcik, pipelineParcaSayisi_, pipelineTuru);
                    opencl.performanceReport(hesapId);


                    /* aynı dizi hash gelirse onu kullanacak ama dizi sırasını da koruyacak */
                    /* zaten tek bi işlem no için bu kullanılacak, dizi sayısı sabit kalacak, boyu değişebilir */
                    /* sonuç dizileri geri gönder (sadece "yaz" işlemi olanları) */
                    // yapılacak: "true ||" kaldırılacak ama o zaman da güncelleme kısmı çalışmıyor çünkü hash farklı oluyor
                    //            /
                    //           /
                    //          /
                    //         /
                    //        |
                    //        v
                    if (true || nbHesapCevap == null)
                    {
                        nbHesapCevap = new NetworkBuffer(NetworkBuffer.CEVAP_HESAPLANDI);
                        // serverin verdiği cevap: "yaz" yani referanslı+menzilli yazma
                        for (int m = 0; m < diziler_adet; m++)
                        {
                            if (tmpDiziler[m].GetType() == typeof(float[]))
                                nbHesapCevap.ekle((float[])tmpDiziler[m], tmpHashler[m],
                                    threadReferans,toplamMenzil,enKucukElemanGrubundakiElemanSayisi[m]);
                            else if (tmpDiziler[m].GetType() == typeof(int[]))
                                nbHesapCevap.ekle((int[])tmpDiziler[m], tmpHashler[m]);
                            else if (tmpDiziler[m].GetType() == typeof(byte[]))
                                nbHesapCevap.ekle((byte[])tmpDiziler[m], tmpHashler[m]);
                            else if (tmpDiziler[m].GetType() == typeof(char[]))
                                nbHesapCevap.ekle((char[])tmpDiziler[m], tmpHashler[m]);
                            else if (tmpDiziler[m].GetType() == typeof(double[]))
                                nbHesapCevap.ekle((double[])tmpDiziler[m], tmpHashler[m]);
                            else if (tmpDiziler[m].GetType() == typeof(long[]))
                                nbHesapCevap.ekle((long[])tmpDiziler[m], tmpHashler[m]);
                        }
                    }
                    else
                    {

                        for (int m = 0; m < diziler_adet; m++)
                        {
                            if (tmpDiziler[m].GetType() == typeof(float[]))
                                nbHesapCevap.guncelle((float[])tmpDiziler[m], tmpHashler[m],
                                    threadReferans, toplamMenzil, enKucukElemanGrubundakiElemanSayisi[m]);
                            else if (tmpDiziler[m].GetType() == typeof(int[]))
                                nbHesapCevap.guncelle((int[])tmpDiziler[m], tmpHashler[m]);
                            else if (tmpDiziler[m].GetType() == typeof(byte[]))
                                nbHesapCevap.guncelle((byte[])tmpDiziler[m], tmpHashler[m]);
                            else if (tmpDiziler[m].GetType() == typeof(char[]))
                                nbHesapCevap.guncelle((char[])tmpDiziler[m], tmpHashler[m]);
                            else if (tmpDiziler[m].GetType() == typeof(double[]))
                                nbHesapCevap.guncelle((double[])tmpDiziler[m], tmpHashler[m]);
                            else if (tmpDiziler[m].GetType() == typeof(long[]))
                                nbHesapCevap.guncelle((long[])tmpDiziler[m], tmpHashler[m]);
                        }
                    }

                    byte[] bytesToSend = nbHesapCevap.buf();
                    int pos = 0;
                    while (pos < bytesToSend.Length)
                    {
                        int l = Math.Min(bytesToSend.Length - pos, NetworkBuffer.receiveSendBufferSize);
                        nwStream.Write(bytesToSend, pos, l);
                        pos += l;
                    }
                }
                else if (nb.bufferKomut() == NetworkBuffer.SIL)
                {
                    Console.WriteLine("server çekirdek api siliniyor");
                    opencl.dispose();

                    NetworkBuffer nbCevap = new NetworkBuffer(NetworkBuffer.CEVAP_SILINDI);
                    byte[] bytesToSend = nbCevap.buf();
                    int pos = 0;
                    while (pos < bytesToSend.Length)
                    {
                        Console.WriteLine("server: Segment yazılıyor");
                        int l = Math.Min(bytesToSend.Length - pos, NetworkBuffer.receiveSendBufferSize);
                        nwStream.Write(bytesToSend, pos, l);
                        pos += l;
                        Console.WriteLine("server: segment yazıldı: " + pos + "  " + bytesToSend.Length);
                    }
                    Console.WriteLine("--------------------------------------------------");
                    tmpCalisiyor = false;
                    sil();
                }
                else if (nb.bufferKomut() == NetworkBuffer.SERVER_DURDUR)
                {
                    Console.WriteLine("server durduruluyor");
                    NetworkBuffer nbCevap = new NetworkBuffer(NetworkBuffer.CEVAP_DURDURULDU);
                    byte[] bytesToSend = nbCevap.buf();
                    int pos = 0;
                    while (pos < bytesToSend.Length)
                    {
                        Console.WriteLine("server: Segment yazılıyor");
                        int l = Math.Min(bytesToSend.Length - pos, NetworkBuffer.receiveSendBufferSize);
                        nwStream.Write(bytesToSend, pos, l);
                        pos += l;
                        Console.WriteLine("server: segment yazıldı: " + pos + "  " + bytesToSend.Length);
                    }
                    Console.WriteLine("--------------------------------------------------");
                    tmpCalisiyor = false;
                    server.dur();
                }
                else if (nb.bufferKomut() == NetworkBuffer.SERVER_SINAMA )
                {
                    Console.WriteLine("server sınanıyor");
                    NetworkBuffer nbCevap = new NetworkBuffer(NetworkBuffer.CEVAP_SINAMA );
                    byte[] bytesToSend = nbCevap.buf();
                    int pos = 0;
                    while (pos < bytesToSend.Length)
                    {
                        Console.WriteLine("server: Segment yazılıyor");
                        int l = Math.Min(bytesToSend.Length - pos, NetworkBuffer.receiveSendBufferSize);
                        nwStream.Write(bytesToSend, pos, l);
                        pos += l;
                        Console.WriteLine("server: segment yazıldı: " + pos + "  " + bytesToSend.Length);
                    }
                    Console.WriteLine("--------------------------------------------------");
                }
                else if (nb.bufferKomut() == NetworkBuffer.SERVER_AYGIT_SAYISI)
                {
                    Console.WriteLine("server aygıt sayısı alınıyor");
                    NetworkBuffer nbCevap = new NetworkBuffer(NetworkBuffer.CEVAP_AYGIT_SAYISI);
                    int[] aygitSayisi_ = new int[1];
                    aygitSayisi_[0] = opencl.numberOfDevices();
                    nbCevap.ekle(aygitSayisi_,aygitSayisi_.GetHashCode());
                    byte[] bytesToSend = nbCevap.buf();
                    int pos = 0;
                    while (pos < bytesToSend.Length)
                    {
                        Console.WriteLine("server: Segment yazılıyor");
                        int l = Math.Min(bytesToSend.Length - pos, NetworkBuffer.receiveSendBufferSize);
                        nwStream.Write(bytesToSend, pos, l);
                        pos += l;
                        Console.WriteLine("server: segment yazıldı: " + pos + "  " + bytesToSend.Length);
                    }
                    Console.WriteLine("--------------------------------------------------");
                }
            }

        }

        private void cekirdekKurulum()
        {

        }

        private void cekirdekHesapla()
        {

        }

        private void cekirdekSil()
        {

        }
    }
}
