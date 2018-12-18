﻿using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace es.ArmySOF.WebSite.Classes
{
	/// <summary>
	/// Token validator for Authorization Request using a DelegatingHandler
	/// </summary>
	public class TokenValidationHandler : DelegatingHandler
	{
		private static bool TryRetrieveToken(HttpRequestMessage request, out string token)
		{
			token = null;
			if (!request.Headers.TryGetValues("Authorization", out IEnumerable<string> authzHeaders) || authzHeaders.Count() > 1)
				return false;
			var bearerToken = authzHeaders.ElementAt(0);
			token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;
			return true;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			HttpStatusCode statusCode;
			string token;

			//exists jwt?
			if (!TryRetrieveToken(request, out token))
			{
				statusCode = HttpStatusCode.Unauthorized;
				return base.SendAsync(request, cancellationToken);
			}

			try
			{
				var secretKey = ConfigurationManager.AppSettings["JWT_SECRET_KEY"];
				var audienceToken = ConfigurationManager.AppSettings["JWT_AUDIENCE_TOKEN"];
				var issuerToken = ConfigurationManager.AppSettings["JWT_ISSUER_TOKEN"];
				var securityKey = new SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(secretKey));

				var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
				TokenValidationParameters validationParameters = new TokenValidationParameters()
				{
					ValidAudience = audienceToken,
					ValidIssuer = issuerToken,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					LifetimeValidator = this.LifetimeValidator,
					IssuerSigningKey = securityKey
				};
				SecurityToken securityToken;
				//Assign current principal identity

				Thread.CurrentPrincipal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);
				HttpContext.Current.User = tokenHandler.ValidateToken(token, validationParameters, out securityToken);
				return base.SendAsync(request, cancellationToken);

			}
			catch (SecurityTokenValidationException)
			{
				statusCode = HttpStatusCode.Unauthorized;
			}
			catch (Exception)
			{
				statusCode = HttpStatusCode.InternalServerError;
			}

			return Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(statusCode) { });
		}

		public bool LifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
		{
			if (expires.HasValue)
			{
				if (DateTime.UtcNow < expires.Value)
					return true;
			}
			return false;
		}
	}
}