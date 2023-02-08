using System.Collections.Generic;

namespace ApiGenerator.Packaging;

public class PackageDetails
{
    public PackageDetails(string packageName, string versionInfo = "latest")
    {
        PackageName = packageName;
        VersionInfo = versionInfo;
        this.Namespaces = new List<string>();
    }

    public string PackageName { get; }
    public string VersionInfo { get; set; }
    public IEnumerable<string> Namespaces { get; set; }

}
