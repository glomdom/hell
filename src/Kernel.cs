// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Runtime;
using System.Runtime.CompilerServices;

unsafe class Kernel {
    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("sbi_call")]
    static extern long SbiCall(ulong eid, ulong fid, ulong arg0, ulong arg1, ulong arg2);

    [RuntimeExport("kmain")]
    static void KMain() {
        SbiCall(0x4442434E, 2, 'h', 0, 0);
        SbiCall(0x4442434E, 2, 'e', 0, 0);
        SbiCall(0x4442434E, 2, 'l', 0, 0);
        SbiCall(0x4442434E, 2, 'l', 0, 0);

        while (true) {}
    }
}