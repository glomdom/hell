using System.Runtime;
using Hell.Time;

namespace Hell.Arch.Riscv64;

public static class Traps {
    [RuntimeExport("trap_handler")]
    private static unsafe void TrapHandler(TrapFrame* frame) {
        var isInterrupt = (frame->Scause >> 63) != 0;
        var code = (frame->Scause << 1) >> 1;

        if (isInterrupt) {
            switch (code) {
                case 5: {
                    Timer.OnTick();

                    return;
                }

                default: {
                    DumpFrame(frame, code);

                    Console.Write("trap: unexpected interrupt ");
                    Console.WriteLine(code);
                    Panic.WithMessage("trap: unexpected interrupt");

                    return;
                }
            }
        }

        switch (code) {
            case 3: {
                Console.Write("trap: unreachable code at ");
                Console.WriteHexLine(frame->Sepc);

                return;
            }

            default: {
                DumpFrame(frame, code);
                Panic.WithMessage("trap: unhandled exception");
                
                return;
            }
        }
    }

    private static unsafe void DumpFrame(TrapFrame* frame, ulong code) {
        Console.Write("exception: ");
        Console.WriteLine(ExceptionCodeToFriendly(code));

        Console.Write("sepc    ");
        Console.WriteHexLine(frame->Sepc);
        Console.Write("stval   ");
        Console.WriteHexLine(frame->Stval);
        Console.Write("sstatus ");
        Console.WriteHexLine(frame->Sstatus);

        var regs = &frame->Zero;
        for (var i = 1; i < 32; i++) {
            Console.Write(RegisterName(i));
            Console.Write(" ");
            Console.WriteHex(regs[i]);

            if ((i & 3) == 3) {
                Console.WriteLine();
            } else {
                Console.Write("  ");
            }
        }
    }

    private static string RegisterName(int index) {
        return index switch {
            1 => "ra ", 2 => "sp ", 3 => "gp ", 4 => "tp ",
            5 => "t0 ", 6 => "t1 ", 7 => "t2 ",
            8 => "s0 ", 9 => "s1 ",
            10 => "a0 ", 11 => "a1 ", 12 => "a2 ", 13 => "a3 ",
            14 => "a4 ", 15 => "a5 ", 16 => "a6 ", 17 => "a7 ",
            18 => "s2 ", 19 => "s3 ", 20 => "s4 ", 21 => "s5 ",
            22 => "s6 ", 23 => "s7 ", 24 => "s8 ", 25 => "s9 ",
            26 => "s10", 27 => "s11",
            28 => "t3 ", 29 => "t4 ", 30 => "t5 ", 31 => "t6 ",
            _ => "?? ",
        };
    }

    private static string ExceptionCodeToFriendly(ulong code) {
        return code switch {
            0 => "instruction address misaligned",
            1 => "instruction access fault",
            2 => "illegal instruction",
            3 => "breakpoint",
            4 => "load address misaligned",
            5 => "load access fault",
            6 => "store address misaligned",
            7 => "store access fault",
            8 => "ecall from U - mode",
            9 => "ecall from S - mode",
            12 => "instruction page fault",
            13 => "load page fault",
            15 => "store page fault",

            _ => "unknown exception code",
        };
    }
}