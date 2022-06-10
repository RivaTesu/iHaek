using System.Runtime.InteropServices;

namespace iHaek.iqvw64e;

internal static class KernelStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct _HANDLE_TABLE
    {
        private readonly ulong TableCode;
        private readonly ulong QuotaProcess;
        private readonly ulong UniqueProcessId;
        private readonly ulong HandleLock;
        private readonly ulong Flink;
        private readonly ulong Blink;
        private readonly ulong HandleContentionEvent;
        private readonly ulong DebugInfo;
        private readonly int ExtraInfoPages;
        private readonly uint Flags;
        private readonly uint FirstFreeHandle;
        private readonly ulong LastFreeHandleEntry;
        private readonly uint HandleCount;
        private readonly uint NextHandleNeedingPool;
        private readonly uint HandleCountHighWatermark;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _HANDLE_TABLE_ENTRY
    {
        private readonly ulong Object;
        private readonly ulong GrantedAccess;
    }
}