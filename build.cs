// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

Console.OutputEncoding = Encoding.UTF8;

const string Out = "build";
const string LinkerScript = "src/Arch/Riscv64/linker.ld";
const int LabelWidth = 24;

// symbol table blob: sentinel values
const uint NoName = uint.MaxValue;
const ushort NoFile = ushort.MaxValue;

var sdk = Env("DOTNET_SDK_DIR");
var csc = Path.Combine(sdk, "Roslyn", "bincore", "csc.dll");
var ilc = Env("ILC");
var jit = Env("ILC_JIT", @"C:\src\runtime\artifacts\bin\coreclr\windows.x64.Checked\clrjit_unix_riscv64_x64.dll");
var clang = Env("CLANG", Tool("clang"));
var lld = Env("LLD", Tool("ld.lld"));
var nm = Env("LLVM_NM", Tool("llvm-nm"));
var dwarfdump = Env("LLVM_DWARFDUMP", Tool("llvm-dwarfdump"));
var qemu = Env("QEMU", Tool("qemu-system-riscv64"));

string[] cscFlags = ["/nologo", "/noconfig", "/nostdlib", "/unsafe", "/nullable", "/debug:portable", "/langversion:latest", "/runtimemetadataversion:v4.0.30319"];
string[] asmFlags = ["--target=riscv64", "-march=rv64gc", "-mabi=lp64d"];

// ilc prefixes every symbol with its module name: kernel_Hell_..., corelib_System_...
string[] modules = ["kernel", "corelib"];

var linkerMarker = new Regex(@"^_*(text|rodata|data|bss|kernel)_(start|end)$");
var directoryEntry = new Regex(@"^include_directories\[\s*(\d+)\]\s*=\s*""(.*)""$");
var fileEntry = new Regex(@"^file_names\[\s*(\d+)\]:$");

var command = args.Length > 0 ? args[0] : "build";
var total = Stopwatch.StartNew();

switch (command) {
    case "build": {
        Banner("build");
        Build();
        Finish(total);

        break;
    }

    case "run": {
        Banner("run");
        Build();
        Finish(total);
        Section("qemu");

        var code = Run(qemu,
            ["-machine", "virt", "-bios", "default", "-kernel", $"{Out}/hell.elf", "-serial", "stdio", "-monitor", "tcp:127.0.0.1:4444,server,nowait", "-no-shutdown"]);
        Environment.Exit(code);

        break;
    }

    case "clean": {
        Banner("clean");

        if (Directory.Exists(Out)) {
            Directory.Delete(Out, recursive: true);
            Status('Y', ConsoleColor.Green, "removed", $"{Out}/");
        } else {
            Status('.', ConsoleColor.DarkGray, "nothing to clean", "");
        }

        break;
    }

    default: {
        Fail($"unknown command '{command}' (expected build, run or clean)");

        break;
    }
}

void Build() {
    Directory.CreateDirectory(Out);

    var corelibSources = Sources("corelib", ".cs");
    var corelib = $"{Out}/corelib.dll";
    Produce("csc corelib", corelib, corelibSources, "dotnet",
        [csc, ..cscFlags, "/target:library", $"/out:{corelib}", ..corelibSources]);

    var kernelSources = Sources("src", ".cs");
    var kernelIl = $"{Out}/kernel.ilexe";
    Produce("csc kernel", kernelIl, [..kernelSources, corelib], "dotnet",
        [csc, ..cscFlags, "/target:library", $"/out:{kernelIl}", $"/r:{corelib}", ..kernelSources]);

    var kernelObject = $"{Out}/kernel.o";
    Produce("ilc kernel.o", kernelObject, [kernelIl, corelib], ilc,
    [
        kernelIl, corelib, "-o", kernelObject, "--nativelib", "-g", "--systemmodule", "corelib", "--targetos", "linux", "--targetarch", "riscv64", "-O", "--map",
        $"{Out}/kernel.map", "--jitpath", jit
    ]);

    List<string> objects = [kernelObject];
    foreach (var source in Sources("src", ".S")) {
        var obj = $"{Out}/{Path.GetFileNameWithoutExtension(source)}.o";
        Produce($"clang {Path.GetFileName(source)}", obj, [source], clang, [..asmFlags, "-c", source, "-o", obj]);
        objects.Add(obj);
    }

    Link(objects);
}

