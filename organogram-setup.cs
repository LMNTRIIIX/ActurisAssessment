#!/usr/bin/dotnet run
// 
// © Copyright 2026 Acturis Ltd.
// 
// Single-file setup for the Organogram coding test, requiring .NET 10 SDK and git.
// Run from a new empty folder: dotnet run organogram-setup.cs

#:property PublishAot=false

using System.Diagnostics;
using System.Text;

#region files

const string Readme = """
# Introduction

You have been provided with a console application project containing a `Program` class and an 
`ExampleData` class. 

The `ExampleData` class contains example CSV data containing organisation contact information in no
 particular order.

* The first column is the Row ID, a unique sequence number assigned to that row.
* The second column links the row to its parent record within the file, which may in turn link to 
another record, etc.

The task is to update the application to display an "Organogram" (a hierarchical view of an 
organisation).

# Objective

* Run through the records and print an "Organogram" in a hierarchical view.
* Within each level of the hierarchy, records should be printed in ascending order by Row ID.
* Code should be able to cater for n-level depth.
* Code should be written in a testable way. If you are familiar with unit tests and/or acceptance 
tests, please include them.

# Example

```
Peter Ndoro, IBM, Managing Director
 -> Jackie Smith, IBM, Assistant Director
 -> Chris Thorpe, IBM, Technical Director
    -> John Major, IBM, Lead Developer
       -> Peter South, IBM, Senior Developer
       -> James McDonald, IBM, Developer
```

# How to submit

When you are ready to submit, run `dotnet run submit.cs` from the root of your solution. This
creates a single file, `submission.patch`, containing your complete solution and commit history.
Email this file back to us - no other files are required.

> Tip: commit your work incrementally as you go. The commit history is part of what we review.

© Copyright 2026 Acturis Ltd.
""";

const string CsvClass = """"

internal static class ExampleData
{
    // © Copyright 2026 Acturis Ltd.
    public const string CompaniesData = """
    19,18,Samantha,McFee,Oracle,Johannesburg,Junior Analyst,0826984512,0113589799,0113589798
    8,7,Christos,Papazian,Microsoft,Johannesburg,IT Manager,0765421456,0115425230,0115425290
    7,0,Mathew,Burke,Microsoft,Johannesburg,Account Manager,0856429873,0115425220,0115425210
    4,33,John,Major,IBM,Johannesburg,Lead Developer,0865429871,0116725452,0116725322
    6,4,James,McDonald,IBM,Johannesburg,Developer,0725496328,0116725452,0116725322
    13,11,Wayne,Wiestra,Oracle,Johannesburg,Technical Executive,0856542256,0113589799,0113589798
    20,18,Ralph,Blake,Oracle,Johannesburg,Junior Analyst,0725698745,0113589799,0113589798
    10,9,Chris,Johnson,Microsoft,Johannesburg,Software Architect,0865496125,0115426589,0115426585
    15,1414,Mike,Davidson,Oracle,Johannesburg,Software Developer,0856545236,0113589799,0113589798
    33,1,Chris,Thorpe,IBM,Johannesburg,Technical Director,0843546837,0116725321,0116725322
    5,4,Peter,South,IBM,Johannesburg,Senior Developer,0835648213,0116725452,0116725322
    17,1414,Manoj,Puri,Oracle,Johannesburg,Software Developer,0726594521,0113589799,0113589798
    16,1414,Lizl,Steck,Oracle,Johannesburg,Software Developer,0765986321,0113589799,0113589798
    11,0,Marco,Gouws,Oracle,Johannesburg,Managing Director,0765983245,0113589741,0113589743
    9,7,Vince,Vaughn,Microsoft,Johannesburg,Senior Manager,0852654875,0115425250,0115425290
    1414,13,John,Lyons,Oracle,Johannesburg,Project Manager,0868751256,0113589799,0113589798
    1,0,Peter,Ndoro,IBM,Johannesburg,Managing Director,0762349827,0116725362,0116725368
    12,11,Johan,Philips,Oracle,Johannesburg,Sales Executive,0896542114,0113589743,0113589743
    18,1414,Caron,Du Preez,Oracle,Johannesburg,Senior Analyst,0826587441,0113589799,0113589798
    2,1,Jackie,Smith,IBM,Johannesburg,Assistant Director,0742546877,0116725351,0116725371
    """;
}
"""";

const string Gitignore = """
# .NET build outputs
[Bb]in/
[Oo]bj/

# Visual Studio
.vs/
*.user
*.suo

# Test results
[Tt]est[Rr]esult*/
*.trx

# Submission patch
submission.patch
""";

