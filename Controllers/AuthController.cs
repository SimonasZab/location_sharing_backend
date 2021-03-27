using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using location_sharing_backend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using APIUtils;
using location_sharing_backend.Backends;
using location_sharing_backend.Services;

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

		public class RegistrationData {
			[Required]
			[StringLength(30, MinimumLength = 5)]
			public string Username { get; set; }
			[Required]
			[StringLength(30, MinimumLength = 8)]
			public string Password { get; set; }
			[Required]
			[EmailAddress]
			public string Email { get; set; }
			public string? ProfilePhotoURL { get; set; }
		}

		[HttpPost("register")]
		public async Task<ActionResult> Register(RegistrationData registartionData) {
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

		public class LoginnData {
			[Required]
			[StringLength(30, MinimumLength = 5)]
			public string Username { get; set; }
			[Required]
			[StringLength(30, MinimumLength = 8)]
			public string Password { get; set; }
		}

		[HttpPost("login")]
		public async Task<ActionResult> Login(LoginnData loginData) {
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

			var claims = new List<Claim>{
				new Claim(ClaimTypes.Name, user.Username),
				new Claim("UserId", user.Id),
			};
			await Common.cookieLogin(claims, false, HttpContext);

			return Ok();
		}

		[Authorize]
		[HttpPost("logout")]
		public async Task<IActionResult> logout() {
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return Ok();
		}
	}
}
