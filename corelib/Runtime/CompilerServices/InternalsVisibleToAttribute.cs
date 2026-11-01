namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class InternalsVisibleToAttribute : Attribute {
    public InternalsVisibleToAttribute(string assemblyName) {
        AssemblyName = assemblyName;
    }

    public string AssemblyName { get; }
}