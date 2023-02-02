using System.Diagnostics;
using System;
using System.Xml;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Linq;

namespace ApiGenerator;

public static class PackageUtilities
{
    public static string  CreateClientProjectFile(string projectFilePath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(projectFilePath);

        XmlNodeList packageReferenceNodes = doc.GetElementsByTagName("PackageReference");
        foreach (XmlNode packageReferenceNode in packageReferenceNodes)
        {
            //Console.WriteLine(packageReferenceNode.Attributes["Include"].Value);
        }

        return "";
    }

    // TODO use this to get the project file and thus the project references... use an xml parser maybe so it is prettier
    public static string GetApiProjectName(Compilation compilation)
    {
        if (compilation.SyntaxTrees.Count() != 0)
        {
            return Path.GetFileName(Path.GetDirectoryName(compilation.SyntaxTrees.First().FilePath));
        }

        return string.Empty;
    }

    //private static string GetApiProjectName(Compilation compilation)
    //{
    //    if (compilation.SyntaxTrees.Count() != 0)
    //    {
    //        return Path.GetFileName(Path.GetDirectoryName(compilation.SyntaxTrees.First().FilePath));
    //    }

    //    return string.Empty;
    //}

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
}
