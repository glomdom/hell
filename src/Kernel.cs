// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace Hell;

internal class Hell {
    private const ulong DBCN = 0x4442434E;
    
    [RuntimeExport("kmain")]
    internal static unsafe void Main() {
        Panic("hell");

        while (true) {}
    }
    
    private static void Panic(string message) {
        Print("panic: ");
        Print(message);
        Print("\n");
        
        unsafe {
            SbiCall(0x53525354, 0, 0, 1, 0, null); // maybe write to qemu's test device after as well
        }

        while (true) { }
    }
    
    private static unsafe void Print(string s) {
        var buf = stackalloc byte[256];
        var i = 0;

        while (i < s.Length) {
            var n = 0;
            while (n < 256 && i < s.Length) {
                buf[n++] = (byte)s[i++];
            }

            var done = 0;
            while (done < n) {
                long written;

                var err = SbiCall(DBCN, 0, (ulong)(n - done), (ulong)(buf + done), 0, &written);
                if (err != 0) {
                    for (var k = done; k < n; k++) {
                        SbiCall(0x01, 0, buf[k], 0, 0, null);
                    }

                    break;
                }

                done += (int)written;
            }
        }
    }
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("sbi_call")]
    private static unsafe extern long SbiCall(ulong eid, ulong fid, ulong arg0, ulong arg1, ulong arg2, long* value);

    [RuntimeExport("RhpThrowEx")]
    private static void RhpThrowEx(object exception) {
        Panic(((Exception)exception).Message);
    }
    
    [RuntimeExport("RuntimeFailFast")]
    private static void FailFast(string message) {
        Panic(message);
    }

    [RuntimeExport("RhpFallbackFailFast")]
    private static void RhpFallbackFailFast() {
        Panic("stack corruption");
    }
}