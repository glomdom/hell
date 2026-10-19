// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using Hell.DeviceTree;

namespace Hell;

internal class Hell {
    [DoesNotReturn]
    [RuntimeExport("kmain")]
    private static unsafe void Main(ulong hartId, byte* dtb) {
        Console.WriteLine("welcome to hell");

        var fdt = new Fdt(dtb);

        if (fdt.TryFindDeviceType("memory", out var memory)) {
            var count = fdt.RegCount(memory);

            for (var i = 0u; i < count; i++) {
                fdt.GetReg(memory, i, out var addr, out var size);

                Console.Write("ram       ");
                Console.WriteHex(addr);
                Console.Write(" - ");
                Console.WriteHexLine(addr + size);
            }
        }

        if (fdt.TryFindNode("/reserved-memory", out var reserved) && fdt.TryGetFirstChild(reserved, out var region)) {
            do {
                fdt.GetReg(region, 0, out var addr, out var size);

                Console.Write("reserved  ");
                Console.WriteHex(addr);
                Console.Write(" - ");
                Console.WriteHexLine(addr + size);
            } while (fdt.TryGetNextSibling(region, out region));
        }

        for (var i = 0u; fdt.TryGetMemoryReservation(i, out var addr, out var size); i++) {
            Console.Write("rsvmap    ");
            Console.WriteHex(addr);
            Console.Write(" - ");
            Console.WriteHexLine(addr + size);
        }

        _ = *(ulong*)0x1;

        while (true) { }
    }
}