using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using NuFridge.Service.Authentication.Managers;
using NuFridge.Service.Model;
using NuFridge.Service.Model.Dto;

namespace NuFridge.Service.Website.Controllers
{
    public class AccountController : ApiController
    {
        private IdentityManager _repo = null;

        public AccountController()
        {
            _repo = new IdentityManager();
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("api/account/register")]
        [HttpPost]
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

        //TODO change once auth is fully implemented. This is not secure.
        [HttpGet]
        [Route("api/account/{id}")]
        public HttpResponseMessage Get(string id)
        {
            var user = _repo.FindByName(id).Result;

            if (user == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(string.Format("No user with username = {0}", id)),
                    ReasonPhrase = "User not found"
                };
                throw new HttpResponseException(resp);
            }

            var dtoUser = Mapper.Map<DtoApplicationUser>(user);

            return Request.CreateResponse(dtoUser);
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