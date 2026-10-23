using System.Runtime;
using System.Runtime.CompilerServices;

namespace Hell.Memory;

internal static class Paging {
    internal static ulong _root;

    internal static unsafe void Init(ulong ramStart) {
        _root = Freelist.AllocZeroed();

        var addr = (ulong*)Addresses.PhysToVirt(_root);

        var index = (ramStart >> 30) & 0x1FF;
        var entry = Entry(
            ramStart,
            PageTableEntryFlags.Valid | PageTableEntryFlags.Read | PageTableEntryFlags.Write |
            PageTableEntryFlags.Execute | PageTableEntryFlags.Accessed | PageTableEntryFlags.Dirty
        );

        addr[index] = entry;

        index = (0xffffffc080200000 >> 30) & 0x1FF;
        entry = Entry(
            ramStart,
            PageTableEntryFlags.Valid | PageTableEntryFlags.Read | PageTableEntryFlags.Write |
            PageTableEntryFlags.Accessed | PageTableEntryFlags.Dirty
        );
        
        addr[index] = entry;

        var satp = (8UL << 60) | (_root >> 12);
        WriteSatp(satp);
        
        Addresses.EnableDirectMap();
    }

    internal static ulong Entry(ulong phys, PageTableEntryFlags flags) {
        return (phys >> 12) << 10 | (ulong)flags;
    }
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("write_satp")]
    internal extern static void WriteSatp(ulong value);
}