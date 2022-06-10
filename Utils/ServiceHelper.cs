using System.Runtime.InteropServices;

namespace iHaek.Utils;

public static class ServiceHelper
{
    public static bool CreateService(
        ref IntPtr hService,
        string serviceName,
        string displayName,
        string binPath,
        uint desiredAccess,
        uint serviceType,
        uint startType,
        uint errorControl)
    {
        var hScManager = Nt.OpenSCManager(0, 0, 0x0002 /*SC_MANAGER_CREATE_SERVICE*/);

        if (hScManager == IntPtr.Zero)
            return false;

        hService = Nt.CreateServiceW(
            hScManager,
            serviceName, displayName,
            desiredAccess,
            serviceType, startType,
            errorControl, binPath,
            0, 0, 0, 0, 0, 0);

        Nt.CloseServiceHandle(hScManager);

        return hService != IntPtr.Zero;
    }

    public static bool OpenService(out IntPtr hService, string szServiceName, uint desiredAccess)
    {
        var hScManager = Nt.OpenSCManager(0, 0, desiredAccess);
        hService = Nt.OpenService(hScManager, szServiceName, desiredAccess);
        Nt.CloseServiceHandle(hScManager);
        return hService != IntPtr.Zero;
    }

    public static bool StopService(IntPtr hService)
    {
        var serviceStatus = new Nt.SERVICE_STATUS();
        return Nt.ControlService(hService, Nt.SERVICE_CONTROL.STOP, ref serviceStatus);
    }

    public static bool StartService(IntPtr hService)
    {
        return Nt.StartService(hService, 0, null);
    }

    public static bool DeleteService(IntPtr hService)
    {
        return Nt.DeleteService(hService);
    }

    public static void CloseServiceHandle(IntPtr hService)
    {
        Nt.CloseServiceHandle(hService);
    }

    private static class Nt
    {
        [Flags]
        public enum SERVICE_CONTROL : uint
        {
            STOP = 0x00000001,
            PAUSE = 0x00000002,
            CONTINUE = 0x00000003,
            INTERROGATE = 0x00000004,
            SHUTDOWN = 0x00000005,
            PARAMCHANGE = 0x00000006,
            NETBINDADD = 0x00000007,
            NETBINDREMOVE = 0x00000008,
            NETBINDENABLE = 0x00000009,
            NETBINDDISABLE = 0x0000000A,
            DEVICEEVENT = 0x0000000B,
            HARDWAREPROFILECHANGE = 0x0000000C,
            POWEREVENT = 0x0000000D,
            SESSIONCHANGE = 0x0000000E
        }

        public enum SERVICE_STATE : uint
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007
        }

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode,
            SetLastError = true)]
        public static extern IntPtr OpenSCManager(uint machineName, uint databaseName, uint dwAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseServiceHandle(IntPtr hSCObject);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ControlService(IntPtr hService, SERVICE_CONTROL dwControl,
            ref SERVICE_STATUS lpServiceStatus);

        [DllImport("advapi32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool StartService(
            IntPtr hService,
            int dwNumServiceArgs,
            string[]? lpServiceArgVectors
        );

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteService(IntPtr hService);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateServiceW(
            IntPtr hScManager,
            string lpServiceName,
            string lpDisplayName,
            uint dwDesiredAccess,
            uint dwServiceType,
            uint dwStartType,
            uint dwErrorControl,
            string lpBinaryPathName,
            uint lpLoadOrderGroup,
            uint lpdwTagId,
            uint lpdwTagId1,
            uint lpDependencies,
            uint lpServiceStartName,
            uint lpPassword);

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct SERVICE_STATUS
        {
            public readonly SERVICE_TYPE dwServiceType;
            public readonly SERVICE_STATE dwCurrentState;
            public readonly uint dwControlsAccepted;
            public readonly uint dwWin32ExitCode;
            public readonly uint dwServiceSpecificExitCode;
            public readonly uint dwCheckPoint;
            public readonly uint dwWaitHint;
        }

        [Flags]
        internal enum SERVICE_TYPE
        {
            SERVICE_KERNEL_DRIVER = 0x00000001,
            SERVICE_FILE_SYSTEM_DRIVER = 0x00000002,
            SERVICE_WIN32_OWN_PROCESS = 0x00000010,
            SERVICE_WIN32_SHARE_PROCESS = 0x00000020,
            SERVICE_INTERACTIVE_PROCESS = 0x00000100
        }
    }
}