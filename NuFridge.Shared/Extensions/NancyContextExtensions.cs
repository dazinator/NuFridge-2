using Nancy;
using Nancy.Responses;
using NuFridge.Shared.Server.Web.Nancy;

namespace NuFridge.Shared.Extensions
{
    public static class NancyContextExtensions
    {
        public static Response AsNuFridgeJson<TModel>(this IResponseFormatter formatter, TModel model, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            CustomJsonNetSerializer jsonNetSerializer = new CustomJsonNetSerializer(formatter.Context);
            JsonResponse<TModel> jsonResponse = new JsonResponse<TModel>(model, jsonNetSerializer)
            {
                StatusCode = (statusCode)
            };
            return jsonResponse;
        }
    }
}
