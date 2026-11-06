using System.Runtime;
using System.Runtime.CompilerServices;

namespace Hell.Diagnostics;

internal static class StackTrace {
    
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("read_fp")]
    internal static extern ulong ReadFp();
}