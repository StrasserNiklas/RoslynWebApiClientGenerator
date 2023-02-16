namespace TestingPlayground.Models
{
    public class ServerSideErrorResponse
    {
        public ErrorResponse ErrorResponse { get; set; }

        public Guid? ServerGuid { get; set; }
    }
}
