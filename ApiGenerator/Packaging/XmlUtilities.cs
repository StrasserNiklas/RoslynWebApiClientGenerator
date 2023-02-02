using System.Xml;
using System.Collections.Generic;

namespace ApiGenerator.Packaging;

public static class XmlUtilities
{
    private static readonly string BaseProjectFilePath = "Packaging/baseProjectFile.csproj";

    public static string CreateProjectFile(List<PackageInformation> packageReferences, string filePath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(BaseProjectFilePath);

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

        doc.Save(filePath);
        return filePath;
    }

    public static List<PackageInformation> ParseClientProjectFilePackageReferences(string projectFilePath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(projectFilePath);
        XmlNodeList packageReferenceNodes = doc.GetElementsByTagName("PackageReference");

        var packageList = new List<PackageInformation>();

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
            var version = packageReferenceNode.Attributes?["Version"]?.Value;

            if (!string.IsNullOrEmpty(packageName) && !string.IsNullOrEmpty(version))
            {
                packageList.Add(new PackageInformation(packageName, version));
            }
        }

        return packageList;
    }
}
