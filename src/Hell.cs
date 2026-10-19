// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace Hell;

internal class Hell {
    private const ulong DBCN = 0x4442434E;

    [DoesNotReturn]
    [RuntimeExport("kmain")]
    private static void Main() {
        Println("hell");
        PrintHex(0x80200000);
        PrintDec(1234);

        while (true) { }
    }

    [DoesNotReturn]
    private static void Panic(string message) {
        Println("panic: ");
        Println(message);

        unsafe {
            SbiCall(0x53525354, 0, 0, 1, 0, null); // maybe write to qemu's test device after as well
        }

        while (true) { }
    }

    private static unsafe void Write(byte* buf, int len) {
        var done = 0;
        while (done < len) {
            long written;

            var err = SbiCall(DBCN, 0, (ulong)(len - done), (ulong)(buf + done), 0, &written);
            if (err != 0) {
                for (var k = done; k < len; k++) {
                    SbiCall(0x01, 0, buf[k], 0, 0, null);
                }

                break;
            }

            done += (int)written;
        }
    }

    private static unsafe void PrintDec(ulong value) {
        var buf = stackalloc byte[20];
        var pos = 20;

        do {
            buf[--pos] = (byte)('0' + value % Ten());
            value /= Ten();
        } while (value != 0);

        Write(buf + pos, 20 - pos);
    }
    
    private static unsafe void PrintHex(ulong value) {
        var buf = stackalloc byte[18];
        buf[0] = (byte)'0';
        buf[1] = (byte)'x';

        for (var i = 0; i < 16; i++) {
            var nibble = (int)((value >> ((15 - i) * 4)) & 0xF);
            buf[2 + i] = (byte)(nibble < 10 ? '0' + nibble : 'a' + nibble - 10);
        }

        Write(buf, 18);
        Println();
    }

    private static unsafe void Print(string s) {
        var buf = stackalloc byte[256];
        var i = 0;

        while (i < s.Length) {
            var n = 0;
            while (n < 256 && i < s.Length) {
                buf[n++] = (byte)s[i++];
            }

            Write(buf, n);
        }
    }
    
    private static void Println(string s) {
        Print(s);
        Println();
    }

    private static unsafe void Println() {
        var nl = (byte)'\n';

        Write(&nl, 1);
    }
    
    // workaround while waiting for https://github.com/dotnet/runtime/issues/136034 to get fixed
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ulong Ten() {
        return 10;
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