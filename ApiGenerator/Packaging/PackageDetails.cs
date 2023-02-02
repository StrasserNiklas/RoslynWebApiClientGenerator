namespace ApiGenerator.Packaging;

public class PackageDetails
{
    public PackageDetails(string packageName, string versionInfo)
    {
        PackageName = packageName;
        VersionInfo = versionInfo;
    }

    public string PackageName { get; }
    public string VersionInfo { get; }
}
