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

namespace Cekirdekler
{
    /// <summary>
    /// <para>prealpha cluster add-on</para>
    /// <para>hesaplayacak nesne, Cekirdekler api de olabilir,</para> 
    /// <para>bir pc(ip tcp) grubunda çalışan Hizlandirici api de(içinde Cekirdekler var) olabilir</para> 
    /// </summary>
    interface IHesapNode
    {
        // kurulumun verilen parametrelere göre yapılışı
        void kur(string aygitTurleri, string kerneller_,
                        string[] kernelIsimleri_, int localThreadSayisi = 256,
                        int kullanilacakGPUSayisi = -1, bool GPU_STREAM = true,
                        int MAX_CPU = -1);

        /// <summary>
        /// parametreli hesap
        /// </summary>
        /// <returns></returns>
        void hesapla(string[] kernelAdi___ = null,
            int adimSayisi_ = 0, string adimFonksiyonu = "",
            object[] diziler_ = null, string[] oku_yaz = null,
            int[] enKucukElemanGrubundakiElemanSayisi = null,
            int toplamMenzil_ = 1024, int hesapId_ = 1,
            int threadReferans_ = 0, bool pipelineAcik_ = false,
            int pipelineParcaSayisi__ = 4, bool pipelineTuru_ = Cores.PIPELINE_EVENT);

        /// <summary>
        /// upload + hesap + download süresi
        /// </summary>
        /// <returns>milisaniye değeri</returns>
        double hesapSuresi();

        /// <summary>
        // tüm C++ kaynaklarını serbest bırakmak için
        /// </summary>
        /// <returns></returns>
        void sil();
    }
}
