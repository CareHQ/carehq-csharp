using System;
using System.Net;
using System.Text.Json;

namespace CareHQ.Exception
{
    [Serializable]
    class Unauthorized : APIException
    {
        public Unauthorized(
            HttpStatusCode statusCode,
            JsonElement? hint,
            JsonElement? argErrors
        ) : base(
                "The API credentials provided are not valid.",
                statusCode,
                hint,
                argErrors
            )
        { }
    }
}