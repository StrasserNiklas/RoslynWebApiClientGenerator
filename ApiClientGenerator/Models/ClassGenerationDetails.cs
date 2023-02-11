using System.Collections.Generic;

namespace ApiGenerator.Models;

public class ClassGenerationDetails
{
    public ClassGenerationDetails()
    {
        this.AdditionalUsings = new List<string>();
        this.GeneratedCodeClasses = new Dictionary<string, string>();
    }

    public List<string> AdditionalUsings { get; set; }
    public IDictionary<string, string> GeneratedCodeClasses { get; set; }

}
