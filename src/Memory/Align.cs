namespace Hell.Memory;

internal static class Align {
    internal static ulong RoundDown(ulong value) {
        return (value >> 12) << 12;
    }

    internal static ulong RoundUp(ulong value) {
        return ((value + Paging.PageSize - 1) >> 12) << 12;
    }
}