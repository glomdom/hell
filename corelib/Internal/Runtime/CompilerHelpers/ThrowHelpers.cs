using System;
using Internal.TypeSystem;

namespace Internal.Runtime.CompilerHelpers;

// fixme: make these proper so ilc can output actual helpful throw info
internal static class ThrowHelpers {
    internal static void ThrowInvalidProgramException(ExceptionStringID id) {
        throw new InvalidProgramException();
    }

    internal static void ThrowInvalidProgramExceptionWithArgument(ExceptionStringID id, string methodName) {
        throw new InvalidProgramException(methodName);
    }
    
    internal static void ThrowTypeLoadException(ExceptionStringID id, string className, string typeName) {
        throw new InvalidProgramException(className);
    }
}