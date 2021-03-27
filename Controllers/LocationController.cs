using location_sharing_backend.Backends;
using location_sharing_backend.Models;
using location_sharing_backend.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Controllers {
	[ApiController]
	[Route(Settings.URL_PREFIX + "[controller]")]
	public class LocationController : ControllerBase {
		private readonly LocationService _locationService;
		private readonly UserShareService _userShareService;

		public LocationController(LocationService locationService, UserShareService userShareService) {
			_locationService = locationService;
			_userShareService = userShareService;
		}

		public class GetLocationsIn {
			
		}

		public class GetLocationsOut {
			public class UserLocation {
				public string UserId { get; set; }
				public float Longitude { get; set; }
				public float Latitude { get; set; }
			}
			public List<UserLocation> UserLocations { get; set; } = new List<UserLocation>();
		}

		[HttpGet]
		public async Task<ActionResult<GetLocationsOut>> GetLocationsSharedWithCurrentUser() {
			string currentUserId = UserBackend.GetUserIdFromClaims(User);
			List<UserShare> userShares = await _userShareService.GetUserLocationsSharedWithUser(currentUserId);
			List<string> locationIds = new List<string>();
			foreach (UserShare item in userShares) {
				locationIds.Add(item.SharedObj.Id.AsString);
			}
			List<Location> locations = await _locationService.GetByIds(locationIds);

			GetLocationsOut getLocationsOut = new GetLocationsOut();
			foreach (Location item in locations) {
				getLocationsOut.UserLocations.Add(
					new GetLocationsOut.UserLocation() {
						UserId = item.LinkedObj.Id.AsString,
						Latitude = item.Latitude,
						Longitude = item.Longitude
					}
				);
			}

			return Ok(getLocationsOut);
		}

		public class CurrentUserLocationIn {
			public float Latitude { get; set; }
			public float Longitude { get; set; }
		}

		[HttpPost]
		public IActionResult UpdateCurrentUserLocation([FromBody]CurrentUserLocationIn currentUserLocationIn) {
			string currentUserId = UserBackend.GetUserIdFromClaims(User);
			Location location = new Location() {
				Latitude = currentUserLocationIn.Latitude,
				Longitude = currentUserLocationIn.Longitude,
				LinkedObj = new MongoDBRef("Users", currentUserId)
			};
			_locationService.CreateOrUpdate(location);
			return Ok();
		}
	}
}
