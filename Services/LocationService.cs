using location_sharing_backend.Models.DB;
using location_sharing_backend.Models.Settings;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Services
{
	public class LocationService : ServiceBase<Location>
	{
		public LocationService() : base(Assets.DbInfo.Collections.Locations) { }

		public async Task<Location> GetByUserId(string userId)
		{
			return await collection.Find(x => x.LinkedObj.CollectionName == Assets.DbInfo.Collections.Users && x.LinkedObj.Id == userId).FirstOrDefaultAsync();
		}

		public void CreateOrUpdate(Location location)
		{
			ReplaceOptions options = new ReplaceOptions();
			options.IsUpsert = true;
			collection.ReplaceOne(x => x.LinkedObj.CollectionName == Assets.DbInfo.Collections.Users && x.LinkedObj.Id == location.LinkedObj.Id, location, options);
		}
	}
}
