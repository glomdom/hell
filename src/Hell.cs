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

        var headerReader = new BigEndianReader(dtb, dtb + 0x28);
        var header = Fdt.ParseHeader(headerReader);
        
        Console.Write("using dtb version: ");
        Console.WriteLine(header.Version);

        var structsReader = new BigEndianReader(dtb, dtb + header.TotalSize);
        structsReader.GoTo(header.StructsOffset);
        Fdt.ParseStructs(structsReader, header);

        while (true) { }
    }

    private static unsafe uint ReadBe32(byte* p) {
        return (uint)p[0] << 24 | (uint)p[1] << 16 | (uint)p[2] << 8 | p[3];
    }
}