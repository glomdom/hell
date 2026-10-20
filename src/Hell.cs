// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;
using Hell.DeviceTree;
using Hell.Time;

namespace Hell;

internal class Hell {
    [DoesNotReturn]
    [RuntimeExport("kmain")]
    private static unsafe void Main(ulong hartId, byte* dtb) {
        Console.WriteLine("welcome to hell");
        Console.WriteLine();

        var fdt = new Fdt(dtb);

        if (fdt.TryFindDeviceType("memory", out var memory)) {
            var count = fdt.RegCount(memory);

            for (var i = 0u; i < count; i++) {
                fdt.GetReg(memory, i, out var addr, out var size);

                Console.Write("dtb: ram       ");
                Console.WriteHex(addr);
                Console.Write(" - ");
                Console.WriteHexLine(addr + size);
            }
        }

        if (fdt.TryFindNode("/reserved-memory", out var reserved) && fdt.TryGetFirstChild(reserved, out var region)) {
            do {
                fdt.GetReg(region, 0, out var addr, out var size);

                Console.Write("dtb: reserved  ");
                Console.WriteHex(addr);
                Console.Write(" - ");
                Console.WriteHexLine(addr + size);
            } while (fdt.TryGetNextSibling(region, out region));
        }

        for (var i = 0u; fdt.TryGetMemoryReservation(i, out var addr, out var size); i++) {
            Console.Write("dtb: rsvmap    ");
            Console.WriteHex(addr);
            Console.Write(" - ");
            Console.WriteHexLine(addr + size);
        }
        
        
        if (fdt.TryFindNode("/cpus", out var cpus) && fdt.TryGetU32(cpus, "timebase-frequency", out var frequency)) {
            Console.Write("dtb: timer ");
            Console.Write(frequency / 1000 / 1000);
            Console.WriteLine("MHz");

            Timer.Init(frequency, 10_000_000 / 10_000_000);
            EnableTimerInterrupt();
        }
        
        EnableInterrupts();

        IdleCore();
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