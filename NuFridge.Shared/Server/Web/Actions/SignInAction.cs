using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Authentication.Token;
using Nancy.ModelBinding;

namespace NuFridge.Shared.Server.Web.Actions
{
    public class SignInAction : IAction
    {
        private readonly ITokenizer _tokenizer;

        public SignInAction(ITokenizer tokenizer)
        {
            _tokenizer = tokenizer;
        }

        public dynamic Execute(INancyModule module)
        {
            SignInRequest signInRequest;

            try
            {
                signInRequest = module.Bind<SignInRequest>();
            }
            catch (Exception ex)
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