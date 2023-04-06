using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ApiGenerator.Packaging;

public static class XmlUtilities
{
    /*
    TODO Until this is fixed, we will use a string literal of it
    Could not find a part of the path 'C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\Roslyn\Packaging\baseProjectFile.csproj'.'
     */
    private static readonly string BaseProjectFilePath = "Packaging/baseProjectFile.csproj";
    private static readonly string ProjectString = """
        <Project Sdk="Microsoft.NET.Sdk">
        	<PropertyGroup>
        		<TargetFramework>netstandard2.0</TargetFramework>
        		<ImplicitUsings>enable</ImplicitUsings>
        		<Nullable>enable</Nullable>
        		<LangVersion>latest</LangVersion>
        		<IsPackable>true</IsPackable>
        	</PropertyGroup>

            <ItemGroup>
        	    <PackageReference Include="System.Text.Json" Version="7.0.2" />
            </ItemGroup>
        </Project>
        """;

    public static string CreateProjectFile(IEnumerable<PackageDetails> packageReferences, string fileDirectory, string fileName, string version = "1.0.0")
    {
        XmlDocument doc = new XmlDocument();

        doc.Load(new MemoryStream(Encoding.UTF8.GetBytes(ProjectString)));
        //doc.Load(BaseProjectFilePath);

        // add package version
        XmlNode propertyGroupNode = doc.CreateElement("PropertyGroup");
        doc.DocumentElement.AppendChild(propertyGroupNode);
        XmlNode packageVersionNode = doc.CreateElement("PackageVersion");
        packageVersionNode.InnerText = version;
        propertyGroupNode.AppendChild(packageVersionNode);

        // add package references
        XmlNode itemGroupNode = doc.CreateElement("ItemGroup");
        doc.DocumentElement.AppendChild(itemGroupNode);

        foreach (var packageReference in packageReferences)
        {
            XmlNode packageReferenceNode = doc.CreateElement("PackageReference");
            XmlAttribute includeAttribute = doc.CreateAttribute("Include");
            XmlAttribute versionAttribute = doc.CreateAttribute("Version");
            includeAttribute.Value = packageReference.PackageName;
            versionAttribute.Value = packageReference.VersionInfo;
            packageReferenceNode.Attributes.Append(includeAttribute);
            packageReferenceNode.Attributes.Append(versionAttribute);
            itemGroupNode.AppendChild(packageReferenceNode);
        }

        var filePath = $"{fileDirectory}\\{fileName}";
        doc.Save(filePath);
        return filePath;
    }

    public static ProjectDetails ParseClientProjectFilePackageReferences(string projectFilePath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(projectFilePath);
        XmlNodeList packageReferenceNodes = doc.GetElementsByTagName("PackageReference");

        var version = GetProjectVersionInformation(doc);

        var packageList = new List<PackageDetails>();

        foreach (XmlNode packageReferenceNode in packageReferenceNodes)
        {
            bool hasPrivateAssetsSetToAll = false;

            foreach (XmlNode childNode in packageReferenceNode.ChildNodes)
            {
                if (childNode.Name == "PrivateAssets" && childNode.InnerText == "all")
                {
                    hasPrivateAssetsSetToAll = true;
                    break;
                }
            }

            if (hasPrivateAssetsSetToAll)
            {
                continue;
            }

            var packageName = packageReferenceNode.Attributes?["Include"]?.Value;
            var packageVersion = packageReferenceNode.Attributes?["Version"]?.Value;

            if (!string.IsNullOrEmpty(packageName) && !string.IsNullOrEmpty(packageVersion))
            {
                packageList.Add(new PackageDetails(packageName, packageVersion));
            }
        }

        return new ProjectDetails(version, packageList);
    }

    private static string GetProjectVersionInformation(XmlDocument xmlDocument)
    {
        var version = string.Empty;

        XmlNodeList versionNodes = xmlDocument.GetElementsByTagName("Version");

        if (versionNodes.Count == 0)
        {
            XmlNodeList versionPrefixNodes = xmlDocument.GetElementsByTagName("VersionPrefix");

            if (versionPrefixNodes.Count != 0)
            {
                version += versionPrefixNodes[0].InnerText;
            }

            XmlNodeList versionSuffixNodes = xmlDocument.GetElementsByTagName("VersionSuffix");

            if (versionSuffixNodes.Count != 0 && !string.IsNullOrWhiteSpace(version))
            {
                version += "-" + versionPrefixNodes[0].InnerText;
            }
        }
        else
        {
            version = versionNodes[0].InnerText;
        }

        return version;
    }
}
