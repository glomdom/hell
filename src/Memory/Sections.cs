using System.Runtime;
using System.Runtime.CompilerServices;

namespace Hell.Memory;

internal static class Sections {
    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("_text_start")]
    private static extern void TextStartSymbol();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("_text_end")]
    private static extern void TextEndSymbol();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("_rodata_start")]
    private static extern void RodataStartSymbol();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("_rodata_end")]
    private static extern void RodataEndSymbol();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("_data_start")]
    private static extern void DataStartSymbol();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("_data_end")]
    private static extern void DataEndSymbol();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("_bss_start")]
    private static extern void BssStartSymbol();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("_bss_end")]
    private static extern void BssEndSymbol();

    internal static unsafe ulong TextStart => (ulong)(delegate*<void>)&TextStartSymbol;
    internal static unsafe ulong TextEnd => (ulong)(delegate*<void>)&TextEndSymbol;
    internal static unsafe ulong RodataStart => (ulong)(delegate*<void>)&RodataStartSymbol;
    internal static unsafe ulong RodataEnd => (ulong)(delegate*<void>)&RodataEndSymbol;
    internal static unsafe ulong DataStart => (ulong)(delegate*<void>)&DataStartSymbol;
    internal static unsafe ulong DataEnd => (ulong)(delegate*<void>)&DataEndSymbol;
    internal static unsafe ulong BssStart => (ulong)(delegate*<void>)&BssStartSymbol;
    internal static unsafe ulong BssEnd => (ulong)(delegate*<void>)&BssEndSymbol;
}