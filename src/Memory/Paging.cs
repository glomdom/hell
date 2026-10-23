using System.Runtime;
using System.Runtime.CompilerServices;

namespace Hell.Memory;

internal static class Paging {
    internal static ulong _root;

    internal static unsafe void Init(ulong rootPhys) {
        _root = Freelist.AllocZeroed();

        var addr = (ulong*)Freelist.PhysToVirt(_root);

        var index = rootPhys >> 30;
        var entry = Entry(
            rootPhys,
            PageTableEntryFlags.Valid | PageTableEntryFlags.Read | PageTableEntryFlags.Write |
            PageTableEntryFlags.Execute | PageTableEntryFlags.Accessed | PageTableEntryFlags.Dirty
        );

        addr[index] = entry;

        var satp = (8UL << 60) | (_root >> 12);
        WriteSatp(satp);
    }

    internal static ulong Entry(ulong phys, PageTableEntryFlags flags) {
        return (phys >> 12) << 10 | (ulong)flags;
    }
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("write_satp")]
    internal extern static void WriteSatp(ulong value);
}