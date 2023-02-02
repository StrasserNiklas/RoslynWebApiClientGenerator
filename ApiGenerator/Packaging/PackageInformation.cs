namespace ApiGenerator.Packaging;

public class PackageInformation
{
    public PackageInformation(string packageName, string versionInfo)
    {
        PackageName = packageName;
        VersionInfo = versionInfo;
    }

    public string PackageName { get; }
    public string VersionInfo { get; }
}
