// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using Hell.Arch.Riscv64;

namespace Hell;

public class Panic {
    [DoesNotReturn]
    public static void WithMessage(string message) {
        Console.Write("panic: ");
        Console.WriteLine(message);

        Sbi.Shutdown(true); // maybe write to qemu's test device after as well

        while (true) { }
    }
}