namespace System;

public class Exception {
    private readonly string? _message;

    public Exception(string? message) {
        _message = message;
    }

    public virtual string Message => _message ?? "Exception was thrown. <no message provided>";
}