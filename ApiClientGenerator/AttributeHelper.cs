namespace ApiGenerator;

public static class AttributeHelper
{
    public static string FromBodyAttributeNamespace => "Microsoft.AspNetCore.Mvc.FromBodyAttribute";
    public static string FromHeaderAttributebuteNamespace => "Microsoft.AspNetCore.Mvc.FromHeaderAttribute";
    public static string FromQueryAttributeNamespace => "Microsoft.AspNetCore.Mvc.FromQueryAttribute";
    public static string FromFormAttributeNamespace => "Microsoft.AspNetCore.Mvc.FromFormAttribute";
    public static string FromServicesAttributeNamespace => "Microsoft.AspNetCore.Mvc.FromRouteAttribute";
    public static string FromRouteAttributeNamespace => "Microsoft.AspNetCore.Mvc.FromServicesAttribute";
    public static string RouteAttributeNamespace => "Microsoft.AspNetCore.Mvc.RouteAttribute";
    public static string ProducesResponseTypeAttributeNamespace => "Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute";
}
