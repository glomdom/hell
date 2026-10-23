// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics;

var sdk = Environment.GetEnvironmentVariable("DOTNET_SDK_DIR") ?? throw new Exception("missing DOTNET_SDK_DIR");
var csc = Path.Combine(sdk, "Roslyn", "bincore", "csc.dll");
var ilc = Environment.GetEnvironmentVariable("ILC") ?? throw new Exception("missing ILC");
var ld = OperatingSystem.IsWindows() ? "ld.lld.exe" : "ld.lld"; // maybe unhardcode this

const string Out = "build";

string[] cscFlags = ["/nologo", "/noconfig", "/nostdlib", "/unsafe", "/nullable", "/debug:portable", "/langversion:latest", "/runtimemetadataversion:v4.0.30319"];
string[] asmFlags = ["--target=riscv64", "-march=rv64gc", "-mabi=lp64d"];

var command = args.Length > 0 ? args[0] : "build";

switch (command) {
    case "build": {
        Build();

        break;
    }

    case "run": {
        Build();
        Run("qemu-system-riscv64", ["-machine", "virt", "-bios", "default", "-kernel", $"{Out}/hell.elf", "-serial", "stdio", "-monitor", "tcp:127.0.0.1:4444,server,nowait", "-no-shutdown"]);

        break;
    }

    case "clean": {
        if (Directory.Exists(Out)) {
            Directory.Delete(Out, recursive: true);
        }

        break;
    }

    default: {
        Fail($"unknown command '{command}'");

        break;
    }
}

void Build() {
    Directory.CreateDirectory(Out);

    var corelibSources = Sources("corelib", ".cs");
    var corelib = $"{Out}/corelib.dll";
    if (Stale(corelib, corelibSources)) {
        Step("csc corelib", "dotnet", [csc, ..cscFlags, "/target:library", $"/out:{corelib}", ..corelibSources]);
    }

    var kernelSources = Sources("src", ".cs");
    var kernelIl = $"{Out}/kernel.ilexe";
    if (Stale(kernelIl, [..kernelSources, corelib])) {
        Step("csc kernel", "dotnet", [csc, ..cscFlags, "/target:library", $"/out:{kernelIl}", $"/r:{corelib}", ..kernelSources]);
    }

    var kernelObj = $"{Out}/kernel.o";
    if (Stale(kernelObj, [kernelIl, corelib])) {
        Step("ilc kernel.o", ilc, [kernelIl, corelib, "-o", kernelObj, "--nativelib", "-g", "--systemmodule", "corelib", "--targetos", "linux", "--targetarch", "riscv64", "-O", "--map", $"{Out}/kernel.map", "--jitpath", @"C:\src\runtime\artifacts\bin\coreclr\windows.x64.Checked\clrjit_unix_riscv64_x64.dll"]);
    }

    List<string> objects = [kernelObj];
    foreach (var source in Sources("src", ".S")) {
        var obj = $"{Out}/{Path.GetFileNameWithoutExtension(source)}.o";
        if (Stale(obj, [source])) {
            Step($"clang {Path.GetFileName(source)}", "clang", [..asmFlags, "-c", source, "-o", obj]);
        }

        objects.Add(obj);
    }

    var elf = $"{Out}/hell.elf";
    if (Stale(elf, [..objects, "src/linker.ld"])) {
        Step("ld.lld hell.elf", ld, ["-T", "src/arch/riscv64/linker.ld", "-o", elf, ..objects]);
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("  done");
    Console.ResetColor();
}

string[] Sources(string dir, string extension) {
    return [
        ..Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories)
            .Where(f => Path.GetExtension(f).Equals(extension, StringComparison.OrdinalIgnoreCase))
    ];
}

bool Stale(string output, IEnumerable<string> inputs) {
    if (!File.Exists(output)) {
        return true;
    }

    var built = File.GetLastWriteTimeUtc(output);

    return inputs.Any(i => File.GetLastWriteTimeUtc(i) > built);
}

void Step(string label, string exe, IEnumerable<string> arguments) {
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"  {label}");
    Console.ResetColor();

    var code = Run(exe, arguments);
    if (code != 0) {
        Fail($"{label} (exit {code})");
    }
}

int Run(string exe, IEnumerable<string> arguments) {
    var info = new ProcessStartInfo(exe);
    foreach (var argument in arguments) {
        info.ArgumentList.Add(argument);
    }

    using var process = Process.Start(info)!;
    process.WaitForExit();

    return process.ExitCode;
}

void Fail(string message) {
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  failed: {message}");
    Console.ResetColor();
    Environment.Exit(1);
}