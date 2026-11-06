using Hell.Memory;

namespace Hell.Diagnostics;

internal static unsafe class Symbols {
    private const ulong HeaderSize = 16;
    private const ulong EntrySize = 16;
    private const uint NoName = uint.MaxValue;
    private const ushort NoFile = ushort.MaxValue;
 
    private static bool _loaded;
    private static bool _available;
 
    private static byte* _functions;
    private static ulong _functionCount;
    private static byte* _lines;
    private static ulong _lineCount;
    private static uint* _files;
    private static ulong _fileCount;
    private static byte* _strings;
    private static ulong _stringsSize;
    
    internal static void Print(ulong address) {
        Console.WriteHex(address);
 
        if (!TryFind(address, out var function, out var file, out var line)) {
            return;
        }
 
        Console.Write(" ");
        WriteString(function);
 
        if (file == null) {
            return;
        }
 
        Console.Write(" (");
        WriteString(file);
        Console.Write(":");
        Console.Write(line);
        Console.Write(")");
    }
    
    private static bool TryFind(ulong address, out byte* function, out byte* file, out uint line) {
        function = null;
        file = null;
        line = 0;
 
        if (!Load()) {
            return false;
        }
 
        var functionIndex = FindLast(_functions, _functionCount, address);
        if (functionIndex < 0) {
            return false;
        }
 
        function = String(*(uint*)(_functions + (ulong)functionIndex * EntrySize + 8));
        if (function == null) {
            return false;
        }
 
        var lineIndex = FindLast(_lines, _lineCount, address);
        if (lineIndex < 0) {
            return true;
        }
 
        var row = _lines + (ulong)lineIndex * EntrySize;
        var lineNumber = *(uint*)(row + 8);
        var fileIndex = *(ushort*)(row + 12);
 
        if (lineNumber == 0 || fileIndex == NoFile || fileIndex >= _fileCount) {
            return true;
        }
 
        file = String(_files[fileIndex]);
        line = file == null ? 0 : lineNumber;
 
        return true;
    }

    
    private static bool Load() {
        if (_loaded) {
            return _available;
        }
 
        _loaded = true;

        var start = (byte*)Sections.SymbolsStart;
        var end = (byte*)Sections.SymbolsEnd;
 
        if (end < start || (ulong)(end - start) < HeaderSize) {
            return false;
        }
 
        _functionCount = *(uint*)(start + 4);
        _lineCount = *(uint*)(start + 8);
        _fileCount = *(uint*)(start + 12);
 
        _functions = start + HeaderSize;
        _lines = _functions + _functionCount * EntrySize;
        _files = (uint*)(_lines + _lineCount * EntrySize);
        _strings = (byte*)(_files + _fileCount);
 
        if (_strings > end) {
            return false;
        }
 
        _stringsSize = (ulong)(end - _strings);
        _available = true;
 
        return true;
    }
    
    private static byte* String(uint offset) {
        if (offset == NoName || offset >= _stringsSize) {
            return null;
        }
 
        return _strings + offset;
    }
    
    private static long FindLast(byte* table, ulong count, ulong target) {
        long low = 0;
        var high = (long)count - 1;

        long found = -1;

        while (low <= high) {
            var mid = low + (high - low) / 2;
            var address = *(ulong*)(table + (ulong)mid * 16);

            if (address <= target) {
                found = mid;
                low = mid + 1;
            } else {
                high = mid - 1;
            }
        }

        return found;
    }
    
    private static void WriteString(byte* value) {
        var limit = _strings + _stringsSize;
        var length = 0;
 
        while (value + length < limit && value[length] != 0) {
            length++;
        }
 
        Console.Write(value, length);
    }
}