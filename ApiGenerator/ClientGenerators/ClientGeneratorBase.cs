using ApiGenerator.Models;
using System.Collections.Generic;

namespace ApiGenerator.ClientGenerators;

public abstract class ClientGeneratorBase
{
	public ClientGeneratorBase(Configuration configuration, string projectName)
	{
        Configuration = configuration;
        ProjectName = projectName;
    }

    public Configuration Configuration { get; }
    public string ProjectName { get; }

    public abstract void GenerateClient(IEnumerable<ControllerClientDetails> controllerClientDetails);

    public virtual void GenerateClient(ControllerClientDetails controllerClientDetails)
    {
        this.GenerateClient(new List<ControllerClientDetails> { controllerClientDetails });
    }
}
