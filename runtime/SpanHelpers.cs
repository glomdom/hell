// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace System;

internal static class SpanHelpers {
    public static void ClearWithoutReferences(ref byte dest, nuint len) {}

    internal static void Memmove(ref byte dest, ref byte src, nuint len) {}
    internal static void Fill(ref byte dest, byte value, nuint len) {}

}