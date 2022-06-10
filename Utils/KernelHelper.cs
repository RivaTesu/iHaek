using System.Runtime.InteropServices;

namespace iHaek.Utils;

internal static unsafe class KernelHelper
{
    private static ulong GetKernelBase()
    {
        uint bufferSize = 2048;

        var buffer = (ulong)Marshal.AllocHGlobal((int)bufferSize);

        var status = pInvoke.NtQuerySystemInformation(11 /*SystemModuleInformation*/, buffer, bufferSize,
            out bufferSize);

        if (status == 0xC0000004L /*STATUS_INFO_LENGTH_MISMATCH*/)
        {
            Marshal.FreeHGlobal((IntPtr)buffer);
            buffer = (ulong)Marshal.AllocHGlobal((int)bufferSize);

            status = pInvoke.NtQuerySystemInformation(11 /*SystemModuleInformation*/, buffer, bufferSize,
                out bufferSize);
        }

        if (status != 0)
            throw new Exception("GetKernelBase Failed");

        var modulesPointer = (pInvoke._RTL_PROCESS_MODULES*)buffer;

        return (ulong)modulesPointer->Modules.ImageBase;
    }

    public static byte* FindKernelProcedure(string szName)
    {
        var ntoskrnlHandle = pInvoke.LoadLibrary("ntoskrnl.exe");

        if (ntoskrnlHandle == 0)
            throw new Exception("Cant load ntoskrnl!");

        var functionPointer = pInvoke.GetProcAddress(ntoskrnlHandle, szName);
        var kernelBase = GetKernelBase();

        if (kernelBase == 0)
        {
            pInvoke.FreeLibrary(ntoskrnlHandle);
            throw new Exception("Cant get ntoskrnl base!");
        }

        pInvoke.FreeLibrary(ntoskrnlHandle);

        return (byte*)(functionPointer - ntoskrnlHandle + kernelBase);
    }
}