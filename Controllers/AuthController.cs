using System.Threading.Tasks;
using Api.Models.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Api.Backends;
using Api.Services;
using MongoDB.Driver;
using Api.Models.IO.Auth;
using Api.Models.Internal;

namespace Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly UserService userService;
		private readonly UserVerificationService userVerificationService;

		public AuthController(UserService _userService, UserVerificationService _userVerificationService)
		{
			userService = _userService;
			userVerificationService = _userVerificationService;
		}

		[HttpPost("register")]
		public async Task<ActionResult> Register([FromBody] RegisterIn registartionData)
		{
			if (await userService.CheckIfUssernameExists(registartionData.Username))
			{
				throw new ApiException(Assets.ErrorCodes.UserWithUsernameAlreadyExists);
			}

			if (await userService.CheckIfEmailExists(registartionData.Email))
			{
				throw new ApiException(Assets.ErrorCodes.UserWithEmailAlreadyExists);
			}

			User user = new User
			{
				Username = registartionData.Username,
				Password = AuthBackend.HashPassword(registartionData.Password, Assets.Secrets.Salt),
				Email = registartionData.Email,
			};

			string verificationToken = Common.GenerateAlphaNumericString(32);

			AuthBackend.SendUserConfirmationLetter(user, verificationToken);

			userService.Create(user);
			UserVerification userVerification = new UserVerification
			{
				Token = verificationToken,
				User = new MongoDBRef(Assets.DbInfo.Collections.Users, user.Id),
			};
			userVerificationService.Create(userVerification);
			return Ok();
		}

		[HttpGet("verify")]
		public async Task<ActionResult> VerifyUser([FromQuery] VerifyUserIn data)
		{
			UserVerification verification = await userVerificationService.FindByToken(data.Token);
			if (verification == null)
			{
				return base.Content(Assets.UserValidationErrorPage, "text/html");
			}
			userVerificationService.Remove(verification.Id);
			return base.Content(Assets.UserValidationSuccessPage, "text/html");
		}

		[HttpPost("login")]
		public async Task<ActionResult> Login([FromBody] LoginIn loginData)
		{
			User user = await userService.GetByUsername(loginData.Username);
			if (user == null)
			{
				throw new ApiException(Assets.ErrorCodes.IncorrectLoginCredentials);
			}

			string hashedPassword = AuthBackend.HashPassword(loginData.Password, Assets.Secrets.Salt);
			if (user.Password != hashedPassword)
			{
				throw new ApiException(Assets.ErrorCodes.IncorrectLoginCredentials);
			}

			UserVerification userVerification = await userVerificationService.FindByUser(user);
			if (userVerification != null)
			{
				AuthBackend.SendUserConfirmationLetter(user, userVerification.Token);
				throw new ApiException(Assets.ErrorCodes.UnverifiedUser);
			}

			RaTokens raTokens = AuthBackend.GenerateRaTokens(user, loginData.Persist);

			Cookie assessTokenCookie = AuthBackend.CreateAccessTokenCookie(raTokens.AccessTokenData);
			assessTokenCookie.AppendToResponse(Response);

			Cookie refreshTokenCookie = AuthBackend.CreateRefreshTokenCookie(raTokens.RefreshTokenData);
			refreshTokenCookie.AppendToResponse(Response);

			LoginOut loginOut = new LoginOut(user, raTokens);
			return Ok(loginOut);
		}

		[HttpPost("logout")]
		public IActionResult Logout()
		{
			AuthBackend.CreateAccessTokenCookie().DeleteFromResponse(Response);
			AuthBackend.CreateRefreshTokenCookie().DeleteFromResponse(Response);
			return Ok();
		}

		[HttpPost("refresh")]
		public async Task<IActionResult> RefreshToken()
		{
			HttpContext.Request.Cookies.TryGetValue(Assets.Secrets.RefreshTokenCookieName, out string? refreshToken);
			if (refreshToken == null)
			{
				throw new ApiException();
			}

			JwtToken accessToken = await AuthBackend.GenerateNewAccessToken(refreshToken, userService);

			Cookie assessTokenCookie = AuthBackend.CreateAccessTokenCookie(accessToken);
			assessTokenCookie.AppendToResponse(Response);
			return Ok(new RefreshOut(accessToken.ExpirationDate));
		}
	}
}
