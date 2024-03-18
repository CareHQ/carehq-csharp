using System;
using System.Net;
using System.Text.Json;

namespace CareHQ.Exception
{
    [Serializable]
    class InvalidRequest : APIException
    {
        public InvalidRequest(
            HttpStatusCode statusCode,
            JsonElement? hint,
            JsonElement? argErrors
        ) : base(
                "Not a valid request, most likely a missing or invalid " +
                "parameters.",
                statusCode,
                hint,
                argErrors
            )
        { }
    }
}