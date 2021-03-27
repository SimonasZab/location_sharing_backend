using location_sharing_backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace location_sharing_backend.Services {
	public class UserShareService : ServiceBase<UserShare> {
		public UserShareService(IDatabaseSettings settings) : base(settings, settings.UserSharesCollectionName) { }

		public async Task<List<UserShare>> GetUserLocationsSharedWithUser(string userId) {
			return await collection.Find(x => x.Receiver.CollectionName == "Users" && x.Receiver.Id == userId && x.SharedObj.CollectionName == "Locations").ToListAsync();
		}
	}
}