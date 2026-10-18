// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace System;

public sealed class String {
    public int Length => _length;

    private readonly int _length;
    private char _first;

    [IndexerName("Chars")]
    public unsafe char this[int index] {
        get {
            if ((uint)index >= (uint)_length) {
                RuntimeImports.FailFast("index out of range");

                return '\0';
            }

            fixed (char* p = &_first) {
                return p[index];
            }
        }
    }

    public ref readonly char GetPinnableReference() {
        return ref _first;
    }
}