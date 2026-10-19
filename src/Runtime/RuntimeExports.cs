using System;
using System.Runtime;

namespace Hell.Runtime;

public class RuntimeExports {
    [RuntimeExport("RhpThrowEx")]
    private static void RhpThrowEx(object exception) {
        Panic.WithMessage(((Exception)exception).Message);
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