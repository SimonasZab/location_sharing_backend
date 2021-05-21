using location_sharing_backend.Backends;
using location_sharing_backend.Models.DB;
using location_sharing_backend.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using location_sharing_backend.Models.IO.Location;

namespace location_sharing_backend.Controllers
{
	[ApiController]
	[Route(Settings.URL_PREFIX + "[controller]")]
	public class LocationController : ControllerBase
	{
		private readonly LocationService locationService;
		private readonly UserShareService userShareService;

		public LocationController(LocationService _locationService, UserShareService _userShareService)
		{
			locationService = _locationService;
			userShareService = _userShareService;
		}

		[HttpGet]
		public async Task<ActionResult<GetLocationsOut>> GetLocationsSharedWithCurrentUser()
		{
			AuthClaims authClaims = AuthClaims.ParseClaimsPrincipal(User);
			List<UserShare> userShares = await userShareService.GetUserLocationsSharedWithUser(authClaims.UserId);
			List<string> locationIds = new List<string>();
			foreach (UserShare item in userShares)
			{
				locationIds.Add(item.SharedObj.Id.AsString);
			}
			List<Location> locations = await locationService.GetByIds(locationIds);

			GetLocationsOut getLocationsOut = new GetLocationsOut();
			foreach (Location item in locations)
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
