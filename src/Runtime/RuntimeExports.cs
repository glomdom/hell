// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime;
using Hell.Memory;

namespace Hell.Runtime;

public class RuntimeExports {
    [RuntimeExport("RhpThrowEx")]
    private static void RhpThrowEx(object exception) {
        Panic.WithMessage(((Exception)exception).Message);
    }
    
    [RuntimeExport("RhpNewFast")]
    private static unsafe void* RhpNewFast(MethodTable* methodTable) {
        var slot = Slab.Alloc(methodTable->BaseSize);
        var obj = slot + 8;
        *(MethodTable**)obj = methodTable;

        return obj;
    }
    
    [RuntimeExport("RhpNewPtrArrayFast")]
    private static unsafe void* RhpNewPtrArrayFast(MethodTable* methodTable, int length) {
        if (length < 0) {
            throw new IndexOutOfRangeException(); // change this to overflow exception
        }

        var size = methodTable->BaseSize + (ulong)length * methodTable->ComponentSize;

        var slot = Slab.Alloc(size);
        var obj = slot + 8;
        *(MethodTable**)obj = methodTable;
        *(int*)(obj + 8) = length;

        return obj;
    }

    [RuntimeExport("RuntimeFailFast")]
    private static void FailFast(string message) {
        Panic.WithMessage(message);
    }

    [RuntimeExport("RhpFallbackFailFast")]
    private static void RhpFallbackFailFast() {
        Panic.WithMessage((string)"stack corruption");
    }
}