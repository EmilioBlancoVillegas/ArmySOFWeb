using es.ArmySOF.WebSite.Classes;
using es.ArmySOF.WebSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;

namespace es.ArmySOF.WebSite.Controllers
{
	[AllowAnonymous]
	[RoutePrefix("api/login")]
    public class LoginController : ApiController
    {
		[HttpGet]
		[Route("echoping")]
		public IHttpActionResult EchoPing()
		{
			return Ok(true);
		}

		[HttpGet]
		[Route("echouser")]
		public IHttpActionResult EchoUser()
		{
			var identity = Thread.CurrentPrincipal.Identity;
			return Ok($" IPrincipal-user: {identity.Name} - IsAuthenticated: {identity.IsAuthenticated}");
		}

		[HttpPost]
		[Route("authenticate")]
		public IHttpActionResult Authenticate(LoginRequest login)
		{
			if (login == null)
				throw new HttpResponseException(HttpStatusCode.BadRequest);
			bool isValid = login.Password == "zz123";
			if (isValid)
			{
				var token = TokenGenerator.GenerateTokenJwt(login.UserName);
				return Ok(token);
			}
			else
				return Unauthorized();
		}
    }
}
