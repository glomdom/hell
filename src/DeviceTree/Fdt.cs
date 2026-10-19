namespace Hell.DeviceTree;

internal unsafe struct Fdt {
    private const uint TokenBeginNode = 1;
    private const uint TokenEndNode = 2;
    private const uint TokenProp = 3;
    private const uint TokenNop = 4;
    private const uint TokenEnd = 9;

    private const uint DefaultAddressCells = 2;
    private const uint DefaultSizeCells = 1;

    private readonly byte* _dtb;
    private readonly FdtHeader _header;

    internal Fdt(byte* dtb) {
        _dtb = dtb;
        _header = ParseHeader(dtb);
    }

    internal FdtHeader Header => _header;

    internal FdtNode Root {
        get {
            var reader = CreateReader();
            reader.GoTo(_header.StructsOffset);

            if (ReadToken(ref reader, out var offset) != TokenBeginNode) {
                Panic.WithMessage("dtb: struct block does not start with a node");
            }

            return new FdtNode(offset, DefaultAddressCells, DefaultSizeCells);
        }
    }

    internal byte* GetName(FdtNode node, out int length) {
        var reader = CreateReader();
        reader.GoTo(node.Offset);
        reader.ReadU32();

        return reader.ReadCString(out length);
    }

    internal bool TryFindNode(string path, out FdtNode node) {
        node = Root;

        var start = 0;
        while (start < path.Length) {
            if (path[start] == '/') {
                start++;

                continue;
            }

            var end = start;
            while (end < path.Length && path[end] != '/') {
                end++;
            }

            if (!TryFindChild(node, path, start, end, out var next)) {
                return false;
            }

            node = next;
            start = end;
        }

        return true;
    }

    internal bool TryFindCompatible(string compatible, out FdtNode node) {
        return TryFindFirst(Root, "compatible", compatible, out node);
    }

    internal bool TryFindDeviceType(string deviceType, out FdtNode node) {
        return TryFindFirst(Root, "device_type", deviceType, out node);
    }

    internal bool TryGetFirstChild(FdtNode node, out FdtNode child) {
        var addressCells = TryGetU32(node, "#address-cells", out var ac) ? ac : DefaultAddressCells;
        var sizeCells = TryGetU32(node, "#size-cells", out var sc) ? sc : DefaultSizeCells;

        var reader = CreateReader();
        EnterNode(ref reader, node);

        while (TryReadProperty(ref reader, out _, out _)) {
        }

        if (ReadToken(ref reader, out var offset) == TokenBeginNode) {
            child = new FdtNode(offset, addressCells, sizeCells);

            return true;
        }

        child = default;

        return false;
    }

    internal bool TryGetNextSibling(FdtNode node, out FdtNode sibling) {
        var reader = CreateReader();
        reader.GoTo(node.Offset);
        SkipSubtree(ref reader);

        if (ReadToken(ref reader, out var offset) == TokenBeginNode) {
            sibling = new FdtNode(offset, node.AddressCells, node.SizeCells);

            return true;
        }

        sibling = default;

        return false;
    }

    internal bool TryGetProperty(FdtNode node, string name, out FdtProperty property) {
        var reader = CreateReader();
        EnterNode(ref reader, node);

        while (TryReadProperty(ref reader, out var nameOffset, out property)) {
            var propertyName = GetString(nameOffset, out var length);
            if (BytesEqual(propertyName, length, name)) {
                return true;
            }
        }

        property = default;

        return false;
    }

    internal bool TryGetU32(FdtNode node, string name, out uint value) {
        if (!TryGetProperty(node, name, out var property) || property.Length != 4) {
            value = 0;

            return false;
        }

        var reader = new BigEndianReader(property.Value, property.Value + property.Length);
        value = reader.ReadU32();

        return true;
    }

    internal bool PropertyContains(FdtNode node, string propertyName, string value) {
        if (!TryGetProperty(node, propertyName, out var property)) {
            return false;
        }

        var p = property.Value;
        var end = property.Value + property.Length;

        while (p < end) {
            var length = 0;
            while (p + length < end && p[length] != 0) {
                length++;
            }

            if (BytesEqual(p, length, value)) {
                return true;
            }

            p += length + 1;
        }

        return false;
    }

