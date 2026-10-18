// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Runtime;
using System.Runtime.CompilerServices;

namespace System;

internal static class RuntimeImports {
    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("RuntimeFailFast")]
    internal static extern void FailFast(string message);
}