using CareHQ.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CareHQ
{
    /// <summary>
    /// Provides a class for handling reqeuests to CareHQ API endpoints and 
    /// receiving a response from an API object
    /// </summary>
    public class ApiClient
    {

        /// <summary>
        /// Gets the current rate limit - The maximum number of requests per 
        /// second that can be made with the given API key. 
        /// </summary>
        /// <remarks>
        /// NOTE: Rate limiting information is only available after a request 
        /// has been made.
        /// </remarks>
        public float RateLimit { get; private set; }

        /// <summary>
        /// Gets the time of when the current <see cref="RateLimit"/> will 
        /// reset.
        /// </summary>
        /// <remarks>
        /// NOTE: Rate limiting information is only available after a request 
        /// has been made.
        /// </remarks>
        public float RateLimitReset { get; private set; }

        /// <summary>
        /// Gets the number of requests remaining within the current limit 
        /// before the next reset.
        /// </summary>
        /// <remarks>
        /// NOTE: Rate limiting information is only available after a request
        /// has been made.
        /// </remarks>
        public float RateLimitRemaining { get; private set; }

        /// <summary>
        /// The Id of the CareHQ account the API key relates to
        /// </summary>
        private string _accountId;

        /// <summary>
        /// A key used to authenticate API calls to an account
        /// </summary>
        private string _apiKey;

        /// <summary>
        /// A secret used to generate a signature for each API request
        /// </summary>
        private string _apiSecret;

        /// <summary>
        /// The base URL to use when calling the API
        /// </summary>
        private string _apiBaseUrl;

        /// <summary>
        /// The period of time before requests to the API should timeout
        /// </summary>
        private double? _timeout;

        /// <summary>
        /// The <see cref="HttpClient"/> used to send API requests
        /// </summary>
        private HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CareHQ.ApiClient"/>
        /// class 
        /// </summary>
        /// <param name="accountId">The Id of the CareHQ account the API key 
        /// relates to</param>
        /// <param name="apiKey">A key used to authenticate API calls to an 
        /// account</param>
        /// <param name="apiSecret">A secret used to generate a signature for 
        /// each API request</param>
        /// <param name="apiBaseUrl">The base URL to use when calling the 
        /// API</param>
        /// <param name="timeout">The period of time before requests to the API 
        /// should timeout</param>
        public ApiClient(
            string accountId,
            string apiKey,
            string apiSecret,
            string apiBaseUrl = "https://api.carehq.co.uk",
            double timeout = -1
        )
        {
            _accountId = accountId;
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _apiBaseUrl = apiBaseUrl;
            _httpClient = new HttpClient();
            _timeout = timeout;
            if (_timeout > 0)
                _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
        }

        /// <summary>
        /// Makes a request to the API
        /// </summary>
        /// <param name="method">The HTTP method/verb</param>
        /// <param name="path">The endpoint path</param>
        /// <param name="parameters">Query string parameters</param>
        /// <param name="data">Form data</param>
        /// <returns>
        /// Returns a <see cref="JsonDocument"/> object with the 
        /// response data
        /// </returns> 
        public JsonDocument Request(
            HttpMethod method,
            string path,
            MultiValueDict parameters = null,
            MultiValueDict data = null
        )
        {
            // Build the signature
            MultiValueDict signatureData =
                method == HttpMethod.Get ? parameters : data;

            List<string> signatureValues = new List<string>();
            if (signatureData != null)
                foreach (KeyValuePair<string, List<string>> kvp in signatureData)
                {
                    signatureValues.Add(kvp.Key);
                    signatureValues.AddRange(kvp.Value.ToList());
                }

            var signatureBody = string.Join("", signatureValues);

            string timestamp = new DateTimeOffset(DateTime.UtcNow)
                .ToUnixTimeSeconds().ToString();

            string signature = GenerateSignature(
                timestamp,
                signatureBody,
                _apiSecret
            );

            var headers = new List<(string headerKey, string headerValue)>(){
                ("Accept", "application/json"),
                ("X-CareHQ-AccountId", _accountId),
                ("X-CareHQ-APIKey", _apiKey),
                ("X-CareHQ-Signature", signature),
                ("X-CareHQ-Timestamp", timestamp)
            };

            // Make the request
            var url = $"{_apiBaseUrl}/v1/{path}";
            if (parameters != null)
                url += $"?{parameters.ToQueryString()}";

            var request = new HttpRequestMessage(method, url)
            {
                Method = method,
                RequestUri = new Uri(url),
                Content = data?.ToFormBody()
            };

            headers.ForEach(
                header => request.Headers.Add(
                    header.headerKey,
                    header.headerValue
                )
            );

            HttpResponseMessage r = _httpClient.SendAsync(request).Result;

            // Update the rate limit
            if (r.Headers.TryGetValues(
                "X-CareHQ-RateLimit-Limit",
                out var rateLimit)
            )
                RateLimit = float.Parse(rateLimit.First());

            if (r.Headers.TryGetValues(
                "X-CareHQ-RateLimit-Reset",
                out var rateLimitReset)
            )
                RateLimitReset = float.Parse(rateLimitReset.First());

            if (r.Headers.TryGetValues(
                "X-CareHQ-RateLimit-Remaining",
                out var rateLimitRemaining)
            )
                RateLimitRemaining = float.Parse(rateLimitRemaining.First());


            // Handle a successful response
            var jsonData = r.Content.ReadAsStringAsync().Result;
            JsonDocument json = JsonDocument.Parse(jsonData);

            if (
                r.StatusCode == HttpStatusCode.OK ||
                r.StatusCode == HttpStatusCode.NoContent
            )
            {
                return json;
            }

            throw APIException.GetExceptionByStatusCode(
                r.StatusCode,
                json.RootElement.GetPropertyNullable("hint"),
                json.RootElement.GetPropertyNullable("arg_errors")
            );
        }

        /// <summary>
        /// Generates a signature string to be used with an API request
        /// </summary>
        /// <param name="timestamp">The unix timestamp</param>
        /// <param name="signatureBody">The signature body</param>
        /// <param name="apiSecret">The api key</param>
        /// <returns>Returns a SHA-1 hash/digest</returns>
        public string GenerateSignature(
            string timestamp,
            string signatureBody,
            string apiSecret
        )
        {
            SHA1 sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(
                Encoding.UTF8.GetBytes(timestamp + signatureBody + apiSecret)
            );

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}