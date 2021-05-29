using Api.Models.DB;
using Api.Services;
using Flurl;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Api.Models.Internal;

namespace Api.Backends
{
	public class AuthBackend
	{
		private static readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();
		private static readonly SigningCredentials SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Assets.Secrets.JWTSecret), SecurityAlgorithms.HmacSha256Signature);
		public static TokenValidationParameters TokenValidationParameters = new TokenValidationParameters
		{
			
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Assets.Secrets.JWTSecret),
			ValidateIssuer = false,
			ValidateAudience = false,
			RequireExpirationTime = true,
			LifetimeValidator = LifetimeValidator,
		};

		public static void SendUserConfirmationLetter(User user, string token)
		{
			StringBuilder sb = new StringBuilder(Assets.RegistartionEmailTemplate);
			sb.Replace("{username}", user.Username);
			sb.Replace("{server_url}", Url.Combine(Assets.Secrets.ServerUrl, Assets.Config.PathBase));
			sb.Replace("{token}", token);
			if (!MailSender.SendLetter(user.Email, Assets.Misc.RegistrationEmailTitle, sb.ToString()))
			{
				throw new ApiException();
			}
		}

		public static RaTokens GenerateRaTokens(User user, bool persist)
		{
			AuthClaims authClaims = new AuthClaims(user.Id, persist);

			DateTime accessTokenExpirationDate = AccessTokenExpirationFromNow();
			DateTime refreshTokenExpirationDate = RefreshTokenExpirationFromNow();

			JwtToken AccessToken = GenerateJwtToken(authClaims, accessTokenExpirationDate);
			JwtToken RefreshToken = GenerateJwtToken(authClaims, refreshTokenExpirationDate);
			return new RaTokens(AccessToken, RefreshToken);
		}

		private static JwtToken GenerateJwtToken(AuthClaims authClaims, DateTime expiration)
		{
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = authClaims.ToClaimsIdentity(),
				Expires = expiration,
				SigningCredentials = SigningCredentials
			};
			var token = JwtTokenHandler.CreateToken(tokenDescriptor);

			return new JwtToken
			{
				Value = JwtTokenHandler.WriteToken(token),
				ExpirationDate = expiration,
				AuthClaims = authClaims
			};
		}

		public static ClaimsPrincipal ValidateJwtToken(string token) {
			try
			{
				var claimsPrincipal = JwtTokenHandler.ValidateToken(token, TokenValidationParameters, out SecurityToken securityToken);
				if (securityToken is JwtSecurityToken jwtRefrehSecurityToken)
				{
					if (!jwtRefrehSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
					{
						throw new Exception();
					}
				}
				else
				{
					throw new Exception();
				}
				return claimsPrincipal;
			}
			catch
			{
				throw new ApiException();
			}
		}

		public static async Task<JwtToken> GenerateNewAccessToken(string refreshToken, UserService userService)
		{
			ClaimsPrincipal claimsPrincipal = ValidateJwtToken(refreshToken);

			AuthClaims claims = AuthClaims.ParseClaimsPrincipal(claimsPrincipal);

			User user = await userService.Get(claims.UserId);
			if (user == null)
			{
				throw new ApiException();
			}

			DateTime accessTokenExpirationDate = AccessTokenExpirationFromNow();
			return GenerateJwtToken(claims, accessTokenExpirationDate);
		}

		public static string HashPassword(string text, byte[] salt)
		{
			string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password: text,
				salt: salt,
				prf: KeyDerivationPrf.HMACSHA1,
				iterationCount: 10000,
				numBytesRequested: 256 / 8));
			return hashed;
		}

		public static Cookie CreateRefreshTokenCookie(JwtToken jwtToken = null)
		{
			Cookie refreshTokenCookie = CreateTokenCookie(Assets.Secrets.RefreshTokenCookieName, jwtToken);
			refreshTokenCookie.Options.Path = Url.Combine(Assets.Config.PathBase, "/auth");
			return refreshTokenCookie;
		}

		public static Cookie CreateAccessTokenCookie(JwtToken jwtToken = null) =>
			CreateTokenCookie(Assets.Secrets.AccessTokenCookieName, jwtToken);

		private static Cookie CreateTokenCookie(string key, JwtToken jwtToken)
		{
			if (jwtToken == null)
			{
				jwtToken = new JwtToken();
			}
			CookieOptions cookieOptions = new CookieOptions();
			cookieOptions.Expires = jwtToken.AuthClaims.Persist ? jwtToken.ExpirationDate : null;
			//cookieOptions.Expires = DateTime.UtcNow.AddDays(15);

			Cookie cookie = new Cookie
			{
				Key = key,
				Value = jwtToken.Value,
				Options = cookieOptions
			};
			return cookie;
		}

		private static DateTime AccessTokenExpirationFromNow() =>
			DateTime.UtcNow.AddMinutes(Assets.Config.AccessTokenValidityInMinutes);
			//DateTime.UtcNow.AddSeconds(20);

		private static DateTime RefreshTokenExpirationFromNow() =>
			DateTime.UtcNow.AddDays(Assets.Config.RefreshTokenValidityInDays);
			//DateTime.UtcNow.AddSeconds(21);

		private static bool LifetimeValidator(
			DateTime? notBefore,
			DateTime? expires,
			SecurityToken securityToken,
			TokenValidationParameters validationParameters) {
			return !(expires < DateTime.UtcNow);
		}
	}
}