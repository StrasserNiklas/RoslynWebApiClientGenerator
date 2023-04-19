
using NSwag.ComplexTestingApi.CSharp;

namespace TestingClient.ComplexApi.NSwag;

public static class NSwagClassHelper
{
    public static NoPropertiesAttributedClass SimpleClass = new NoPropertiesAttributedClass()
    {
        ExampleInteger = 2023,
        ExampleString = "CombinedSimpleClass"
    };

    public static SomePropertiesHeaderAttributed SomePropertiesHeaderAttributed = new SomePropertiesHeaderAttributed()
    {
        ExampleInteger = 2023,
        ExampleString = "SomePropertiesHeaderAttributed"
    };

    public static AllPropertiesHeaderAttributed AllPropertiesHeaderAttributed = new AllPropertiesHeaderAttributed()
    {
        ExampleInteger = 2023,
        ExampleString = "AllPropertiesHeaderAttributed"
    };

    public static SomePropertiesQueryAttributed SomePropertiesQueryAttributed = new SomePropertiesQueryAttributed()
    {
        ExampleInteger = 2023,
        ExampleString = "SomePropertiesQueryAttributed"
    };

    public static AllPropertiesQueryAttributed AllPropertiesQueryAttributed = new AllPropertiesQueryAttributed()
    {
        ExampleInteger = 2023,
        ExampleString = "AllPropertiesQueryAttributed"
    };

    public static ErrorResponse ErrorResponse = new ErrorResponse()
    {
        ErrorCode = 1234,
        ErrorMessage = "UnknownError"
    };

    public static NotFoundResponse NotFoundResponse = new NotFoundResponse()
    {
        ErrorResponse = new ErrorResponse()
        {
            ErrorCode = 404,
            ErrorMessage = "NotFound"
        },
        Reason = "NotFound"
    };

    public static ServerSideErrorResponse ServerSideErrorResponse = new ServerSideErrorResponse()
    {
        ErrorResponse = new ErrorResponse()
        {
            ErrorCode = 500,
            ErrorMessage = "InternalServerError"
        },
        ServerGuid = Guid.Parse("2c5b3b16-7b75-4d82-9a9e-974ca52bbae2")
    };
}