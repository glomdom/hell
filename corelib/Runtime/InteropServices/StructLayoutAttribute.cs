// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace System.Runtime.InteropServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class StructLayoutAttribute(LayoutKind layoutKind) : Attribute {
    public LayoutKind Value { get; } = layoutKind;

    public int Pack;
    public int Size;
    public CharSet CharSet;
}

