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
using System.Text;

namespace ClCluster
{
    /// <summary>
    /// prealpha cluster add-on
    /// </summary>
    public class ClusterLoadBalancer
    {
        int toplamMenzil;
        List<int> menziller;

        /// <summary>
        /// her çalışma adımında önceki adımdaki aygıt performanslarına göre aygıtların yeni
        /// yüklerini belirler.
        /// Sonradan yeni aygıt eklenebilir
        /// </summary>
        /// <param name="toplamMenzil">dengelenecek yükün tamamı</param>
        /// <param name="enKucukParcalar">her aygıt için ayrı olarak,
        /// dengeleme için eklenebilecek veya çıkartılabilecek en küçük yük miktarı
        /// Bu değer, aygıttaki(bilgisayardaki) ekran kartı + işlemci sayısının,
        /// opencl-local-thread sayıları ve pipeline parça sayıları ile çarpımlarının toplamlarına eşittir
        /// Bir bilgisayarda 3 ekran kartı varsa ve her biri 256 local thread sayısına sahipse
        /// o bilgisayarın en küçük yük adımı 768 thread olur. Pipeline optimizasyonu kullanılmışsa ve
        /// pipeline adımı sayısı 16 ise, en küçük yük adımı 768 * 16 yani 12288 thread olur.
        /// Bu şekilde tüm bilgisayarların mutlaka toplam thread sayısı kadar thread paylaşabilmeleri zorunludur
        /// En küçük adımları 512 olan iki bilgisayara toplam 1280 thread paylaştırılamaz.
        /// Veya 256 fazladan thread kullanılır ama kernel içinde bunlar kullanılmaz(verimsiz de olsa çalışmış olur)</param>
        public ClusterLoadBalancer()
        {
            
        }


        public int[] tmpMenziller;
        public double[] tmpHizlar;
        public void sonuc()
        {
            if (tmpMenziller != null)
            {
                Console.WriteLine();
                for (int i = 0; i < tmpMenziller.Length; i++)
                {
                    Console.Write(" " + tmpMenziller[i] + " ");

                }
                Console.WriteLine();
                Console.WriteLine(tmpMenziller.Sum());
                Console.WriteLine();
            }
        }

        public int obeb(int a, int b)
        {
            int ret = 0;
            if (a == b)
                return a;

            if (a == 0)
            {
                if(b!=0)
                    return b;
                return 64;
            }

            if (b == 0)
            {
                if (a != 0)
                    return a;
                return 64;
            }

            if (a < b)
            {
                a ^= b;
                b ^= a;
                a ^= b;
            }
            int bolum = a / b;
            int kalan = a - bolum * b;
            if (kalan == 0)
            {
                ret = b;
            }
            else
            {
                while (kalan > 0)
                {
                    a = b;
                    b = kalan;

                    bolum = a / b;
                    kalan = a - bolum * b;
                    ret = b;
                    b = a;
                }

            }
            return ret;
        }

        public int okek(int[] data)
        {
            int ret = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                int a = ret;
                int b = data[i];
                if (a < b)
                {
                    a ^= b;
                    b ^= a;
                    a ^= b;
                }
                ret = (a / obeb(a, b)) * b;


            }

            return ret;
        }

        // ilk hesaplama adımında tüm bilgisayarlara eşit veya ona yakın yük dağıtılır
        public int dengeleEsit(int toplamMenzil, int[] menziller, int[] adim)
        {
            int toplam = 0;
            int sayac = 0;
            if (tmpMenziller == null)
                tmpMenziller = new int[menziller.Length];
            for (int i = 0; i < menziller.Length; i++)
                tmpMenziller[i] = 0;
            bool cik = false;
            int seviye = 0;
            int okek_ = okek(adim);
            int okekSayisi = toplamMenzil / okek_; // toplam menzilde kaç adet okek var?
            int okekKatmanlari = okekSayisi / menziller.Length; // her pc için eşit paylaştırılan okek thread sayısı
            if (okekKatmanlari == 0)
            {
                Console.WriteLine("thread sayısı, tüm bilgisayarların kullanımı için yetersiz");
                for (int i = 0; i < menziller.Length; i++)
                {
                    menziller[i] = adim[i];

                }
                return toplamMenzil - menziller.Sum();
            }


            for (int i = 0; i < menziller.Length; i++)
            {
                tmpMenziller[i] = okekKatmanlari * okek_; // her pc için en az bu kadar paylaştırılabildi
            }
            int kalanThreadler = toplamMenzil - okekKatmanlari * okek_ * menziller.Length;
            int kalanOkekler = kalanThreadler / okek_;
            if (kalanOkekler == 0)
            {
                // sadece ana bilgisayarın kullanabileceği sayıda thread kalmış
                if (kalanThreadler % 64 != 0)
                    return -1;
                else
                {
                    for (int i = 0; i < menziller.Length; i++)
                        menziller[i] = tmpMenziller[i];
                    return kalanThreadler; // bu da 64 ün katı olmalıdır
                }
            }
            else
            {
                // okekler ilk n adet pc için ayrılıyor.
                for (int i = 0; i < kalanOkekler; i++)
                {
                    tmpMenziller[i] += okek_; // her pc için en az bu kadar paylaştırılabildi
                }
                kalanThreadler -= kalanOkekler * okek_;
                for (int i = 0; i < menziller.Length; i++)
                    menziller[i] = tmpMenziller[i];
                return kalanThreadler;
            }


            return -1;
            Console.WriteLine("sayaç=" + sayac);
        }