const string Submit = """
#!/usr/bin/dotnet run
// 
// © Copyright 2026 Acturis Ltd.
// 
// Run from the root of your solution: dotnet run submit.cs

#:property PublishAot=false

using System.Diagnostics;
using System.Text;

#region consts
const string DoneSuffix = ": Done";
const string ErrorSuffix = ": Error";
const string HorizontalRule = "================================================";
#endregion

#region header
Console.WriteLine("Organogram Coding Test Submission");
Console.WriteLine("© Copyright 2026 Acturis Ltd.");
Console.WriteLine(HorizontalRule);
Console.WriteLine(string.Empty);
#endregion

Console.Write("Submitting solution");

var statusOutput = await RunWithOutputAsync("git", "status", "--porcelain").ConfigureAwait(false) ?? "";
if (!string.IsNullOrWhiteSpace(statusOutput))
{
    Console.WriteLine(ErrorSuffix);
    Console.Error.WriteLine("You have uncommitted changes. Commit everything before submitting:");
    Console.Error.WriteLine(statusOutput);
    return 1;
}

var trackedFiles = await RunWithOutputAsync("git", "ls-files").ConfigureAwait(false) ?? "";
var binaryExts = new HashSet<string> { ".dll", ".exe", ".pdb", ".nupkg", ".zip" };
var binaries = trackedFiles.Split('\n', StringSplitOptions.RemoveEmptyEntries)
    .Where(file => binaryExts.Contains(Path.GetExtension(file)))
    .ToList();
if (binaries.Count > 0)
{
    Console.WriteLine(ErrorSuffix);
    Console.Error.WriteLine("Binary files are tracked in git and must not be submitted:");
    foreach (var file in binaries) Console.Error.WriteLine($"  {file}");
    return 2;
}

if (await RunWithOutputAsync("git", "format-patch", "--root", "--stdout").ConfigureAwait(false) is not string patch)
{
    Console.WriteLine(ErrorSuffix);
    Console.Error.WriteLine("Failed to create submission patch: git format-patch failed");
    return 3;
}

await File.WriteAllTextAsync("submission.patch", patch).ConfigureAwait(false);

Console.WriteLine(DoneSuffix);

#region footer
Console.WriteLine(string.Empty);
Console.WriteLine("Submission complete. File submission.patch created - Please email this file back to Acturis.");
Console.WriteLine(HorizontalRule);
#endregion

return 0;

static async Task<string?> RunWithOutputAsync(string cmd, params string[] cmdArgs)
{
    var processStartInfo = new ProcessStartInfo(cmd) { RedirectStandardOutput = true, StandardOutputEncoding = Encoding.UTF8 };
    foreach (var cmdArg in cmdArgs) processStartInfo.ArgumentList.Add(cmdArg);
    using var process = Process.Start(processStartInfo)!;
    var output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
    await process.WaitForExitAsync().ConfigureAwait(false);
    return process.ExitCode == 0 ? output : null;
}
""";

#endregion

#region consts
const string DoneSuffix = ": Done";
const string ErrorSuffix = ": Error";
const string HorizontalRule = "================================================";
#endregion

#region validation

if (Directory.Exists(".git"))
{
    Console.Error.WriteLine("ERROR: This directory is already a git repository. Run from a new empty folder.");
    return 1;
}

var existingEntries = Directory.GetFileSystemEntries(".")
    .Where(e => !string.Equals(Path.GetFileName(e), "organogram-setup.cs", StringComparison.OrdinalIgnoreCase))
    .ToArray();
if (existingEntries.Length > 0)
{
    Console.Error.WriteLine("ERROR: This directory is not empty. Run from a new empty folder.");
    return 1;
}

if (await RunWithOutputAsync("git", "--version").ConfigureAwait(false) is null)
{
    Console.Error.WriteLine("ERROR: git is not available on PATH. Install git and try again.");
    return 1;
}

if (string.IsNullOrEmpty(await RunWithOutputAsync("git", "config", "--global", "user.email").ConfigureAwait(false)) ||
    string.IsNullOrEmpty(await RunWithOutputAsync("git", "config", "--global", "user.name").ConfigureAwait(false)))
{
    Console.Error.WriteLine("ERROR: git global user.email and user.name must be set before running this script.");
    Console.Error.WriteLine("  git config --global user.email you@example.com");
    Console.Error.WriteLine("  git config --global user.name \"Your Name\"");
    return 1;
}

#endregion

