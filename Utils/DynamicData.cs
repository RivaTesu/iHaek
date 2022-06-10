using System.Runtime.InteropServices;

namespace iHaek.Utils;

public class DynamicData
{
    public DynamicData()
    {
        var dwBuildNumber = Marshal.ReadInt32((IntPtr)0x7FFE0260);
        switch (dwBuildNumber)
        {
			case 19044: // W10
            case 22000: // W11
                ObjectTable = 0x570;
                break;
            default:
                Console.WriteLine(@"[!] Unknown Windows build.");
                break;
        }
    }
    // https://www.vergiliusproject.com/kernels/x64

    public uint ObjectTable { get; } // struct _HANDLE_TABLE* ObjectTable;
}