// Two-pass link:
//   1. link with an empty symbol table (stage 1)
//   2. read functions (llvm-nm) and line rows (llvm-dwarfdump) from stage 1, pack them into symbols.bin
//   3. link again with symbols.bin in .rodata
// .text comes before .rodata and relaxation is off, so code addresses can't change between the passes.
// That's checked at the end anyway.
void Link(List<string> objects) {
    var elf = $"{Out}/hell.elf";
    if (!Stale(elf, [..objects, LinkerScript])) {
        Skip("ld.lld hell.elf");

        return;
    }

    var stubSource = $"{Out}/symbols.stub.S";
    var stubObject = $"{Out}/symbols.stub.o";
    WriteIfChanged(stubSource, Encoding.UTF8.GetBytes(SymbolsAsm(null)));
    Produce("clang symbols (stub)", stubObject, [stubSource], clang, [..asmFlags, "-c", stubSource, "-o", stubObject]);

    var stage1 = $"{Out}/hell.stage1.elf";
    Step("ld.lld stage 1", lld, LinkArgs(stage1, [..objects, stubObject]));

    var timer = Stopwatch.StartNew();
    var symbols = ReadSymbols(stage1);
    var functions = Functions(symbols);
    var textEnd = symbols.FirstOrDefault(s => s.Name == "_text_end")?.Address;
    var lines = ReadLines(stage1, out var files);
    var blob = SymbolTable(functions, textEnd, lines, files);

    var blobPath = $"{Out}/symbols.bin";
    WriteIfChanged(blobPath, blob);
    Status('S', ConsoleColor.Cyan, "symbols",
        $"{functions.Count} functions . {lines.Count} lines . {files.Count} files . {Size(blob.Length)} . {Elapsed(timer)}");

    var symbolsSource = $"{Out}/symbols.S";
    var symbolsObject = $"{Out}/symbols.o";
    WriteIfChanged(symbolsSource, Encoding.UTF8.GetBytes(SymbolsAsm(blobPath)));
    Produce("clang symbols", symbolsObject, [symbolsSource, blobPath], clang, [..asmFlags, "-c", symbolsSource, "-o", symbolsObject]);

    Step("ld.lld hell.elf", lld, LinkArgs(elf, [..objects, symbolsObject]));
}

string[] LinkArgs(string output, IEnumerable<string> inputs) {
    return ["-T", LinkerScript, "--no-relax", "-o", output, ..inputs];
}

string SymbolsAsm(string? blob) {
    var asm = new StringBuilder();
    asm.AppendLine("// generated by build.cs, do not edit");
    asm.AppendLine("  .section .rodata.hellsym, \"a\"");
    asm.AppendLine("  .balign 8");
    asm.AppendLine("  .globl __hell_symbols");
    asm.AppendLine("__hell_symbols:");

    if (blob is not null) {
        asm.AppendLine($"  .incbin \"{Path.GetFullPath(blob).Replace('\\', '/')}\"");
    }

    asm.AppendLine("  .globl __hell_symbols_end");
    asm.AppendLine("__hell_symbols_end:");

    return asm.ToString();
}

List<Symbol> ReadSymbols(string elf) {
    var output = CaptureOrFail("llvm-nm", nm, ["-n", "--defined-only", elf]);
    var symbols = new List<Symbol>();

    foreach (var raw in output.Split('\n')) {
        var parts = raw.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 || parts[1].Length != 1) {
            continue;
        }

        if (!ulong.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var address)) {
            continue;
        }

        symbols.Add(new Symbol(address, parts[1][0], parts[2]));
    }

    return symbols;
}

List<Function> Functions(List<Symbol> symbols) {
    var functions = new List<Function>();

    foreach (
        var symbol in from symbol in symbols
        where symbol.Kind is 'T' or 't'
        where !symbol.Name.StartsWith(".L", StringComparison.Ordinal) && !linkerMarker.IsMatch(symbol.Name)
        where functions.Count <= 0 || functions[^1].Address != symbol.Address
        select symbol
    ) {
        functions.Add(new Function(symbol.Address, symbol.Name));
    }

    return functions;
}

