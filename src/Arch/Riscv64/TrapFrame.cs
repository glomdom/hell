// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Runtime.InteropServices;

namespace Hell.Arch.Riscv64;

[StructLayout(LayoutKind.Sequential)]
internal struct TrapFrame {
    internal ulong Zero, Ra, Sp, Gp, Tp;
    internal ulong T0, T1, T2;
    internal ulong S0, S1;
    internal ulong A0, A1, A2, A3, A4, A5, A6, A7;
    internal ulong S2, S3, S4, S5, S6, S7, S8, S9, S10, S11;
    internal ulong T3, T4, T5, T6;
    internal ulong Sepc, Sstatus, Scause, Stval;
}
