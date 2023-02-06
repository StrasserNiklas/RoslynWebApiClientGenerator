using System.Collections.Generic;

namespace ApiGenerator.Packaging;

public class PackageDetails
{
    public PackageDetails(string packageName, string versionInfo)
    {
        PackageName = packageName;
        VersionInfo = versionInfo;
        this.Namespaces = new List<string>();
    }

    public string PackageName { get; }
    public string VersionInfo { get; }
    public IEnumerable<string> Namespaces { get; set; }

}
