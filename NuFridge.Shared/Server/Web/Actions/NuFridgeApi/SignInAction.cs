using System;
using Nancy;
using Nancy.Authentication.Token;
using Nancy.ModelBinding;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class SignInAction : IAction
    {
        private readonly ITokenizer _tokenizer;

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
            catch (Exception)
            {
                return null;
            }


            var userIdentity = UserDatabase.ValidateUser(signInRequest);

            if (userIdentity == null)
            {
                return HttpStatusCode.Unauthorized;
            }

            var token = _tokenizer.Tokenize(userIdentity, module.Context);

            return new
            {
                Token = token
            };
        }
    }
}