using System;
using System.Threading.Tasks;
using location_sharing_backend.Models.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using APIUtils;
using location_sharing_backend.Backends;
using location_sharing_backend.Services;
using MongoDB.Driver;
using location_sharing_backend.Models.IO.Auth;
using System.Text;

namespace location_sharing_backend.Controllers
{
	[ApiController]
	[Route(Settings.URL_PREFIX + "[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly UserService userService;
		private readonly UserVerificationService userVerificationService;
		private readonly MailSender mailSender;

		public AuthController(UserService _userService, UserVerificationService _userVerificationService, MailSender _mailSender)
		{
			userService = _userService;
			userVerificationService = _userVerificationService;
			mailSender = _mailSender;
		}

		[HttpPost("register")]
		public async Task<ActionResult> Register([FromBody] RegisterIn registartionData)
		{
			registartionData.Validate();
			if (await userService.CheckIfUssernameExists(registartionData.Username))
			{
				throw new APIException(APIErrorCode.USERNAME_TAKEN);
			}
			if (await userService.CheckIfEmailExists(registartionData.Email))
			{
				throw new APIException(APIErrorCode.EMAIL_TAKEN);
			}

			string registrationToken = Common.GenerateAlphaNumericString(32);

			StringBuilder sb = new StringBuilder(Assets.RegistartionEmailTemplate);
			sb.Replace("{username}", registartionData.Username);
			sb.Replace("{server_url}", Assets.Secrets.ServerUrl);
			sb.Replace("{token}", registrationToken);
			if (!mailSender.SendLetter(registartionData.Email, Assets.OtherSettings.RegistrationEmailTitle, sb.ToString()))
			{
				return BadRequest();
			}

			User user = new User()
			{
				Username = registartionData.Username,
				Password = Common.hashText(registartionData.Password, Assets.Secrets.Salt),
				Email = registartionData.Email,
			};
			userService.Create(user);

			UserVerification userVerification = new UserVerification()
			{
				Token = registrationToken,
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
			loginData.Validate();
			if (User.Identity != null && User.Identity.IsAuthenticated)
			{
				throw new APIException(APIErrorCode.ALREADY_LOGGED_IN);
			}

			User user = await userService.GetByUsername(loginData.Username);
			if (user == null)
			{
				return BadRequest();
			}

			UserVerification userVerification = await userVerificationService.FindByUser(user);
			if (userVerification != null)
			{
				return BadRequest();
			}

			string hashedPassword = Common.hashText(loginData.Password, Assets.Secrets.Salt);
			if (user.Password != hashedPassword)
			{
				return BadRequest();
			}

			LoginOut loginOut = new LoginOut(user, AuthBackend.GenerateJwtTokens(user, loginData.Persist, Response));

			CookieOptions rtOptions = new CookieOptions();

			return Ok(loginOut);
		}

		[HttpPost("logout")]
		public IActionResult Logout()
		{
			Response.Cookies.Delete(Assets.Secrets.AccessTokenCookieName);
			CookieOptions rtOptions = new CookieOptions();
			rtOptions.Path = "/api/auth";
			Response.Cookies.Delete(Assets.Secrets.RefreshTokenCookieName, rtOptions);
			return Ok();
		}

		[HttpPost("refresh")]
		public async Task<IActionResult> RefreshToken()
		{
			if (User.Identity != null && User.Identity.IsAuthenticated)
			{
				throw new APIException(APIErrorCode.ALREADY_LOGGED_IN);
			}
			HttpContext.Request.Cookies.TryGetValue(Assets.Secrets.RefreshTokenCookieName, out string? refreshToken);
			if (refreshToken == null)
			{
				return BadRequest();
			}
			DateTime? accesTokenExpirationDate = await AuthBackend.RefreshAccessToken(refreshToken, userService, Response);
			if (accesTokenExpirationDate == null)
			{
				return BadRequest();
			}
			return Ok(new RefreshOut(accesTokenExpirationDate.Value));
		}
	}
}
