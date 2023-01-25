using ApiGenerator.Models;
using System.Collections.Generic;

namespace ApiGenerator.ClientGenerators;

public interface IClientGenerator
{
    void GenerateClient(IEnumerable<ControllerClientDetails> controllerClientDetails);
}
