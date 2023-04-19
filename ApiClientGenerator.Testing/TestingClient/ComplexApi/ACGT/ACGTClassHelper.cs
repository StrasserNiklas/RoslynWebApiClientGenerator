using ComplexTestingApi.CSharp;
using TestingContracts.Enums;
using TestingContracts.Models;

namespace TestingClient.ComplexApi.ACGT;

public static class ACGTClassHelper
{
    public static NoPropertiesAttributedClass NoPropertiesAttributed = new NoPropertiesAttributedClass()
    {
        ExampleInteger = 2023,
        ExampleString = "CombinedSimpleClass"
    };

    public static SomePropertiesHeaderAttributed SomePropertiesHeaderAttributed = new SomePropertiesHeaderAttributed()
    {
        ExampleDouble = 5.5,
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

    public static Accessory Accessory = new Accessory()
    {
        AccessoryFeatures = new List<AccessoryFeature>()
                            {
                                new AccessoryFeature()
                                {
                                    Description = "AccessoryFeature1"
                                },
                                new AccessoryFeature()
                                {
                                    Description = "AccessoryFeature2"
                                }
                            },
        AccessoryGuid = Guid.Parse("2c5b3b16-7b75-4d82-9a9e-974ca52bbae2"),
        AccessoryType = AccessoryType.Internal
    };

    public static Car Car = new Car()
    {
        Accessories = new List<Accessory>()
                    {
                        Accessory,
                        new Accessory()
                        {
                            AccessoryFeatures = new List<AccessoryFeature>()
                            {
                                new AccessoryFeature()
                                {
                                    Description = "AccessoryFeature3"
                                },
                                new AccessoryFeature()
                                {
                                    Description = "AccessoryFeature4"
                                }
                            },
                            AccessoryGuid = Guid.Parse("2c5b3b16-7b75-4d82-9a9e-974ca52bbae2"),
                            AccessoryType = AccessoryType.External
                        }
                    },
        CarIdentifier = "CarIdentifier",
        CarImgBase64 = "CarImgBase64",
        EngineType = EngineType.Electric,
        Properties = new Dictionary<string, string>()
                    {
                        { "TestProperty1", "1" },
                        { "TestProperty2", "2" }
                    }
    };

    public static CarPool CarPool = new CarPool()
    {
        Cars = new List<Car>()
            {
                Car
            },
            LocationName = "Test",
            ZipCode = 1234
        };
}
