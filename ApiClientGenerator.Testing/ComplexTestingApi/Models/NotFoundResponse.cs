namespace ComplexTestingApi.Models;

public class NotFoundResponse
{
    public string? Reason { get; set; }

    public ErrorResponse? ErrorResponse { get; set; }
}