    internal uint RegCount(FdtNode node) {
        var entrySize = (node.AddressCells + node.SizeCells) * 4;
        if (!TryGetProperty(node, "reg", out var reg) || entrySize == 0) {
            return 0;
        }

        return reg.Length / entrySize;
    }

    internal void GetReg(FdtNode node, uint index, out ulong address, out ulong size) {
        var entrySize = (node.AddressCells + node.SizeCells) * 4;
        if (!TryGetProperty(node, "reg", out var reg) || entrySize == 0 || index >= reg.Length / entrySize) {
            Panic.WithMessage("dtb: reg index out of range");
        }

        var reader = new BigEndianReader(reg.Value, reg.Value + reg.Length);
        reader.Skip((int)(index * entrySize));

        address = ReadCells(ref reader, node.AddressCells);
        size = ReadCells(ref reader, node.SizeCells);
    }

    internal bool TryGetMemoryReservation(uint index, out ulong address, out ulong size) {
        var reader = CreateReader();
        reader.GoTo(_header.MemoryReservationOffset + index * 16);

        address = ReadCells(ref reader, 2);
        size = ReadCells(ref reader, 2);

        return address != 0 || size != 0;
    }

    internal void Dump() {
        var reader = CreateReader();
        reader.GoTo(_header.StructsOffset);

        var depth = 0;

        while (true) {
            var token = reader.ReadU32();

            switch (token) {
                case TokenBeginNode: {
                    Pad(depth);
                    Console.Write("BEGIN_NODE ");

                    var name = reader.ReadCString(out var length);
                    reader.AlignTo4();

                    Console.WriteLine(name, length);

                    depth += 1;

                    break;
                }

                case TokenEndNode: {
                    depth -= 1;

                    Pad(depth);
                    Console.WriteLine("END_NODE");

                    break;
                }

                case TokenProp: {
                    var length = reader.ReadU32();
                    var nameOffset = reader.ReadU32();

                    var name = GetString(nameOffset, out var nameLength);

                    Pad(depth);
                    Console.Write("PROP `");
                    Console.Write(name, nameLength);
                    Console.WriteLine("`");

                    reader.Skip((int)length);
                    reader.AlignTo4();

                    break;
                }

                case TokenNop: {
                    break;
                }

                case TokenEnd: {
                    return;
                }

                default: {
                    Console.Write("dtb: unhandled token ");
                    Console.WriteLine(token);
                    Panic.WithMessage("dtb: failed to read structs");

                    return;
                }
            }
        }
    }

    private static FdtHeader ParseHeader(byte* dtb) {
        var reader = new BigEndianReader(dtb, dtb + 0x28);

        var magic = reader.ReadU32();
        if (magic != 0xD00DFEED) {
            Panic.WithMessage("dtb: invalid magic");
        }

        var totalSize = reader.ReadU32();
        var structsOffset = reader.ReadU32();
        var stringsOffset = reader.ReadU32();
        var memoryReservationOffset = reader.ReadU32();
        var version = reader.ReadU32();
        var lastCompatVersion = reader.ReadU32();
        var bootPhysCpuId = reader.ReadU32();
        var stringsSize = reader.ReadU32();
        var structsSize = reader.ReadU32();

        if (lastCompatVersion > 17) {
            Panic.WithMessage("dtb: unsupported version");
        }

        return new FdtHeader {
            Magic = magic,
            TotalSize = totalSize,
            StructsOffset = structsOffset,
            StringsOffset = stringsOffset,
            MemoryReservationOffset = memoryReservationOffset,
            Version = version,
            LastCompatibleVersion = lastCompatVersion,
            BootPhysicalCpuId = bootPhysCpuId,
            StringsSize = stringsSize,
            StructsSize = structsSize,
        };
    }

    private BigEndianReader CreateReader() {
        return new BigEndianReader(_dtb, _dtb + _header.TotalSize);
    }

    private uint ReadToken(ref BigEndianReader reader, out uint offset) {
        while (true) {
            offset = (uint)(reader.Position - _dtb);
            var token = reader.ReadU32();

            if (token != TokenNop) {
                return token;
            }
        }
    }

    private static void EnterNode(ref BigEndianReader reader, FdtNode node) {
        reader.GoTo(node.Offset);
        reader.ReadU32();
        reader.ReadCString(out _);
        reader.AlignTo4();
    }