List<LineRow> ReadLines(string elf, out List<string> files) {
    var output = CaptureOrFail("llvm-dwarfdump", dwarfdump, ["--debug-line", elf]);

    var fileIds = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase);
    var fileList = new List<string>();
    var directories = new Dictionary<int, string>();
    var tableFiles = new Dictionary<int, ushort>();
    var rows = new List<LineRow>();
    var sequenceStart = 0;

    var pendingIndex = -1;
    string? pendingName = null;

    foreach (var raw in output.Split('\n')) {
        var line = raw.Trim();
        if (line.StartsWith("debug_line[", StringComparison.Ordinal)) {
            directories.Clear();
            tableFiles.Clear();

            continue;
        }

        var directory = directoryEntry.Match(line);
        if (directory.Success) {
            directories[int.Parse(directory.Groups[1].Value, CultureInfo.InvariantCulture)] = Unescape(directory.Groups[2].Value);

            continue;
        }

        var file = fileEntry.Match(line);
        if (file.Success) {
            pendingIndex = int.Parse(file.Groups[1].Value, CultureInfo.InvariantCulture);
            pendingName = null;

            continue;
        }

        if (pendingIndex >= 0 && line.StartsWith("name:", StringComparison.Ordinal)) {
            pendingName = Unescape(Quoted(line));

            continue;
        }

        if (pendingIndex >= 0 && pendingName is not null && line.StartsWith("dir_index:", StringComparison.Ordinal)) {
            var dirIndex = int.Parse(line["dir_index:".Length..].Trim(), CultureInfo.InvariantCulture);
            var path = directories.TryGetValue(dirIndex, out var dir) ? Path.Combine(dir, pendingName) : pendingName;

            tableFiles[pendingIndex] = FileId(path);
            pendingIndex = -1;

            continue;
        }

        if (!line.StartsWith("0x", StringComparison.Ordinal)) {
            continue;
        }

        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4) {
            continue;
        }

        var address = ulong.Parse(parts[0].AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        if (line.Contains("end_sequence", StringComparison.Ordinal)) {
            var end = new LineRow(address, 0, NoFile);
            if (rows.Count > sequenceStart && rows[^1].Address == address) {
                rows[^1] = end;
            } else {
                rows.Add(end);
            }

            sequenceStart = rows.Count;

            continue;
        }

        var lineNumber = uint.Parse(parts[1], CultureInfo.InvariantCulture);
        var fileIndex = int.Parse(parts[3], CultureInfo.InvariantCulture);
        var id = tableFiles.GetValueOrDefault(fileIndex, NoFile);

        rows.Add(new LineRow(address, lineNumber, id));
    }

    files = fileList;

    return Compact(rows);

    ushort FileId(string path) {
        var relative = Path.GetRelativePath(Directory.GetCurrentDirectory(), path).Replace('\\', '/');
        if (fileIds.TryGetValue(relative, out var existing)) {
            return existing;
        }

        var id = (ushort)fileList.Count;
        fileIds[relative] = id;
        fileList.Add(relative);

        return id;
    }
}

// sort by address and check shared addresses
List<LineRow> Compact(List<LineRow> rows) {
    var result = new List<LineRow>();

    foreach (var row in rows.OrderBy(r => r.Address)) {
        if (result.Count > 0 && result[^1].Address == row.Address) {
            if (IsEnd(row) && !IsEnd(result[^1])) {
                continue;
            }

            result[^1] = row;

            continue;
        }

        if (result.Count > 0 && result[^1].Line == row.Line && result[^1].File == row.File) {
            continue;
        }

        result.Add(row);
    }

    return result;
}

bool IsEnd(LineRow row) {
    return row.Line == 0 && row.File == NoFile;
}

