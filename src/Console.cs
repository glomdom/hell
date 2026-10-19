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

    public static unsafe void PrintDec(ulong value) {
        var buf = stackalloc byte[20];
        var pos = 20;

        do {
            buf[--pos] = (byte)('0' + value % Ten());
            value /= Ten();
        } while (value != 0);

        Write(buf + pos, 20 - pos);
        Println();
    }
    
    public static unsafe void PrintHex(ulong value) {
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

    public static unsafe void Print(string s) {
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
    
    public static void Println(string s) {
        Print(s);
        Println();
    }

    public static unsafe void Println() {
        var nl = (byte)'\n';

        Write(&nl, 1);
    }
    
    // workaround while waiting for https://github.com/dotnet/runtime/issues/136034 to get fixed
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ulong Ten() {
        return 10;
    }
}