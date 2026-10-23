// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Runtime;
using System.Runtime.CompilerServices;
using Hell.DeviceTree;

namespace Hell.Memory;

internal struct FreePage {
    internal ulong Next;
    internal ulong Magic;
}

internal static unsafe class Freelist {
    internal static ulong FreePages { get; set; }
    
    // FIXME: temporarily fitting in 32-bit because of our favorite issue https://github.com/dotnet/runtime/issues/136034
    private const ulong FreeMagic = 0x46524545; // haha get it its "FREE" in ascii
    private const ulong PageSize = 4096;
    private const int MaxReservedRanges = 32;

    private static ulong _head;

    /// <summary>
    /// Initializes a freelist at an <see cref="Fdt"/>'s <c>memory</c> device type start address,
    /// avoiding reserved regions.
    /// </summary>
    /// 
    /// <param name="fdt">The device tree to take reserved regions from, as well as the freelist start.</param>
    internal static void Init(ref readonly Fdt fdt) {
        var ranges = stackalloc ulong[MaxReservedRanges * 2];
        var count = 0;

        CollectReserved(in fdt, ranges, ref count);

        if (fdt.TryGetFirstChild(fdt.Root, out var node)) {
            do {
                if (!fdt.PropertyContains(node, "device_type", "memory")) {
                    continue;
                }

                var regCount = fdt.RegCount(node);
                for (var i = 0u; i < regCount; i++) {
                    fdt.GetReg(node, i, out var address, out var size);

                    AddRegion(ranges, count, address, address + size);
                }
            } while (fdt.TryGetNextSibling(node, out node));
        }

        Console.Write("freelist: ");
        Console.Write(FreePages);
        Console.WriteLine(" free pages");
    }

    internal static ulong Alloc() {
        if (_head == 0) {
            Panic.WithMessage("freelist: out of memory");
        }

        var address = _head;
        var page = (FreePage*)PhysToVirt(address);

        if (page->Magic != FreeMagic) {
            Panic.WithMessage("freelist: corrupted free page (use after free?)");
        }

        _head = page->Next;
        FreePages--;

        page->Next = 0;
        page->Magic = 0;

        return address;
    }

    internal static ulong AllocZeroed() {
        var addr = Alloc();
        var page = PhysToVirt(addr);

        for (var i = 2UL; i < PageSize; i++) {
            page[i] = 0;
        }

        return addr;
    }

    internal static void Free(ulong address) {
        if ((address & (PageSize - 1)) != 0) {
            Panic.WithMessage("freelist: freeing unaligned address");
        }

        var page = (FreePage*)PhysToVirt(address);
        if (page->Magic == FreeMagic) {
            Console.Write("freelist: found already free magic at ");
            Console.WriteHexLine(address);
            
            Panic.WithMessage("freelist: double free");
        }

        Push(address);
    }
    
    internal static byte* PhysToVirt(ulong physAddr) {
        return (byte*)physAddr;
    }

    private static void CollectReserved(ref readonly Fdt fdt, ulong* ranges, ref int count) {
        if (fdt.TryFindNode("/reserved-memory", out var reserved) && fdt.TryGetFirstChild(reserved, out var region)) {
            do {
                fdt.GetReg(region, 0, out var address, out var size);
                AddReserved(ranges, ref count, address, address + size, "reserved ");
            } while (fdt.TryGetNextSibling(region, out region));
        }

        for (var i = 0u; fdt.TryGetMemoryReservation(i, out var address, out var size); i++) {
            AddReserved(ranges, ref count, address, address + size, "rsvmap   ");
        }

        AddReserved(ranges, ref count, KernelStart(), KernelEnd(), "kernel   ");

        var dtb = (ulong)fdt.Base;
        AddReserved(ranges, ref count, dtb, dtb + fdt.Header.TotalSize, "dtb      ");
    }

    private static void AddReserved(ulong* ranges, ref int count, ulong start, ulong end, string label) {
        if (count == MaxReservedRanges) {
            Panic.WithMessage("freelist: too many reserved ranges");
        }

        Console.Write("freelist: ");
        Console.Write(label);
        Console.WriteHex(start);
        Console.Write(" - ");
        Console.WriteHexLine(end);

        ranges[count * 2] = RoundDown(start);
        ranges[count * 2 + 1] = RoundUp(end);
        count++;
    }

    private static void AddRegion(ulong* ranges, int count, ulong start, ulong end) {
        Console.Write("freelist: ram      ");
        Console.WriteHex(start);
        Console.Write(" - ");
        Console.WriteHexLine(end);

        var first = RoundUp(start);
        var last = RoundDown(end);

        outer: for (var page = first; page < last; page += PageSize) {
            for (var i = 0; i < count; i++) {
                if (Overlaps(page, ranges[i * 2], ranges[i * 2 + 1])) {
                    continue outer;
                }
            }

            Push(page);
        }
    }

    private static void Push(ulong address) {
        var page = (FreePage*)PhysToVirt(address);
        page->Next = _head;
        page->Magic = FreeMagic;

        _head = address;
        FreePages++;
    }

    private static ulong RoundDown(ulong value) {
        return (value >> 12) << 12;
    }

    private static ulong RoundUp(ulong value) {
        return ((value + PageSize - 1) >> 12) << 12;
    }
    
    private static bool Overlaps(ulong page, ulong start, ulong end) {
        return page < end && page + PageSize > start;
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("kernel_start")]
    private static extern ulong KernelStart();

    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("kernel_end")]
    private static extern ulong KernelEnd();
}