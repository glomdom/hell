// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Runtime.CompilerServices;
using Hell.Arch.Riscv64;

namespace Hell;

public static class Console {
    public static unsafe void Write(byte* buf, int len) {
        var done = 0;
        while (done < len) {
            long written;

            var err = Sbi.ConsoleWrite(buf + done, (ulong)(len - done), &written);
            if (err != 0 || written <= 0) {
                return;
            }

            done += (int)written;
        }
    }

    public static unsafe void Write(string s) {
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
    
    public static unsafe void Write(ulong value) {
        var buf = stackalloc byte[20];
        var pos = 20;

        do {
            buf[--pos] = (byte)('0' + value % Ten());
            value /= Ten();
        } while (value != 0);

        Write(buf + pos, 20 - pos);
    }

    public static void Write(bool b) {
        Write(b ? "true" : "false");
    }

    public static unsafe void WriteLine(byte* buf, int len) {
        Write(buf, len);
        WriteLine();
    }

    public static unsafe void WriteLine(ulong value) {
        Write(value);
        WriteLine();
    }
    
    public static void WriteLine(bool b) {
        Write(b ? "true" : "false");
        WriteLine();
    }

    public static void WriteLine(string s) {
        Write(s);
        WriteLine();
    }

    public static unsafe void WriteLine() {
        var nl = (byte)'\n';

        Write(&nl, 1);
    }
    
    public static unsafe void WriteHex(ulong value) {
        var buf = stackalloc byte[18];
        buf[0] = (byte)'0';
        buf[1] = (byte)'x';

        for (var i = 0; i < 16; i++) {
            var nibble = (int)((value >> ((15 - i) * 4)) & 0xF);
            buf[2 + i] = (byte)(nibble < 10 ? '0' + nibble : 'a' + nibble - 10);
        }

        Write(buf, 18);
    }

    public static unsafe void WriteHexLine(ulong value) {
        WriteHex(value);
        WriteLine();
    }

    // workaround while waiting for https://github.com/dotnet/runtime/issues/136034 to get fixed
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ulong Ten() {
        return 10;
    }
}