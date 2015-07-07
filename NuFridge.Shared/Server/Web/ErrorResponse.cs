using Nancy;
using Nancy.Responses;
using Nancy.Serialization.JsonNet;
using Newtonsoft.Json;
using NuFridge.Shared.Server.Web.Serializers;

namespace NuFridge.Shared.Server.Web
{
    public class ErrorResponse : JsonResponse
    {
        private readonly Error _error;

        public string ErrorMessage
        {
            get
            {
                return _error.ErrorMessage;
            }
        }

        public string FullException
        {
            get
            {
                return _error.FullException;
            }
        }

        public string[] Errors
        {
            get
            {
                return _error.Errors;
            }
        }

        private ErrorResponse(Error error)
            : base(error, new JsonNetSerializer(JsonSerializer.Create(JsonSerialization.GetDefaultSerializerSettings())))
        {
            _error = error;
        }

        public static ErrorResponse FromMessage(HttpStatusCode statusCode, string message)
        {
            ErrorResponse errorResponse = new ErrorResponse(new Error
            {
                ErrorMessage = message
            });
            errorResponse.StatusCode = statusCode;
            return errorResponse;
        }



        public static ErrorResponse BadRequest(params string[] errors)
        {
            return BadRequest((HttpStatusCode)400, errors);
        }

        public static ErrorResponse BadRequest(HttpStatusCode statusCode, params string[] errors)
        {
            ErrorResponse errorResponse = new ErrorResponse(new Error
            {
                ErrorMessage = "There was a problem with your request.",
                Errors = errors ?? new string[0]
            });
            errorResponse.StatusCode = statusCode;
            return errorResponse;
        }




        private class Error
        {
            public string ErrorMessage { get; set; }

            [JsonProperty]
            public string FullException { get; set; }

            [JsonProperty]
            public string[] Errors { get; set; }

            [JsonProperty]
            public string HelpText { get; set; }

            [JsonProperty]
            public string HelpLink { get; set; }
        }
    }
}
