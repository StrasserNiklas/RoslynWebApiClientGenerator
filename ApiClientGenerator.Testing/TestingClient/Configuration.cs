namespace TestingClient;

public static class HttpClientHelper
{
    public static HttpClient CreateHttpClient() => new HttpClient() { BaseAddress = new Uri("https://localhost:7205") };
}