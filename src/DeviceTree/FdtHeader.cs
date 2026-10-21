// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

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