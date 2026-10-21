// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace Hell.DeviceTree;

internal unsafe struct FdtProperty {
    internal readonly byte* Value;
    internal readonly uint Length;

    internal FdtProperty(byte* value, uint length) {
        Value = value;
        Length = length;
    }
}