using System;
using System.IO;
using System.Text;
using Nancy;
using Nancy.Authentication.Token;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class SignInAction : IAction
    {
        private readonly ITokenizer _tokenizer;
        private readonly IUserService _userService;
        private readonly ILog _log = LogProvider.For<SignInAction>();

        public SignInAction(ITokenizer tokenizer, IUserService userService)
        {
            _tokenizer = tokenizer;
            _userService = userService;
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

                return
                    module.Negotiate.WithStatusCode(HttpStatusCode.InternalServerError)
                        .WithModel("There was a server error trying to sign in.");
            }


            IUserIdentity user = _userService.ValidateSignInRequest(signInRequest);

            if (user == null)
            {
                _log.Warn("Failed sign in for user '" + signInRequest.Email + "' from IP address " +
                          module.Request.UserHostAddress);

                return
                    module.Negotiate.WithStatusCode(HttpStatusCode.Unauthorized)
                        .WithModel("Incorrect username or password.");
            }

            var token = _tokenizer.Tokenize(user, module.Context);

            _log.Info("User '" + signInRequest.Email + "' signed in from IP address " + module.Request.UserHostAddress);

            return module.Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(new
            {
                token,
                roles = user.Claims
            });
        }
    }
}