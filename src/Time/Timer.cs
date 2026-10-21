// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Runtime;
using System.Runtime.CompilerServices;
using Hell.Arch.Riscv64;

namespace Hell.Time;

internal static class Timer {
    private static ulong _interval;
    private static ulong _ticks;
    private static ulong _deadline;
    
    // workaround while waiting for https://github.com/dotnet/runtime/issues/136034 to get fixed
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Init(ulong frequency, ulong hz) {
        _interval = frequency / hz;
        _deadline = ReadClock() + _interval;
        
        Sbi.SetTimer(_deadline);
    }

    internal static void OnTick() {
        _ticks++;

        _deadline += _interval;
        Sbi.SetTimer(_deadline);
        
        Console.WriteLine("tick");
    }
    
    [MethodImpl(MethodImplOptions.InternalCall)]
    [RuntimeImport("read_clock")]
    internal static extern ulong ReadClock();
}