using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;

using NuFridge.Service.Authentication;
using NuFridge.Service.Authentication.Managers;
using NuFridge.Service.Authentication.Model;
using NuFridge.Service.Data;

namespace NuFridge.Service.Api.Controllers
{
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private IdentityManager _repo = null;

        public AccountController()
        {
            _repo = new IdentityManager();
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(ApplicationUser userModel, string password)
        {
            IdentityResult result = await _repo.RegisterUser(userModel, password);

            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }

            return Ok();
        }


        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}