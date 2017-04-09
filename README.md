# Cekirdekler
Very simple C# Multi-device GPGPU(OpenCL) compute API with an iterative interdevice-loadbalancing feature using multiple pipelining on read/write/compute operations for developers' custom opencl kernels. 

Needs extra C++ dll built in 64-bit(x86_64) from https://github.com/tugrul512bit/CekirdeklerCPP which must be named KutuphaneCL.dll

The other needed dll is Microsoft's System.Threading.dll and its xml helper for .Net 2.0 - or - you can adjust "using" and use .Net 3.5+ for your own project and don't need System.Threading.dll.

In total, Cekirdekler.dll and KutuphaneCL.dll and using .Net 3.5 should be enough.

Usage: add only Cekirdekler.dll and system.threading.dll as references to your C# projects. Other files needs to exist in same folder with Cekirdekler.dll.

This project is being enhanced using ZenHub: <a href="https://zenhub.com"><img src="https://raw.githubusercontent.com/ZenHubIO/support/master/zenhub-badge.png"></a>

<h1>Features</h1>
<ul>
<li><b>Implicit multi device control:</b> from CPUs to any number of GPUs and ACCeelerators. Explicit in library-side for compatibility and performance, implicit for client-coder for the ease of GPGPU to concentrate on opencl kernel code. Selection of devices can be done implicitly or explicitly to achieve ease-of-setup or detailed device query. </li>
<li><b>Iterative load balancing between devices:</b> uniquely done for each different compute(explicit control with user-given compute-id). Multiple devices get more and more fair work loads until the ratio of work distribution converges to some point. Partitionig workload completes a kernel with less latency which is applicable for hot-spot loops and some simple embarrassingly-parallel algorithms. Even better for streaming data with pipelining option enabled.</li>
<li><b>Pipelining for reads, computes and writes:</b> either by the mercy of device drivers or explicit event-based queue management. Hides the latency of least time consuming part(such as writes) behind the most time consuming part(such as compute). GPUs can run buffer copies and opencl kernels concurrently.</li>
<li><b>Working with different numeric arrays:</b> Either C#-arrays like float[], int[], byte[],... or C++-array wrappers like ClFloatArray, ClArray&lt;float&gt;, ClByteArray, ClArray&lt;byte&gt; </li>
<li><b>Automatic buffer copy optimizations for devices:</b> If a device shares RAM with CPU, it uses map/unmap commands to reduce number of array copies(instead of read/write). If also that device is given a C++ wrapper array(such as ClArray&lt;float&gt;), it also uses cl_use_host_ptr flag on buffer for a <b>zero-copy</b> access aka" streaming". By default, all devices have their own buffers.</li>
<li><b>Two different usage types: </b>First one lets the developer choose all kernel parameters as arrays more explicitly for a more explicitly readable execution, second one creates same thing using a much shorter definition to complete in less code lines and change only the necessary flags instead of all.</li>
<li><b>Automatic resource dispose:</b> When C++ array wrappers are finalized(out-of-scope, garbage collected), they release resources. Also dispose method can be called explicitly by developer.</li>
<li><b>Uses OpenCL 1.2:</b> <a href="https://www.khronos.org/">C++ bindings from  Khronos.org</a> for its base.</li>
</ul>
<hr></hr>
<h2>Documentation</h2>
You can see details and tutorial <a href="https://github.com/tugrul512bit/Cekirdekler/wiki"> here in Cekirdekler-wiki </a>
<hr></hr>
<h2>Known Issues</h2>
<ul>
<li>For C++ array wrappers like Array&lt;float&gt; there is no out-of-bounds-check, don't cross boundaries when accessing array indexing.</li>
<li>Don't use C++ array wrappers after they are disposed. These features are not added to speed-up array indexing.</li>
<li>Don't use ClNumberCruncher or Core instances after they are disposed.</li>
<li>Pay attention to "number of array elements used" per workitem in kernel and how they are given as parameters from API compute() method.</li>
<li>Pay attenton to "partial read"/"read"/"write" array copy modifiers when your kernel is altering(or reading) whole array or just a part of it.</li>
<li>No performance output at first iteration. <b>Load balancer</b> needs at least several iterations to distribute fairly and <b>performance report</b> needs at least 2 iterations for console output.</li>
</ul>
<hr></hr>
<h3>Example</h3>


            Cekirdekler.ClNumberCruncher cr = new Cekirdekler.ClNumberCruncher(
                Cekirdekler.AcceleratorType.GPU, @"
                    __kernel void hello(__global char * arr)
                    {
                        printf(""hello world"");
                    }
                ");

            Cekirdekler.ClArray<byte> array = new Cekirdekler.ClArray<byte>(1000);
            array.compute(cr, 1, "hello", 1000, 100); 
