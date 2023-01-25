using ApiGenerator.Extensions;
using ApiGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiGenerator.ClientGenerators;

public class CSharpClientGenerator : IClientGenerator
{
    public void GenerateClient(IEnumerable<ControllerClientDetails> controllerClientDetails)
    {
        // TODO for now place all clients into single file
        // if all clients are in one file, merge its generated (possible duplicates) classes 

        var mergedCodeClasses = controllerClientDetails
            .Select(client => client.GeneratedCodeClasses)
            .Merge();


        // TODO decide where to write to (out folder?)
        // write to file
    }
}
