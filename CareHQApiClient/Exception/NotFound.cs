using System;
using System.Net;
using System.Text.Json;

namespace CareHQ.Exception
{
    [Serializable]
    class NotFound : APIException
    {
        public NotFound(
            HttpStatusCode statusCode,
            JsonElement? hint,
            JsonElement? argErrors
        ) : base(
                "The endpoint you are calling or the document you referenced " +
                "doesn't exist.",
                statusCode,
                hint,
                argErrors
            )
        { }
    }
}