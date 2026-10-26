namespace Hell.Memory;

internal static unsafe class Addresses {
    public const ulong DirectMapBase = 0xFFFFFFC000000000;

    private static ulong _physOffset = 0;

    internal static void EnableDirectMap() {
        _physOffset = DirectMapBase;
    }
    
    internal static byte* PhysToVirt(ulong physAddr) {
        return (byte*)(physAddr + _physOffset);
    }
    
    internal static byte* VirtToPhys(ulong virtAddr) {
        if (virtAddr >= DirectMapBase) {
            return (byte*)(virtAddr - DirectMapBase);
        }
        
        return (byte*)virtAddr;
    }
}