        /// <summary>
        /// önceki hesaplama adımındaki thread sayılarına ve benchmark(tamamlama süreleri) 
        /// değerlerine göre dağılım değiştirilir
        /// bir pc için,
        /// t=önceki thread sayısı (normalize edilmiş)
        /// p=önceki performans değeri (normalize edilmiş)
        /// yt= yeni thread sayısı (normalize)
        /// k = frenleme katsayısı (salınımı engellemek için, 0.3)
        /// yt = t +  k * (p - t)
        /// örnek:
        /// t=0.1(tüm threadlerin %10 u),    p=0.5(toplam performansların %50 si)
        /// yt= 0.1 + 0.3(0.5-0.1) = 0.22 (tüm threadlerin %22 si)
        /// sonraki adımda %29 sonra 35 ... %50

        /// gerekli ön hesaplamalar:
        /// p(i)=pe(i)/(toplam(pe))
        /// pe(i)= (thread(i)/süre(i))

        /// döndürülen değer = küsürat thread sayısı (toplam thread sayısına tamamlar, 64'ün katıdır)
        ///                    ana bilgisayarda 1 adet gpu (veya cpu) tarafından hesaplanılır

        /// </summary>
        /// <param name="sureler">milisaniye, saniye</param>
        /// <param name="toplamMenzil">256,16k, 1M, ... bilgisayarın en küçük thread adımının tam katı</param>
        /// <param name="menziller"></param>
        /// <param name="adim"></param>
        /// <param name="anaBilgisayarThread"></param>
        /// <returns></returns>
        public int dengelePerformansaGore(
            double[] sureler, int toplamMenzil,
            int[] menziller, int[] adim,
            int anaBilgisayarThread,
            double anaBilgisayarSure)
        {
            double[] normalizePerformanslar = new double[sureler.Length];
            double[] normalizeThreadler = new double[sureler.Length];
            double toplamPerformans = 0;
            for (int i = 0; i < menziller.Length; i++)
            {
                if (Math.Abs(sureler[i]) < 0.0001)
                    sureler[i] = 0.0001;
                toplamPerformans += (normalizePerformanslar[i] = ((double)menziller[i]) / sureler[i]);
            }

            if (Math.Abs(toplamPerformans) < 0.001)
                toplamPerformans = 0.001;

            if ((toplamMenzil - anaBilgisayarThread) == 0)
                anaBilgisayarThread = 0;

                // performanslar ve threadler normalize ediliyor
                for (int i = 0; i < menziller.Length; i++)
            {
                normalizePerformanslar[i] /= toplamPerformans;
                normalizeThreadler[i] = ((double)menziller[i]) / (double)(toplamMenzil - anaBilgisayarThread);
            }

            int toplamThreadler = 0;
            // bilgisaarların yeni thread sayıları belirleniyor
            for (int i = 0; i < menziller.Length; i++)
            {
                double tmp = (normalizeThreadler[i] + 0.3 * (normalizePerformanslar[i] - normalizeThreadler[i]));
                int enYakimAdiminKati = enYakinBul(tmp, adim[i], toplamMenzil);
                toplamThreadler += enYakimAdiminKati;
                menziller[i] = enYakimAdiminKati;

            }

            if (toplamThreadler <= toplamMenzil)
            {
                return toplamMenzil - toplamThreadler;
            }
            else
            {
                int eksiltilecek = toplamThreadler - toplamMenzil;
                // en fazla  menzil/eksiltilecek oranına sahip olan menzil eksiltilerek
                // dengelenecek. Yoksa zamanlama fazla değişebilir.

                // en fazla menzil/eksiltilecek oranına sahip olanların arasında
                // en küçük adım değeri eksiltilecek değere yakın olan seçilecek
                // eksik kalan kısım ana bilgisayar tarafından tamamlanacak

                // eksiltme sonucunda son menzil değeri sıfır olmamalı
                for (int i = 0; i < menziller.Length; i++)
                {
                    int kat = eksiltilecek / adim[i];
                    if (kat == 0)
                        kat++;
                    int azaltılan = adim[i] * kat;
                    int fark = eksiltilecek - azaltılan;
                    if (fark > 0)
                    {
                        kat++;
                        azaltılan = adim[i] * kat;
                        fark = eksiltilecek - azaltılan;
                    }
                    if (menziller[i] - azaltılan > 0)
                    {
                        menziller[i] -= azaltılan;

                        if (fark == 0)
                        {
                            // ana bilgisayara gerek yok
                            // paylaşım işlemi tam yapıldı
                            return 0;
                        }
                        else if (fark < 0)
                        {
                            return -fark;
                        }

                        break;
                    }
                }
            }

            return -1;
        }

        // sıfır döndürmemelidir
        int enYakinBul(double d, int a, int m)
        {
            // d 'nin gösterdiği 0-1 arasındaki bir noktaya en yakın a'nın katı olan yer(0-m arası)
            int r = 0;
            int tmp = (int)(d * ((double)m));

            // adımın yarısına kadarki yerler aşağı yuvarlanır, diğerleri yukarı

            // seçilen thread sayısı, adım değerinin en az kaç katı
            int carpan = tmp / a;

            int kalan = tmp - carpan * a;

            if (kalan >= a / 2)
            {
                carpan++;
            }

            if (carpan == 0)
            {
                carpan++;
            }

            return carpan * a;
        }

        static int indisEnBuyuk(int[] dizi)
        {
            int sonuc = 0;
            int deger = -100000000;
            for (int i = 0; i < dizi.Length; i++)
            {
                if (deger < dizi[i])
                {
                    deger = dizi[i];
                    sonuc = i;
                }
            }
            return sonuc;
        }
        static int indisEnKucuk(int[] dizi)
        {
            int sonuc = 0;
            double deger = 1800000000;
            for (int i = 0; i < dizi.Length; i++)
            {
                if (deger > dizi[i])
                {
                    deger = dizi[i];
                    sonuc = i;
                }
            }
            return sonuc;
        }
    }
}
