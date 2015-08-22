using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Nancy;
using Nancy.Authentication.Token;
using Nancy.ModelBinding;
using Nancy.Responses;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class SignInAction : IAction
    {
        private readonly ITokenizer _tokenizer;
        private readonly ILog _log = LogProvider.For<SignInAction>();

        public SignInAction(ITokenizer tokenizer)
        {
            _tokenizer = tokenizer;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            SignInRequest signInRequest;
            
            try
            {
                signInRequest = module.Bind<SignInRequest>();
            }
            catch (Exception ex)
            {
                _log.WarnException("There was a server error trying to sign in.", ex);
                return new Response
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Contents =
                        delegate(Stream stream)
                        {
                            var byteData =
                                Encoding.UTF8.GetBytes(
                                    "There was a server error trying to sign in. See the server logs for more information.");
                            stream.Write(byteData, 0, byteData.Length);
                        }
                };
            }


            var userIdentity = UserDatabase.ValidateUser(signInRequest);

            if (userIdentity == null)
            {
                _log.Warn("Failed sign in for user '" + signInRequest.Email + "' from IP address " +
                          module.Request.UserHostAddress);

                var response = new Response
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Contents =
                        delegate(Stream stream)
                        {
                            var byteData = Encoding.UTF8.GetBytes("The provided credentials are incorrect. Please try again.");
                            stream.Write(byteData, 0, byteData.Length);
                        }
                };

                response.ReasonPhrase = "Test";

                return response;
            }

            var token = _tokenizer.Tokenize(userIdentity, module.Context);

            return new
            {
                token
            };
        }
    }
}