namespace Hell.DeviceTree;

internal unsafe struct FdtProperty {
    internal readonly byte* Value;
    internal readonly uint Length;

    internal FdtProperty(byte* value, uint length) {
        Value = value;
        Length = length;
    }
}