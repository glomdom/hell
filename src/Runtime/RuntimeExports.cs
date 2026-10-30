// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Runtime;

namespace Hell.Runtime;

public class RuntimeExports {
    [RuntimeExport("RhpThrowEx")]
    private static void RhpThrowEx(object exception) {
        Panic.WithMessage(((Exception)exception).Message);
    }
    
    [RuntimeExport("RhpNewFast")]
    private static void RhpNewFast() {
        Panic.WithMessage("alloc: new called");
    }
    
    [RuntimeExport("RhpAssignRefRiscV64")]
    private static void RhpAssignRefRiscV64() {
        Panic.WithMessage("alloc: ref assign");
    }
    
    [RuntimeExport("RhpNewPtrArrayFast")]
    private static void RhpNewPtrArrayFast() {
        Panic.WithMessage("alloc: new array called");
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