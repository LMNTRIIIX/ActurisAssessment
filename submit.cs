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