//   header     u32 functionCount, u32 lineCount, u32 fileCount
//   functions  functionCount  : { u64 address,   u32 nameOffset, u32 pad }           sorted
//   lines      lineCount      : { u64 address,   u32 line,       u16 file, u16 pad } sorted
//   files      fileCount      : { u32 nameOffset }
//   strings    NUL-terminated and relative offsets
byte[] SymbolTable(List<Function> functions, ulong? textEnd, List<LineRow> lines, List<string> files) {
    var strings = new MemoryStream();
    var interned = new Dictionary<string, uint>();

    var entries = new List<(ulong Address, uint Name)>();
    foreach (var function in functions) {
        entries.Add((function.Address, Intern(Demangle(function.Name))));
    }

    // past the last function there is no function
    if (textEnd is { } end && (entries.Count == 0 || entries[^1].Address < end)) {
        entries.Add((end, NoName));
    }

    var fileNames = files.Select(Intern).ToList();

    using var blob = new MemoryStream();
    using var writer = new BinaryWriter(blob);

    writer.Write(0u);
    writer.Write((uint)entries.Count);
    writer.Write((uint)lines.Count);
    writer.Write((uint)fileNames.Count);

    foreach (var (address, name) in entries) {
        writer.Write(address);
        writer.Write(name);
        writer.Write(0u);
    }

    foreach (var row in lines) {
        writer.Write(row.Address);
        writer.Write(row.Line);
        writer.Write(row.File);
        writer.Write((ushort)0);
    }

    foreach (var name in fileNames) {
        writer.Write(name);
    }

    writer.Write(strings.ToArray());
    writer.Flush();

    return blob.ToArray();

    uint Intern(string value) {
        if (interned.TryGetValue(value, out var offset)) {
            return offset;
        }

        offset = (uint)strings.Length;
        interned[value] = offset;
        strings.Write(Encoding.UTF8.GetBytes(value));
        strings.WriteByte(0);

        return offset;
    }
}

string Demangle(string symbol) {
    foreach (var module in modules) {
        var prefix = module + "_";
        if (!symbol.StartsWith(prefix, StringComparison.Ordinal)) {
            continue;
        }

        var rest = symbol[prefix.Length..];
        var split = rest.IndexOf("__", StringComparison.Ordinal);
        if (split <= 0) {
            return rest;
        }

        var type = rest[..split].Replace('_', '.');
        var method = rest[(split + 2)..];
        if (method.StartsWith('_')) {
            method = "." + method[1..];
        }

        return $"{type}.{method}";
    }

    return symbol;
}

