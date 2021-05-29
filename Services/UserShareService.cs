using Api.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Api.Models.Settings;

namespace Api.Services
{
	public class UserShareService : ServiceBase<UserShare>
	{
		public UserShareService() : base(Assets.DbInfo.Collections.UserShares) { }

		public async Task<List<UserShare>> GetUserLocationsSharedWithUser(string userId)
		{
			return await collection.Find(
				x => x.Receiver.CollectionName == Assets.DbInfo.Collections.Users &&
				x.Receiver.Id == userId && x.SharedObj.CollectionName == Assets.DbInfo.Collections.Locations
			).ToListAsync();
		}
	}
}