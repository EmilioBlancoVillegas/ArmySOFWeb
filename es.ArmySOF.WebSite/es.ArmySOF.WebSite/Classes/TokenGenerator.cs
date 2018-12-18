using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace es.ArmySOF.WebSite.Classes
{
	/// <summary>
	/// JWT Token generator class using "secret-key"
	/// more info: https://self-issued.info/docs/draft-ietf-oauth-json-web-token.html
	/// </summary>
	internal static class TokenGenerator
	{
		public static string GenerateTokenJwt(string userName)
		{
			//AppSettings
			var secretKey = ConfigurationManager.AppSettings["JWT_SECRET_KEY"];
			var audienceToken = ConfigurationManager.AppSettings["JWT_AUDIENCE_TOKEN"];
			var issuerToken = ConfigurationManager.AppSettings["JWT_ISSUER_TOKEN"];
			var expireTime = ConfigurationManager.AppSettings["JWT_EXPIRE_MINUTES"];

			var securityKey = new SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(secretKey));
			var singinCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

			//Creation of claims
			ClaimsIdentity claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userName) });

			//Creting token
			var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
			var jwtSecurityToken = tokenHandler.CreateJwtSecurityToken(
				audience: audienceToken,
				issuer: issuerToken,
				subject: claimsIdentity,
				notBefore: DateTime.UtcNow,
				expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(expireTime)),
				signingCredentials: singinCredentials
				);
			var jwtTokenString = tokenHandler.WriteToken(jwtSecurityToken);
			return jwtTokenString;
		}
	}
}