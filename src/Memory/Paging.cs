using System.Runtime;
using System.Runtime.CompilerServices;

namespace Hell.Memory;

internal static class Paging {
    private static ulong _root;

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

    internal static void Dump() {
        if (_root == 0) {
            Panic.WithMessage("paging: attempted to dump pages without root");
        }
        
        DumpTable(_root, 2, 0);
    }

    internal static unsafe void DumpTable(ulong tablePhys, byte level, ulong baseVirt) {
        var table = (ulong*)Addresses.PhysToVirt(tablePhys);

        for (var i = 0UL; i < 512; i++) {
            var pte = table[i];

            var flags = (PageTableEntryFlags)(pte & 0xFF);
            if (!IsFlagSet(flags, PageTableEntryFlags.Valid)) {
                continue;
            }

            var virt = baseVirt + (i << (12 + 9 * level));
            var phys = (pte >> 10) << 12;
            var size = 1UL << (12 + 9 * level);

            var printableVirt = (ulong)((long)(virt << 25) >> 25);

            if (IsFlagSet(flags, PageTableEntryFlags.Read) || IsFlagSet(flags, PageTableEntryFlags.Write) || IsFlagSet(flags, PageTableEntryFlags.Execute)) {
                Console.WriteHex(printableVirt);
                Console.Write(" -> ");
                Console.WriteHex(phys);
                Console.Write(" size ");
                Console.WriteHex(size);
                Console.Write("  ");

                Console.Write(IsFlagSet(flags, PageTableEntryFlags.Read) ? "r" : "-");
                Console.Write(IsFlagSet(flags, PageTableEntryFlags.Write) ? "w" : "-");
                Console.Write(IsFlagSet(flags, PageTableEntryFlags.Execute) ? "x" : "-");
                Console.Write(IsFlagSet(flags, PageTableEntryFlags.User) ? "u" : "-");
                Console.Write(IsFlagSet(flags, PageTableEntryFlags.Global) ? "g" : "-");
                Console.Write(IsFlagSet(flags, PageTableEntryFlags.Accessed) ? "a" : "-");
                Console.Write(IsFlagSet(flags, PageTableEntryFlags.Dirty) ? "d" : "-");

                Console.WriteLine();
            } else {
                if (level - 1 < 0) {
                    Panic.WithMessage("paging: malformed page table");
                }
                
                DumpTable(phys, (byte)(level - 1), virt);
            }
        }
    }

    internal static unsafe void Map(ulong virt, ulong phys, PageTableEntryFlags flags) {
        var table = (ulong*)Addresses.PhysToVirt(_root);

        for (var level = 2; level > 0; level--) {
            var index = (virt >> (12 + 9 * level)) & 0x1FF;
            var entry = table[index];
            var entryFlags = (PageTableEntryFlags)(entry & 0xFF);

            ulong nextPhys;

            if (!IsFlagSet(entryFlags, PageTableEntryFlags.Valid)) {
                nextPhys = Freelist.AllocZeroed();
                table[index] = Entry(nextPhys, PageTableEntryFlags.Valid);
            } else if (IsFlagSet(entryFlags, PageTableEntryFlags.Read | PageTableEntryFlags.Write | PageTableEntryFlags.Execute)) {
                Panic.WithMessage("paging: address already covered by a superpage");

                return;
            } else {
                nextPhys = Entry(entry, PageTableEntryFlags.None);
            }

            table = (ulong*)Addresses.PhysToVirt(nextPhys);
        }

        var l0Index = (virt >> 12) & 0x1FF;
        var l0Entry = table[l0Index];
        
        var l0Flags = (PageTableEntryFlags)(l0Entry & 0xFF);
        if (IsFlagSet(l0Flags, PageTableEntryFlags.Valid)) {
            Panic.WithMessage("paging: address already mapped");
        }

        table[l0Index] = Entry(phys, flags | PageTableEntryFlags.Valid);

        SfenceVmaPage(virt);
    }

    private static ulong Entry(ulong phys, PageTableEntryFlags flags) {
        return (phys >> 12) << 10 | (ulong)flags;
    }

    private static bool IsFlagSet(PageTableEntryFlags flags, PageTableEntryFlags f) {
        return (flags & f) != 0;
    }
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("write_satp")]
    private static extern void WriteSatp(ulong value);
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("sfence_vma_page")]
    private static extern void SfenceVmaPage(ulong virt);
}