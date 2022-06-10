using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace iHaek.Utils;

public static unsafe class pInvoke
{
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode,
        IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern bool DeviceIoControl(SafeFileHandle hDevice, uint IoControlCode, UIntPtr InBuffer,
        int InBufferSize, UIntPtr OutBuffer, int OutBufferSize, out int BytesReturned, IntPtr Overlapped);

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern ulong LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeLibrary(ulong hModule);

    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    public static extern ulong GetProcAddress(ulong hModule, string procName);

    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern uint NtQuerySystemInformation(uint InfoClass, ulong Info, uint Size, out uint Length);

    [StructLayout(LayoutKind.Sequential)]
    public struct _RTL_PROCESS_MODULES
    {
        private readonly uint NumberOfModules;
        public _RTL_PROCESS_MODULE_INFORMATION Modules;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _RTL_PROCESS_MODULE_INFORMATION
    {
        private readonly void* Section;
        private readonly void* MappedBase;
        public readonly void* ImageBase;
        private readonly uint ImageSize;
        private readonly uint Flags;
        private readonly ushort LoadOrderIndex;
        private readonly ushort InitOrderIndex;
        private readonly ushort LoadCount;
        private readonly ushort OffsetToFileName;
        private fixed sbyte FullPathName[256];
    }
}