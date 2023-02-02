using System.Diagnostics;
using System;
using System.Xml;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Linq;

namespace ApiGenerator.Packaging;

public static class PackageUtilities
{
    // TODO put in readme that the dotnet exe has to be in environment variables
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
            return FindProjectFilePath(compilation.SyntaxTrees.First().FilePath);
        }

        return string.Empty;
    }

    private static string FindProjectFilePath(string filePath)
    {
        string[] files = Directory.GetFiles(filePath);

        foreach (string file in files)
        {
            if (Path.GetExtension(file) == ".csproj")
            {
                return file;
            }
        }
        string parentPath = Directory.GetParent(filePath)?.FullName;

        if (parentPath == null)
        {
            return string.Empty;
        }

        return FindProjectFilePath(parentPath);
    }
}
