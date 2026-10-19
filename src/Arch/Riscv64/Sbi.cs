using System.Runtime;
using System.Runtime.CompilerServices;

namespace Hell.Arch.Riscv64;

internal static unsafe class Sbi {
    public const ulong ExtDbcn = 0x4442434E;
    public const ulong ExtSrst = 0x53525354;

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