# Cekirdekler
Very simple C# Multi-device GPGPU(OpenCL) compute API with an iterative interdevice-loadbalancing feature using multiple pipelining on read/write/compute operations for developers' custom opencl kernels. 

<h1>Features</h1>
<ul>
<li><b>Implicit multi device control:</b> from CPUs to any number of GPUs and ACCeelerators. Explicit in library-side for compatibility and performance, implicit for client-coder for the ease of GPGPU to concentrate on opencl kernel code.</li>
<li><b>Dynamic load balancing between devices:</b> uniquely done for each different compute(explicit control with user-given compute-id).</li>
<li><b>Pipelining for reads, computes and writes:</b> either by the mercy of device drivers or explicit event-based queue management.</li>
<li><b>Working with different numeric arrays:</b> Either C#-arrays like float[], int[], byte[],... or C++-array wrappers like ClFloatArray, ClArray&lt;float&gt;, ClByteArray, ClArray&lt;byte&gt; </li>
<li><b>Automatic buffer copy optimizations for devices:</b> If a device shares RAM with CPU, it uses map/unmap commands to reduce number of array copies(instead of read/write). If also that device is given a C++ wrapper array(such as ClArray&lt;float&gt;), it also uses cl_use_host_ptr flag on buffer for a <b>zero-copy</b> access aka" streaming".</li>
</ul>
