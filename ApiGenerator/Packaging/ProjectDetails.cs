using System.Collections.Generic;

namespace ApiGenerator.Packaging;

public class ProjectDetails
{

    public ProjectDetails(string version, List<PackageDetails> packageReferences)
    {
        Version = string.IsNullOrWhiteSpace(version) ? "1.0.0" : version;
        this.PackageReferences = packageReferences;
    }

    public string Version { get; }
    public List<PackageDetails> PackageReferences { get; set; }

}
