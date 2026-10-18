namespace Internal.TypeSystem;

public enum ExceptionStringID {
    // TypeLoadException
    ClassLoadGeneral,
    ClassLoadExplicitGeneric,
    ClassLoadBadFormat,
    ClassLoadExplicitLayout,
    ClassLoadValueClassTooLarge,
    ClassLoadRankTooLarge,

    ClassLoadInlineArrayFieldCount,
    ClassLoadInlineArrayLength,
    ClassLoadInlineArrayExplicit,
    ClassLoadInlineArrayExplicitSize,

    // MissingMethodException
    MissingMethod,

    // MissingFieldException
    MissingField,

    // FileNotFoundException
    FileLoadErrorGeneric,

    // InvalidProgramException
    InvalidProgramDefault,
    InvalidProgramSpecific,
    InvalidProgramVararg,
    InvalidProgramCallVirtFinalize,
    InvalidProgramNonStaticMethod,
    InvalidProgramGenericMethod,
    InvalidProgramNonBlittableTypes,
    InvalidProgramMultipleCallConv,
    InvalidProgramAsync,

    // BadImageFormatException
    BadImageFormatGeneric,

    // MarshalDirectiveException
    MarshalDirectiveGeneric,

    // AmbiguousMatchException
    AmbiguousMatchUnsafeAccessor,
}