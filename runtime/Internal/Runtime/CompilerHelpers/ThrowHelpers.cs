using System;
using Internal.TypeSystem;

namespace Internal.Runtime.CompilerHelpers;

internal static class ThrowHelpers {
    internal static void ThrowInvalidProgramException(ExceptionStringID id) {
        throw new InvalidProgramException();
    }

    internal static void ThrowInvalidProgramExceptionWithArgument(ExceptionStringID id, string methodName) {
        throw new InvalidProgramException(methodName);
    }
}