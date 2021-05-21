using APIUtils;
using location_sharing_backend.Backends;
using location_sharing_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace location_sharing_backend.Controllers
{
	[ApiController]
	[Authorize]
	[Route(Settings.URL_PREFIX + "[controller]")]
	public class UserController : ControllerBase
	{
		private readonly UserService userService;
		public UserController(UserService _userService)
		{
			userService = _userService;
		}

		/*[HttpPost("IsUsernameTaken")]
		public async Task<ActionResult> CheckIfUsernameIsTaken(CheckUsernameIn dataIn) {
			if (await userService.UserExists(dataIn.Username, registartionData.Email)) {
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
		}*/
	}
}