#region header
Console.WriteLine("Organogram Coding Test Setup");
Console.WriteLine("© Copyright 2026 Acturis Ltd.");
Console.WriteLine(HorizontalRule);
Console.WriteLine(string.Empty);
#endregion

#region baseline
Console.Write("Setting up git repository");
await File.WriteAllTextAsync("README.md", Readme).ConfigureAwait(false);
await File.WriteAllTextAsync(".gitignore", Gitignore).ConfigureAwait(false);
await File.WriteAllTextAsync("submit.cs", Submit).ConfigureAwait(false);

await RunAsync("git", "init").ConfigureAwait(false);
await RunAsync("git", "add", ".").ConfigureAwait(false);
await RunAsync("git", "commit", "-m", "Initial distribution files (baseline)").ConfigureAwait(false);
Console.WriteLine(DoneSuffix);

#endregion

#region scaffolding

Console.Write("Creating C# solution");
await RunAsync("dotnet", "new", "sln", "-n", "Organogram", "--format", "slnx").ConfigureAwait(false);
Console.WriteLine(DoneSuffix);

Console.Write("Creating console app project");
await RunAsync("dotnet", "new", "console", "-n", "Organogram", "--use-program-main").ConfigureAwait(false);
await File.AppendAllTextAsync("Organogram/Program.cs", CsvClass).ConfigureAwait(false);
await RunAsync("dotnet", "sln", "Organogram.slnx", "add", "Organogram/Organogram.csproj").ConfigureAwait(false);
Console.WriteLine(DoneSuffix);

Console.Write("Creating unit test project");
await RunAsync("dotnet", "new", "nunit", "-n", "Organogram.Tests").ConfigureAwait(false);
await RunAsync("dotnet", "sln", "Organogram.slnx", "add", "Organogram.Tests/Organogram.Tests.csproj").ConfigureAwait(false);
await RunAsync("dotnet", "add", "Organogram.Tests/Organogram.Tests.csproj", "reference", "Organogram/Organogram.csproj").ConfigureAwait(false);
Console.WriteLine(DoneSuffix);

Console.Write("Committing solution to git");
await RunAsync("git", "add", ".").ConfigureAwait(false);
await RunAsync("git", "commit", "-m", "Scaffold: dotnet new console + nunit (do not modify)").ConfigureAwait(false);
Console.WriteLine(DoneSuffix);

#endregion

#region footer
Console.WriteLine(string.Empty);
Console.WriteLine("Setup complete. Please refer to README.md for instructions on how to complete the coding test.");
Console.WriteLine(HorizontalRule);
#endregion

return 0;

#region helpers

static async Task RunAsync(string cmd, params string[] cmdArgs)
{
    var processStartInfo = new ProcessStartInfo(cmd)
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        StandardOutputEncoding = Encoding.UTF8,
        StandardErrorEncoding = Encoding.UTF8
    };
    foreach (var cmdArg in cmdArgs) processStartInfo.ArgumentList.Add(cmdArg);

    using var process = Process.Start(processStartInfo)!;
    var outputTask = process.StandardOutput.ReadToEndAsync();
    var errorTask = process.StandardError.ReadToEndAsync();

    await process.WaitForExitAsync().ConfigureAwait(false);

    var outputText = await outputTask.ConfigureAwait(false);
    var errorText = await errorTask.ConfigureAwait(false);

    if (process.ExitCode != 0)
    {
        Console.WriteLine(ErrorSuffix);

        if (!string.IsNullOrWhiteSpace(outputText))
        {
            await Console.Error.WriteAsync(outputText).ConfigureAwait(false);
        }
        if (!string.IsNullOrWhiteSpace(errorText))
        {
            await Console.Error.WriteAsync(errorText).ConfigureAwait(false);
        }
        throw new Exception($"'{cmd} {string.Join(' ', cmdArgs)}' failed");
    }
}

static async Task<string?> RunWithOutputAsync(string cmd, params string[] cmdArgs)
{
    var processStartInfo = new ProcessStartInfo(cmd) { RedirectStandardOutput = true, StandardOutputEncoding = Encoding.UTF8 };
    foreach (var cmdArg in cmdArgs) processStartInfo.ArgumentList.Add(cmdArg);
    try
    {
        using var process = Process.Start(processStartInfo)!;
        var output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
        await process.WaitForExitAsync().ConfigureAwait(false);
        return process.ExitCode == 0 ? output : null;
    }
    catch (System.ComponentModel.Win32Exception)
    {
        return null;
    }
}

#endregion