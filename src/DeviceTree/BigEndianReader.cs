namespace Hell.DeviceTree;

internal unsafe struct BigEndianReader {
    public byte* Position => _pos;
    
    private byte* _pos;
    private readonly byte* _start;
    private readonly byte* _end;

    public BigEndianReader(byte* start, byte* end) {
        _pos = start;

        _start = start;
        _end = end;
    }

    public uint ReadU32() {
        var p = Take(4);
        
        return (uint)p[0] << 24 | (uint)p[1] << 16 | (uint)p[2] << 8 | p[3];
    }
    
    public ulong ReadU64() {
        return (ulong)ReadU32() << 32 | ReadU32();
    }

    public void GoTo(int loc) {
        if (_start + loc > _end) {
            Panic.WithMessage("dtb: attempted goto outside end");
        }

        _pos = _start + loc;
    }
    
    public void GoTo(uint loc) {
        GoTo((int)loc);
    }

    public void GoTo(byte* pos) {
        if (pos > _end || pos < _start) {
            Panic.WithMessage("dtb: attempted goto outside end or start");
        }

        _pos = pos;
    }

    public byte* ReadCString(out int length) {
        var start = _pos;
        while (_pos < _end && *_pos != 0) {
            _pos++;
        }

        length = (int)(_pos - start);

        Take(1); // \0

        return start;
    }

    public void Skip(int count) {
        Take(count);
    }

    public void AlignTo4() {
        var misalign = (int)((ulong)_pos & 0b11);
        if (misalign != 0) {
            Take(4 - misalign);
        }
    }

    private byte* Take(int count) {
        if (_pos + count > _end) {
            Panic.WithMessage("dtb: read past end");
        }

        var p = _pos;
        _pos += count;

        return p;
    }
}