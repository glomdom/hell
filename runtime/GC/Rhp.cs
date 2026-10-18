// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Runtime;

namespace System.GC;

internal static partial class Rhp {
    [RuntimeExport("RhpReversePInvoke")]
    internal static void RhpReversePInvoke(IntPtr frame) {}

    [RuntimeExport("RhpReversePInvokeReturn")]
    internal static void RhpReversePInvokeReturn(IntPtr frame) {}
}