using Api.Backends;
using Api.Models.DB;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models.IO.Location;
using Api.Models.Internal;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class LocationController : ControllerBase
	{
		private readonly LocationService locationService;
		private readonly UserShareService userShareService;

		public LocationController(LocationService _locationService, UserShareService _userShareService)
		{
			locationService = _locationService;
			userShareService = _userShareService;
		}

		[Authorize]
		[HttpGet]
		public async Task<ActionResult<GetLocationsOut>> GetLocationsSharedWithCurrentUser()
		{
			AuthClaims authClaims = AuthClaims.ParseClaimsPrincipal(User);
			List<UserShare> userShares = await userShareService.GetUserLocationsSharedWithUser(authClaims.UserId);
			List<string> locationIds = new List<string>();
			foreach (var item in userShares)
			{
				locationIds.Add(item.SharedObj.Id.AsString);
			}
			List<Location> locations = await locationService.GetByIds(locationIds);

			GetLocationsOut getLocationsOut = new GetLocationsOut();
			foreach (var item in locations)
			{
				getLocationsOut.UserLocations.Add(
					new GetLocationsOut.UserLocation()
					{
						UserId = item.LinkedObj.Id.AsString,
						Latitude = item.Latitude,
						Longitude = item.Longitude
					}
				);
			}

			return Ok(getLocationsOut);
		}

		[Authorize]
		[HttpPost]
		public IActionResult UpdateCurrentUserLocation([FromBody] CurrentUserLocationIn currentUserLocationIn)
		{
			AuthClaims authClaims = AuthClaims.ParseClaimsPrincipal(User);
			Location location = new Location()
			{
				Latitude = currentUserLocationIn.Latitude,
				Longitude = currentUserLocationIn.Longitude,
				LinkedObj = new MongoDBRef(Assets.DbInfo.Collections.Users, authClaims.UserId)
			};
			locationService.CreateOrUpdate(location);
			return Ok();
		}
	}
}
