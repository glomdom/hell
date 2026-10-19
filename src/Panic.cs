using System.Diagnostics.CodeAnalysis;
using Hell.Arch.Riscv64;

namespace Hell;

public class Panic {
    [DoesNotReturn]
    public static void WithMessage(string message) {
        Console.Println("panic: ");
        Console.Println(message);

        unsafe {
            Sbi.Shutdown(true); // maybe write to qemu's test device after as well
        }

        while (true) { }
    }
}