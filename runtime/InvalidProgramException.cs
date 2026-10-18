namespace System;

public sealed class InvalidProgramException : SystemException {
    public InvalidProgramException() : base("invalid program") {}
    public InvalidProgramException(string? message) : base(message) {}
}