string Unescape(string value) {
    return value.Replace(@"\\", @"\");
}

string Quoted(string line) {
    var first = line.IndexOf('"');
    var last = line.LastIndexOf('"');

    return first >= 0 && last > first ? line[(first + 1)..last] : line;
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

bool WriteIfChanged(string path, byte[] content) {
    if (File.Exists(path) && File.ReadAllBytes(path).AsSpan().SequenceEqual(content)) {
        return false;
    }

    File.WriteAllBytes(path, content);

    return true;
}

void Produce(string label, string output, IEnumerable<string> inputs, string exe, IEnumerable<string> arguments) {
    if (Stale(output, inputs)) {
        Step(label, exe, arguments);
    } else {
        Skip(label);
    }
}

void Step(string label, string exe, IEnumerable<string> arguments) {
    var live = !Console.IsOutputRedirected;
    if (live) {
        Write($"  R {label}", ConsoleColor.DarkGray);
    }

    var timer = Stopwatch.StartNew();
    var result = Capture(exe, arguments);

    if (live) {
        Console.Write('\r');
    }

    if (result.Code != 0) {
        Status('X', ConsoleColor.Red, label, $"exit {result.Code}");
        Diagnostics(result);
        Fail($"{label} failed");
    }

    Status('S', ConsoleColor.Cyan, label, Elapsed(timer));
    Diagnostics(result);
}

string CaptureOrFail(string label, string exe, IEnumerable<string> arguments) {
    var result = Capture(exe, arguments);
    if (result.Code != 0) {
        Status('X', ConsoleColor.Red, label, $"exit {result.Code}");
        Diagnostics(result);
        Fail($"{label} failed");
    }

    return result.Stdout;
}

ProcessResult Capture(string exe, IEnumerable<string> arguments) {
    var info = new ProcessStartInfo(exe) {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
    };

    foreach (var argument in arguments) {
        info.ArgumentList.Add(argument);
    }

    try {
        using var process = Process.Start(info)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        return new ProcessResult(process.ExitCode, stdout.Result, stderr.Result);
    } catch (Win32Exception) {
        Fail($"couldn't start '{exe}', is it on PATH?");

        return null!;
    }
}

int Run(string exe, IEnumerable<string> arguments) {
    var info = new ProcessStartInfo(exe);
    foreach (var argument in arguments) {
        info.ArgumentList.Add(argument);
    }

    try {
        using var process = Process.Start(info)!;
        process.WaitForExit();

        return process.ExitCode;
    } catch (Win32Exception) {
        Fail($"couldn't start '{exe}', is it on PATH?");

        return -1;
    }
}

void Diagnostics(ProcessResult result) {
    foreach (var raw in $"{result.Stdout}\n{result.Stderr}".Split('\n')) {
        var line = raw.Trim();
        if (line.Length == 0) {
            continue;
        }

        var color = line.Contains("error", StringComparison.OrdinalIgnoreCase) ? ConsoleColor.Red
            : line.Contains("warning", StringComparison.OrdinalIgnoreCase) ? ConsoleColor.Yellow
            : ConsoleColor.DarkGray;

        Write("    | ", ConsoleColor.DarkGray);
        Write(line, color);
        Console.WriteLine();
    }
}

void Banner(string verb) {
    Write("hell", ConsoleColor.Red);
    Write($" {verb}", ConsoleColor.DarkGray);

    Console.WriteLine();
}

void Section(string name) {
    Console.WriteLine();

    Write($"-- {name} ", ConsoleColor.DarkGray);
    Write(new string('-', Math.Max(0, 40 - name.Length)), ConsoleColor.DarkGray);

    Console.WriteLine();
}

void Status(char glyph, ConsoleColor color, string label, string detail) {
    Write($"  {glyph} ", color);
    Write(label.PadRight(LabelWidth), color == ConsoleColor.DarkGray ? ConsoleColor.DarkGray : ConsoleColor.White);
    Write(detail, ConsoleColor.DarkGray);

    Console.WriteLine();
}

void Skip(string label) {
    Status('.', ConsoleColor.DarkGray, label, "up to date");
}

void Finish(Stopwatch timer) {
    Write("  Y ", ConsoleColor.Green);
    Write("hell.elf", ConsoleColor.White);
    Write($" in {Elapsed(timer)}", ConsoleColor.DarkGray);

    Console.WriteLine();
}

void Write(string text, ConsoleColor color) {
    Console.ForegroundColor = color;
    Console.Write(text);
    Console.ResetColor();
}

string Elapsed(Stopwatch timer) {
    var ms = timer.Elapsed.TotalMilliseconds;

    return ms < 1000
        ? $"{ms.ToString("0", CultureInfo.InvariantCulture)} ms"
        : $"{(ms / 1000).ToString("0.0", CultureInfo.InvariantCulture)} s";
}

string Size(long bytes) {
    return bytes < 1024
        ? $"{bytes} B"
        : $"{(bytes / 1024.0).ToString("0.0", CultureInfo.InvariantCulture)} KiB";
}

string Tool(string name) {
    return OperatingSystem.IsWindows() ? $"{name}.exe" : name;
}

string Env(string name, string? fallback = null) {
    var value = Environment.GetEnvironmentVariable(name);
    if (!string.IsNullOrEmpty(value)) {
        return value;
    }

    if (fallback is null) {
        Fail($"missing environment variable {name}");
    }

    return fallback;
}

[DoesNotReturn]
void Fail(string message) {
    Write("  X ", ConsoleColor.Red);
    Write(message, ConsoleColor.Red);

    Console.WriteLine();
    Environment.Exit(1);

    throw new UnreachableException();
}

internal record ProcessResult(int Code, string Stdout, string Stderr);

internal record Symbol(ulong Address, char Kind, string Name);

internal record Function(ulong Address, string Name);

internal record LineRow(ulong Address, uint Line, ushort File);