// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using System.Runtime;

namespace Hell;

internal class Hell {
    [DoesNotReturn]
    [RuntimeExport("kmain")]
    private static unsafe void Main(ulong hartId, byte* dtb) {
        Console.Println("hell");
        Console.PrintHex(0x80200000);
        Console.PrintDec(1234);

        var magic = ReadBe32(dtb);
        Console.PrintHex(magic);

        while (true) { }
    }

    private static unsafe uint ReadBe32(byte* p) {
        return (uint)p[0] << 24 | (uint)p[1] << 16 | (uint)p[2] << 8 | p[3];
    }
}