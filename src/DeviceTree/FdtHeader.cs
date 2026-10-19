namespace Hell.DeviceTree;

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