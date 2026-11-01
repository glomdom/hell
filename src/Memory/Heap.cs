namespace Hell.Memory;

internal static class Heap {
    private static ulong _cursor = 0xffffffd000000000;
    
    internal static unsafe byte* Alloc(ulong size) {
        return size <= 2048 ? Slab.Alloc(size) : LargeAlloc(size);
    }

    private static unsafe byte* LargeAlloc(ulong size) {
        var pageCount = Align.RoundUp(size) / 4096;
        var va = _cursor;
        
        for (var page = 0UL; page < pageCount; page++) {
            var phys = Freelist.AllocZeroed();
            
            Paging.Map(_cursor + Paging.PageSize * page, phys, PageTableEntryFlags.Read | PageTableEntryFlags.Write | PageTableEntryFlags.Accessed | PageTableEntryFlags.Dirty);
        }

        _cursor += Paging.PageSize * pageCount;

        return (byte*)va;
    }
}