// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;
using Hell.DeviceTree;
using Hell.Memory;
using Hell.Time;

namespace Hell;

internal class Hell {
    [DoesNotReturn]
    [RuntimeExport("kmain")]
    private static unsafe void Main(ulong hartId, byte* dtb) {
        Console.WriteLine("welcome to hell");
        Console.WriteLine();

        var fdt = new Fdt(dtb);
        
        Freelist.Init(in fdt);
        
        if (fdt.TryFindNode("/cpus", out var cpus) && fdt.TryGetU32(cpus, "timebase-frequency", out var frequency)) {
            Console.Write("dtb: timer ");
            Console.Write(frequency / 1000 / 1000);
            Console.WriteLine("MHz");

            Timer.Init(frequency, 10_000_000 / 10_000_000);
            EnableTimerInterrupt();
        }
        
        EnableInterrupts();
        
        var page1 = AllocatePage();
        var page2 = AllocatePage();
        var page3 = AllocatePage();
        
        FreePage(page1);
        FreePage(page2);
        FreePage(page3);

        var page4 = AllocatePage();
        FreePage(page4);
        FreePage(page4);

        IdleCore();
    }

    private static ulong AllocatePage() {
        var page = Freelist.Alloc();
        Console.Write("freelist: allocated page ");
        Console.WriteHexLine(page);

        return page;
    }
    
    private static void FreePage(ulong pageAddr) {
        Freelist.Free(pageAddr);
        Console.Write("freelist: freed page ");
        Console.WriteHexLine(pageAddr);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("enable_interrupts")]
    private static extern void IdleCore();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("enable_interrupts")]
    private static extern void EnableInterrupts();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("enable_timer_interrupt")]
    private static extern void EnableTimerInterrupt();
}