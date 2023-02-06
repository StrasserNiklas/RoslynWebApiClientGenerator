using ApiGenerator.Models;
using System.Collections.Generic;

namespace ApiGenerator.ClientGenerators;

public abstract class ClientGeneratorBase
{
	public ClientGeneratorBase(string projectName)
	{
        ProjectName = projectName;
    }

    public string ProjectName { get; }

    public abstract void GenerateClient(IEnumerable<ControllerClientDetails> controllerClientDetails, string directoryPath);

    public virtual void GenerateClient(ControllerClientDetails controllerClientDetails, string directoryPath)
    {
        this.GenerateClient(new List<ControllerClientDetails> { controllerClientDetails }, directoryPath);
    }
}
