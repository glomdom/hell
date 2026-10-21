// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

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