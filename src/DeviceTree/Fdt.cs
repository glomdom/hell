namespace Hell.DeviceTree;

internal struct Fdt {
    internal static FdtHeader ParseHeader(BigEndianReader reader) {
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

        if (version != 17 || lastCompatVersion < 16) {
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

    internal static unsafe void ParseStructs(BigEndianReader reader, FdtHeader header) {
        var depth = 0;

        while (true) {
            var token = reader.ReadU32();
            
            switch (token) {
                case 1: {
                    Pad(depth);
                    Console.Write("BEGIN_NODE ");

                    var name = reader.ReadCString(out var length);
                    reader.AlignTo4();

                    Console.WriteLine(name, length);

                    depth += 1;

                    break;
                }
                
                case 2: {
                    depth -= 1;

                    Pad(depth);
                    Console.WriteLine("END_NODE");
                    
                    break;
                }

                case 3: {
                    var len = reader.ReadU32();
                    var nameOff = reader.ReadU32();
                    var pos = reader.Position;

                    reader.GoTo(header.StringsOffset + nameOff);
                    var name = reader.ReadCString(out var strLength);

                    Pad(depth);
                    Console.Write("PROP `");
                    Console.Write(name, strLength);
                    Console.WriteLine("`");
                    
                    reader.GoTo(pos);
                    reader.Skip((int)len);
                    reader.AlignTo4();
                    
                    break;
                }

                case 4: {
                    break;
                }

                case 9: {
                    return;
                }

                default: {
                    Console.Write("dbt: unhandled token ");
                    Console.WriteLine(token);
                    Panic.WithMessage("dtb: failed to read structs");

                    return;
                }
            }
        }
    }

    private static unsafe void Pad(int count) {
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

internal struct FdtHeader {
    internal uint Magic;
    internal uint TotalSize;
    internal uint StructsOffset;
    internal uint StringsOffset;
    internal uint MemoryReservationOffset;
    internal uint Version;
    internal uint LastCompatibleVersion;
    internal uint BootPhysicalCpuId;
    internal uint StringsSize;
    internal uint StructsSize;
}