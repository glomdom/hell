namespace Hell.DeviceTree;

internal struct FdtNode {
    internal readonly uint Offset;
    internal readonly uint AddressCells;
    internal readonly uint SizeCells;

    internal FdtNode(uint offset, uint addressCells, uint sizeCells) {
        Offset = offset;
        AddressCells = addressCells;
        SizeCells = sizeCells;
    }
}