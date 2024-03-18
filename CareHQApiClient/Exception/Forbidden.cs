using System;
using System.Net;
using System.Text.Json;

namespace CareHQ.Exception
{
    [Serializable]
    class Forbidden : APIException
    {
        public Forbidden(
            HttpStatusCode statusCode,
            JsonElement? hint,
            JsonElement? argErrors
        ) : base(
            "The request is not not allowed, most likely the HTTP method used " +
            "to call the API endpoint is incorrect or the API key (via its " +
            "associated account) does not have permission to call the " +
            "endpoint and/or perform the action.",
            statusCode,
            hint,
            argErrors)
        { }
    }
}