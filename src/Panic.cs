// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using Hell.Arch.Riscv64;
using Hell.Diagnostics;

namespace Hell;

public class Panic {
    [DoesNotReturn]
    public static unsafe void WithMessage(string message) {
        Console.Write("panic: ");
        Console.WriteLine(message);

        var fp = StackTrace.ReadFp();
        while (fp != 0) {
            var ra = ((ulong*)fp)[1];
            var callerFp = ((ulong*)fp)[0];

            Console.Write("  at ");
            Symbols.Print(ra - 4);
            Console.WriteLine();

            if (callerFp != 0 && callerFp <= fp) {
                break;
            }

            fp = callerFp;
        }

        Sbi.Shutdown(true); // maybe write to qemu's test device after as well

        while (true) { }
    }
}