namespace Hell.Memory;

internal static unsafe class Slab {
    private static ClassHeads _heads;
    
    internal static ulong Alloc(ulong size) {
        var sizeClass = ClassFor(size);
        if (_heads.Head[sizeClass] == 0) {
            Refill(sizeClass);
        }

        var slot = _heads.Head[sizeClass];
        _heads.Head[sizeClass] = ((ulong*)slot)[0];

        var words = (ulong*)slot;
        var wordCount = SlotSize(sizeClass) / 8;
        for (var i = 0UL; i < wordCount; i++) {
            words[i] = 0;
        }

        return slot;
    }

    internal static void Free(ulong slot, ulong size) {
        var sizeClass = ClassFor(size);

        if ((slot & (SlotSize(sizeClass) - 1)) != 0) {
            Panic.WithMessage("slab: misaligned slot");
        }

        ((ulong*)slot)[0] = _heads.Head[sizeClass];
        _heads.Head[sizeClass] = slot;
    }

    internal static void DumpClass(int sizeClass) {
        var slot = _heads.Head[sizeClass];
        var count = 0UL;

        while (slot != 0) {
            var next = *(ulong*)slot;

            count++;
            slot = next;
        }

        Console.Write("slab: class ");
        Console.Write((ulong)sizeClass);
        Console.Write(" has ");
        Console.Write(count);
        Console.WriteLine(" free slots");
    }

    private static void Refill(int sizeClass) {
        var slotSize = SlotSize(sizeClass);
        var slotCount = Paging.PageSize / slotSize;
        var page = Addresses.PhysToVirt(Freelist.Alloc());

        for (var i = 0UL; i < slotCount; i++) {
            var slot = page + i * slotSize;
            var isLast = i == slotCount - 1;

            *(ulong*)slot = isLast ? 0 : (ulong)(slot + slotSize);
        }

        _heads.Head[sizeClass] = (ulong)page;
    }
    
    private static ulong SlotSize(int sizeClass) {
        return 16UL << sizeClass;
    }

    private static int ClassFor(ulong size) {
        if (size > 2048) {
            Panic.WithMessage("slab: more than 2048 for allocation is not supported, idk how to do this yet");
        }
        
        var sizeClass = 0;
        while (SlotSize(sizeClass) < size) {
            sizeClass++;
        }

        return sizeClass;
    }

    // fixme: add [InlineArray] to corelib after allocator is done
    private struct ClassHeads {
        internal fixed ulong Head[8];
    }
}