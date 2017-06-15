using System.Runtime.CompilerServices;
using System.Reflection;

[assembly: InternalsVisibleTo("Pingu.Tests")]
[assembly: InternalsVisibleTo("Pingu.Benchmarks")]

[assembly: AssemblyCompany("Code, Rinse, Repeat")]

[assembly: AssemblyVersion(
    ThisAssembly.Git.SemVer.Major + "." +
    ThisAssembly.Git.SemVer.Minor
)]

[assembly: AssemblyFileVersion(
    ThisAssembly.Git.SemVer.Major + "." +
    ThisAssembly.Git.SemVer.Minor + "." +
    ThisAssembly.Git.SemVer.Patch
)]

[assembly: AssemblyInformationalVersion(
    ThisAssembly.Git.SemVer.Major + "." +
    ThisAssembly.Git.SemVer.Minor + "." +
    ThisAssembly.Git.SemVer.Patch + "-" +
    ThisAssembly.Git.Branch + "+" +
    ThisAssembly.Git.Commit
)]