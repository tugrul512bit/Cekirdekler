# Cekirdekler
Very simple C# Multi-device GPGPU(OpenCL) compute API with an iterative interdevice-loadbalancing feature using multiple pipelining on read/write/compute operations for developers' custom opencl kernels. 

<h1>Features</h1>
<ul>
<li><b>Explicit multi device control:</b> from CPUs to any number of GPUs and ACCeelerators. Explicit in library, implicit for client coder for the ease of GPGPU.</li>
<li><b>Dynamic load balancing between devices:</b> uniquely for each compute function(explicit control with user-given compute-id).</li>
<li><b>Pipelining for reads, computes and writes:</b> either by the mercy of device drivers or explicit event-based queue management.</li>
<li><b></b></li>
</ul>
