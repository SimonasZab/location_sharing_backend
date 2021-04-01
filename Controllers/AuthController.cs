using System.Threading.Tasks;
using location_sharing_backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using APIUtils;
using location_sharing_backend.Backends;
using location_sharing_backend.Services;
using static location_sharing_backend.IOModels.AuthModels;
using System;

namespace location_sharing_backend.Controllers {
	[ApiController]
	[Route(Settings.URL_PREFIX + "[controller]")]
	public class AuthController : ControllerBase {
		private readonly UserService userService;
		private readonly ISecrets secrets;

		public AuthController(ISecrets _secrets, UserService _userService) {
			userService = _userService;
			secrets = _secrets;
		}

		[HttpPost("register")]
		public async Task<ActionResult> Register(RegisterIn registartionData) {
			registartionData.Validate();
			if (await userService.UserExists(registartionData.Username, registartionData.Email)) {
				return BadRequest();
			}
			User user = new User() {
				Username = registartionData.Username,
				Password = Common.hashText(registartionData.Password, secrets.SALT),
				Email = registartionData.Email,
				ProfilePhotoURL = registartionData.ProfilePhotoURL
			};
			userService.Create(user);

			return Ok();
		}

		[HttpPost("login")]
		public async Task<ActionResult> Login(LoginIn loginData) {
			loginData.Validate();
			if (User.Identity != null && User.Identity.IsAuthenticated) {
				throw new APIException(APIErrorCode.ALREADY_LOGGED_IN);
			}

			User user = await userService.GetByUsername(loginData.Username);
			if (user == null) {
				return BadRequest();
			}

			string hashedPassword = Common.hashText(loginData.Password, secrets.SALT);
			if (user.Password != hashedPassword) {
				return BadRequest();
			}

			LoginOut loginOut = new LoginOut(user, AuthBackend.GenerateJwtTokens(user, secrets, loginData.Persist, Response));
			return Ok(loginOut);
		}

		[HttpPost("logout")]
		public IActionResult Logout() {
			Response.Cookies.Delete(secrets.AccessTokenCookieName);
			CookieOptions rtOptions = new CookieOptions();
			rtOptions.Path = "/api/auth";
			Response.Cookies.Delete(secrets.RefreshTokenCookieName, rtOptions);
			return Ok();
		}

		[HttpPost("refresh")]
		public async Task<IActionResult> RefreshToken() {
			if (User.Identity != null && User.Identity.IsAuthenticated) {
				throw new APIException(APIErrorCode.ALREADY_LOGGED_IN);
			}
			HttpContext.Request.Cookies.TryGetValue(secrets.RefreshTokenCookieName, out string? refreshToken);
			if (refreshToken == null) {
				return BadRequest();
			}
			DateTime? accesTokenExpirationDate = await AuthBackend.RefreshAccessToken(refreshToken, secrets, userService, Response);
			if (accesTokenExpirationDate == null) {
				return BadRequest();
			}
			return Ok(new RefreshOut(accesTokenExpirationDate.Value));
		}
	}
}
