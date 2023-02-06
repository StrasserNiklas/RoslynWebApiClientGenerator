using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
namespace ApiGenerator.Packaging;

public static class PackageUtilities
{
    public static void CreateNugetPackage(string projectFilePath)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"pack {projectFilePath}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            // TODO maybe we use diagnostic here
            throw new Exception($"Failed to create NuGet package: {output}");
        }
    }

    public static string GetApiProjectName(Compilation compilation)
    {
        if (compilation.SyntaxTrees.Count() != 0)
        {
            var directoryPath = FindProjectFileDirectory(compilation.SyntaxTrees.First().FilePath);
            return FindProjectFilePath(directoryPath);
        }

        return string.Empty;
    }

    public static string FindProjectFileDirectory(string filePath)
    {
        string[] files = Directory.GetFiles(Path.GetDirectoryName(filePath), "*.csproj");

        if (files.Count() != 0)
        {
            return Path.GetDirectoryName(filePath);
        }

        string parentPath = Directory.GetParent(filePath)?.FullName;

        if (parentPath == null)
        {
            return string.Empty;
        }

        return FindProjectFileDirectory(parentPath);
    }

    private static string FindProjectFilePath(string directoryPath)
    {
        string[] files = Directory.GetFiles(directoryPath, "*.csproj");

        if (files.Count() != 0)
        {
            return files.First();
        }

        return string.Empty;
    }

    public static string GetProjectVersionInformation(string directoryPath)
    {
        var gitBranchName = string.Empty;

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("git.exe");
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = directoryPath;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.Arguments = "rev-parse --abbrev-ref HEAD";

            var branchNameProcess = new Process
            {
                StartInfo = startInfo
            };

            branchNameProcess.Start();

            string branchname = branchNameProcess.StandardOutput.ReadLine();

            if (string.IsNullOrWhiteSpace(branchname))
            {
                return gitBranchName;
            }

            gitBranchName = branchname;

            startInfo.Arguments = "log -1 --format='%ct'";
            var branchTimestampProcess = new Process
            {
                StartInfo = startInfo
            };

            branchTimestampProcess.Start();

            // simply using Trim only trims the beginning ' and also adds \n for whatever reason
            string commitTimestampString = branchTimestampProcess.StandardOutput.ReadToEnd().Trim('\'').Trim().TrimEnd('\'');

            if (!string.IsNullOrWhiteSpace(commitTimestampString))
            {
                gitBranchName = $"1.0.{commitTimestampString}-{gitBranchName}";
            }
        }
        catch (Exception)
        {
        }

        return gitBranchName;
    }
}
