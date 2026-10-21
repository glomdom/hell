// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Runtime;
using System.Runtime.CompilerServices;

namespace Hell.Arch.Riscv64;

internal static unsafe class Sbi {
    private const ulong ExtDbcn = 0x4442434E;
    private const ulong ExtSrst = 0x53525354;
    private const ulong ExtTime = 0x54494D45;

    internal static long SetTimer(ulong absoluteDeadline) {
        return Call(ExtTime, 0, absoluteDeadline, 0, 0, null);
    }
    
    internal static long ConsoleWrite(byte* buf, ulong len, long* written) {
        return Call(ExtDbcn, 0, len, (ulong)buf, 0, written);
    }
    
    internal static void Shutdown(bool failure) {
        Call(ExtSrst, 0, 0, failure ? 1UL : 0UL, 0, null);
    }
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("sbi_call")]
    private static extern long Call(ulong eid, ulong fid, ulong arg0, ulong arg1, ulong arg2, long* value);
}