    private bool TryReadProperty(ref BigEndianReader reader, out uint nameOffset, out FdtProperty property) {
        var start = (uint)(reader.Position - _dtb);
        var token = ReadToken(ref reader, out _);

        if (token != TokenProp) {
            reader.GoTo(start);
            nameOffset = 0;
            property = default;

            return false;
        }

        var length = reader.ReadU32();
        nameOffset = reader.ReadU32();
        property = new FdtProperty(reader.Position, length);

        reader.Skip((int)length);
        reader.AlignTo4();

        return true;
    }

    private static void SkipSubtree(ref BigEndianReader reader) {
        var depth = 0;

        while (true) {
            var token = reader.ReadU32();

            switch (token) {
                case TokenBeginNode: {
                    reader.ReadCString(out _);
                    reader.AlignTo4();
                    depth++;

                    break;
                }

                case TokenEndNode: {
                    depth--;
                    if (depth == 0) {
                        return;
                    }

                    break;
                }

                case TokenProp: {
                    var length = reader.ReadU32();
                    reader.ReadU32();
                    reader.Skip((int)length);
                    reader.AlignTo4();

                    break;
                }

                case TokenNop: {
                    break;
                }

                default: {
                    Panic.WithMessage("dtb: unexpected token while skipping node");

                    return;
                }
            }
        }
    }

    private bool TryFindChild(FdtNode parent, string path, int start, int end, out FdtNode found) {
        if (!TryGetFirstChild(parent, out var child)) {
            found = default;

            return false;
        }

        do {
            if (!NameMatches(child, path, start, end)) {
                continue;
            }

            found = child;

            return true;
        } while (TryGetNextSibling(child, out child));

        found = default;

        return false;
    }

    private bool TryFindFirst(FdtNode node, string propertyName, string value, out FdtNode found) {
        if (PropertyContains(node, propertyName, value)) {
            found = node;

            return true;
        }

        if (TryGetFirstChild(node, out var child)) {
            do {
                if (TryFindFirst(child, propertyName, value, out found)) {
                    return true;
                }
            } while (TryGetNextSibling(child, out child));
        }

        found = default;

        return false;
    }

    private bool NameMatches(FdtNode node, string path, int start, int end) {
        var name = GetName(node, out var length);

        var segmentHasUnit = false;
        for (var i = start; i < end; i++) {
            if (path[i] == '@') {
                segmentHasUnit = true;

                break;
            }
        }

        var compareLength = length;
        if (!segmentHasUnit) {
            for (var i = 0; i < length; i++) {
                if (name[i] != '@') {
                    continue;
                }

                compareLength = i;

                break;
            }
        }

        if (compareLength != end - start) {
            return false;
        }

        for (var i = 0; i < compareLength; i++) {
            if (name[i] != (byte)path[start + i]) {
                return false;
            }
        }

        return true;
    }

    private byte* GetString(uint offset, out int length) {
        var start = _dtb + _header.StringsOffset + offset;
        var end = _dtb + _header.StringsOffset + _header.StringsSize;

        length = 0;
        while (start + length < end && start[length] != 0) {
            length++;
        }

        if (start + length >= end) {
            Panic.WithMessage("dtb: unterminated string");
        }

        return start;
    }

    private static bool BytesEqual(byte* bytes, int length, string value) {
        if (length != value.Length) {
            return false;
        }

        for (var i = 0; i < length; i++) {
            if (bytes[i] != (byte)value[i]) {
                return false;
            }
        }

        return true;
    }

    private static ulong ReadCells(ref BigEndianReader reader, uint cells) {
        switch (cells) {
            case 0: {
                return 0;
            }

            case 1: {
                return reader.ReadU32();
            }

            case 2: {
                var high = (ulong)reader.ReadU32();
                var low = reader.ReadU32();

                return high << 32 | low;
            }

            default: {
                Panic.WithMessage("dtb: more than 2 cells");

                return 0;
            }
        }
    }

    private static void Pad(int count) {
        if (count == 0) {
            return;
        }

        var buf = stackalloc byte[count * 2];
        for (var i = 0; i < count * 2; i++) {
            buf[i] = (byte)' ';
        }

        Console.Write(buf, count * 2);
    }
}