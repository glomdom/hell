using System;
using Internal.TypeSystem;

namespace Internal.Runtime.CompilerHelpers;

internal static class ThrowHelpers {
    internal static void ThrowInvalidProgramException(ExceptionStringID id) {
        RuntimeImports.FailFast("invalid program");
    }

    internal static void ThrowInvalidProgramExceptionWithArgument(ExceptionStringID id, string methodName) {
        RuntimeImports.FailFast(methodName);
    }

    internal static void ThrowTypeLoadException(ExceptionStringID id, string className, string typeName) {
        RuntimeImports.FailFast(typeName);
    }

    internal static void ThrowDivideByZeroException() {
        RuntimeImports.FailFast("divide by zero");
    }
}