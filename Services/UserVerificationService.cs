using location_sharing_backend.Models.DB;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Services
{
	public class UserVerificationService : ServiceBase<UserVerification>
	{
		public UserVerificationService() : base(Assets.DbInfo.Collections.UserVerifications) { }

		public async Task<UserVerification> FindByToken(string token)
		{
			return await collection.Find(x => x.Token == token).FirstOrDefaultAsync();
		}

		public async Task<UserVerification> FindByUser(User user)
		{
			return await collection.Find(x => x.User.CollectionName == Assets.DbInfo.Collections.Users && x.User.Id == user.Id).FirstOrDefaultAsync();
		}
	}
}
