using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models.IO.Location
{
	public class GetLocationsOut
	{
		public class UserLocation
		{
			public string UserId { get; set; }
			public float Longitude { get; set; }
			public float Latitude { get; set; }
		}
		public List<UserLocation> UserLocations { get; set; } = new List<UserLocation>();
	}
}
