using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;

namespace ApiGenerator.SyntaxAnalysis_unused
{
    public class TestClient
    {
        /// <summary>
        /// The HTTP client.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// The base address.
        /// </summary>
        private readonly string baseAddress;

        /// <summary>
        /// The timeout duration.
        /// </summary>
        private readonly TimeSpan timeoutDuration;

        /// <summary>
        /// The json serializer options.
        /// </summary>
        private readonly JsonSerializerOptions jsonSerializerOptions;

        /// <summary>
        /// To detect redundant calls.
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BetslipClient"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="timeoutDuration">Duration of the timeout.</param>
        /// <param name="maxConnections">The maximum connections.</param>
        /// <exception cref="ArgumentNullException">endpoint.</exception>
        public TestClient(string endpoint, TimeSpan timeoutDuration, int maxConnections = 50)
        {
            this.baseAddress = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            this.timeoutDuration = timeoutDuration;

            this.httpClient = new HttpClient(new HttpClientHandler { MaxConnectionsPerServer = maxConnections, AutomaticDecompression = System.Net.DecompressionMethods.GZip });

            this.httpClient.DefaultRequestHeaders.Accept.Clear();
            this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            this.jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        /// <inheritdoc />
        public Task Test(CancellationToken cancellationToken)
        {
            var endpoint = $"{this.baseAddress}/api/betslip/placebet";
            return this.PostJsonAsync<PlaceBetRequest, PlaceBetResponse>(endpoint, placeBetRequest, cancellationToken);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposedValue)
            {
                return;
            }

            if (disposing)
            {
                this.httpClient.CancelPendingRequests();
                this.httpClient.Dispose();
            }

            this.disposedValue = true;
        }

        private async Task<TResponse> PostJsonAsync<TRequest, TResponse>(string endpoint, TRequest requestObject, CancellationToken cancellationToken)
        {
            using var timeout = new CancellationTokenSource(this.timeoutDuration);

            try
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

                using var request = new HttpRequestMessage();
                request.Method = HttpMethod.Post;

                using var response = await this.httpClient.SendAsync(request, linked.Token);
                //response.EnsureSuccessStatusCode();

                var status = response.StatusCode;

                switch (status)
                {
                    case HttpStatusCode.OK:
                        //
                        break;

                    case HttpStatusCode.NotFound:
                        //
                        break;

                    case HttpStatusCode.InternalServerError:
                        //
                        break;

                    case HttpStatusCode.Unauthorized:
                        //
                        break;

                    default:
                        //
                        break;
                }

                if (status == HttpStatusCode.OK)
                {

                }

                return await response.Content.ReadFromJsonAsync<TResponse>(this.jsonSerializerOptions, linked.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException) when (timeout.IsCancellationRequested)
            {
                throw new TimeoutException($"Did not receive an answer from the betslip service within a timespan of {this.timeoutDuration}.");
            }
        }

        private async Task PostJsonAsync<TRequest>(string endpoint, TRequest request, CancellationToken cancellationToken)
        {
            using var timeout = new CancellationTokenSource(this.timeoutDuration);

            try
            {
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
                using var response = await this.httpClient.PostAsJsonAsync(endpoint, request, linked.Token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (TaskCanceledException) when (timeout.IsCancellationRequested)
            {
                throw new TimeoutException($"Did not receive an answer from the betslip service within a timespan of {this.timeoutDuration}.");
            }
        }
    }
}
