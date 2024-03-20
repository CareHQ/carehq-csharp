using System;
using System.Net;
using System.Text;
using System.Text.Json;

namespace CareHQ.Exception
{
    [Serializable]
    public class APIException : System.Exception
    {

        public string message;
        private HttpStatusCode _statusCode;
        private JsonElement? _hint;
        private JsonElement? _argErrors;

        public APIException(
            string message,
            HttpStatusCode statusCode,
            JsonElement? hint,
            JsonElement? argErrors
        ) : base( BuildErrorMessage(message, statusCode, hint, argErrors) )
        {
            this.message = message;
            _statusCode = statusCode;
            _hint = hint;
            _argErrors = argErrors;
        }

        public static System.Exception GetExceptionByStatusCode(
            HttpStatusCode httpStatusCode,
            JsonElement? hint,
            JsonElement? argErrors
        )
        {

            switch (httpStatusCode)
            {
                case HttpStatusCode.BadRequest: // 400
                    return new InvalidRequest(
                        httpStatusCode,
                        hint,
                        argErrors
                    );

                case HttpStatusCode.Unauthorized: // 401
                    return new Unauthorized(
                        httpStatusCode,
                        hint,
                        argErrors
                    );

                case HttpStatusCode.Forbidden: // 403
                case HttpStatusCode.MethodNotAllowed: // 405
                    return new Forbidden(
                        httpStatusCode,
                        hint,
                        argErrors
                    );

                case HttpStatusCode.NotFound: // 404
                    return new NotFound(
                        httpStatusCode,
                        hint,
                        argErrors
                    );

                case (HttpStatusCode)429: // 429
                    return new RateLimitExceeded(
                        httpStatusCode,
                        hint,
                        argErrors
                    );

                default:
                    return new APIException(
                        "Unhandled Exception",
                        httpStatusCode,
                        hint,
                        argErrors
                    );
            }
        }

        public static string BuildErrorMessage(string message, HttpStatusCode statusCode, JsonElement? hint, JsonElement? argErrors)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[{(int)statusCode}] {message}");
            sb.AppendLine("---");

            if( hint != null)
            {
                sb.AppendLine($"Hint: {hint}");
            }

            if( argErrors != null )
            foreach (var jsonElement in ((JsonElement)argErrors).EnumerateObject())
            {
                sb.Append($"- {jsonElement.Name}: ");
                sb.AppendLine($"{String.Join(" ", jsonElement.Value.EnumerateArray())}");
            }

            return sb.ToString();
        }

    }
}