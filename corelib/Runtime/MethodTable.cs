// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Runtime.InteropServices;

namespace System.Runtime;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MethodTable {
    private const uint HasComponentSizeFlag = 0x80000000;

    public uint Flags;
    public uint BaseSize;
    public MethodTable* RelatedType;
    public ushort NumVtableSlots;
    public ushort NumInterfaces;
    public uint HashCode;

    public bool HasComponentSize => (Flags & HasComponentSizeFlag) != 0;

    public ushort ComponentSize => HasComponentSize ? (ushort)Flags : (ushort)0;
}