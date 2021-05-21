using location_sharing_backend.Models.DB;
using location_sharing_backend.Models.Settings;
using location_sharing_backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace location_sharing_backend.Backends
{
	public class AuthBackend
	{

		public static DateTime GenerateJwtTokens(User user, bool persist, HttpResponse httpResponse)
		{
			var jwtTokenHandler = new JwtSecurityTokenHandler();

			var key = Assets.Secrets.JWTSecret;
			var guid = Guid.NewGuid().ToString();
			var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

			DateTime accessTokenExpirationDate = DateTime.UtcNow.AddMinutes(15);
			DateTime refreshTokenExpirationDate = DateTime.UtcNow.AddMonths(2);

			ClaimsIdentity claimsIdentity = AuthClaims.ToClaimsIdentity(user, guid, persist);

			string accessToken = GenerateJwtToken(jwtTokenHandler, claimsIdentity, DateTime.UtcNow.AddMinutes(15), signingCredentials);
			string refreshToken = GenerateJwtToken(jwtTokenHandler, claimsIdentity, DateTime.UtcNow.AddMonths(2), signingCredentials);

			CookieOptions atOptions = new CookieOptions();
			if (persist)
			{
				atOptions.Expires = accessTokenExpirationDate;
			}
			httpResponse.Cookies.Append(Assets.Secrets.AccessTokenCookieName, accessToken, atOptions);

			CookieOptions rtOptions = new CookieOptions();
			if (persist)
			{
				rtOptions.Expires = refreshTokenExpirationDate;
			}
			rtOptions.Path = "/api/auth";
			httpResponse.Cookies.Append(Assets.Secrets.RefreshTokenCookieName, refreshToken, rtOptions);

			return accessTokenExpirationDate;
		}

		private static string GenerateJwtToken(JwtSecurityTokenHandler jwtTokenHandler, ClaimsIdentity claimsIdentity, DateTime expiration, SigningCredentials signingCredentials)
		{
			var refreshTokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = claimsIdentity,
				Expires = expiration,
				SigningCredentials = signingCredentials
			};

			var token = jwtTokenHandler.CreateToken(refreshTokenDescriptor);

			return jwtTokenHandler.WriteToken(token);
		}

		public static async Task<DateTime?> RefreshAccessToken(string refreshToken, UserService userService, HttpResponse httpResponse)
		{
			var jwtTokenHandler = new JwtSecurityTokenHandler();

			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Assets.Secrets.JWTSecret),
				ValidateIssuer = false,
				ValidateAudience = false,
				ValidateLifetime = true,
				RequireExpirationTime = false
			};

			var refreshTokenPrincipals = jwtTokenHandler.ValidateToken(refreshToken, tokenValidationParameters, out var validatedRefreshToken);

			if (validatedRefreshToken is JwtSecurityToken jwtRefrehSecurityToken)
			{
				var result = jwtRefrehSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

				if (result == false)
				{
					return null;
				}
			}

			var expiryClaim = refreshTokenPrincipals.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp);
			if (expiryClaim == null)
			{
				return null;
			}
			var utcExpiryDate = long.Parse(expiryClaim.Value);

			var expDate = UnixTimeStampToDateTime(utcExpiryDate);
			if (DateTime.UtcNow.CompareTo(expDate) >= 0)
			{
				return null;//dont give acces token if refresh token is expired
			}

			AuthClaims claims = AuthClaims.ParseClaimsPrincipal(refreshTokenPrincipals);

			User user = await userService.Get(claims.UserId);
			if (user == null)
			{
				return null;
			}

			var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Assets.Secrets.JWTSecret), SecurityAlgorithms.HmacSha256Signature);
			DateTime AccessTokenExpirationDate = DateTime.UtcNow.AddMinutes(15);
			string AccessToken = GenerateJwtToken(jwtTokenHandler, claims.ToClaimsIdentity(), AccessTokenExpirationDate, signingCredentials);

			CookieOptions atOptions = new CookieOptions();
			if (claims.Persist)
			{
				atOptions.Expires = AccessTokenExpirationDate;
			}
			httpResponse.Cookies.Append(Assets.Secrets.AccessTokenCookieName, AccessToken, atOptions);

			return AccessTokenExpirationDate;
		}

		private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
			return dtDateTime;
		}
	}
}