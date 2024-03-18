using System;
using System.Net;
using System.Text.Json;

namespace CareHQ.Exception
{
    [Serializable]
    class RateLimitExceeded : APIException
    {
        public RateLimitExceeded(
            HttpStatusCode statusCode,
            JsonElement? hint,
            JsonElement? argErrors
        ) : base(
                "You have exceeded the number of API requests allowed per " +
                "second.",
                statusCode,
                hint,
                argErrors
            )
        { }
    }
}