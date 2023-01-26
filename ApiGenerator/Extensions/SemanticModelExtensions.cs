using Microsoft.CodeAnalysis;

namespace ApiGenerator.Extensions;

public static class SemanticModelExtensions
{
    // TODO is always true because the metadata is there wtf...
    public static bool ContainsControllerTypes(this SemanticModel semanticModel)
    {
        var controllerBase = semanticModel.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.ControllerBase");
        var controller = semanticModel.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.Controller");

        if (controllerBase is null && controller is null)
        {
            return false;
        }

        return true;
    }
}
