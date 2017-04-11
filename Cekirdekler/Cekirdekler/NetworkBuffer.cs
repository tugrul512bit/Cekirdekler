using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClCluster
{
    /// <summary>
    /// prealpha cluster add-on
    /// </summary>
    class NetworkBuffer
    {

        // buffer
        // çekirdek api header     endianness   buffer byte sayısı
        // Cekirdek                   e              b                    nesne  nesne nesne ...

        // nesne  = e h s d r m
        // e=nesne eleman türü (1 byte) (0=byte, 1=char, 2=int, 3=float, 4=long, 5=double, 6=bool(byte))
        // h=nesne hashcode, çekirdek apideki otomatik buffer takibi için(1 int -->4 byte)
        // s=nesnedeki eleman sayısı (1 int-->4 byte) --> dizinin uzunluğu
        // r=referans --> nesnenin yazma başlangıcı (networkten gelen kısmın yazılacağı yerin başlangıcı)
        // m=menzil --> nesnenin referansından itibaren yazılacak eleman sayısı
        // d=data (x byte)
        // string için 5 eleman = 10 byte
        // bool için 15 eleman = 15 byte 

        // oku() işlemlerinde eğer aynı hashcode varsa dictionary ile alınacak yoksa yaratılıp eklenecek

        private List<byte> buffer;
        private Dictionary<int, object> data;  // client hashcode --> nesne
        private Dictionary<int, int> data2; // hash --> buffer indisi(dizinin ilk elemanı) : 
        private Dictionary<object, int> data3;//nesne ----> hash : hashcode sabit kalsın diye
        public static int receiveSendBufferSize = 1024*8;
        private int elemanSayisi;
        public NetworkBuffer(int komut = -1)
        {
           
            data3 = new Dictionary<object, int>();
            data2 = new Dictionary<int, int>();
            data = new Dictionary<int, object>();
            buffer = new List<byte>();
            elemanSayisi = 0;
            buffer.Add(System.Text.Encoding.ASCII.GetBytes("C")[0]);
            buffer.Add(System.Text.Encoding.ASCII.GetBytes("e")[0]);
            buffer.Add(System.Text.Encoding.ASCII.GetBytes("k")[0]);
            buffer.Add(System.Text.Encoding.ASCII.GetBytes("i")[0]);
            buffer.Add(System.Text.Encoding.ASCII.GetBytes("r")[0]);
            buffer.Add(System.Text.Encoding.ASCII.GetBytes("d")[0]);
            buffer.Add(System.Text.Encoding.ASCII.GetBytes("e")[0]);
            buffer.Add(System.Text.Encoding.ASCII.GetBytes("k")[0]);


            buffer.Add(BitConverter.IsLittleEndian ? (byte)0 : (byte)1);
            intEkle(0);
            intEkle(komut);
            KOMUT = komut;
        }


        public void resetExceptArrays()
        {
            buffer.Clear();
            elemanSayisi = 0;
        }

        public void dizileriAl(Dictionary<int,object> d0, Dictionary<object,int> d1)
        {

            var l = d0.ToList();
            for (int i = 0; i < d0.Count; i++)
            {
                if (!data.ContainsKey(l[i].Key))
                {
                    Console.WriteLine("Dizi eklendi.");
                    data.Add(l[i].Key, l[i].Value);
                    data3.Add(l[i].Value, l[i].Key);
                }

            }
        }


        public int elemanSay()
        {
            return elemanSayisi;
        }

        private int KOMUT;
        public int bufferCommand() { return KOMUT; }
        public const int SETUP = 0;
        public const int COMPUTE = 1;
        public const int DISPOSE = 2;

        // kurulum başarılı
        public const int ANSWER_SUCCESS = 3;


        public const int ANSWER_DELETED = 4;
        public const int ANSWER_COMPUTE_COMPLETE = 5;
        public const int SERVER_STOP = 6;

        // server dinleme işlemi durduruldu
        public const int ANSWER_STOPPED = 7;
        public const int SERVER_CONTROL = 8;
        public const int ANSWER_CONTROL = 9;
        public const int SERVER_NUMBER_OF_DEVICES = 10;
        public const int ANSWER_NUMBER_OF_DEVICES = 11;
        public class HashIndisSiraDizi
        {
            public int hash_; // dizilerin hash kodu(client tarafında)
            public int indis_;// dizilerin serialize edilmiş nesnedeki byte adresi
            public int sira_; // dizilerin api için kullanım sırası
            public object backingArray;
        }
        /// <summary>
        /// gelen buffer bilgisinden Dictionary üretir, dizileri sırasıyla list halinde döndürür
        /// </summary>
        public List<HashIndisSiraDizi> oku(byte[] b, string serverClient, object[] okunacakDiziler = null)
        {
            List<HashIndisSiraDizi> liste = new List<HashIndisSiraDizi>();
            if (System.Text.Encoding.ASCII.GetString(b, 0, 8).Equals("Cekirdek"))
            {
                bool endiannessAyniMi = (b[8] == Convert.ToByte(!BitConverter.IsLittleEndian));
                int toplamByte = readLengthOfBuffer(b);
                if (bBuf == null || bBuf.Length < toplamByte)
                    bBuf = new byte[toplamByte];
                Buffer.BlockCopy(b, 0, bBuf, 0, toplamByte);
                KOMUT = komutOku(b);
                int indis = 17;
                //dizileri sırayla oku, dictionary ve list e ekle
                int sira = 0;
                while (indis < toplamByte)
                {
                    int turu = bBuf[indis];
                    indis++;
                    int hash = intOku(bBuf, indis, endiannessAyniMi);
                    indis += 4;
                    int uzunlugu = intOku(bBuf, indis, endiannessAyniMi);
                    indis += 4;
                    int referans = 0;
                    int menzil = -1;
                    if (turu == 3) {
                        referans = intOku(bBuf, indis, endiannessAyniMi);
                        indis += 4;
                        menzil = intOku(bBuf, indis, endiannessAyniMi);
                        indis += 4;
                    }
                    
                    liste.Add(new HashIndisSiraDizi() {
                        hash_ = hash,
                        indis_ = indis,
                        sira_ = sira,
                        backingArray = nesneOku(
                            bBuf, indis, 
                            endiannessAyniMi, turu, 
                            hash, uzunlugu, 
                            okunacakDiziler == null ? null : okunacakDiziler[sira],
                            referans,menzil) });
                    sira++;
                    if (turu == 0) indis += uzunlugu;
                    if (turu == 1) indis += uzunlugu * 2;
                    if (turu == 2) indis += uzunlugu * 4;
                    if (turu == 3) indis += (menzil==-1?uzunlugu * 4: menzil*4);
                    if (turu == 4) indis += uzunlugu * 8;
                    if (turu == 5) indis += uzunlugu * 8;
                    if (turu == 6) indis += uzunlugu;
                }
                elemanSayisi = indis;
            }
            else
            {
                Console.WriteLine(serverClient + ": Hata! Buffer, Çekirdek apiye uygun değil. ---->>" + System.Text.Encoding.ASCII.GetString(b, 0, 8) + "<<---------");
            }
            return liste;
        }

        public int readLengthOfBuffer(byte[] b)
        {
            if (System.Text.Encoding.ASCII.GetString(b, 0, 8).Equals("Cekirdek"))
            {
                bool endiannessAyniMi = (b[8] == Convert.ToByte(!BitConverter.IsLittleEndian));
                return intOku(b, 9, endiannessAyniMi);
            }
            else
            {
                Console.WriteLine("Hata! Buffer, Çekirdek apiye uygun değil. ----->" + System.Text.Encoding.ASCII.GetString(b, 0, 8) + "<----");
                return 0;
            }
        }

        public int komutOku(byte[] b)
        {
            if (System.Text.Encoding.ASCII.GetString(b, 0, 8).Equals("Cekirdek"))
            {
                bool endiannessAyniMi = (b[8] == Convert.ToByte(!BitConverter.IsLittleEndian));
                return intOku(b, 13, endiannessAyniMi);
            }
            else
            {
                Console.WriteLine("Hata! Buffer, Çekirdek apiye uygun değil. ----->>>" + System.Text.Encoding.ASCII.GetString(b, 0, 8) + "<----");
                return 0;
            }
        }

        private int intOku(byte[] b, int adres, bool endiannessAyniMi = true)
        {
            if (endiannessAyniMi)
                return BitConverter.ToInt32(b, adres);
            else
            {
                byte[] tmp = new byte[4];
                tmp[0] = b[adres + 3];
                tmp[1] = b[adres + 2];
                tmp[2] = b[adres + 1];
                tmp[3] = b[adres];
                return BitConverter.ToInt32(tmp, 0);
            }
        }



        // buferdan dizi okur
        // yapılacak: dizilerin (nesnelerin) header kısmına padding byte sayısı eklenecek
        // dizi kopyalama / güncelleme hızını etkileyecek
        private object nesneOku(
            byte[] b, int indis, 
            bool endiannessAyniMi, int turu, 
            int hash, int uzunlugu, object okunacakDizi,
            int referans,int menzil)
        {
            // dizi önceden yaratılmışsa, 
            // uzunluğu aynıysa,
            // güncellenecek ve döndürecek

            if (data.ContainsKey(hash))
            {
                // güncelle
                // tek farkı byte[] tmp = new byte[uzunlugu]; yerine tmp=data[hash] olacak
                if (turu == 0)
                {
                    // byte dizisi
                    byte[] tmp = okunacakDizi != null ? (byte[])okunacakDizi : (byte[])data[hash];
                    Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length);
                }
                else if (turu == 1)
                {
                    // char dizisi
                    if (endiannessAyniMi)
                    {
                        char[] tmp = okunacakDizi != null ? (char[])okunacakDizi : (char[])data[hash];

                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 2);
                    }
                    else
                    {
                        char[] tmp = okunacakDizi != null ? (char[])okunacakDizi : (char[])data[hash];
                        for (int i = indis; i < indis + tmp.Length * 2; i += 2)
                        {
                            byte c = b[i];
                            b[i] = b[i + 1];
                            b[i + 1] = c;
                        }
                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 2);
                    }
                }
                else if (turu == 2)
                {
                    // int dizisi
                    if (endiannessAyniMi)
                    {
                        int[] tmp = okunacakDizi != null ? (int[])okunacakDizi : (int[])data[hash];

                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 4);
                    }
                    else
                    {
                        int[] tmp = okunacakDizi != null ? (int[])okunacakDizi : (int[])data[hash];
                        for (int i = indis; i < indis + tmp.Length * 4; i += 4)
                        {
                            byte c = b[i];
                            b[i] = b[i + 3];
                            b[i + 3] = c;
                            c = b[i + 1];
                            b[i + 1] = b[i + 2];
                            b[i + 2] = c;
                        }
                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 4);
                    }
                }
                else if (turu == 3)
                {
                    // float dizisi
                    if (endiannessAyniMi)
                    {
                        float[] tmp = okunacakDizi != null ? (float[])okunacakDizi : (float[])data[hash];

                        Buffer.BlockCopy(b, indis, tmp, referans*4, (menzil<0?tmp.Length:menzil) * 4);
                    }
                    else
                    {
                        float[] tmp = okunacakDizi != null ? (float[])okunacakDizi : (float[])data[hash];
                        for (int i = indis; i < indis + tmp.Length * 4; i += 4)
                        {
                            byte c = b[i];
                            b[i] = b[i + 3];
                            b[i + 3] = c;
                            c = b[i + 1];
                            b[i + 1] = b[i + 2];
                            b[i + 2] = c;
                        }
                        Buffer.BlockCopy(b, indis, tmp, referans*4, (menzil<0? tmp.Length:menzil) * 4);
                    }
                }
                else if (turu == 4)
                {
                    // long dizisi
                    if (endiannessAyniMi)
                    {
                        long[] tmp = okunacakDizi != null ? (long[])okunacakDizi : (long[])data[hash];

                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 8);
                    }
                    else
                    {
                        long[] tmp = okunacakDizi != null ? (long[])okunacakDizi : (long[])data[hash];
                        for (int i = indis; i < indis + tmp.Length * 8; i += 8)
                        {
                            byte c = b[i];
                            b[i] = b[i + 7];
                            b[i + 7] = c;

                            c = b[i + 1];
                            b[i + 1] = b[i + 6];
                            b[i + 6] = c;

                            c = b[i + 2];
                            b[i + 2] = b[i + 5];
                            b[i + 5] = c;

                            c = b[i + 3];
                            b[i + 3] = b[i + 4];
                            b[i + 4] = c;
                        }
                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 8);
                    }
                }
                else if (turu == 5)
                {
                    // double dizisi
                    if (endiannessAyniMi)
                    {
                        double[] tmp = okunacakDizi != null ? (double[])okunacakDizi : (double[])data[hash];
                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 8);
                    }
                    else
                    {
                        double[] tmp = okunacakDizi != null ? (double[])okunacakDizi : (double[])data[hash];
                        for (int i = indis; i < indis + tmp.Length * 8; i += 8)
                        {
                            byte c = b[i];
                            b[i] = b[i + 7];
                            b[i + 7] = c;

                            c = b[i + 1];
                            b[i + 1] = b[i + 6];
                            b[i + 6] = c;

                            c = b[i + 2];
                            b[i + 2] = b[i + 5];
                            b[i + 5] = c;

                            c = b[i + 3];
                            b[i + 3] = b[i + 4];
                            b[i + 4] = c;
                        }
                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 8);
                    }
                }
                else if (turu == 6)
                {
                    // bool dizisi
                    bool[] tmp = okunacakDizi != null ? (bool[])okunacakDizi : (bool[])data[hash];

                    Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length);
                }
                return data[hash];
            }
            else
            {
                // diziyi oku, oluştur, ekle
                if (turu == 0)
                {
                    // byte dizisi
                    byte[] tmp = okunacakDizi != null ? (byte[])okunacakDizi : new byte[uzunlugu];
                    data3.Add(tmp, hash);
                    data.Add(hash, tmp);
                    data2.Add(hash, indis);
                    Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length);
                }
                else if (turu == 1)
                {
                    // char dizisi
                    if (endiannessAyniMi)
                    {
                        char[] tmp = okunacakDizi != null ? (char[])okunacakDizi : new char[uzunlugu];
                        data3.Add(tmp, hash);
                        data.Add(hash, tmp);
                        data2.Add(hash, indis);
                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 2);
                    }
                    else
                    {
                        char[] tmp = okunacakDizi != null ? (char[])okunacakDizi : new char[uzunlugu];
                        data3.Add(tmp, hash);
                        data.Add(hash, tmp);
                        data2.Add(hash, indis);
                        for (int i = indis; i < indis + tmp.Length * 2; i += 2)
                        {
                            byte c = b[i];
                            b[i] = b[i + 1];
                            b[i + 1] = c;
                        }
                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 2);
                    }
                }
                else if (turu == 2)
                {
                    // int dizisi
                    if (endiannessAyniMi)
                    {
                        int[] tmp = okunacakDizi != null ? (int[])okunacakDizi : new int[uzunlugu];
                        data3.Add(tmp, hash);
                        data.Add(hash, tmp);
                        data2.Add(hash, indis);
                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 4);
                    }
                    else
                    {
                        int[] tmp = okunacakDizi != null ? (int[])okunacakDizi : new int[uzunlugu];
                        data3.Add(tmp, hash);
                        data.Add(hash, tmp);
                        data2.Add(hash, indis);
                        for (int i = indis; i < indis + tmp.Length * 4; i += 4)
                        {
                            byte c = b[i];
                            b[i] = b[i + 3];
                            b[i + 3] = c;
                            c = b[i + 1];
                            b[i + 1] = b[i + 2];
                            b[i + 2] = c;
                        }
                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 4);
                    }
                }
                else if (turu == 3)
                {
                    // float dizisi
                    if (endiannessAyniMi)
                    {
                        float[] tmp = okunacakDizi != null ? (float[])okunacakDizi : new float[uzunlugu];
                        data3.Add(tmp, hash);
                        data.Add(hash, tmp);
                        data2.Add(hash, indis);
                        Buffer.BlockCopy(b, indis, tmp, referans*4, (menzil<0? tmp.Length:menzil) * 4);
                    }
                    else
                    {
                        float[] tmp = okunacakDizi != null ? (float[])okunacakDizi : new float[uzunlugu];
                        data3.Add(tmp, hash);
                        data.Add(hash, tmp);
                        data2.Add(hash, indis);
                        for (int i = indis; i < indis + tmp.Length * 4; i += 4)
                        {
                            byte c = b[i];
                            b[i] = b[i + 3];
                            b[i + 3] = c;
                            c = b[i + 1];
                            b[i + 1] = b[i + 2];
                            b[i + 2] = c;
                        }
                        Buffer.BlockCopy(b, indis, tmp, referans*4,(menzil<0? tmp.Length:menzil) * 4);
                    }
                }
                else if (turu == 4)
                {
                    // long dizisi
                    if (endiannessAyniMi)
                    {
                        long[] tmp = okunacakDizi != null ? (long[])okunacakDizi : new long[uzunlugu];
                        data3.Add(tmp, hash);
                        data.Add(hash, tmp);
                        data2.Add(hash, indis);
                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 8);
                    }
                    else
                    {
                        long[] tmp = okunacakDizi != null ? (long[])okunacakDizi : new long[uzunlugu];
                        data3.Add(tmp, hash);
                        data.Add(hash, tmp);
                        data2.Add(hash, indis);
                        for (int i = indis; i < indis + tmp.Length * 8; i += 8)
                        {
                            byte c = b[i];
                            b[i] = b[i + 7];
                            b[i + 7] = c;

                            c = b[i + 1];
                            b[i + 1] = b[i + 6];
                            b[i + 6] = c;

                            c = b[i + 2];
                            b[i + 2] = b[i + 5];
                            b[i + 5] = c;

                            c = b[i + 3];
                            b[i + 3] = b[i + 4];
                            b[i + 4] = c;
                        }
                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 8);
                    }
                }
                else if (turu == 5)
                {
                    // double dizisi
                    if (endiannessAyniMi)
                    {
                        double[] tmp = okunacakDizi != null ? (double[])okunacakDizi : new double[uzunlugu];
                        data3.Add(tmp, hash);
                        data.Add(hash, tmp);
                        data2.Add(hash, indis);
                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 8);
                    }
                    else
                    {
                        double[] tmp = okunacakDizi != null ? (double[])okunacakDizi : new double[uzunlugu];
                        data3.Add(tmp, hash);
                        data.Add(hash, tmp);
                        data2.Add(hash, indis);
                        for (int i = indis; i < indis + tmp.Length * 8; i += 8)
                        {
                            byte c = b[i];
                            b[i] = b[i + 7];
                            b[i + 7] = c;

                            c = b[i + 1];
                            b[i + 1] = b[i + 6];
                            b[i + 6] = c;

                            c = b[i + 2];
                            b[i + 2] = b[i + 5];
                            b[i + 5] = c;

                            c = b[i + 3];
                            b[i + 3] = b[i + 4];
                            b[i + 4] = c;
                        }
                        Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length * 8);
                    }
                }
                else if (turu == 6)
                {
                    // bool dizisi
                    bool[] tmp = okunacakDizi != null ? (bool[])okunacakDizi : new bool[uzunlugu];
                    data3.Add(tmp, hash);
                    data.Add(hash, tmp);
                    data2.Add(hash, indis);
                    Buffer.BlockCopy(b, indis, tmp, 0, tmp.Length);
                }
                return data[hash];
            }
        }

        private byte[] bBuf;

        public void komutBelirle(int k)
        {
            KOMUT = k;
            Console.WriteLine("KOMUT = k; --->" + KOMUT);
        }

        public byte[] buf()
        {

            if (bBuf == null || buffer.Count != bBuf.Length)
            {
                intYaz(buffer.Count, 9);
                intYaz(KOMUT, 13);
                bBuf = buffer.ToArray();
            }
            else
            {
                intGuncelle(KOMUT, 13);
            }
            return bBuf;
        }

        private void intEkle(int deger)
        {
            byte[] lb = BitConverter.GetBytes(deger);
            for (int j = 0; j < lb.Length; j++)
            {
                buffer.Add(lb[j]);
            }
        }

        private void intGuncelle(int deger, int indis)
        {
            byte[] lb = BitConverter.GetBytes(deger);
            for (int j = 0; j < lb.Length; j++)
            {
                bBuf[j + indis] = lb[j];
            }
        }

        private void intYaz(int deger, int indis)
        {
            byte[] lb = BitConverter.GetBytes(deger);
            for (int j = 0; j < lb.Length; j++)
            {
                buffer[indis + j] = lb[j];
            }
        }



        public void addArray(byte[] l, int hash, int byteSayisi = 0,
                    int threadReferans_ = 0, int toplamMenzil_ = -1,
                    int enKucukElemanGrubundakiElemanSayisi = 1)
        {
            buffer.Add(0);
            if (byteSayisi == 0)
                byteSayisi = l.Length;
            if (!data2.ContainsKey(hash))
            {
                data3.Add(l, hash);
                data2.Add(hash, buffer.Count + 8);
                data.Add(hash, l);
            }
            intEkle(l.GetHashCode());
            intEkle(byteSayisi);
            byte[] tmp = new byte[byteSayisi];
            Buffer.BlockCopy(l, 0, tmp, 0, byteSayisi);
            buffer.AddRange(tmp);
        }


        public void update(byte[] l, int hash)
        {

            int indis = data2[hash];
            Buffer.BlockCopy(l, 0, bBuf, indis, l.Length);
        }



        public void update(char[] l, int hash)
        {
            int indis = data2[hash];
            Buffer.BlockCopy(l, 0, bBuf, indis, l.Length * 2);
        }

        public void update(int[] l, int hash)
        {
            int indis = data2[hash];
            Buffer.BlockCopy(l, 0, bBuf, indis, l.Length * 4);
        }
        object oKilit = new object();
        public void update(float[] l, int hash,
                             int  threadReferans=0, int toplamMenzil=-1, 
                             int enKucukElemanGrubundakiElemanSayisi=1)
        {
            int indis = data2[hash];
            if (toplamMenzil == -1)
            {
                // tümü yazılıyor
                Buffer.BlockCopy(l, 0, bBuf, indis, l.Length * 4);
            }
            else
            {
                // bilgisayarın kendi payına düşeni yazılıyor
                lock (oKilit)
                {
                    

                    Buffer.BlockCopy(l, threadReferans * enKucukElemanGrubundakiElemanSayisi * 4,
                        bBuf, indis, toplamMenzil * enKucukElemanGrubundakiElemanSayisi * 4);
                }
            }
        }

        public void update(long[] l, int hash)
        {
            int indis = data2[hash];
            Buffer.BlockCopy(l, 0, bBuf, indis, l.Length * 8);
        }

        public void update(double[] l, int hash)
        {
            int indis = data2[hash];
            Buffer.BlockCopy(l, 0, bBuf, indis, l.Length * 8);
        }

        public void guncelle(bool[] l, int hash)
        {
            int indis = data2[hash];
            Buffer.BlockCopy(l, 0, bBuf, indis, l.Length);
        }

        public void addCompute(char[] l, int hash,
                    int threadReferans_ = 0, int toplamMenzil_ = -1,
                    int enKucukElemanGrubundakiElemanSayisi = 1)
        {
            buffer.Add(1);
            if (!data2.ContainsKey(hash))
            {
                data3.Add(l, hash);
                data2.Add(hash, buffer.Count + 8);
                data.Add(hash, l);
            }
            intEkle(l.GetHashCode());
            intEkle(l.Length);
            byte[] tmp = new byte[l.Length * 2];
            Buffer.BlockCopy(l, 0, tmp, 0, tmp.Length);
            buffer.AddRange(tmp);
        }

        public void addComputeSteps(int[] l, int hash,
                    int threadReferans_ = 0, int toplamMenzil_ = -1,
                    int enKucukElemanGrubundakiElemanSayisi = 1)
        {
            buffer.Add(2);
            if (!data2.ContainsKey(hash))
            {
                data3.Add(l, hash);
                data2.Add(hash, buffer.Count + 8);
                data.Add(hash, l);
            }
            intEkle(l.GetHashCode());
            intEkle(l.Length);
            byte[] tmp = new byte[l.Length * 4];
            Buffer.BlockCopy(l, 0, tmp, 0, tmp.Length);
            buffer.AddRange(tmp);
        }

        public void addArray(float[] l, int hash,
                    int threadReferans_=0, int toplamMenzil_=-1, 
                    int enKucukElemanGrubundakiElemanSayisi=1)
        {
            int kopyalanacakElemanlar = enKucukElemanGrubundakiElemanSayisi * toplamMenzil_;
            int kopyaReferans = threadReferans_ * enKucukElemanGrubundakiElemanSayisi;
            buffer.Add(3);
            if (!data2.ContainsKey(hash))
            {
                data3.Add(l, hash);
                data2.Add(hash, buffer.Count + 16); // ? 12 16 olabilir. 
                                                   // Referans + menzil  yüzünden
                data.Add(hash, l);
            }
            intEkle(l.GetHashCode());
            intEkle(l.Length); // öbür tarafta oluşturulacak dizi uzunluğu olacak

            intEkle(kopyaReferans); // dizinin neresinden itibaren yazılacağı
            intEkle(kopyalanacakElemanlar); // dizinin ne kadarının yazılacağı
                                            // negatifse, l.Length kadar okunacak

            // karşı taraftan okunurken bu dikkate alınacak
            byte[] tmp = toplamMenzil_==-1?new byte[l.Length * 4]: new byte[kopyalanacakElemanlar * 4] ;
   
            Buffer.BlockCopy(l, kopyaReferans*4, tmp, 0, tmp.Length);
            buffer.AddRange(tmp);

        }

        public void addArray(long[] l, int hash,
                    int threadReferans_ = 0, int toplamMenzil_ = -1,
                    int enKucukElemanGrubundakiElemanSayisi = 1)
        {
            buffer.Add(4);
            if (!data2.ContainsKey(hash))
            {
                data3.Add(l, hash);
                data2.Add(hash, buffer.Count + 8);
                data.Add(hash, l);
            }
            intEkle(l.GetHashCode());
            intEkle(l.Length);
            byte[] tmp = new byte[l.Length * 8];
            Buffer.BlockCopy(l, 0, tmp, 0, tmp.Length);
            buffer.AddRange(tmp);
        }

        public void addArray(double[] l, int hash,
                    int threadReferans_ = 0, int toplamMenzil_ = -1,
                    int enKucukElemanGrubundakiElemanSayisi = 1)
        {
            buffer.Add(5);
            if (!data2.ContainsKey(hash))
            {
                data3.Add(l, hash);
                data2.Add(hash, buffer.Count + 8);
                data.Add(hash, l);
            }
            intEkle(l.GetHashCode());
            intEkle(l.Length);
            byte[] tmp = new byte[l.Length * 8];
            Buffer.BlockCopy(l, 0, tmp, 0, tmp.Length);
            buffer.AddRange(tmp);
        }

        public void addPipeline(bool[] l, int hash,
                    int threadReferans_ = 0, int toplamMenzil_ = -1,
                    int enKucukElemanGrubundakiElemanSayisi = 1)
        {
            buffer.Add(6);
            if (!data2.ContainsKey(hash))
            {
                data3.Add(l, hash);
                data2.Add(hash, buffer.Count + 8);
                data.Add(hash, l);
            }
            intEkle(l.GetHashCode());
            intEkle(l.Length);
            byte[] tmp = new byte[l.Length];
            Buffer.BlockCopy(l, 0, tmp, 0, tmp.Length);
            buffer.AddRange(tmp);
        }
    }
}
