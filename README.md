### iHaek - Intel Handle Access Elevation Kernel
Handle access elevation by [DKOM](#DKOM).

## About
This project uses [CVE-2015-2291](https://cve.mitre.org/cgi-bin/cvename.cgi?name=CVE-2015-2291) which allows low-level interation though a vulnerable ioctl.
- Tested on the latest versions of Windows (10/11).

## DKOM
Direct kernel object manipulation is a technique that can be used to bypass security controls and gain access to sensitive information or perform unauthorized actions. DKOM involves directly manipulating kernel objects, such as memory addresses, to bypass security checks. This can be done by using a debugger to examine kernel memory, or by injecting code into the kernel that modifies its behavior.

## Reference
- [Handlemaster - UC](https://www.unknowncheats.me/forum/anti-cheat-bypass/216970-handlemaster-elevating-handle-access-cpu.html)
- [ElevateMe - Github](https://github.com/vmcall/ElevateMe)
- [CVE-2015-2291](https://cve.mitre.org/cgi-bin/cvename.cgi?name=CVE-2015-2291)
- [Vergilius](https://www.vergiliusproject.com/kernels/x64)